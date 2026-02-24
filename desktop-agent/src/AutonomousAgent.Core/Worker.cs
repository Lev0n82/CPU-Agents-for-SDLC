using AutonomousAgent.Core.SelfTest;
using AutonomousAgent.Core.Telemetry;
using System.Diagnostics;

namespace AutonomousAgent.Core
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly SelfTestManager _selfTestManager;

        public Worker(ILogger<Worker> logger, SelfTestManager selfTestManager)
        {
            _logger = logger;
            _selfTestManager = selfTestManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var startupActivity = TelemetryUtilities.ActivitySource.StartActivity("agent.startup");
            
            _logger.LogInformation("========================================");
            _logger.LogInformation("Autonomous Agent starting up at: {time}", DateTimeOffset.Now);
            _logger.LogInformation("========================================");

            // Record startup metric
            TelemetryUtilities.TasksExecutedCounter.Add(1, new KeyValuePair<string, object?>("type", "startup"));

            // 1. Run Self-Test Sequence
            _logger.LogInformation("");
            _logger.LogInformation("PHASE 1: Self-Test Sequence");
            _logger.LogInformation("----------------------------");
            
            var startTime = DateTime.UtcNow;
            var testSummary = await _selfTestManager.RunAllTestsAsync();
            var testDuration = DateTime.UtcNow - startTime;
            
            TelemetryUtilities.SelfTestsCompletedCounter.Add(testSummary.Results.Count);
            TelemetryUtilities.TaskExecutionTimeHistogram.Record(testDuration.TotalMilliseconds);

            if (testSummary.AllTestsPassed)
            {
                startupActivity?.SetTag("selftest.status", "success");
                startupActivity?.SetTag("selftest.duration_ms", testDuration.TotalMilliseconds);
                
                _logger.LogInformation("");
                _logger.LogInformation("✓ Self-test sequence completed successfully.");
                _logger.LogInformation("");

                // 2. Agent is operational
                _logger.LogInformation("PHASE 2: Operational Mode");
                _logger.LogInformation("----------------------------");
                _logger.LogInformation("Autonomous Agent is now fully operational.");
                _logger.LogInformation("Scheduling Service is running in the background.");
                _logger.LogInformation("");

                // Main agent loop for processing tasks
                int iteration = 0;
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var iterationActivity = TelemetryUtilities.ActivitySource.StartActivity("agent.iteration");
                    iterationActivity?.SetTag("iteration", iteration);
                    
                    TelemetryUtilities.SchedulingIterationsCounter.Add(1);
                    
                    // This is where the main agent logic would go
                    // For now, we just keep the service running
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    iteration++;
                }
            }
            else
            {
                startupActivity?.SetTag("selftest.status", "failed");
                startupActivity?.SetTag("selftest.duration_ms", testDuration.TotalMilliseconds);
                startupActivity?.SetTag("selftest.failed_count", testSummary.Results.Count(r => !r.Passed));
                
                _logger.LogCritical("");
                _logger.LogCritical("✗ Self-test sequence failed!");
                _logger.LogCritical("Agent will not start. Please review the logs above.");
                _logger.LogCritical("");
                _logger.LogCritical("Failed tests:");
                foreach (var failedTest in testSummary.Results.Where(r => !r.Passed))
                {
                    _logger.LogCritical("  - {level}/{name}: {error}",
                        failedTest.TestLevel, failedTest.TestName, failedTest.ErrorMessage);
                }
                _logger.LogCritical("");
            }

            _logger.LogInformation("========================================");
            _logger.LogInformation("Autonomous Agent shutting down at: {time}", DateTimeOffset.Now);
            _logger.LogInformation("========================================");

            TelemetryUtilities.TasksExecutedCounter.Add(1, new KeyValuePair<string, object?>("type", "shutdown"));
        }
    }
}
