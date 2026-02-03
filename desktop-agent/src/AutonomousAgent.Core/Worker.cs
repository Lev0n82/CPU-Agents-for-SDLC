using AutonomousAgent.Core.SelfTest;

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
            _logger.LogInformation("========================================");
            _logger.LogInformation("Autonomous Agent starting up at: {time}", DateTimeOffset.Now);
            _logger.LogInformation("========================================");

            // 1. Run Self-Test Sequence
            _logger.LogInformation("");
            _logger.LogInformation("PHASE 1: Self-Test Sequence");
            _logger.LogInformation("----------------------------");
            
            var testSummary = await _selfTestManager.RunAllTestsAsync();
            
            if (testSummary.AllTestsPassed)
            {
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
                while (!stoppingToken.IsCancellationRequested)
                {
                    // This is where the main agent logic would go
                    // For now, we just keep the service running
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
            else
            {
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
        }
    }
}
