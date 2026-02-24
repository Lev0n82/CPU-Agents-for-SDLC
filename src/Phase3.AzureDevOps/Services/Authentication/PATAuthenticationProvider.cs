namespace Phase3.AzureDevOps.Services.Authentication;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

/// <summary>
/// Provides authentication using Personal Access Tokens (PAT) from Azure Key Vault.
/// Supports both cached (lazy) and fresh retrieval modes.
/// </summary>
public class PATAuthenticationProvider : IAuthenticationProvider
{
    private readonly ISecretsProvider _secretsProvider;
    private readonly ILogger<PATAuthenticationProvider> _logger;
    private readonly bool _cacheToken;
    private readonly Lazy<Task<string>>? _cachedPatTask;
    private static readonly Regex PATPattern = new Regex(@"^[a-z0-9]{52}$", RegexOptions.IgnoreCase);

    public string AuthenticationMethod => "PAT";
    public bool IsCached => _cacheToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="PATAuthenticationProvider"/> class.
    /// </summary>
    /// <param name="secretsProvider">The secrets provider to retrieve PAT from Azure Key Vault.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="cacheToken">Whether to cache the PAT token (default: true for lazy retrieval).</param>
    /// <exception cref="ArgumentNullException">Thrown if secrets provider is null.</exception>
    public PATAuthenticationProvider(
        ISecretsProvider secretsProvider,
        ILogger<PATAuthenticationProvider> logger,
        bool cacheToken = true)
    {
        _secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheToken = cacheToken;

        if (_cacheToken)
        {
            _cachedPatTask = new Lazy<Task<string>>(async () =>
            {
                var pat = await _secretsProvider.GetSecretAsync("AzureAgentPat", CancellationToken.None);
                if (string.IsNullOrWhiteSpace(pat))
                    throw new ArgumentNullException(nameof(pat), "PAT cannot be null or empty.");

                if (!PATPattern.IsMatch(pat))
                    throw new ArgumentException("PAT format is invalid. Expected 52 alphanumeric characters.", nameof(pat));

                return pat;
            });
        }

        _logger.LogDebug("PATAuthenticationProvider initialized with Azure Key Vault (CacheToken: {CacheToken})", _cacheToken);
    }

    /// <summary>
    /// Gets the PAT token from Azure Key Vault.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The PAT token.</returns>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cacheToken && _cachedPatTask != null)
        {
            _logger.LogDebug("Retrieving cached PAT token from Azure Key Vault");
            return await _cachedPatTask.Value;
        }

        _logger.LogDebug("Retrieving fresh PAT token from Azure Key Vault");
        
        var pat = await _secretsProvider.GetSecretAsync("AzureAgentPat", cancellationToken);
        if (string.IsNullOrWhiteSpace(pat))
            throw new ArgumentNullException(nameof(pat), "PAT cannot be null or empty.");

        if (!PATPattern.IsMatch(pat))
            throw new ArgumentException("PAT format is invalid. Expected 52 alphanumeric characters.", nameof(pat));

        _logger.LogDebug("PAT token retrieved successfully");
        return pat;
    }

    /// <summary>
    /// Clears the cached token (no-op if caching is disabled).
    /// </summary>
    public void ClearCache()
    {
        if (_cacheToken)
        {
            _logger.LogWarning("ClearCache called but token caching is enabled - token will remain cached for the lifetime of the provider");
        }
        else
        {
            _logger.LogDebug("ClearCache called (no-op - caching is disabled)");
        }
    }
}
