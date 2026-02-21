namespace Phase3.AzureDevOps.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

/// <summary>
/// Provides Azure DevOps client operations for work items.
/// </summary>
public interface IAzureDevOpsClient
{
    /// <summary>
    /// Gets a work item by ID.
    /// </summary>
    Task<WorkItem> GetWorkItemAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a work item with a JSON patch document.
    /// </summary>
    Task<WorkItem> UpdateWorkItemAsync(int id, JsonPatchDocument patchDocument, int? revision = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Queries work items using WIQL.
    /// </summary>
    Task<IEnumerable<WorkItem>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default);
}
