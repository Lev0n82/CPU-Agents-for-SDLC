using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Phase3.AzureDevOps.Exceptions;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.QA;
using System.Text.RegularExpressions;

namespace Phase3.AzureDevOps.Services.QA;

/// <summary>
/// Service for synchronizing Azure DevOps QA data to local cache database
/// </summary>
public class QADataSyncService
{
    private readonly IAuthenticationProvider _authProvider;
    private readonly ILogger<QADataSyncService> _logger;
    private readonly IConfiguration _configuration;
    private readonly QaCacheDbContext _dbContext;
    private readonly WorkItemTrackingHttpClient _witClient;
    private readonly TestManagementHttpClient _testClient;
    private readonly VssConnection _connection;
    
    public QADataSyncService(
        IAuthenticationProvider authProvider,
        ILogger<QADataSyncService> logger,
        IConfiguration configuration,
        QaCacheDbContext dbContext,
        VssConnection connection)
    {
        _authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        
        _witClient = _connection.GetClient<WorkItemTrackingHttpClient>();
        _testClient = _connection.GetClient<TestManagementHttpClient>();
    }
    
    /// <summary>
    /// Synchronize all QA data for the specified project
    /// </summary>
    public async Task SyncProjectDataAsync(string projectName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting QA data synchronization for project: {ProjectName}", projectName);
        
        try
        {
            await EnsureProjectExistsAsync(projectName, cancellationToken);
            await SyncRequirementsAsync(projectName, cancellationToken);
            await SyncTestCasesAsync(projectName, cancellationToken);
            await SyncTestExecutionsAsync(projectName, cancellationToken);
            await UpdateMetricsAsync(projectName, cancellationToken);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("QA data synchronization completed for project: {ProjectName}", projectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synchronize QA data for project: {ProjectName}", projectName);
            throw;
        }
    }
    
    /// <summary>
    /// Ensure project exists in cache database
    /// </summary>
    private async Task EnsureProjectExistsAsync(string projectName, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Name == projectName, cancellationToken);
            
        if (project == null)
        {
            project = new Project
            {
                Name = projectName,
                AzureDevOpsProjectId = projectName,
                Description = $"Azure DevOps project: {projectName}",
                CreatedDate = DateTime.UtcNow,
                LastSyncDate = DateTime.UtcNow
            };
            
            await _dbContext.Projects.AddAsync(project, cancellationToken);
        }
        else
        {
            project.LastSyncDate = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Synchronize requirements (User Stories, Features) from Azure DevOps
    /// </summary>
    private async Task SyncRequirementsAsync(string projectName, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .FirstAsync(p => p.Name == projectName, cancellationToken);
            
        // Query for requirements (User Stories and Features with requirements)
        var wiql = @"
            SELECT [System.Id], [System.WorkItemType], [System.Title], [System.Description], 
                   [System.CreatedDate], [System.State], [Microsoft.VSTS.Common.Priority],
                   [System.IterationPath], [System.AreaPath], [Microsoft.VSTS.Scheduling.TargetDate]
            FROM WorkItems 
            WHERE [System.TeamProject] = @project 
            AND ([System.WorkItemType] = 'User Story' OR [System.WorkItemType] = 'Feature')
            ORDER BY [System.CreatedDate]";
            
        var query = new Wiql { Query = wiql };
        var queryResult = await _witClient.QueryByWiqlAsync(query, cancellationToken: cancellationToken);
        
        if (queryResult.WorkItems == null || !queryResult.WorkItems.Any())
        {
            _logger.LogWarning("No requirements found for project: {ProjectName}", projectName);
            return;
        }
        
        var workItemIds = queryResult.WorkItems.Select(wi => wi.Id).ToList();
        var workItems = await _witClient.GetWorkItemsAsync(workItemIds, expand: WorkItemExpand.All, cancellationToken: cancellationToken);
        
        foreach (var workItem in workItems)
        {
            await SyncRequirementAsync(project, workItem, cancellationToken);
        }
    }
    
    /// <summary>
    /// Synchronize a single requirement
    /// </summary>
    private async Task SyncRequirementAsync(Project project, WorkItem workItem, CancellationToken cancellationToken)
    {
        var requirement = await _dbContext.Requirements
            .FirstOrDefaultAsync(r => r.AzureDevOpsWorkItemId == workItem.Id.ToString(), cancellationToken);
            
        var createdDate = GetFieldValue<DateTime>(workItem, "System.CreatedDate");
        var iterationPath = GetFieldValue<string>(workItem, "System.IterationPath");
        
        // Extract release from iteration path (e.g., "Project\\Release 1.0\\Sprint 1")
        var releaseName = ExtractReleaseFromIterationPath(iterationPath) ?? "Unassigned";
        var release = await GetOrCreateReleaseAsync(project, releaseName, cancellationToken);
        
        if (requirement == null)
        {
            requirement = new Requirement
            {
                AzureDevOpsWorkItemId = workItem.Id.ToString(),
                ProjectId = project.Id,
                ReleaseId = release?.Id,
                Title = GetFieldValue<string>(workItem, "System.Title") ?? "Untitled",
                WorkItemType = GetFieldValue<string>(workItem, "System.WorkItemType") ?? "User Story",
                Description = GetFieldValue<string>(workItem, "System.Description") ?? string.Empty,
                CreatedDate = createdDate == default ? DateTime.UtcNow : createdDate,
                RequirementsPublishedDate = createdDate
            };
            
            await _dbContext.Requirements.AddAsync(requirement, cancellationToken);
        }
        else
        {
            requirement.Title = GetFieldValue<string>(workItem, "System.Title") ?? requirement.Title;
            requirement.Description = GetFieldValue<string>(workItem, "System.Description") ?? requirement.Description;
            requirement.ReleaseId = release?.Id;
        }
    }
    
    /// <summary>
    /// Synchronize test cases and link them to requirements
    /// </summary>
    private async Task SyncTestCasesAsync(string projectName, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .FirstAsync(p => p.Name == projectName, cancellationToken);
            
        // Query for test cases
        var wiql = @"
            SELECT [System.Id], [System.Title], [System.Description], [System.CreatedDate], 
                   [System.State], [Microsoft.VSTS.TCM.AutomationStatus]
            FROM WorkItems 
            WHERE [System.TeamProject] = @project 
            AND [System.WorkItemType] = 'Test Case'
            ORDER BY [System.CreatedDate]";
            
        var query = new Wiql { Query = wiql };
        var queryResult = await _witClient.QueryByWiqlAsync(query, cancellationToken: cancellationToken);
        
        if (queryResult.WorkItems == null || !queryResult.WorkItems.Any())
        {
            _logger.LogWarning("No test cases found for project: {ProjectName}", projectName);
            return;
        }
        
        var workItemIds = queryResult.WorkItems.Select(wi => wi.Id).ToList();
        var workItems = await _witClient.GetWorkItemsAsync(workItemIds, expand: WorkItemExpand.All, cancellationToken: cancellationToken);
        
        foreach (var workItem in workItems)
        {
            await SyncTestCaseAsync(project, workItem, cancellationToken);
        }
    }
    
    /// <summary>
    /// Synchronize a single test case and link to requirement
    /// </summary>
    private async Task SyncTestCaseAsync(Project project, WorkItem workItem, CancellationToken cancellationToken)
    {
        var testCase = await _dbContext.TestCases
            .FirstOrDefaultAsync(tc => tc.AzureDevOpsWorkItemId == workItem.Id.ToString(), cancellationToken);
            
        if (testCase == null)
        {
            testCase = new TestCase
            {
                AzureDevOpsWorkItemId = workItem.Id.ToString(),
                Title = GetFieldValue<string>(workItem, "System.Title") ?? "Untitled Test Case",
                Description = GetFieldValue<string>(workItem, "System.Description") ?? string.Empty,
                CreatedDate = GetFieldValue<DateTime?>(workItem, "System.CreatedDate") ?? DateTime.UtcNow,
                CurrentState = GetFieldValue<string>(workItem, "System.State") ?? "New"
            };
            
            await _dbContext.TestCases.AddAsync(testCase, cancellationToken);
        }
        else
        {
            testCase.Title = GetFieldValue<string>(workItem, "System.Title") ?? testCase.Title;
            testCase.Description = GetFieldValue<string>(workItem, "System.Description") ?? testCase.Description;
            testCase.CurrentState = GetFieldValue<string>(workItem, "System.State") ?? testCase.CurrentState;
        }
        
        // Link test case to requirement
        await LinkTestCaseToRequirementAsync(testCase, workItem, cancellationToken);
    }
    
    /// <summary>
    /// Link test case to its requirement
    /// </summary>
    private async Task LinkTestCaseToRequirementAsync(TestCase testCase, WorkItem workItem, CancellationToken cancellationToken)
    {
        if (workItem.Relations == null)
            return;
            
        // Find "Tests" relation (test case is linked to requirement)
        var testRelation = workItem.Relations
            .FirstOrDefault(r => r.Rel == "Microsoft.VSTS.Common.TestedBy-Reverse");
            
        if (testRelation != null)
        {
            // Extract requirement ID from relation URL
            var match = Regex.Match(testRelation.Url, @"/workItems/(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var requirementId))
            {
                var requirement = await _dbContext.Requirements
                    .FirstOrDefaultAsync(r => r.AzureDevOpsWorkItemId == requirementId.ToString(), cancellationToken);
                    
                if (requirement != null && testCase.RequirementId != requirement.Id)
                {
                    testCase.RequirementId = requirement.Id;
                    
                    // Update requirement metrics
                    if (requirement.FirstTestCaseCreatedDate == null)
                    {
                        requirement.FirstTestCaseCreatedDate = testCase.CreatedDate;
                    }
                    
                    // Update planned test case count
                    var plannedCount = await _dbContext.TestCases
                        .CountAsync(tc => tc.RequirementId == requirement.Id, cancellationToken);
                    requirement.PlannedTestCaseCount = plannedCount;
                }
            }
        }
    }
    
    /// <summary>
    /// Synchronize test execution data
    /// </summary>
    private async Task SyncTestExecutionsAsync(string projectName, CancellationToken cancellationToken)
    {
        // Get test runs for the project
        var testRuns = await _testClient.GetTestRunsAsync(projectName, cancellationToken: cancellationToken);
        
        foreach (var testRun in testRuns)
        {
            var results = await _testClient.GetTestResultsAsync(projectName, testRun.Id, cancellationToken: cancellationToken);
            
            foreach (var result in results)
            {
                await SyncTestExecutionAsync(projectName, result, cancellationToken);
            }
        }
    }
    
    /// <summary>
    /// Synchronize a single test execution
    /// </summary>
    private async Task SyncTestExecutionAsync(string projectName, TestCaseResult result, CancellationToken cancellationToken)
    {
        if (result.TestCase?.Id == null)
            return;
            
        var testCase = await _dbContext.TestCases
            .FirstOrDefaultAsync(tc => tc.AzureDevOpsWorkItemId == result.TestCase.Id.ToString(), cancellationToken);
            
        if (testCase == null)
            return;
            
        var execution = new TestExecution
        {
            TestCaseId = testCase.Id,
            AzureDevOpsTestRunId = result.TestRun?.Id.ToString() ?? string.Empty,
            ExecutedDate = result.StartedDate != default ? result.StartedDate : DateTime.UtcNow,
            DurationMs = result.DurationInMs == default ? 0L : (long)result.DurationInMs,
            Outcome = result.Outcome ?? "Not Executed",
            Comment = result.Comment ?? string.Empty
        };
        
        await _dbContext.TestExecutions.AddAsync(execution, cancellationToken);
        
        // Update test case statistics
        testCase.ExecutionCount++;
        testCase.LastExecutedDate = execution.ExecutedDate;
        testCase.LastOutcome = execution.Outcome;
        
        if (execution.Outcome?.Equals("Passed", StringComparison.OrdinalIgnoreCase) == true)
        {
            testCase.PassedCount++;
        }
        else if (execution.Outcome?.Equals("Failed", StringComparison.OrdinalIgnoreCase) == true)
        {
            testCase.FailedCount++;
        }
        
        // Update requirement metrics
        var requirement = await _dbContext.Requirements
            .FirstOrDefaultAsync(r => r.Id == testCase.RequirementId, cancellationToken);
            
        if (requirement != null)
        {
            requirement.ExecutedTestCaseCount++;
            
            if (execution.Outcome?.Equals("Passed", StringComparison.OrdinalIgnoreCase) == true)
            {
                requirement.PassedTestCaseCount++;
            }
            else if (execution.Outcome?.Equals("Failed", StringComparison.OrdinalIgnoreCase) == true)
            {
                requirement.FailedTestCaseCount++;
            }
            
            requirement.ActualTestCaseCount = await _dbContext.TestCases
                .CountAsync(tc => tc.RequirementId == requirement.Id, cancellationToken);
                
            if (requirement.FirstTestCaseExecutedDate == null)
            {
                requirement.FirstTestCaseExecutedDate = execution.ExecutedDate;
            }
            requirement.LastTestCaseExecutedDate = execution.ExecutedDate;
        }
    }
    
    /// <summary>
    /// Update aggregated metrics
    /// </summary>
    private async Task UpdateMetricsAsync(string projectName, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Requirements)
            .FirstAsync(p => p.Name == projectName, cancellationToken);
            
        // Update requirement-level metrics
        foreach (var requirement in project.Requirements)
        {
            var testCases = await _dbContext.TestCases
                .Where(tc => tc.RequirementId == requirement.Id)
                .ToListAsync(cancellationToken);
                
            requirement.PlannedTestCaseCount = testCases.Count;
            requirement.ActualTestCaseCount = testCases.Count;
            requirement.ExecutedTestCaseCount = testCases.Sum(tc => tc.ExecutionCount);
            requirement.PassedTestCaseCount = testCases.Sum(tc => tc.PassedCount);
            requirement.FailedTestCaseCount = testCases.Sum(tc => tc.FailedCount);
        }
    }
    
    /// <summary>
    /// Get or create release entity
    /// </summary>
    private async Task<Release?> GetOrCreateReleaseAsync(Project project, string releaseName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(releaseName) || releaseName == "Unassigned")
            return null;
            
        var release = await _dbContext.Releases
            .FirstOrDefaultAsync(r => r.ProjectId == project.Id && r.Name == releaseName, cancellationToken);
            
        if (release == null)
        {
            release = new Release
            {
                ProjectId = project.Id,
                Name = releaseName,
                AzureDevOpsReleaseId = releaseName,
                PlannedStartDate = DateTime.UtcNow
            };
            
            await _dbContext.Releases.AddAsync(release, cancellationToken);
        }
        
        return release;
    }
    
    /// <summary>
    /// Extract release name from iteration path
    /// </summary>
    private string? ExtractReleaseFromIterationPath(string? iterationPath)
    {
        if (string.IsNullOrEmpty(iterationPath))
            return null;
            
        var parts = iterationPath.Split('\\');
        return parts.Length >= 2 ? parts[1] : parts[0];
    }
    
    /// <summary>
    /// Safe helper to get field values from work items
    /// </summary>
    private T? GetFieldValue<T>(WorkItem workItem, string fieldName)
    {
        if (workItem.Fields.ContainsKey(fieldName))
        {
            try
            {
                return (T)workItem.Fields[fieldName];
            }
            catch (InvalidCastException)
            {
                _logger.LogWarning("Field {FieldName} has unexpected type for work item {WorkItemId}", fieldName, workItem.Id);
            }
        }
        return default(T);
    }
}