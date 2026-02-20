namespace Phase3.AzureDevOps.Interfaces;

using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Phase3.AzureDevOps.Models;

public interface IAzureDevOpsClient
{
    Task<WorkItem> GetWorkItemAsync(int id, CancellationToken cancellationToken = default);
    Task<WorkItem> UpdateWorkItemAsync(int id, JsonPatchDocument patchDocument, int? revision = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkItem>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default);
}
