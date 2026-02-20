namespace Phase3.AzureDevOps.Services.Concurrency;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models;
using Phase3.AzureDevOps.Configuration;
using Phase3.AzureDevOps.Core;
using Microsoft.Extensions.Logging;
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

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/Custom.ProcessingAgent",
                    Value = agentId
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/Custom.ClaimExpiry",
                    Value = claimExpiry
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
        var currentOwner = workItem.Fields.GetValueOrDefault("Custom.ProcessingAgent") as string;

        if (currentOwner != agentId)
        {
            _logger.LogWarning("Agent {AgentId} cannot release work item {WorkItemId}: owned by {Owner}",
                agentId, workItemId, currentOwner);
            return;
        }

        var patch = new JsonPatchDocument
        {
            new JsonPatchOperation
            {
                Operation = Operation.Remove,
                Path = "/fields/Custom.ProcessingAgent"
            },
            new JsonPatchOperation
            {
                Operation = Operation.Remove,
                Path = "/fields/Custom.ClaimExpiry"
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
        var currentOwner = workItem.Fields.GetValueOrDefault("Custom.ProcessingAgent") as string;

        if (currentOwner != agentId)
        {
            throw new UnauthorizedAccessException(
                $"Agent {agentId} cannot renew claim on work item {workItemId}: owned by {currentOwner}");
        }

        var newExpiry = DateTime.UtcNow.AddMinutes(_config.ClaimDurationMinutes);

        var patch = new JsonPatchDocument
        {
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Custom.ClaimExpiry",
                Value = newExpiry
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
            SELECT [System.Id], [System.Rev], [Custom.ProcessingAgent], [Custom.ClaimExpiry]
            FROM WorkItems
            WHERE [Custom.ClaimExpiry] < '{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}'";

        var workItems = await _azureDevOpsClient.QueryWorkItemsAsync(wiql, cancellationToken);

        var claims = workItems.Select(wi => new WorkItemClaim
        {
            WorkItemId = wi.Id,
            Revision = wi.Rev,
            AgentId = wi.Fields.GetValueOrDefault("Custom.ProcessingAgent") as string ?? string.Empty,
            ClaimedAt = DateTime.UtcNow, // Not stored, approximation
            ExpiresAt = (DateTime)(wi.Fields.GetValueOrDefault("Custom.ClaimExpiry") ?? DateTime.MinValue)
        }).ToList();

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
}
