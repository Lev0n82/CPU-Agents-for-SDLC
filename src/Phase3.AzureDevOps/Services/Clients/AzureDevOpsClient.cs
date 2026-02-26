namespace Phase3.AzureDevOps.Services.Clients;

using Phase3.AzureDevOps.Interfaces;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation of Azure DevOps client operations.
/// </summary>
public class AzureDevOpsClient : IAzureDevOpsClient
{
    private readonly WorkItemTrackingHttpClient _httpClient;
    private readonly ILogger<AzureDevOpsClient> _logger;

    public AzureDevOpsClient(
        WorkItemTrackingHttpClient httpClient,
        ILogger<AzureDevOpsClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<WorkItem> GetWorkItemAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting work item {WorkItemId}", id);
        return await _httpClient.GetWorkItemAsync(id, expand: WorkItemExpand.All, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<WorkItem> UpdateWorkItemAsync(int id, JsonPatchDocument patchDocument, int? revision = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating work item {WorkItemId}", id);
        return await _httpClient.UpdateWorkItemAsync(patchDocument, id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkItem>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying work items with WIQL");
        var queryResult = await _httpClient.QueryByWiqlAsync(new Wiql { Query = wiql }, cancellationToken: cancellationToken);
        
        if (queryResult.WorkItems == null || !queryResult.WorkItems.Any())
        {
            return Enumerable.Empty<WorkItem>();
        }

        var workItemIds = queryResult.WorkItems.Select(wi => wi.Id).ToArray();
        return await _httpClient.GetWorkItemsAsync(workItemIds, expand: WorkItemExpand.All, cancellationToken: cancellationToken);
    }
}