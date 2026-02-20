namespace Phase3.AzureDevOps.Services.Secrets;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Configuration;
using Phase3.AzureDevOps.Core;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// Provides secrets storage using DPAPI-encrypted files.
/// </summary>
public class DPAPISecretsProvider : ISecretsProvider
{
    private readonly string _storePath;
    private readonly ILogger<DPAPISecretsProvider> _logger;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public string ProviderName => "DPAPI";

    /// <summary>
    /// Initializes a new instance of the <see cref="DPAPISecretsProvider"/> class.
    /// </summary>
    /// <param name="config">DPAPI configuration.</param>
    /// <param name="logger">Logger instance.</param>
    public DPAPISecretsProvider(
        DPAPIConfiguration config,
        ILogger<DPAPISecretsProvider> logger)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _storePath = config.StorePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AutonomousAgent",
            "Secrets");

        Directory.CreateDirectory(_storePath);

        _logger.LogInformation("DPAPISecretsProvider initialized with store path {StorePath}", _storePath);
    }

    /// <summary>
    /// Gets a secret from DPAPI-encrypted storage.
    /// </summary>
    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        _logger.LogDebug("Retrieving secret {SecretName} from DPAPI storage", secretName);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var filePath = GetSecretFilePath(secretName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Secret {SecretName} not found in DPAPI storage", secretName);
                throw new SecretNotFoundException(secretName);
            }

            var encryptedBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
            var decryptedBytes = ProtectedData.Unprotect(
                encryptedBytes,
                null,
                DataProtectionScope.CurrentUser);

            var secretValue = Encoding.UTF8.GetString(decryptedBytes);

            _logger.LogDebug("Secret {SecretName} retrieved successfully", secretName);
            return secretValue;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Sets a secret in DPAPI-encrypted storage.
    /// </summary>
    public async Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        if (string.IsNullOrEmpty(secretValue))
            throw new ArgumentException("Secret value cannot be null or empty.", nameof(secretValue));

        _logger.LogInformation("Setting secret {SecretName} in DPAPI storage", secretName);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var filePath = GetSecretFilePath(secretName);

            var secretBytes = Encoding.UTF8.GetBytes(secretValue);
            var encryptedBytes = ProtectedData.Protect(
                secretBytes,
                null,
                DataProtectionScope.CurrentUser);

            await File.WriteAllBytesAsync(filePath, encryptedBytes, cancellationToken);

            _logger.LogInformation("Secret {SecretName} set successfully", secretName);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Deletes a secret from DPAPI-encrypted storage.
    /// </summary>
    public async Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        _logger.LogInformation("Deleting secret {SecretName} from DPAPI storage", secretName);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var filePath = GetSecretFilePath(secretName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Secret {SecretName} not found in DPAPI storage", secretName);
                throw new SecretNotFoundException(secretName);
            }

            File.Delete(filePath);

            _logger.LogInformation("Secret {SecretName} deleted successfully", secretName);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Checks if a secret exists in DPAPI-encrypted storage.
    /// </summary>
    public Task<bool> SecretExistsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ValidateSecretName(secretName);

        var filePath = GetSecretFilePath(secretName);
        return Task.FromResult(File.Exists(filePath));
    }

    private string GetSecretFilePath(string secretName)
    {
        // Use SHA256 hash of secret name as filename to avoid filesystem restrictions
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(secretName));
        var hashString = Convert.ToHexString(hash);
        return Path.Combine(_storePath, $"{hashString}.secret");
    }

    private void ValidateSecretName(string secretName)
    {
        if (string.IsNullOrWhiteSpace(secretName))
            throw new ArgumentException("Secret name cannot be null or empty.", nameof(secretName));
    }
}
