namespace Phase3.AzureDevOps.Configuration;

public class WorkItemConfiguration
{
    public string ProjectName { get; set; } = string.Empty;
    public long MaxAttachmentSizeBytes { get; set; } = 60 * 1024 * 1024;
    public bool EnableAttachmentCompression { get; set; } = true;
    public bool CompressAttachments => EnableAttachmentCompression; // Alias
    public long CompressionThresholdBytes { get; set; } = 1024 * 1024;
}
