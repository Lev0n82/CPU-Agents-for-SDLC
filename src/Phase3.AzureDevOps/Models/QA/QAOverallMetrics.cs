namespace Phase3.AzureDevOps.Services.QA;

/// <summary>
/// Overall QA metrics across all projects
/// </summary>
public class QAOverallMetrics
{
    public int TotalProjects { get; set; }
    public int TotalRequirements { get; set; }
    public int TotalTestCases { get; set; }
    public int TotalTestExecutions { get; set; }
    public double AverageTestCaseDevelopmentDelayDays { get; set; }
    public double AverageFirstExecutionDelayDays { get; set; }
    public double AverageDefectPerRequirement { get; set; }
    public double TestCaseProductivity { get; set; }
    public int MetricsWithTimelines { get; set; }
    public int MetricsWithExecutions { get; set; }
    public int MetricsWithDefects { get; set; }
    
    // Industry-standard QA metrics
    public double TotalEscapedBugs { get; set; }
    public double TestCoverageRate { get; set; }
    public double DefectDensity { get; set; }
    public double TestCaseEffectiveness { get; set; }
    public double DefectLeakageRate { get; set; }
}