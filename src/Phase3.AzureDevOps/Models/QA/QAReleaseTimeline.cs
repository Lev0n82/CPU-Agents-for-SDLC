namespace Phase3.AzureDevOps.Services.QA;

/// <summary>
/// Release timeline data
/// </summary>
public class QAReleaseTimeline
{
    public string ReleaseName { get; set; } = string.Empty;
    public DateTime? RequirementsPublishedDate { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int TotalRequirements { get; set; }
    public int TotalTestCases { get; set; }
    public double AverageTestCaseDevelopmentDelayDays { get; set; }
    public double AverageFirstExecutionDelayDays { get; set; }
}