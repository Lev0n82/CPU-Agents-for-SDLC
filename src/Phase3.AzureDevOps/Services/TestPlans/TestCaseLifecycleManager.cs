namespace Phase3.AzureDevOps.Services.TestPlans;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Phase3.AzureDevOps.Interfaces;

/// <summary>
/// Implements test case lifecycle management.
/// </summary>
public class TestCaseLifecycleManager : ITestCaseLifecycleManager
{
    private readonly IWorkItemService _workItemService;
    private readonly ILogger<TestCaseLifecycleManager> _logger;
    private readonly string _projectName;
    private readonly string _closureReason;
    private readonly WorkItemTrackingHttpClient _client;

    public TestCaseLifecycleManager(
        IWorkItemService workItemService,
        ILogger<TestCaseLifecycleManager> logger,
        IConfiguration configuration,
        WorkItemTrackingHttpClient client)
    {
        _workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        
        _projectName = configuration["AzureDevOps:ProjectName"] 
            ?? throw new InvalidOperationException("ProjectName not configured");
        _closureReason = configuration["TestLifecycle:DefaultClosureReason"] 
            ?? "Linked requirement removed";
    }

    public async Task<IEnumerable<int>> IdentifyObsoleteTestCasesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Identifying obsolete test cases");

        // Query all test cases
        var wiql = new Wiql
        {
            Query = @"
                SELECT [System.Id]
                FROM WorkItems
                WHERE [System.WorkItemType] = 'Test Case'
                AND [System.State] <> 'Closed'
                AND [System.TeamProject] = @project"
        };

        var queryResult = await _client.QueryByWiqlAsync(wiql, _projectName, cancellationToken: cancellationToken);
        var testCaseIds = queryResult.WorkItems.Select(wi => wi.Id).ToList();

        var obsoleteTestCases = new List<int>();

        foreach (var testCaseId in testCaseIds)
        {
            var links = await GetTestCaseLinksAsync(testCaseId, cancellationToken);
            
            // Check if all linked requirements are removed
            bool allRequirementsRemoved = true;
            foreach (var requirementId in links)
            {
                try
                {
                    var requirement = await _workItemService.GetWorkItemAsync(requirementId, cancellationToken);
                    if (requirement.Fields.ContainsKey("System.State") && 
                        requirement.Fields["System.State"].ToString() != "Removed")
                    {
                        allRequirementsRemoved = false;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to check requirement {RequirementId}", requirementId);
                    // Assume requirement exists if we can't check
                    allRequirementsRemoved = false;
                    break;
                }
            }

            if (allRequirementsRemoved && links.Any())
            {
                obsoleteTestCases.Add(testCaseId);
            }
        }

        _logger.LogInformation("Identified {Count} obsolete test cases", obsoleteTestCases.Count);
        return obsoleteTestCases;
    }

    public async Task<int> CloseObsoleteTestCasesAsync(
        IEnumerable<int> testCaseIds,
        string reason,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Closing {Count} obsolete test cases", testCaseIds.Count());

        int closedCount = 0;

        foreach (var testCaseId in testCaseIds)
        {
            try
            {
                var patchDocument = new JsonPatchDocument
                {
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.State",
                        Value = "Closed"
                    },
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.Reason",
                        Value = reason
                    },
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.History",
                        Value = $"Automatically closed: {reason}"
                    }
                };

                await _client.UpdateWorkItemAsync(patchDocument, testCaseId, cancellationToken: cancellationToken);
                
                // Record audit trail
                await RecordAuditEntryAsync(testCaseId, "Closed", reason, cancellationToken);
                
                closedCount++;
                _logger.LogInformation("Closed test case {TestCaseId}", testCaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to close test case {TestCaseId}", testCaseId);
            }
        }

        _logger.LogInformation("Closed {ClosedCount}/{TotalCount} test cases", closedCount, testCaseIds.Count());
        return closedCount;
    }

    public async Task<IEnumerable<int>> GetTestCaseLinksAsync(
        int testCaseId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting requirement links for test case {TestCaseId}", testCaseId);

        var workItem = await _client.GetWorkItemAsync(
            testCaseId,
            expand: WorkItemExpand.Relations,
            cancellationToken: cancellationToken);

        if (workItem.Relations == null)
        {
            return Enumerable.Empty<int>();
        }

        var requirementLinks = workItem.Relations
            .Where(r => r.Rel == "Microsoft.VSTS.Common.TestedBy-Reverse")
            .Select(r => ExtractWorkItemId(r.Url))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        _logger.LogDebug("Found {Count} requirement links for test case {TestCaseId}", 
            requirementLinks.Count, testCaseId);

        return requirementLinks;
    }

    public async Task<IEnumerable<LifecycleAuditEntry>> GetAuditTrailAsync(
        int testCaseId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting audit trail for test case {TestCaseId}", testCaseId);

        // Query custom audit table (would be implemented with database)
        // For now, return empty list
        await Task.CompletedTask;
        return Enumerable.Empty<LifecycleAuditEntry>();
    }

    private async Task RecordAuditEntryAsync(
        int testCaseId,
        string action,
        string reason,
        CancellationToken cancellationToken)
    {
        var entry = new LifecycleAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = action,
            Reason = reason,
            PerformedBy = "CPU Agents",
            Metadata = new Dictionary<string, string>
            {
                ["TestCaseId"] = testCaseId.ToString(),
                ["ProjectName"] = _projectName
            }
        };

        // Store audit entry (would be implemented with database)
        _logger.LogInformation("Recorded audit entry for test case {TestCaseId}: {Action}", testCaseId, action);
        await Task.CompletedTask;
    }

    private int? ExtractWorkItemId(string url)
    {
        // Extract work item ID from URL
        // Example: https://dev.azure.com/org/project/_apis/wit/workItems/123
        var parts = url.Split('/');
        if (parts.Length > 0 && int.TryParse(parts[^1], out var id))
        {
            return id;
        }
        return null;
    }
}
