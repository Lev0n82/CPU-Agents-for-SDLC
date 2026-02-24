namespace Phase3.AzureDevOps.Services.QA;

/// <summary>
/// Requirement scope analysis
/// </summary>
public class QARequirementScopeAnalysis
{
    public string RequirementTitle { get; set; } = string.Empty;
    public string RequirementId { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? RequirementsPublishedDate { get; set; }
    public DateTime? FirstTestCaseCreatedDate { get; set; }
    public DateTime? FirstTestCaseExecutedDate { get; set; }
    public int PlannedTestCaseCount { get; set; }
    public int ActualTestCaseCount { get; set; }
    public double ScopeChangeRatio { get; set; }
    public double? TestCaseDevelopmentDelayDays { get; set; }
    public double? FirstExecutionDelayDays { get; set; }
}