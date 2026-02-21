namespace Phase3.AzureDevOps.Configuration;

public class DPAPIConfiguration
{
    public string StoragePath { get; set; } = string.Empty;
    public string StorePath => StoragePath; // Alias for compatibility
}
