namespace Phase3.AzureDevOps.Interfaces;

using Microsoft.TeamFoundation.TestManagement.WebApi;
using Phase3.AzureDevOps.Models.TestPlans;

/// <summary>
/// Provides test plan management operations.
/// </summary>
public interface ITestPlanService
{
    /// <summary>
    /// Retrieves a test plan by ID.
    /// </summary>
    /// <param name="testPlanId">The test plan ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The test plan.</returns>
    /// <exception cref="NotFoundException">Thrown if test plan does not exist.</exception>
    Task<TestPlan> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new test case linked to a requirement.
    /// </summary>
    /// <param name="request">Test case creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created test case ID.</returns>
    /// <exception cref="ValidationException">Thrown if request is invalid.</exception>
    Task<int> CreateTestCaseAsync(CreateTestCaseRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a test result for a test case execution.
    /// </summary>
    /// <param name="request">Test result update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated test result ID.</returns>
    /// <exception cref="NotFoundException">Thrown if test case does not exist.</exception>
    Task<int> UpdateTestResultAsync(UpdateTestResultRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes obsolete test cases for removed or closed requirements.
    /// </summary>
    /// <param name="requirementId">The requirement ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of test cases closed.</returns>
    Task<int> CloseObsoleteTestCasesAsync(int requirementId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads an attachment to a test result.
    /// </summary>
    /// <param name="testResultId">The test result ID.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="content">The file content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The attachment ID.</returns>
    Task<Guid> UploadTestAttachmentAsync(int testResultId, string fileName, byte[] content, CancellationToken cancellationToken = default);
}
