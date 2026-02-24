namespace Phase3.AzureDevOps.Models.TestPlans;

/// <summary>
/// Request model for updating a test result.
/// </summary>
public class UpdateTestResultRequest
{
    /// <summary>
    /// Test case ID.
    /// </summary>
    public int TestCaseId { get; set; }

    /// <summary>
    /// Test plan ID.
    /// </summary>
    public int TestPlanId { get; set; }

    /// <summary>
    /// Test point ID.
    /// </summary>
    public int TestPointId { get; set; }

    /// <summary>
    /// Test case title.
    /// </summary>
    public string TestCaseTitle { get; set; } = string.Empty;

    /// <summary>
    /// Test outcome.
    /// </summary>
    public TestOutcome Outcome { get; set; }

    /// <summary>
    /// Test comment.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Test duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }
}

/// <summary>
/// Test outcome enumeration.
/// </summary>
public enum TestOutcome
{
    /// <summary>
    /// Test not executed.
    /// </summary>
    NotExecuted = 0,

    /// <summary>
    /// Test passed.
    /// </summary>
    Passed = 1,

    /// <summary>
    /// Test failed.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Test blocked.
    /// </summary>
    Blocked = 3
}
