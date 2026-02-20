namespace Phase3.AzureDevOps.Interfaces;

/// <summary>
/// Provides secure storage and retrieval of secrets.
/// </summary>
public interface ISecretsProvider
{
    /// <summary>
    /// Gets a secret by name.
    /// </summary>
    /// <param name="secretName">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The secret value.</returns>
    /// <exception cref="SecretNotFoundException">Thrown if the secret does not exist.</exception>
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a secret value.
    /// </summary>
    /// <param name="secretName">The name of the secret to set.</param>
    /// <param name="secretValue">The secret value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a secret.
    /// </summary>
    /// <param name="secretName">The name of the secret to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a secret exists.
    /// </summary>
    /// <param name="secretName">The name of the secret to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the secret exists; otherwise, false.</returns>
    Task<bool> SecretExistsAsync(string secretName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    string ProviderName { get; }
}
