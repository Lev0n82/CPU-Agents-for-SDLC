namespace Phase3.AzureDevOps.Interfaces;

/// <summary>
/// Provides caching operations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <returns>Cached value or null if not found.</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Sets a cached value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="ttl">Time-to-live.</param>
    void Set<T>(string key, T value, TimeSpan? ttl = null);

    /// <summary>
    /// Removes a cached value.
    /// </summary>
    /// <param name="key">Cache key.</param>
    void Remove(string key);

    /// <summary>
    /// Clears all cached values.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    /// <returns>Cache statistics.</returns>
    CacheStatistics GetStatistics();
}

/// <summary>
/// Cache statistics.
/// </summary>
public class CacheStatistics
{
    public long Hits { get; set; }
    public long Misses { get; set; }
    public double HitRate => Hits + Misses > 0 ? (double)Hits / (Hits + Misses) : 0;
    public int Count { get; set; }
}

/// <summary>
/// Provides rate limiting operations.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Attempts to acquire a token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if token acquired, false if rate limit exceeded.</returns>
    Task<bool> TryAcquireAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of available tokens.
    /// </summary>
    /// <returns>Available tokens.</returns>
    int GetAvailableTokens();

    /// <summary>
    /// Resets the rate limiter.
    /// </summary>
    Task ResetAsync();
}
