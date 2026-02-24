namespace Phase3.AzureDevOps.Models.TestPlans;

/// <summary>
/// Request model for creating a test case.
/// </summary>
public class CreateTestCaseRequest
{
    /// <summary>
    /// Test case title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Test case description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Requirement ID to link to.
    /// </summary>
    public int RequirementId { get; set; }

    /// <summary>
    /// Test steps.
    /// </summary>
    public IEnumerable<TestStep> Steps { get; set; } = Array.Empty<TestStep>();
}

/// <summary>
/// Represents a test step.
/// </summary>
public class TestStep
{
    /// <summary>
    /// Step action.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Expected result.
    /// </summary>
    public string ExpectedResult { get; set; } = string.Empty;
}
