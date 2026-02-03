using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace AutonomousAgent.Core.Scheduling
{
    public class SchedulingService : BackgroundService
    {
        private readonly ILogger<SchedulingService> _logger;
        private readonly SchedulingConfig _config;
        private readonly IHostApplicationLifetime _appLifetime;
        private DateTime? _lastRebootCheck;

        public SchedulingService(
            ILogger<SchedulingService> logger,
            IOptions<SchedulingConfig> config,
            IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            _config = config.Value;
            _appLifetime = appLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduling Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check if it's time for the nightly reboot
                    if (_config.NightlyReboot.Enabled)
                    {
                        await CheckAndExecuteNightlyRebootAsync(stoppingToken);
                    }

                    // Wait for 1 minute before next check
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation, exit gracefully
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in scheduling service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Scheduling Service stopped");
        }

        private async Task CheckAndExecuteNightlyRebootAsync(CancellationToken stoppingToken)
        {
            var now = DateTime.Now;
            var scheduledTime = DateTime.Today
                .AddHours(_config.NightlyReboot.Hour)
                .AddMinutes(_config.NightlyReboot.Minute);

            // Check if we're in the 1-minute window for the scheduled reboot
            var isInRebootWindow = now >= scheduledTime && now < scheduledTime.AddMinutes(1);

            // Ensure we only reboot once per day (check if we already rebooted today)
            var alreadyRebootedToday = _lastRebootCheck.HasValue && 
                                      _lastRebootCheck.Value.Date == DateTime.Today;

            if (isInRebootWindow && !alreadyRebootedToday)
            {
                _logger.LogWarning("Scheduled reboot time reached. Initiating graceful shutdown and reboot...");
                _lastRebootCheck = now;

                await InitiateGracefulRebootAsync(stoppingToken);
            }
        }

        private async Task InitiateGracefulRebootAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Step 1: Signal the application to stop gracefully
                _logger.LogInformation("Step 1: Signaling application to stop gracefully...");
                _appLifetime.StopApplication();

                // Step 2: Wait for graceful shutdown (with timeout)
                _logger.LogInformation("Step 2: Waiting for graceful shutdown (max 30 seconds)...");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                // Step 3: Execute system reboot command
                _logger.LogInformation("Step 3: Executing system reboot command...");
                
                if (OperatingSystem.IsWindows())
                {
                    // Windows reboot command
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "shutdown.exe",
                            Arguments = "/r /t 10 /c \"Autonomous Agent scheduled reboot\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    _logger.LogInformation("System reboot initiated. Rebooting in 10 seconds...");
                }
                else if (OperatingSystem.IsLinux())
                {
                    // Linux reboot command (requires sudo privileges)
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "sudo",
                            Arguments = "shutdown -r +1 \"Autonomous Agent scheduled reboot\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    _logger.LogInformation("System reboot initiated. Rebooting in 1 minute...");
                }
                else
                {
                    _logger.LogWarning("Reboot not supported on this operating system");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initiate system reboot");
            }
        }
    }
}
