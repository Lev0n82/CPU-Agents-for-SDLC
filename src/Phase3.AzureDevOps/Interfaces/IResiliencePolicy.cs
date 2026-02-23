namespace Phase3.AzureDevOps.Interfaces;

/// <summary>
/// Provides resilience policies for fault-tolerant operations.
/// </summary>
public interface IResiliencePolicy
{
    /// <summary>
    /// Executes an async operation with resilience policies applied.
    /// </summary>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <param name="operation">Operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an async operation with resilience policies applied.
    /// </summary>
    /// <param name="operation">Operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current circuit breaker state.
    /// </summary>
    CircuitState GetCircuitState();

    /// <summary>
    /// Resets the circuit breaker to closed state.
    /// </summary>
    void ResetCircuitBreaker();
}

/// <summary>
/// Circuit breaker state enumeration.
/// </summary>
public enum CircuitState
{
    /// <summary>
    /// Circuit is closed - operations flow normally.
    /// </summary>
    Closed,

    /// <summary>
    /// Circuit is open - operations are blocked.
    /// </summary>
    Open,

    /// <summary>
    /// Circuit is half-open - testing if service recovered.
    /// </summary>
    HalfOpen
}
