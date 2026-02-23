namespace Phase3.AzureDevOps.Services.TestPlans;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Phase3.AzureDevOps.Exceptions;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.TestPlans;
using System.Net;
using System.Security;
using System.Text;

/// <summary>
/// Implements test plan management operations.
/// </summary>
public class TestPlanService : ITestPlanService
{
    private readonly IAuthenticationProvider _authProvider;
    private readonly ILogger<TestPlanService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TestManagementHttpClient _testClient;
    private readonly WorkItemTrackingHttpClient _witClient;
    private readonly string _projectName;
    private readonly string _organizationUrl;

    public TestPlanService(
        IAuthenticationProvider authProvider,
        ILogger<TestPlanService> logger,
        IConfiguration configuration,
        VssConnection connection)
    {
        _authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        _testClient = connection.GetClient<TestManagementHttpClient>();
        _witClient = connection.GetClient<WorkItemTrackingHttpClient>();
        
        _projectName = _configuration["AzureDevOps:ProjectName"] 
            ?? throw new InvalidOperationException("AzureDevOps:ProjectName configuration is required");
        _organizationUrl = _configuration["AzureDevOps:OrganizationUrl"] 
            ?? throw new InvalidOperationException("AzureDevOps:OrganizationUrl configuration is required");
    }

    public async Task<TestPlan> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving test plan {TestPlanId}", testPlanId);

        try
        {
            var testPlan = await _testClient.GetPlanByIdAsync(_projectName, testPlanId);

            if (testPlan == null)
            {
                throw new NotFoundException($"Test plan {testPlanId} not found");
            }

            _logger.LogInformation("Retrieved test plan {TestPlanId}: {TestPlanName}", testPlanId, testPlan.Name);
            return testPlan;
        }
        catch (VssServiceException ex)
        {
            _logger.LogError(ex, "Failed to retrieve test plan {TestPlanId}", testPlanId);
            throw new NotFoundException($"Test plan {testPlanId} not found", ex);
        }
    }

    public async Task<int> CreateTestCaseAsync(CreateTestCaseRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateTestCaseRequest(request);

        _logger.LogInformation("Creating test case for requirement {RequirementId}", request.RequirementId);

        // Create work item for test case
        var patchDocument = new JsonPatchDocument
        {
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.Title",
                Value = request.Title
            },
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.Description",
                Value = request.Description
            },
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.TCM.Steps",
                Value = FormatTestSteps(request.Steps)
            },
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/relations/-",
                Value = new
                {
                    rel = "Microsoft.VSTS.Common.TestedBy-Reverse",
                    url = $"{_organizationUrl}/_apis/wit/workItems/{request.RequirementId}"
                }
            }
        };

        var testCase = await _witClient.CreateWorkItemAsync(
            document: patchDocument,
            project: _projectName,
            type: "Test Case",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Created test case {TestCaseId} for requirement {RequirementId}", 
            testCase.Id, request.RequirementId);

        return testCase.Id.Value;
    }

    public async Task<int> UpdateTestResultAsync(UpdateTestResultRequest request, CancellationToken cancellationToken = default)
    {
        ValidateUpdateTestResultRequest(request);

        _logger.LogInformation("Updating test result for test case {TestCaseId}", request.TestCaseId);

        var runModel = new RunCreateModel
        {
            Name = $"Automated Run - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
        };
        
        var testRun = await _testClient.CreateTestRunAsync(
            testRun: runModel,
            project: _projectName);

        var testResult = new TestCaseResult
        {
            TestCaseTitle = request.TestCaseTitle,
            Outcome = request.Outcome.ToString(),
            State = "Completed",
            Comment = request.Comment,
            DurationInMs = request.DurationMs
        };

        var results = await _testClient.UpdateTestResultsAsync(
            results: new[] { testResult },
            project: _projectName,
            runId: testRun.Id,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Updated test result {TestResultId} with outcome {Outcome}", 
            results[0].Id, request.Outcome);

        return results[0].Id;
    }

    public async Task<int> CloseObsoleteTestCasesAsync(int requirementId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Closing obsolete test cases for requirement {RequirementId}", requirementId);

        // Query for test cases linked to this requirement
        var wiql = $@"
            SELECT [System.Id]
            FROM WorkItemLinks
            WHERE Source.[System.Id] = {requirementId}
            AND [System.Links.LinkType] = 'Microsoft.VSTS.Common.TestedBy-Forward'
            AND Target.[System.WorkItemType] = 'Test Case'
            AND Target.[System.State] <> 'Closed'
            MODE (MustContain)";

        var queryResult = await _witClient.QueryByWiqlAsync(
            wiql: new Wiql { Query = wiql },
            project: _projectName,
            cancellationToken: cancellationToken);

        int closedCount = 0;

        if (queryResult.WorkItemRelations != null)
        {
            foreach (var link in queryResult.WorkItemRelations.Where(r => r.Target != null))
            {
                var testCaseId = link.Target.Id;

                var patchDocument = new JsonPatchDocument
                {
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.State",
                        Value = "Closed"
                    },
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.Reason",
                        Value = "Obsolete"
                    },
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.History",
                        Value = $"Automatically closed because requirement {requirementId} was removed or closed."
                    }
                };

                await _witClient.UpdateWorkItemAsync(
                    document: patchDocument,
                    id: testCaseId,
                    cancellationToken: cancellationToken);

                closedCount++;
                _logger.LogInformation("Closed test case {TestCaseId}", testCaseId);
            }
        }

        _logger.LogInformation("Closed {Count} obsolete test cases for requirement {RequirementId}", 
            closedCount, requirementId);

        return closedCount;
    }

    public async Task<Guid> UploadTestAttachmentAsync(int testResultId, string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading attachment {FileName} to test result {TestResultId}", 
            fileName, testResultId);

        // Note: This is a simplified implementation. In production, you would need to:
        // 1. Get the test run ID for the test result
        // 2. Use the correct API to upload attachments
        
        // For now, we'll throw NotImplementedException as this requires additional context
        throw new NotImplementedException("Test attachment upload requires test run context. Implement GetTestRunIdForResult method.");
    }

    private void ValidateCreateTestCaseRequest(CreateTestCaseRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Title)) 
            throw new ValidationException("Test case title is required");
        if (request.RequirementId <= 0) 
            throw new ValidationException("Valid requirement ID is required");
        if (request.Steps == null || !request.Steps.Any()) 
            throw new ValidationException("At least one test step is required");
    }

    private void ValidateUpdateTestResultRequest(UpdateTestResultRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.TestCaseId <= 0) 
            throw new ValidationException("Valid test case ID is required");
        if (request.TestPlanId <= 0) 
            throw new ValidationException("Valid test plan ID is required");
    }

    private string FormatTestSteps(IEnumerable<TestStep> steps)
    {
        // Format test steps as HTML for Azure DevOps
        var sb = new StringBuilder();
        sb.Append("<steps id=\"0\" last=\"").Append(steps.Count()).Append("\">");
        
        int stepNumber = 1;
        foreach (var step in steps)
        {
            sb.Append("<step id=\"").Append(stepNumber).Append("\" type=\"ActionStep\">");
            sb.Append("<parameterizedString isformatted=\"true\">").Append(SecurityElement.Escape(step.Action)).Append("</parameterizedString>");
            sb.Append("<parameterizedString isformatted=\"true\">").Append(SecurityElement.Escape(step.ExpectedResult)).Append("</parameterizedString>");
            sb.Append("<description/></step>");
            stepNumber++;
        }
        
        sb.Append("</steps>");
        return sb.ToString();
    }
}
