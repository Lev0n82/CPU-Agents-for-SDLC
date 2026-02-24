namespace Phase3.AzureDevOps.Services.QA;

/// <summary>
/// Project-specific QA metrics
/// </summary>
public class QAProjectMetrics
{
    public string ProjectName { get; set; } = string.Empty;
    public int TotalReleases { get; set; }
    public int TotalRequirements { get; set; }
    public int TotalTestCases { get; set; }
    public int TotalExecutions { get; set; }
    public double PassRate { get; set; }
}