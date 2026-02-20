namespace Phase3.AzureDevOps.Interfaces;

/// <summary>
/// Provides authentication tokens for Azure DevOps API access.
/// </summary>
public interface IAuthenticationProvider
{
    /// <summary>
    /// Gets an authentication token for Azure DevOps API access.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A valid authentication token.</returns>
    /// <exception cref="AuthenticationException">Thrown if authentication fails.</exception>
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the authentication method name.
    /// </summary>
    string AuthenticationMethod { get; }

    /// <summary>
    /// Gets a value indicating whether the token is cached.
    /// </summary>
    bool IsCached { get; }

    /// <summary>
    /// Clears any cached tokens, forcing re-authentication on next call.
    /// </summary>
    void ClearCache();
}
