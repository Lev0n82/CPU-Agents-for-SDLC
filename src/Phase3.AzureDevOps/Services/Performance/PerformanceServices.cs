namespace Phase3.AzureDevOps.Services.Performance;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Interfaces;

/// <summary>
/// Implements in-memory caching with statistics.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly TimeSpan _defaultTtl;
    private long _hits;
    private long _misses;

    public CacheService(
        IMemoryCache cache,
        ILogger<CacheService> logger,
        IConfiguration configuration)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultTtl = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Cache:DefaultTtlMinutes", 5));
    }

    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            Interlocked.Increment(ref _hits);
            _logger.LogDebug("Cache hit for key {Key}", key);
            return value;
        }

        Interlocked.Increment(ref _misses);
        _logger.LogDebug("Cache miss for key {Key}", key);
        return default;
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
        };

        _cache.Set(key, value, options);
        _logger.LogDebug("Cached value for key {Key} with TTL {Ttl}", key, ttl ?? _defaultTtl);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug("Removed cache entry for key {Key}", key);
    }

    public void Clear()
    {
        // MemoryCache doesn't support clearing all entries
        // Would need to track keys separately for full clear
        _logger.LogWarning("Cache clear not fully implemented - use Remove for specific keys");
    }

    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics
        {
            Hits = Interlocked.Read(ref _hits),
            Misses = Interlocked.Read(ref _misses),
            Count = 0 // MemoryCache doesn't expose count
        };
    }
}

/// <summary>
/// Implements token bucket rate limiting algorithm.
/// </summary>
public class TokenBucketRateLimiter : IRateLimiter
{
    private readonly ILogger<TokenBucketRateLimiter> _logger;
    private readonly int _tokensPerSecond;
    private readonly int _burstCapacity;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private int _availableTokens;
    private DateTime _lastRefillTime;

    public TokenBucketRateLimiter(
        ILogger<TokenBucketRateLimiter> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tokensPerSecond = configuration.GetValue<int>("RateLimiting:TokensPerSecond", 10);
        _burstCapacity = configuration.GetValue<int>("RateLimiting:BurstCapacity", 20);
        _availableTokens = _burstCapacity;
        _lastRefillTime = DateTime.UtcNow;
    }

    public async Task<bool> TryAcquireAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            RefillTokens();

            if (_availableTokens > 0)
            {
                _availableTokens--;
                _logger.LogDebug("Token acquired - {Available} tokens remaining", _availableTokens);
                return true;
            }

            _logger.LogWarning("Rate limit exceeded - no tokens available");
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    public int GetAvailableTokens()
    {
        return _availableTokens;
    }

    public async Task ResetAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _availableTokens = _burstCapacity;
            _lastRefillTime = DateTime.UtcNow;
            _logger.LogInformation("Rate limiter reset");
        }
        finally
        {
            _lock.Release();
        }
    }

    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var elapsed = (now - _lastRefillTime).TotalSeconds;
        var tokensToAdd = (int)(elapsed * _tokensPerSecond);

        if (tokensToAdd > 0)
        {
            _availableTokens = Math.Min(_availableTokens + tokensToAdd, _burstCapacity);
            _lastRefillTime = now;
            _logger.LogDebug("Refilled {Tokens} tokens - {Available} available", 
                tokensToAdd, _availableTokens);
        }
    }
}
