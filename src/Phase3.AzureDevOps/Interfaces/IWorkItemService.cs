namespace Phase3.AzureDevOps.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

/// <summary>
/// Provides operations for managing Azure DevOps work items.
/// </summary>
public interface IWorkItemService
{
    /// <summary>
    /// Gets a work item by ID.
    /// </summary>
    Task<WorkItem> GetWorkItemAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new work item.
    /// </summary>
    Task<WorkItem> CreateWorkItemAsync(string workItemType, Dictionary<string, object> fields, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing work item.
    /// </summary>
    Task<WorkItem> UpdateWorkItemAsync(int id, int revision, Dictionary<string, object> fields, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries work items using WIQL.
    /// </summary>
    Task<List<WorkItem>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an attachment to a work item.
    /// </summary>
    Task<AttachmentReference> AddAttachmentAsync(int workItemId, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all attachments for a work item.
    /// </summary>
    Task<List<AttachmentReference>> GetAttachmentsAsync(int workItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads an attachment.
    /// </summary>
    Task<byte[]> DownloadAttachmentAsync(string attachmentUrl, CancellationToken cancellationToken = default);
}
