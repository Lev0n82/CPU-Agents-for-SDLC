namespace Phase3.IntegrationTests.Scenarios;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.TestPlans;
using Phase3.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Scenario 2: Test Case Automation
/// Tests the complete workflow of creating test cases, executing tests, and managing obsolete test cases.
/// </summary>
[Collection("Integration Tests")]
public class Scenario2_TestCaseAutomationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public Scenario2_TestCaseAutomationTests(
        IntegrationTestFixture fixture,
        ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Scenario2_CompleteTestCaseLifecycle()
    {
        // Arrange
        var testPlanService = _fixture.ServiceProvider.GetRequiredService<ITestPlanService>();
        var workItemService = _fixture.ServiceProvider.GetRequiredService<IWorkItemService>();
        var lifecycleManager = _fixture.ServiceProvider.GetRequiredService<ITestCaseLifecycleManager>();

        _output.WriteLine("=== Scenario 2: Test Case Automation ===");
        _output.WriteLine("");

        // Step 1: Create a requirement work item
        _output.WriteLine("Step 1: Creating requirement work item...");
        var requirement = await workItemService.CreateWorkItemAsync(
            "User Story",
            new Dictionary<string, object>
            {
                ["System.Title"] = "[IntegrationTest] Test Requirement",
                ["System.Description"] = "Test requirement for integration testing",
                ["System.State"] = "New"
            });

        requirement.Should().NotBeNull();
        var requirementId = requirement.Id!.Value;
        _output.WriteLine($"  ✓ Created requirement: {requirementId}");

        int testCaseId = 0; // Declare outside try block for cleanup access
        try
        {
            // Step 2: Create test case linked to requirement
            _output.WriteLine("Step 2: Creating test case linked to requirement...");
            var createRequest = new CreateTestCaseRequest
            {
                Title = "[IntegrationTest] Verify Login Functionality",
                RequirementId = requirementId,
                Steps = new List<TestStep>
                {
                    new TestStep
                    {
                        Action = "Navigate to login page",
                        ExpectedResult = "Login page is displayed"
                    },
                    new TestStep
                    {
                        Action = "Enter valid credentials",
                        ExpectedResult = "User is authenticated"
                    },
                    new TestStep
                    {
                        Action = "Click Login button",
                        ExpectedResult = "User is redirected to dashboard"
                    }
                }
            };

            testCaseId = await testPlanService.CreateTestCaseAsync(createRequest);
            testCaseId.Should().BeGreaterThan(0);
            _output.WriteLine($"  ✓ Created test case: {testCaseId}");

            // Step 3: Execute test and record result
            _output.WriteLine("Step 3: Executing test and recording result...");
            var updateRequest = new UpdateTestResultRequest
            {
                TestCaseId = testCaseId,
                Outcome = TestOutcome.Passed,
                Comment = "Test executed successfully via integration test"
            };

            await testPlanService.UpdateTestResultAsync(updateRequest);
            _output.WriteLine($"  ✓ Test result recorded: Passed");

            // Step 4: Simulate requirement removal by updating state to Removed
            _output.WriteLine("Step 4: Simulating requirement removal...");
            var requirementToUpdate = await workItemService.GetWorkItemAsync(requirementId);
            await workItemService.UpdateWorkItemAsync(requirementId, requirementToUpdate.Rev.Value, new Dictionary<string, object>
            {
                ["System.State"] = "Removed"
            });
            _output.WriteLine($"  ✓ Requirement {requirementId} marked as removed");

            // Step 5: Detect obsolete test case
            _output.WriteLine("Step 5: Detecting obsolete test cases...");
            var obsoleteTestCases = await lifecycleManager.IdentifyObsoleteTestCasesAsync();
            
            var obsoleteList = obsoleteTestCases.ToList();
            obsoleteList.Should().Contain(testCaseId, "test case should be detected as obsolete");
            _output.WriteLine($"  ✓ Detected {obsoleteList.Count} obsolete test case(s)");

            // Step 6: Close obsolete test case
            _output.WriteLine("Step 6: Closing obsolete test case...");
            var closedCount = await lifecycleManager.CloseObsoleteTestCasesAsync(
                new List<int> { testCaseId },
                "Linked requirement removed - Integration Test");

            closedCount.Should().Be(1);
            _output.WriteLine($"  ✓ Closed {closedCount} obsolete test case(s)");

            // Verify test case is closed
            var testCase = await workItemService.GetWorkItemAsync(testCaseId);
            var state = testCase.Fields.ContainsKey("System.State") ? testCase.Fields["System.State"]?.ToString() ?? "" : "";
            state.Should().Be("Closed", "test case should be closed");
            _output.WriteLine($"  ✓ Verified test case {testCaseId} is closed");
        }
        finally
        {
            // Cleanup: Delete test work items
            _output.WriteLine("");
            _output.WriteLine("Cleanup: Removing test work items...");
            try
            {
                // Note: Azure DevOps API doesn't support deleting work items via REST API
                // Work items remain in the project but can be marked as removed
                _output.WriteLine($"  Note: Work items {requirementId} and {testCaseId} remain in project");
            }
            catch
            {
                // Already deleted
            }
        }

        _output.WriteLine("");
        _output.WriteLine("=== Scenario 2 Complete ===");
        _output.WriteLine("✓ All steps passed successfully");
    }
}
