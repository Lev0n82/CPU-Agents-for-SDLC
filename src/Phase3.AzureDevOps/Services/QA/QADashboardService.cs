using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Models.QA;

namespace Phase3.AzureDevOps.Services.QA;

public class QADashboardService
{
    private readonly ILogger<QADashboardService> _logger;
    private readonly QaCacheDbContext _dbContext;
    
    public QADashboardService(
        ILogger<QADashboardService> logger,
        QaCacheDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    /// <summary>
    /// Get QA metrics for all projects
    /// </summary>
    public async Task<QAOverallMetrics> GetOverallMetricsAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _dbContext.Projects
            .Include(p => p.Releases)
            .Include(p => p.Requirements)
            .ThenInclude(r => r.TestCases)
            .ThenInclude(tc => tc.Executions)
            .ToListAsync(cancellationToken);
            
        var metrics = new QAOverallMetrics
        {
            TotalProjects = projects.Count,
            TotalRequirements = projects.Sum(p => p.Requirements.Count),
            TotalTestCases = projects.Sum(p => p.Requirements.Sum(r => r.TestCases.Count)),
            TotalTestExecutions = await _dbContext.TestExecutions.CountAsync(cancellationToken),
            
            // Industry-standard QA metrics
            TotalEscapedBugs = await CalculateEscapedBugsAsync(cancellationToken),
            TestCoverageRate = await CalculateTestCoverageRateAsync(cancellationToken),
            DefectDensity = await CalculateDefectDensityAsync(cancellationToken),
            TestCaseEffectiveness = await CalculateTestCaseEffectivenessAsync(cancellationToken),
            DefectLeakageRate = await CalculateDefectLeakageRateAsync(cancellationToken)
        };
        
        // Calculate timeline metrics
        foreach (var project in projects)
        {
            foreach (var requirement in project.Requirements)
            {
                // Timeline metrics for requirement-to-test-case development
                if (requirement.RequirementsPublishedDate != null && requirement.FirstTestCaseCreatedDate != null)
                {
                    var timelineDelay = (requirement.FirstTestCaseCreatedDate.Value - requirement.RequirementsPublishedDate.Value).TotalDays;
                    metrics.AverageTestCaseDevelopmentDelayDays += timelineDelay;
                    metrics.MetricsWithTimelines++;
                }
                
                // Timeline metrics for test-case-to-execution
                if (requirement.FirstTestCaseCreatedDate != null && requirement.FirstTestCaseExecutedDate != null)
                {
                    var executionDelay = (requirement.FirstTestCaseExecutedDate.Value - requirement.FirstTestCaseCreatedDate.Value).TotalDays;
                    metrics.AverageFirstExecutionDelayDays += executionDelay;
                    metrics.MetricsWithExecutions++;
                }
                
                // Calculate requirement defect density
                if (requirement.TestCases.Any())
                {
                    var failedTests = requirement.FailedTestCaseCount;
                    var totalTests = requirement.TestCases.Count;
                    metrics.AverageDefectPerRequirement += (double)failedTests / totalTests;
                    metrics.MetricsWithDefects++;
                }
            }
        }
        
        if (metrics.MetricsWithTimelines > 0)
            metrics.AverageTestCaseDevelopmentDelayDays /= metrics.MetricsWithTimelines;
            
        if (metrics.MetricsWithExecutions > 0)
            metrics.AverageFirstExecutionDelayDays /= metrics.MetricsWithExecutions;
            
        if (metrics.MetricsWithDefects > 0)
            metrics.AverageDefectPerRequirement /= metrics.MetricsWithDefects;
            
        // Calculate Test Case Productivity (tests per day)
        var totalTestCases = projects.Sum(p => p.Requirements.Sum(r => r.TestCases.Count));
        var totalDays = projects.Any(p => p.Requirements.Any())
            ? (DateTime.UtcNow - projects.SelectMany(p => p.Requirements).Min(r => r.CreatedDate)).TotalDays
            : 1;
            
        metrics.TestCaseProductivity = totalTestCases / Math.Max(totalDays, 1);
        
        return metrics;
    }
    
    /// <summary>
    /// Get project-specific QA metrics
    /// </summary>
    public async Task<QAProjectMetrics> GetProjectMetricsAsync(string projectName, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Releases)
            .Include(p => p.Requirements)
            .ThenInclude(r => r.TestCases)
            .FirstOrDefaultAsync(p => p.Name == projectName, cancellationToken);
            
        if (project == null)
            throw new ArgumentException($"Project '{projectName}' not found");
            
        var metrics = new QAProjectMetrics
        {
            ProjectName = projectName,
            TotalReleases = project.Releases.Count,
            TotalRequirements = project.Requirements.Count,
            TotalTestCases = project.Requirements.Sum(r => r.TestCases.Count),
            TotalExecutions = project.Requirements.Sum(r => r.ExecutedTestCaseCount),
            PassRate = project.Requirements.Sum(r => r.PassedTestCaseCount) / Math.Max(project.Requirements.Sum(r => r.ExecutedTestCaseCount), 1) * 100
        };
        
        return metrics;
    }
    
    /// <summary>
    /// Get release timeline data for a project
    /// </summary>
    public async Task<IEnumerable<QAReleaseTimeline>> GetReleaseTimelinesAsync(string projectName, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Releases)
            .ThenInclude(r => r.Requirements)
            .ThenInclude(r => r.TestCases)
            .FirstOrDefaultAsync(p => p.Name == projectName, cancellationToken);
            
        if (project == null)
            throw new ArgumentException($"Project '{projectName}' not found");
            
        var timelines = new List<QAReleaseTimeline>();
        
        foreach (var release in project.Releases.OrderBy(r => r.PlannedStartDate))
        {
            var timeline = new QAReleaseTimeline
            {
                ReleaseName = release.Name,
                RequirementsPublishedDate = release.RequirementsPublishedDate,
                PlannedStartDate = release.PlannedStartDate,
                PlannedEndDate = release.PlannedEndDate,
                ActualEndDate = release.ActualEndDate
            };
            
            // Calculate timeline metrics for this release
            var releaseRequirements = release.Requirements.ToList();
            timeline.TotalRequirements = releaseRequirements.Count;
            timeline.TotalTestCases = releaseRequirements.Sum(r => r.TestCases.Count);
            
            // Calculate averages
            var requirementsWithData = releaseRequirements
                .Where(r => r.RequirementsPublishedDate != null && r.FirstTestCaseCreatedDate != null)
                .ToList();
                
            if (requirementsWithData.Any())
            {
                timeline.AverageTestCaseDevelopmentDelayDays = requirementsWithData
                    .Average(r => (r.FirstTestCaseCreatedDate!.Value - r.RequirementsPublishedDate!.Value).TotalDays);
            }
            
            var requirementsWithExecutions = releaseRequirements
                .Where(r => r.FirstTestCaseCreatedDate != null && r.FirstTestCaseExecutedDate != null)
                .ToList();
                
            if (requirementsWithExecutions.Any())
            {
                timeline.AverageFirstExecutionDelayDays = requirementsWithExecutions
                    .Average(r => (r.FirstTestCaseExecutedDate!.Value - r.FirstTestCaseCreatedDate!.Value).TotalDays);
            }
            
            timelines.Add(timeline);
        }
        
        return timelines;
    }
    
    /// <summary>
    /// Get requirement scope change analysis
    /// </summary>
    public async Task<IEnumerable<QARequirementScopeAnalysis>> GetRequirementScopeAnalysisAsync(string projectName, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Requirements)
            .ThenInclude(r => r.TestCases)
            .FirstOrDefaultAsync(p => p.Name == projectName, cancellationToken);
            
        if (project == null)
            throw new ArgumentException($"Project '{projectName}' not found");
            
        var analysis = new List<QARequirementScopeAnalysis>();
        
        foreach (var requirement in project.Requirements.OrderBy(r => r.CreatedDate))
        {
            var analysisItem = new QARequirementScopeAnalysis
            {
                RequirementTitle = requirement.Title,
                RequirementId = requirement.AzureDevOpsWorkItemId,
                CreatedDate = requirement.CreatedDate,
                RequirementsPublishedDate = requirement.RequirementsPublishedDate,
                FirstTestCaseCreatedDate = requirement.FirstTestCaseCreatedDate,
                FirstTestCaseExecutedDate = requirement.FirstTestCaseExecutedDate,
                PlannedTestCaseCount = requirement.PlannedTestCaseCount,
                ActualTestCaseCount = requirement.ActualTestCaseCount,
                ScopeChangeRatio = requirement.PlannedTestCaseCount > 0 ? 
                    (requirement.ActualTestCaseCount - requirement.PlannedTestCaseCount) / (double)requirement.PlannedTestCaseCount : 0,
                TestCaseDevelopmentDelayDays = requirement.RequirementsPublishedDate != null && requirement.FirstTestCaseCreatedDate != null ?
                    (requirement.FirstTestCaseCreatedDate.Value - requirement.RequirementsPublishedDate.Value).TotalDays : null,
                FirstExecutionDelayDays = requirement.FirstTestCaseCreatedDate != null && requirement.FirstTestCaseExecutedDate != null ?
                    (requirement.FirstTestCaseExecutedDate.Value - requirement.FirstTestCaseCreatedDate.Value).TotalDays : null
            };
            
            analysis.Add(analysisItem);
        }
        
        return analysis;
    }
    
    /// <summary>
    /// Get test case execution trends
    /// </summary>
    public async Task<IEnumerable<QATestCaseTrend>> GetTestCaseTrendsAsync(string projectName, CancellationToken cancellationToken = default)
    {
        var projections = await _dbContext.TestCases
            .Include(tc => tc.Requirement)
            .ThenInclude(r => r.Project)
            .Where(tc => tc.Requirement.Project.Name == projectName)
            .GroupBy(tc => tc.CreatedDate.Date)
            .Select(g => new QATestCaseTrend
            {
                Date = g.Key,
                TestCasesCreated = g.Count(),
                TestCasesExecuted = g.Count(tc => tc.ExecutionCount > 0),
                TestCasesPassed = g.Count(tc => tc.PassedCount > 0)
            })
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);
            
        return projections;
    }

    /// <summary>
    /// Calculate escaped bugs (bugs found in production)
    /// </summary>
    private async Task<double> CalculateEscapedBugsAsync(CancellationToken cancellationToken)
    {
        // Escaped bugs are typically found by comparing bugs found during testing vs production
        // For now, we'll estimate based on failed tests that weren't fixed before release
        var failedTests = await _dbContext.TestExecutions
            .Where(te => te.Outcome == "Failed")
            .CountAsync(cancellationToken);
            
        var recentReleaseDate = DateTime.UtcNow.AddMonths(-1); // Bugs in last month as "escaped"
        var escapedBugs = await _dbContext.TestExecutions
            .Where(te => te.Outcome == "Failed" && te.ExecutedDate >= recentReleaseDate)
            .CountAsync(cancellationToken);
            
        return failedTests > 0 ? (double)escapedBugs / failedTests * 100 : 0;
    }

    /// <summary>
    /// Calculate test coverage percentage
    /// </summary>
    private async Task<double> CalculateTestCoverageRateAsync(CancellationToken cancellationToken)
    {
        var requirementsWithTests = await _dbContext.Requirements
            .Where(r => r.TestCases.Any())
            .CountAsync(cancellationToken);
            
        var totalRequirements = await _dbContext.Requirements
            .CountAsync(cancellationToken);
            
        return totalRequirements > 0 ? (double)requirementsWithTests / totalRequirements * 100 : 0;
    }

    /// <summary>
    /// Calculate defect density (bugs per requirement)
    /// </summary>
    private async Task<double> CalculateDefectDensityAsync(CancellationToken cancellationToken)
    {
        var totalDefects = await _dbContext.TestExecutions
            .Where(te => te.Outcome == "Failed")
            .CountAsync(cancellationToken);
            
        var totalRequirements = await _dbContext.Requirements
            .CountAsync(cancellationToken);
            
        return totalRequirements > 0 ? (double)totalDefects / totalRequirements : 0;
    }

    /// <summary>
    /// Calculate test case effectiveness
    /// </summary>
    private async Task<double> CalculateTestCaseEffectivenessAsync(CancellationToken cancellationToken)
    {
        var defectsFoundByTests = await _dbContext.TestExecutions
            .Where(te => te.Outcome == "Failed")
            .CountAsync(cancellationToken);
            
        var totalTests = await _dbContext.TestCases
            .CountAsync(cancellationToken);
            
        return totalTests > 0 ? (double)defectsFoundByTests / totalTests * 100 : 0;
    }

    /// <summary>
    /// Calculate defect leakage rate
    /// </summary>
    private async Task<double> CalculateDefectLeakageRateAsync(CancellationToken cancellationToken)
    {
        // Estimate defect leakage based on bugs found after initial testing
        var initialDefects = await _dbContext.TestExecutions
            .Where(te => te.Outcome == "Failed")
            .GroupBy(te => te.TestCaseId)
            .Select(g => g.Min(te => te.ExecutedDate))
            .CountAsync(cancellationToken);
            
        var laterDefects = await _dbContext.TestExecutions
            .Where(te => te.Outcome == "Failed" && te.ExecutedDate >= DateTime.UtcNow.AddDays(-7))
            .GroupBy(te => te.TestCaseId)
            .Select(g => g.Min(te => te.ExecutedDate))
            .CountAsync(cancellationToken);
            
        return initialDefects > 0 ? (double)laterDefects / initialDefects * 100 : 0;
    }
}