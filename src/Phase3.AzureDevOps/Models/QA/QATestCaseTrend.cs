namespace Phase3.AzureDevOps.Services.QA;

/// <summary>
/// Test case execution trends
/// </summary>
public class QATestCaseTrend
{
    public DateTime Date { get; set; }
    public int TestCasesCreated { get; set; }
    public int TestCasesExecuted { get; set; }
    public int TestCasesPassed { get; set; }
}