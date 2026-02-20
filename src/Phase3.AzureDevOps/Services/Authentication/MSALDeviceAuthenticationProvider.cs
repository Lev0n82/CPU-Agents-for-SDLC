namespace Phase3.AzureDevOps.Services.Authentication;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Configuration;
using Phase3.AzureDevOps.Core;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides authentication using MSAL Device Code Flow.
/// </summary>
public class MSALDeviceAuthenticationProvider : IAuthenticationProvider
{
    private readonly MSALDeviceAuthenticationConfiguration _config;
    private readonly ILogger<MSALDeviceAuthenticationProvider> _logger;
    private readonly IPublicClientApplication _msalClient;
    private string? _cachedToken;
    private DateTime _tokenExpiry;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public string AuthenticationMethod => "MSALDevice";
    public bool IsCached => _cachedToken != null && DateTime.UtcNow < _tokenExpiry;

    /// <summary>
    /// Initializes a new instance of the <see cref="MSALDeviceAuthenticationProvider"/> class.
    /// </summary>
    /// <param name="config">MSAL device authentication configuration.</param>
    /// <param name="logger">Logger instance.</param>
    public MSALDeviceAuthenticationProvider(
        MSALDeviceAuthenticationConfiguration config,
        ILogger<MSALDeviceAuthenticationProvider> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Build MSAL public client with token cache
        _msalClient = PublicClientApplicationBuilder
            .Create(_config.ClientId)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{_config.TenantId}"))
            .WithRedirectUri("http://localhost")
            .Build();

        // Enable token cache persistence
        var cacheHelper = CreateTokenCacheHelper();
        cacheHelper.RegisterCache(_msalClient.UserTokenCache);

        _logger.LogInformation("MSALDeviceAuthenticationProvider initialized");
    }

    /// <summary>
    /// Gets an authentication token using device code flow.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A valid authentication token.</returns>
    /// <exception cref="AuthenticationException">Thrown if authentication fails.</exception>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Try to acquire token silently from cache
            var accounts = await _msalClient.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    var result = await _msalClient.AcquireTokenSilent(_config.Scopes, accounts.First())
                        .ExecuteAsync(cancellationToken);

                    _logger.LogDebug("Token acquired silently from cache");
                    return result.AccessToken;
                }
                catch (MsalUiRequiredException)
                {
                    _logger.LogDebug("Silent token acquisition failed, falling back to device code flow");
                }
            }

            // Acquire token with device code flow
            _logger.LogInformation("Initiating device code flow");
            var deviceCodeResult = await _msalClient.AcquireTokenWithDeviceCode(
                _config.Scopes,
                deviceCodeResult =>
                {
                    _logger.LogInformation("Device code authentication required:");
                    _logger.LogInformation("  1. Open browser to: {Url}", deviceCodeResult.VerificationUrl);
                    _logger.LogInformation("  2. Enter code: {Code}", deviceCodeResult.UserCode);
                    _logger.LogInformation("  3. Expires in: {Minutes} minutes", deviceCodeResult.ExpiresOn.Subtract(DateTime.UtcNow).TotalMinutes);

                    Console.WriteLine();
                    Console.WriteLine("=== Device Code Authentication ===");
                    Console.WriteLine($"1. Open browser to: {deviceCodeResult.VerificationUrl}");
                    Console.WriteLine($"2. Enter code: {deviceCodeResult.UserCode}");
                    Console.WriteLine($"3. Expires in: {deviceCodeResult.ExpiresOn.Subtract(DateTime.UtcNow).TotalMinutes:F0} minutes");
                    Console.WriteLine("===================================");
                    Console.WriteLine();

                    return Task.CompletedTask;
                })
                .ExecuteAsync(cancellationToken);

            _logger.LogInformation("Device code authentication successful");

            _cachedToken = deviceCodeResult.AccessToken;
            _tokenExpiry = deviceCodeResult.ExpiresOn.UtcDateTime;

            return _cachedToken;
        }
        catch (MsalException ex)
        {
            _logger.LogError(ex, "Device code authentication failed");
            throw new AuthenticationException("Device code authentication failed", ex);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Clears the cached token and removes all accounts from cache.
    /// </summary>
    public void ClearCache()
    {
        _lock.Wait();
        try
        {
            var accounts = _msalClient.GetAccountsAsync().Result;
            foreach (var account in accounts)
            {
                _msalClient.RemoveAsync(account).Wait();
            }

            _cachedToken = null;
            _tokenExpiry = DateTime.MinValue;
            _logger.LogInformation("Token cache cleared and all accounts removed");
        }
        finally
        {
            _lock.Release();
        }
    }

    private MsalCacheHelper CreateTokenCacheHelper()
    {
        var cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AutonomousAgent",
            "TokenCache");

        Directory.CreateDirectory(cacheDirectory);

        var storageProperties = new StorageCreationPropertiesBuilder(
            "msal_token_cache.bin",
            cacheDirectory)
            .Build();

        return MsalCacheHelper.CreateAsync(storageProperties).Result;
    }
}
