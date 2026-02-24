using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;

namespace Phase3.AzureDevOps.Services.QA;

public class QAMetricsExporterService : BackgroundService
{
    private readonly QADashboardService _dashboardService;
    private readonly ILogger<QAMetricsExporterService> _logger;
    private readonly Meter _meter;
    private readonly Counter<int> _totalTestsCounter;
    private readonly Counter<int> _passedTestsCounter;
    private readonly ObservableGauge<double> _testCoverageGauge;
    private readonly ObservableGauge<double> _defectDensityGauge;
    private readonly ObservableGauge<double> _escapedBugsGauge;
    private readonly ObservableGauge<double> _testProductivityGauge;
    private readonly ObservableGauge<double> _defectLeakageGauge;
    private readonly ObservableGauge<double> _testEffectivenessGauge;
    private readonly ObservableGauge<double> _devDelayGauge;
    private readonly ObservableGauge<double> _execDelayGauge;

    public QAMetricsExporterService(
        QADashboardService dashboardService,
        ILogger<QAMetricsExporterService> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
        
        _meter = new Meter("Phase3.AzureDevOps.QAMetrics");
        
        // Counter metrics
        _totalTestsCounter = _meter.CreateCounter<int>("qa_total_test_cases");
        _passedTestsCounter = _meter.CreateCounter<int>("qa_passed_test_cases");
        
        // Gauge metrics (observable - provide values on demand)
        _testCoverageGauge = _meter.CreateObservableGauge<double>(
            "qa_test_coverage_percentage",
            () => GetTestCoverageValue());
            
        _defectDensityGauge = _meter.CreateObservableGauge<double>(
            "qa_defect_density",
            () => GetDefectDensityValue());
            
        _escapedBugsGauge = _meter.CreateObservableGauge<double>(
            "qa_escaped_bugs_total",
            () => GetEscapedBugsValue());
            
        _testProductivityGauge = _meter.CreateObservableGauge<double>(
            "qa_test_productivity",
            () => GetTestProductivityValue());
            
        _defectLeakageGauge = _meter.CreateObservableGauge<double>(
            "qa_defect_leakage_rate",
            () => GetDefectLeakageValue());
            
        _testEffectivenessGauge = _meter.CreateObservableGauge<double>(
            "qa_test_case_effectiveness",
            () => GetTestCaseEffectivenessValue());
            
        _devDelayGauge = _meter.CreateObservableGauge<double>(
            "qa_development_delay_days",
            () => GetDevelopmentDelayValue());
            
        _execDelayGauge = _meter.CreateObservableGauge<double>(
            "qa_execution_delay_days",
            () => GetExecutionDelayValue());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QA Metrics Exporter Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateMetricsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Update every 5 minutes
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error updating QA metrics");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Retry after 1 minute
            }
        }
    }

    private async Task UpdateMetricsAsync(CancellationToken cancellationToken)
    {
        var overallMetrics = await _dashboardService.GetOverallMetricsAsync(cancellationToken);
        
        // Update counter metrics
        _totalTestsCounter.Add(overallMetrics.TotalTestCases);
        _passedTestsCounter.Add((int)(overallMetrics.TotalTestCases * (overallMetrics.TestCoverageRate / 100)));
        
        _logger.LogInformation("QA metrics updated: {TotalTests} tests, {Coverage}% coverage", 
            overallMetrics.TotalTestCases, overallMetrics.TestCoverageRate);
    }

    private Measurement<double> GetTestCoverageValue()
    {
        try
        {
            var metrics = _dashboardService.GetOverallMetricsAsync(default).GetAwaiter().GetResult();
            return new Measurement<double>(metrics.TestCoverageRate, new KeyValuePair<string, object?>("type", "percentage"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get test coverage metric");
            return new Measurement<double>(0, new KeyValuePair<string, object?>("type", "percentage"));
        }
    }

    private Measurement<double> GetDefectDensityValue()
    {
        try
        {
            var metrics = _dashboardService.GetOverallMetricsAsync(default).GetAwaiter().GetResult();
            return new Measurement<double>(metrics.DefectDensity, new KeyValuePair<string, object?>("type", "density"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get defect density metric");
            return new Measurement<double>(0, new KeyValuePair<string, object?>("type", "density"));
        }
    }

    private Measurement<double> GetEscapedBugsValue()
    {
        try
        {
            var metrics = _dashboardService.GetOverallMetricsAsync(default).GetAwaiter().GetResult();
            return new Measurement<double>(metrics.TotalEscapedBugs, new KeyValuePair<string, object?>("type", "count"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get escaped bugs metric");
            return new Measurement<double>(0, new KeyValuePair<string, object?>("type", "count"));
        }
    }

    private Measurement<double> GetTestProductivityValue()
    {
        try
        {
            var metrics = _dashboardService.GetOverallMetricsAsync(default).GetAwaiter().GetResult();
            return new Measurement<double>(metrics.TestCaseProductivity, new KeyValuePair<string, object?>("type", "tests_per_day"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get test productivity metric");
            return new Measurement<double>(0, new KeyValuePair<string, object?>("type", "tests_per_day"));
        }
    }

    private Measurement<double> GetDefectLeakageValue()
    {
        try
        {
            var metrics = _dashboardService.GetOverallMetricsAsync(default).GetAwaiter().GetResult();
            return new Measurement<double>(metrics.DefectLeakageRate, new KeyValuePair<string, object?>("type", "percentage"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get defect leakage metric");
            return new Measurement<double>(0, new KeyValuePair<string, object?>("type", "percentage"));
        }
    }

    private Measurement<double> GetTestCaseEffectivenessValue()
    {
        try
        {
            var metrics = _dashboardService.GetOverallMetricsAsync(default).GetAwaiter().GetResult();
            return new Measurement<double>(metrics.TestCaseEffectiveness, new KeyValuePair<string, object?>("type", "percentage"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get test case effectiveness metric");
            return new Measurement<double>(0, new KeyValuePair<string, object?>("type", "percentage"));
        }
    }

    private Measurement<double> GetDevelopmentDelayValue()
    {
        try
        {
            var metrics = _dashboardService.GetOverallMetricsAsync(default).GetAwaiter().GetResult();
            return new Measurement<double>(metrics.AverageTestCaseDevelopmentDelayDays, new KeyValuePair<string, object?>("type", "days"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get development delay metric");
            return new Measurement<double>(0, new KeyValuePair<string, object?>("type", "days"));
        }
    }

    private Measurement<double> GetExecutionDelayValue()
    {
        try
        {
            var metrics = _dashboardService.GetOverallMetricsAsync(default).GetAwaiter().GetResult();
            return new Measurement<double>(metrics.AverageFirstExecutionDelayDays, new KeyValuePair<string, object?>("type", "days"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get execution delay metric");
            return new Measurement<double>(0, new KeyValuePair<string, object?>("type", "days"));
        }
    }
}