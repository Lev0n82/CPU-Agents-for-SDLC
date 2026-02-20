namespace Phase3.AzureDevOps.Services.Authentication;

using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

/// <summary>
/// Provides authentication using Personal Access Tokens (PAT).
/// </summary>
public class PATAuthenticationProvider : IAuthenticationProvider
{
    private readonly string _pat;
    private readonly ILogger<PATAuthenticationProvider> _logger;
    private static readonly Regex PATPattern = new Regex(@"^[a-z0-9]{52}$", RegexOptions.IgnoreCase);

    public string AuthenticationMethod => "PAT";
    public bool IsCached => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="PATAuthenticationProvider"/> class.
    /// </summary>
    /// <param name="pat">The Personal Access Token.</param>
    /// <param name="logger">Logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if PAT is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown if PAT format is invalid.</exception>
    public PATAuthenticationProvider(string pat, ILogger<PATAuthenticationProvider> logger)
    {
        if (string.IsNullOrWhiteSpace(pat))
            throw new ArgumentNullException(nameof(pat), "PAT cannot be null or empty.");

        if (!PATPattern.IsMatch(pat))
            throw new ArgumentException("PAT format is invalid. Expected 52 alphanumeric characters.", nameof(pat));

        _pat = pat;
        _logger = logger;

        _logger.LogDebug("PATAuthenticationProvider initialized");
    }

    /// <summary>
    /// Gets the PAT token immediately (no network call required).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The PAT token.</returns>
    public Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Returning PAT token");
        return Task.FromResult(_pat);
    }

    /// <summary>
    /// Clears the cached token (no-op for PAT provider).
    /// </summary>
    public void ClearCache()
    {
        _logger.LogDebug("ClearCache called (no-op for PAT provider)");
    }
}
