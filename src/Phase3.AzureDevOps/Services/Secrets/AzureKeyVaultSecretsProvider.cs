namespace Phase3.AzureDevOps.Services.Secrets;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Configuration;
using Phase3.AzureDevOps.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides secrets storage using Azure Key Vault.
/// </summary>
public class AzureKeyVaultSecretsProvider : ISecretsProvider
{
    private readonly SecretClient _client;
    private readonly ILogger<AzureKeyVaultSecretsProvider> _logger;

    public string ProviderName => "AzureKeyVault";

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultSecretsProvider"/> class.
    /// </summary>
    /// <param name="config">Key Vault configuration.</param>
    /// <param name="logger">Logger instance.</param>
    public AzureKeyVaultSecretsProvider(
        KeyVaultConfiguration config,
        ILogger<AzureKeyVaultSecretsProvider> logger)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var vaultUri = new Uri(config.VaultUri);
        var credential = new DefaultAzureCredential();

        _client = new SecretClient(vaultUri, credential);

        _logger.LogInformation("AzureKeyVaultSecretsProvider initialized with vault {VaultUri}",
            config.VaultUri);
    }

    /// <summary>
    /// Gets a secret from Key Vault.
    /// </summary>
    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        _logger.LogDebug("Retrieving secret {SecretName} from Key Vault", secretName);

        try
        {
            var response = await _client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            _logger.LogDebug("Secret {SecretName} retrieved successfully", secretName);
            return response.Value.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Secret {SecretName} not found in Key Vault", secretName);
            throw new SecretNotFoundException(secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret {SecretName} from Key Vault", secretName);
            throw;
        }
    }

    /// <summary>
    /// Sets a secret in Key Vault.
    /// </summary>
    public async Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        if (string.IsNullOrEmpty(secretValue))
            throw new ArgumentException("Secret value cannot be null or empty.", nameof(secretValue));

        _logger.LogInformation("Setting secret {SecretName} in Key Vault", secretName);

        try
        {
            var secret = new KeyVaultSecret(secretName, secretValue);
            await _client.SetSecretAsync(secret, cancellationToken);
            _logger.LogInformation("Secret {SecretName} set successfully", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set secret {SecretName} in Key Vault", secretName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a secret from Key Vault.
    /// </summary>
    public async Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        _logger.LogInformation("Deleting secret {SecretName} from Key Vault", secretName);

        try
        {
            var operation = await _client.StartDeleteSecretAsync(secretName, cancellationToken);
            await operation.WaitForCompletionAsync(cancellationToken);
            _logger.LogInformation("Secret {SecretName} deleted successfully", secretName);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Secret {SecretName} not found in Key Vault", secretName);
            throw new SecretNotFoundException(secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete secret {SecretName} from Key Vault", secretName);
            throw;
        }
    }

    /// <summary>
    /// Checks if a secret exists in Key Vault.
    /// </summary>
    public async Task<bool> SecretExistsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        try
        {
            await _client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return true;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    private void ValidateSecretName(string secretName)
    {
        if (string.IsNullOrWhiteSpace(secretName))
            throw new ArgumentException("Secret name cannot be null or empty.", nameof(secretName));

        // Key Vault secret names must match ^[0-9a-zA-Z-]+$
        if (!System.Text.RegularExpressions.Regex.IsMatch(secretName, @"^[0-9a-zA-Z-]+$"))
            throw new ArgumentException(
                "Secret name must contain only alphanumeric characters and hyphens.",
                nameof(secretName));
    }
}
