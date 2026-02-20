namespace Phase3.AzureDevOps.Services.Authentication;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Configuration;
using Phase3.AzureDevOps.Core;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Provides authentication using X.509 certificates.
/// </summary>
public class CertificateAuthenticationProvider : IAuthenticationProvider
{
    private readonly CertificateAuthenticationConfiguration _config;
    private readonly ILogger<CertificateAuthenticationProvider> _logger;
    private readonly IConfidentialClientApplication _msalClient;
    private string? _cachedToken;
    private DateTime _tokenExpiry;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public string AuthenticationMethod => "Certificate";
    public bool IsCached => _cachedToken != null && DateTime.UtcNow < _tokenExpiry;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateAuthenticationProvider"/> class.
    /// </summary>
    /// <param name="config">Certificate authentication configuration.</param>
    /// <param name="logger">Logger instance.</param>
    public CertificateAuthenticationProvider(
        CertificateAuthenticationConfiguration config,
        ILogger<CertificateAuthenticationProvider> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Load certificate from store
        var certificate = LoadCertificate();

        // Build MSAL confidential client
        _msalClient = ConfidentialClientApplicationBuilder
            .Create(_config.ClientId)
            .WithCertificate(certificate)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{_config.TenantId}"))
            .Build();

        _logger.LogInformation("CertificateAuthenticationProvider initialized with certificate {Thumbprint}",
            certificate.Thumbprint);
    }

    /// <summary>
    /// Gets an authentication token using the certificate.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A valid authentication token.</returns>
    /// <exception cref="AuthenticationException">Thrown if authentication fails.</exception>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Return cached token if still valid
            if (IsCached)
            {
                _logger.LogDebug("Returning cached token (expires in {Minutes} minutes)",
                    (_tokenExpiry - DateTime.UtcNow).TotalMinutes);
                return _cachedToken!;
            }

            // Acquire new token
            _logger.LogInformation("Acquiring new token with certificate");
            var startTime = DateTime.UtcNow;

            var scopes = new[] { "499b84ac-1321-427f-aa17-267ca6975798/.default" }; // Azure DevOps scope
            var result = await _msalClient.AcquireTokenForClient(scopes)
                .ExecuteAsync(cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Token acquired successfully in {Ms}ms", duration.TotalMilliseconds);

            // Cache token
            _cachedToken = result.AccessToken;
            _tokenExpiry = result.ExpiresOn.UtcDateTime;

            return _cachedToken;
        }
        catch (MsalException ex)
        {
            _logger.LogError(ex, "Failed to acquire token with certificate");
            throw new AuthenticationException("Certificate authentication failed", ex);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Clears the cached token, forcing re-authentication on next call.
    /// </summary>
    public void ClearCache()
    {
        _lock.Wait();
        try
        {
            _cachedToken = null;
            _tokenExpiry = DateTime.MinValue;
            _logger.LogDebug("Token cache cleared");
        }
        finally
        {
            _lock.Release();
        }
    }

    private X509Certificate2 LoadCertificate()
    {
        using var store = new X509Store(_config.StoreName, _config.StoreLocation);
        store.Open(OpenFlags.ReadOnly);

        var certificates = store.Certificates.Find(
            X509FindType.FindByThumbprint,
            _config.Thumbprint,
            validOnly: false);

        if (certificates.Count == 0)
        {
            throw new CertificateNotFoundException(
                _config.Thumbprint,
                $"Certificate with thumbprint {_config.Thumbprint} not found in {_config.StoreLocation}/{_config.StoreName}");
        }

        return certificates[0];
    }
}
