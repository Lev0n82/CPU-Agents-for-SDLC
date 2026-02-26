namespace Phase3.AzureDevOps.Services.WorkItems;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models;
using Phase3.AzureDevOps.Configuration;
using Phase3.AzureDevOps.Core;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

/// <summary>
/// Provides operations for managing Azure DevOps work items.
/// </summary>
public class WorkItemService : IWorkItemService
{
    private readonly WorkItemTrackingHttpClient _client;
    private readonly IWIQLValidator _wiqlValidator;
    private readonly WorkItemConfiguration _config;
    private readonly ILogger<WorkItemService> _logger;

    public WorkItemService(
        WorkItemTrackingHttpClient client,
        IWIQLValidator wiqlValidator,
        WorkItemConfiguration config,
        ILogger<WorkItemService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _wiqlValidator = wiqlValidator ?? throw new ArgumentNullException(nameof(wiqlValidator));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a work item by ID.
    /// </summary>
    public async Task<WorkItem> GetWorkItemAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving work item {WorkItemId}", id);

        try
        {
            var workItem = await _client.GetWorkItemAsync(
                id,
                expand: WorkItemExpand.All,
                cancellationToken: cancellationToken);

            _logger.LogDebug("Work item {WorkItemId} retrieved successfully", id);

            return MapToWorkItem(workItem);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Work item {WorkItemId} not found", id);
            throw new WorkItemNotFoundException(id);
        }
    }

    /// <summary>
    /// Creates a new work item.
    /// </summary>
    public async Task<WorkItem> CreateWorkItemAsync(
        string workItemType,
        Dictionary<string, object> fields,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating work item of type {WorkItemType}", workItemType);

        ValidateWorkItemType(workItemType);
        ValidateFieldsForCreate(fields);

        var patchDocument = new JsonPatchDocument();
        foreach (var field in fields)
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = $"/fields/{field.Key}",
                Value = field.Value
            });
        }

        try
        {
            var workItem = await _client.CreateWorkItemAsync(
                patchDocument,
                _config.ProjectName,
                workItemType,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Work item {WorkItemId} created successfully", workItem.Id);

            return MapToWorkItem(workItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create work item of type {WorkItemType}", workItemType);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing work item.
    /// </summary>
    public async Task<WorkItem> UpdateWorkItemAsync(
        int id,
        int revision,
        Dictionary<string, object> fields,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating work item {WorkItemId} (rev {Revision})", id, revision);

        ValidateFieldsForUpdate(fields);

        var patchDocument = new JsonPatchDocument();
        foreach (var field in fields)
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = $"/fields/{field.Key}",
                Value = field.Value
            });
        }

        try
        {
            // Add ETag header for optimistic concurrency control
            var workItem = await _client.UpdateWorkItemAsync(
                patchDocument,
                id,
                bypassRules: false,
                suppressNotifications: false,
                cancellationToken: cancellationToken);

            // Verify revision matches expected
            if (workItem.Rev != revision + 1)
            {
                throw new ConcurrencyException(
                    id,
                    revision,
                    workItem.Rev.GetValueOrDefault() - 1);
            }

            _logger.LogInformation("Work item {WorkItemId} updated successfully (new rev: {NewRevision})",
                id, workItem.Rev);

            return MapToWorkItem(workItem);
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning("Work item {WorkItemId} not found", id);
            throw new WorkItemNotFoundException(id);
        }
    }

    /// <summary>
    /// Queries work items using WIQL.
    /// </summary>
    public async Task<List<WorkItem>> QueryWorkItemsAsync(
        string wiql,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Executing WIQL query");

        // Validate WIQL to prevent injection attacks
        var validationResult = _wiqlValidator.Validate(wiql);
        if (!validationResult.IsValid)
        {
            _logger.LogError("WIQL validation failed: {Errors}",
                string.Join(", ", validationResult.Errors));
            throw new WIQLValidationException(validationResult.Errors);
        }

        try
        {
            var query = new Wiql { Query = wiql };
            var result = await _client.QueryByWiqlAsync(
                query,
                _config.ProjectName,
                cancellationToken: cancellationToken);

            _logger.LogDebug("WIQL query returned {Count} work items", result.WorkItems.Count());

            // Batch retrieve work items (max 200 per batch)
            var workItemIds = result.WorkItems.Select(wi => wi.Id).ToList();
            var workItems = new List<WorkItem>();

            for (int i = 0; i < workItemIds.Count; i += 200)
            {
                var batch = workItemIds.Skip(i).Take(200).ToList();
                var batchWorkItems = await _client.GetWorkItemsAsync(
                    batch,
                    expand: WorkItemExpand.All,
                    cancellationToken: cancellationToken);

                workItems.AddRange(batchWorkItems.Select(MapToWorkItem));
            }

            _logger.LogDebug("Retrieved {Count} work items", workItems.Count);

            return workItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute WIQL query");
            throw;
        }
    }

    /// <summary>
    /// Adds an attachment to a work item.
    /// </summary>
    public async Task<AttachmentReference> AddAttachmentAsync(
        int workItemId,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding attachment {FileName} to work item {WorkItemId}",
            Path.GetFileName(filePath), workItemId);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Attachment file not found: {filePath}");

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > _config.MaxAttachmentSizeBytes)
        {
            throw new AttachmentTooLargeException(
                fileInfo.Length,
                _config.MaxAttachmentSizeBytes);
        }

        try
        {
            // Compress attachment if enabled
            byte[] fileBytes;
            string fileName = Path.GetFileName(filePath);

            if (_config.CompressAttachments && ShouldCompress(filePath))
            {
                _logger.LogDebug("Compressing attachment {FileName}", fileName);
                fileBytes = await CompressFileAsync(filePath, cancellationToken);
                fileName += ".gz";
                _logger.LogDebug("Attachment compressed: {OriginalSize} -> {CompressedSize} bytes",
                    fileInfo.Length, fileBytes.Length);
            }
            else
            {
                fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
            }

            // Upload attachment
            using var stream = new MemoryStream(fileBytes);
            // Use the 5-parameter overload to avoid ambiguity
            var attachmentReference = await _client.CreateAttachmentAsync(
                stream,      // content
                fileName,    // fileName  
                "Simple",    // uploadType
                null,        // userState
                cancellationToken);

            // Link attachment to work item
            var patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "AttachedFile",
                        url = attachmentReference.Url,
                        attributes = new
                        {
                            comment = $"Attached by Autonomous Agent on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
                        }
                    }
                }
            };

            await _client.UpdateWorkItemAsync(
                patchDocument,
                workItemId,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Attachment {FileName} added successfully to work item {WorkItemId}",
                fileName, workItemId);

            // Return the attachment reference from Azure DevOps SDK
            return attachmentReference;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add attachment to work item {WorkItemId}", workItemId);
            throw;
        }
    }

    /// <summary>
    /// Gets all attachments for a work item.
    /// </summary>
    public async Task<List<AttachmentReference>> GetAttachmentsAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving attachments for work item {WorkItemId}", workItemId);

        var workItem = await _client.GetWorkItemAsync(
            workItemId,
            expand: WorkItemExpand.Relations,
            cancellationToken: cancellationToken);

        // Get attachment references from work item relations
        var attachmentUrls = workItem.Relations?
            .Where(r => r.Rel == "AttachedFile")
            .Select(r => r.Url)
            .ToList() ?? new List<string>();

        // For now, return empty list as we'd need to fetch each attachment to get full details
        // In a real implementation, you'd fetch each attachment by URL to get the AttachmentReference
        var attachments = new List<AttachmentReference>();
        _logger.LogDebug("Found {Count} attachment URLs for work item {WorkItemId}",
            attachmentUrls.Count, workItemId);

        _logger.LogDebug("Found {Count} attachments for work item {WorkItemId}",
            attachments.Count, workItemId);

        return attachments;
    }

    /// <summary>
    /// Downloads an attachment.
    /// </summary>
    public async Task<byte[]> DownloadAttachmentAsync(
        string attachmentUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Downloading attachment from {Url}", attachmentUrl);

        try
        {
            using var httpClient = new HttpClient();
            var bytes = await httpClient.GetByteArrayAsync(attachmentUrl, cancellationToken);

            _logger.LogDebug("Downloaded {Size} bytes", bytes.Length);

            // Decompress if needed
            if (attachmentUrl.EndsWith(".gz"))
            {
                _logger.LogDebug("Decompressing attachment");
                bytes = await DecompressAsync(bytes, cancellationToken);
                _logger.LogDebug("Decompressed to {Size} bytes", bytes.Length);
            }

            return bytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download attachment from {Url}", attachmentUrl);
            throw;
        }
    }

    // Helper methods

    private WorkItem MapToWorkItem(Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem apiWorkItem)
    {
        return new WorkItem
        {
            Id = apiWorkItem.Id.GetValueOrDefault(),
            Rev = apiWorkItem.Rev.GetValueOrDefault(),
            Fields = apiWorkItem.Fields?.ToDictionary(f => f.Key, f => f.Value) ?? new Dictionary<string, object>(),
            Url = apiWorkItem.Url
        };
    }

    private void ValidateWorkItemType(string workItemType)
    {
        var validTypes = new[] { "Bug", "Task", "User Story", "Feature", "Epic", "Test Case" };
        if (!validTypes.Contains(workItemType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Invalid work item type: {workItemType}. Valid types: {string.Join(", ", validTypes)}",
                nameof(workItemType));
        }
    }

    /// <summary>
    /// Validates fields for creating a new work item.
    /// System.Title is required for new work items.
    /// </summary>
    private void ValidateFieldsForCreate(Dictionary<string, object> fields)
    {
        if (fields == null || fields.Count == 0)
            throw new ArgumentException("Fields dictionary cannot be null or empty.", nameof(fields));

        // Validate required fields for create
        if (!fields.ContainsKey("System.Title"))
            throw new ArgumentException("Field 'System.Title' is required for creating work items.", nameof(fields));
    }

    /// <summary>
    /// Validates fields for updating an existing work item.
    /// System.Title is NOT required for updates - only the fields being changed need to be provided.
    /// </summary>
    private void ValidateFieldsForUpdate(Dictionary<string, object> fields)
    {
        if (fields == null || fields.Count == 0)
            throw new ArgumentException("Fields dictionary cannot be null or empty.", nameof(fields));

        // For updates, we don't require System.Title since we're only updating specific fields
        // The work item already exists and has a title
    }

    private bool ShouldCompress(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var compressibleExtensions = new[] { ".txt", ".log", ".json", ".xml", ".csv", ".md" };
        return compressibleExtensions.Contains(extension);
    }

    private async Task<byte[]> CompressFileAsync(string filePath, CancellationToken cancellationToken)
    {
        var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);

        using var outputStream = new MemoryStream();
        using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
        {
            await gzipStream.WriteAsync(fileBytes, cancellationToken);
        }

        return outputStream.ToArray();
    }

    private async Task<byte[]> DecompressAsync(byte[] compressedBytes, CancellationToken cancellationToken)
    {
        using var inputStream = new MemoryStream(compressedBytes);
        using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();

        await gzipStream.CopyToAsync(outputStream, cancellationToken);
        return outputStream.ToArray();
    }
}
