namespace Phase3.AzureDevOps.Services.Secrets;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Background service that automatically rotates Personal Access Tokens.
/// </summary>
public class PATRotationService : BackgroundService
{
    private readonly ISecretsProvider _secretsProvider;
    private readonly IAzureDevOpsClient _azureDevOpsClient;
    private readonly PATRotationConfiguration _config;
    private readonly ILogger<PATRotationService> _logger;

    public PATRotationService(
        ISecretsProvider secretsProvider,
        IAzureDevOpsClient azureDevOpsClient,
        PATRotationConfiguration config,
        ILogger<PATRotationService> logger)
    {
        _secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
        _azureDevOpsClient = azureDevOpsClient ?? throw new ArgumentNullException(nameof(azureDevOpsClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("PAT rotation is disabled");
            return;
        }

        _logger.LogInformation("PATRotationService started (rotation interval: {Days} days)",
            _config.RotationIntervalDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRotatePATAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during PAT rotation check");
            }

            // Check daily
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }

        _logger.LogInformation("PATRotationService stopped");
    }

    private async Task CheckAndRotatePATAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking if PAT rotation is needed");

        // Get current PAT
        var currentPAT = await _secretsProvider.GetSecretAsync("AzureDevOpsPAT", cancellationToken);

        // Check PAT expiry via Azure DevOps API
        var patInfo = await _azureDevOpsClient.GetPATInfoAsync(currentPAT, cancellationToken);

        var daysUntilExpiry = (patInfo.ValidTo - DateTime.UtcNow).TotalDays;

        _logger.LogInformation("Current PAT expires in {Days} days", daysUntilExpiry);

        if (daysUntilExpiry <= _config.RotationIntervalDays)
        {
            _logger.LogWarning("PAT expiry approaching, rotating PAT");
            await RotatePATAsync(currentPAT, cancellationToken);
        }
        else
        {
            _logger.LogDebug("PAT rotation not needed");
        }
    }

    private async Task RotatePATAsync(string currentPAT, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting PAT rotation");

        try
        {
            // Create new PAT with same scopes
            var newPAT = await _azureDevOpsClient.CreatePATAsync(
                name: $"AutonomousAgent-{DateTime.UtcNow:yyyyMMdd}",
                scopes: _config.Scopes,
                validTo: DateTime.UtcNow.AddDays(_config.NewPATValidityDays),
                cancellationToken);

            // Store new PAT
            await _secretsProvider.SetSecretAsync("AzureDevOpsPAT", newPAT, cancellationToken);

            _logger.LogInformation("New PAT created and stored successfully");

            // Revoke old PAT after grace period
            await Task.Delay(TimeSpan.FromHours(_config.GracePeriodHours), cancellationToken);

            await _azureDevOpsClient.RevokePATAsync(currentPAT, cancellationToken);

            _logger.LogInformation("Old PAT revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rotate PAT");
            throw;
        }
    }
}
