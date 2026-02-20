namespace Phase3.AzureDevOps.Services.Concurrency;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Background service that automatically releases expired work item claims.
/// </summary>
public class StaleClaimRecoveryService : BackgroundService
{
    private readonly IWorkItemCoordinator _coordinator;
    private readonly ConcurrencyConfiguration _config;
    private readonly ILogger<StaleClaimRecoveryService> _logger;

    public StaleClaimRecoveryService(
        IWorkItemCoordinator coordinator,
        ConcurrencyConfiguration config,
        ILogger<StaleClaimRecoveryService> logger)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StaleClaimRecoveryService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReleaseStaleClaimsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing stale claims");
            }

            // Wait for configured interval
            var interval = TimeSpan.FromMinutes(_config.StaleClaimCheckIntervalMinutes);
            _logger.LogDebug("Next stale claim check in {Minutes} minutes", interval.TotalMinutes);
            await Task.Delay(interval, stoppingToken);
        }

        _logger.LogInformation("StaleClaimRecoveryService stopped");
    }

    private async Task ReleaseStaleClaimsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking for stale claims");

        var startTime = DateTime.UtcNow;
        var releasedCount = await _coordinator.ReleaseExpiredClaimsAsync(cancellationToken);
        var duration = DateTime.UtcNow - startTime;

        if (releasedCount > 0)
        {
            _logger.LogWarning("Released {Count} stale claims in {Ms}ms",
                releasedCount, duration.TotalMilliseconds);
        }
        else
        {
            _logger.LogDebug("No stale claims found (checked in {Ms}ms)",
                duration.TotalMilliseconds);
        }
    }
}
