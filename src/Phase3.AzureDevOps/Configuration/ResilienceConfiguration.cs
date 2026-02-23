namespace Phase3.AzureDevOps.Configuration;

/// <summary>
/// Resilience configuration settings.
/// </summary>
public class ResilienceConfiguration
{
    /// <summary>
    /// Circuit breaker settings.
    /// </summary>
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Retry policy settings.
    /// </summary>
    public RetrySettings Retry { get; set; } = new();

    /// <summary>
    /// Timeout settings.
    /// </summary>
    public TimeoutSettings Timeout { get; set; } = new();

    /// <summary>
    /// Bulkhead settings.
    /// </summary>
    public BulkheadSettings Bulkhead { get; set; } = new();
}

/// <summary>
/// Circuit breaker settings.
/// </summary>
public class CircuitBreakerSettings
{
    /// <summary>
    /// Number of failures before opening circuit.
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Duration circuit remains open (seconds).
    /// </summary>
    public int DurationOfBreakSeconds { get; set; } = 30;
}

/// <summary>
/// Retry policy settings.
/// </summary>
public class RetrySettings
{
    /// <summary>
    /// Maximum retry attempts.
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Initial delay before first retry (seconds).
    /// </summary>
    public int InitialDelaySeconds { get; set; } = 1;
}

/// <summary>
/// Timeout settings.
/// </summary>
public class TimeoutSettings
{
    /// <summary>
    /// Operation timeout (seconds).
    /// </summary>
    public int Seconds { get; set; } = 30;
}

/// <summary>
/// Bulkhead settings.
/// </summary>
public class BulkheadSettings
{
    /// <summary>
    /// Maximum parallel operations.
    /// </summary>
    public int MaxParallelization { get; set; } = 10;

    /// <summary>
    /// Maximum queued operations.
    /// </summary>
    public int MaxQueuingActions { get; set; } = 20;
}
