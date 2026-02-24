namespace Phase3.AzureDevOps.Services.Concurrency;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models;
using Phase3.AzureDevOps.Configuration;
using Phase3.AzureDevOps.Core;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System.Text.RegularExpressions;

/// <summary>
/// Coordinates work item processing across multiple agents to prevent race conditions.
/// </summary>
public class WorkItemCoordinator : IWorkItemCoordinator
{
    private readonly IAzureDevOpsClient _azureDevOpsClient;
    private readonly ConcurrencyConfiguration _config;
    private readonly ILogger<WorkItemCoordinator> _logger;
    private static readonly Regex AgentIdPattern = new Regex(@"^[a-zA-Z0-9\-]{3,50}$");

    public WorkItemCoordinator(
        IAzureDevOpsClient azureDevOpsClient,
        ConcurrencyConfiguration config,
        ILogger<WorkItemCoordinator> logger)
    {
        _azureDevOpsClient = azureDevOpsClient ?? throw new ArgumentNullException(nameof(azureDevOpsClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Attempts to claim a work item for processing by the specified agent.
    /// </summary>
    public async Task<bool> TryClaimWorkItemAsync(
        int workItemId,
        int revision,
        string agentId,
        CancellationToken cancellationToken = default)
    {
        ValidateAgentId(agentId);

        _logger.LogInformation("Agent {AgentId} attempting to claim work item {WorkItemId} (rev {Revision})",
            agentId, workItemId, revision);

        try
        {
            var claimExpiry = DateTime.UtcNow.AddMinutes(_config.ClaimDurationMinutes);

            var claimTag = $"agent:{agentId}:{DateTime.UtcNow:O}:{claimExpiry:O}";

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Tags",
                    Value = claimTag
                }
            };

            var updated = await _azureDevOpsClient.UpdateWorkItemAsync(
                workItemId,
                patch,
                revision,
                cancellationToken);

            _logger.LogInformation("Work item {WorkItemId} claimed successfully by agent {AgentId} (expires at {Expiry})",
                workItemId, agentId, claimExpiry);

            return true;
        }
        catch (ConcurrencyException ex)
        {
            _logger.LogWarning("Failed to claim work item {WorkItemId}: {Reason}",
                workItemId, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Releases a work item claim, making it available for other agents to process.
    /// </summary>
    public async Task ReleaseWorkItemAsync(
        int workItemId,
        int revision,
        string agentId,
        CancellationToken cancellationToken = default)
    {
        ValidateAgentId(agentId);

        _logger.LogInformation("Agent {AgentId} releasing work item {WorkItemId} (rev {Revision})",
            agentId, workItemId, revision);

        // Verify ownership before releasing
        var workItem = await _azureDevOpsClient.GetWorkItemAsync(workItemId, cancellationToken);
        var tags = workItem.Fields.TryGetValue("System.Tags", out var tagsValue) ? tagsValue?.ToString() : null;
        var currentOwner = ExtractAgentIdFromTags(tags);

        if (currentOwner != agentId)
        {
            _logger.LogWarning("Agent {AgentId} cannot release work item {WorkItemId}: owned by {Owner}",
                agentId, workItemId, currentOwner);
            return;
        }

        // Remove the agent claim tag
        var claimTag = tags?.Split(';').FirstOrDefault(t => t.Trim().StartsWith($"agent:{agentId}:"));
        if (claimTag == null)
        {
            _logger.LogWarning("No claim tag found for agent {AgentId} on work item {WorkItemId}",
                agentId, workItemId);
            return;
        }

        var remainingTags = string.Join(";", tags.Split(';').Where(t => t.Trim() != claimTag.Trim()));

        var patch = new JsonPatchDocument
        {
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.Tags",
                Value = remainingTags
            }
        };

        await _azureDevOpsClient.UpdateWorkItemAsync(
            workItemId,
            patch,
            revision,
            cancellationToken);

        _logger.LogInformation("Work item {WorkItemId} released successfully by agent {AgentId}",
            workItemId, agentId);
    }

    /// <summary>
    /// Renews the claim on a work item, extending its expiry time.
    /// </summary>
    public async Task RenewClaimAsync(
        int workItemId,
        int revision,
        string agentId,
        CancellationToken cancellationToken = default)
    {
        ValidateAgentId(agentId);

        _logger.LogDebug("Agent {AgentId} renewing claim on work item {WorkItemId}",
            agentId, workItemId);

        // Verify ownership before renewing
        var workItem = await _azureDevOpsClient.GetWorkItemAsync(workItemId, cancellationToken);
        var tags = workItem.Fields.TryGetValue("System.Tags", out var tagsValue) ? tagsValue?.ToString() : null;
        var currentOwner = ExtractAgentIdFromTags(tags);

        if (currentOwner != agentId)
        {
            throw new UnauthorizedAccessException(
                $"Agent {agentId} cannot renew claim on work item {workItemId}: owned by {currentOwner}");
        }

        // Remove old claim tag and add new one with updated expiry
        var oldClaimTag = tags?.Split(';').FirstOrDefault(t => t.Trim().StartsWith($"agent:{agentId}:"));
        var remainingTags = oldClaimTag != null 
            ? string.Join(";", tags.Split(';').Where(t => t.Trim() != oldClaimTag.Trim()))
            : tags;

        var newExpiry = DateTime.UtcNow.AddMinutes(_config.ClaimDurationMinutes);
        var newClaimTag = $"agent:{agentId}:{DateTime.UtcNow:O}:{newExpiry:O}";
        var updatedTags = string.IsNullOrEmpty(remainingTags) ? newClaimTag : $"{remainingTags};{newClaimTag}";

        var patch = new JsonPatchDocument
        {
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.Tags",
                Value = updatedTags
            }
        };

        await _azureDevOpsClient.UpdateWorkItemAsync(
            workItemId,
            patch,
            revision,
            cancellationToken);

        _logger.LogDebug("Claim renewed for work item {WorkItemId} (new expiry: {Expiry})",
            workItemId, newExpiry);
    }

    /// <summary>
    /// Gets all work items with expired claims.
    /// </summary>
    public async Task<IEnumerable<WorkItemClaim>> GetExpiredClaimsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying for expired claims");

        var wiql = $@"
            SELECT [System.Id], [System.Rev], [System.Tags]
            FROM WorkItems
            WHERE [System.Tags] CONTAINS 'agent:'";

        var workItems = await _azureDevOpsClient.QueryWorkItemsAsync(wiql, cancellationToken);

        var claims = new List<WorkItemClaim>();

        foreach (var wi in workItems)
        {
            var tags = wi.Fields.TryGetValue("System.Tags", out var tagsValue) ? tagsValue?.ToString() : null;
            var claimInfo = ParseClaimFromTags(tags);

            if (claimInfo != null && claimInfo.Value.ExpiresAt < DateTime.UtcNow)
            {
                claims.Add(new WorkItemClaim
                {
                    WorkItemId = wi.Id.GetValueOrDefault(),
                    Revision = wi.Rev.GetValueOrDefault(),
                    AgentId = claimInfo.Value.AgentId,
                    ClaimedAt = claimInfo.Value.ClaimedAt,
                    ExpiresAt = claimInfo.Value.ExpiresAt
                });
            }
        }

        _logger.LogDebug("Found {Count} expired claims", claims.Count);

        return claims;
    }

    /// <summary>
    /// Releases all expired claims, making them available for processing.
    /// </summary>
    public async Task<int> ReleaseExpiredClaimsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Releasing expired claims");

        var expiredClaims = await GetExpiredClaimsAsync(cancellationToken);
        var releasedCount = 0;

        foreach (var claim in expiredClaims)
        {
            try
            {
                await ReleaseWorkItemAsync(claim.WorkItemId, claim.Revision, claim.AgentId, cancellationToken);
                releasedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to release expired claim for work item {WorkItemId}",
                    claim.WorkItemId);
            }
        }

        _logger.LogInformation("Released {Count} expired claims", releasedCount);

        return releasedCount;
    }

    private void ValidateAgentId(string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId))
            throw new ArgumentException("Agent ID cannot be null or empty.", nameof(agentId));

        if (!AgentIdPattern.IsMatch(agentId))
            throw new ArgumentException(
                "Agent ID must be 3-50 alphanumeric characters (hyphens allowed).",
                nameof(agentId));
    }

    /// <summary>
    /// Extracts the agent ID from work item tags.
    /// </summary>
    private string? ExtractAgentIdFromTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return null;

        var claimTag = tags.Split(';').FirstOrDefault(t => t.Trim().StartsWith("agent:"));
        if (claimTag == null)
            return null;

        var parts = claimTag.Trim().Split(':');
        return parts.Length >= 2 ? parts[1] : null;
    }

    /// <summary>
    /// Parses claim information from work item tags.
    /// </summary>
    private (string AgentId, DateTime ClaimedAt, DateTime ExpiresAt)? ParseClaimFromTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return null;

        var claimTag = tags.Split(';').FirstOrDefault(t => t.Trim().StartsWith("agent:"));
        if (claimTag == null)
            return null;

        var parts = claimTag.Trim().Split(':');
        if (parts.Length < 4)
            return null;

        if (!DateTime.TryParse(parts[2], out var claimedAt))
            return null;

        if (!DateTime.TryParse(parts[3], out var expiresAt))
            return null;

        return (parts[1], claimedAt, expiresAt);
    }
}
