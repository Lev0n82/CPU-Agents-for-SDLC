namespace Phase3.AzureDevOps.Services.Resilience;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Phase3.AzureDevOps.Interfaces;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

/// <summary>
/// Implements resilience policies for fault-tolerant operations.
/// </summary>
public class ResiliencePolicy : IResiliencePolicy
{
    private readonly ILogger<ResiliencePolicy> _logger;
    private readonly ResiliencePipeline _pipeline;
    private int _circuitState = 0; // 0=Closed, 1=Open, 2=HalfOpen
    private readonly object _stateLock = new();

    public ResiliencePolicy(
        ILogger<ResiliencePolicy> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Build resilience pipeline with Polly 8.x API
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = configuration.GetValue<int>("Resilience:Retry:MaxAttempts", 3),
                Delay = TimeSpan.FromSeconds(configuration.GetValue<int>("Resilience:Retry:InitialDelaySeconds", 1)),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} after {Delay}s due to: {Exception}",
                        args.AttemptNumber, args.RetryDelay.TotalSeconds, args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                },
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => IsTransientException(ex))
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("Resilience:Timeout:Seconds", 30)),
                OnTimeout = args =>
                {
                    _logger.LogWarning("Operation timed out after {Timeout}s", args.Timeout.TotalSeconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(
                async (ct) => await operation(ct),
                cancellationToken);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Operation timed out");
            throw new ResilienceException("Operation timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resilience policy failed");
            throw new ResilienceException("Operation failed after retries", ex);
        }
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async (ct) =>
        {
            await operation(ct);
            return true;
        }, cancellationToken);
    }

    public Interfaces.CircuitState GetCircuitState()
    {
        lock (_stateLock)
        {
            return _circuitState switch
            {
                0 => Interfaces.CircuitState.Closed,
                1 => Interfaces.CircuitState.Open,
                2 => Interfaces.CircuitState.HalfOpen,
                _ => Interfaces.CircuitState.Closed
            };
        }
    }

    public void ResetCircuitBreaker()
    {
        lock (_stateLock)
        {
            _circuitState = 0;
            _logger.LogInformation("Circuit breaker manually reset to closed state");
        }
    }

    private bool IsTransientException(Exception ex)
    {
        // Transient exceptions that should be retried
        return ex is HttpRequestException ||
               ex is TimeoutException ||
               ex is System.Net.Sockets.SocketException ||
               (ex.GetType().Name == "VssServiceException" && IsTransientHttpStatusCode(GetHttpStatusCode(ex)));
    }

    private int GetHttpStatusCode(Exception ex)
    {
        // Try to get HttpStatusCode from exception using reflection
        var property = ex.GetType().GetProperty("HttpStatusCode");
        if (property != null && property.GetValue(ex) is int statusCode)
        {
            return statusCode;
        }
        return 0;
    }

    private bool IsTransientHttpStatusCode(int statusCode)
    {
        // HTTP status codes that indicate transient failures
        return statusCode == 408 || // Request Timeout
               statusCode == 429 || // Too Many Requests
               statusCode == 500 || // Internal Server Error
               statusCode == 502 || // Bad Gateway
               statusCode == 503 || // Service Unavailable
               statusCode == 504;   // Gateway Timeout
    }
}

/// <summary>
/// Exception thrown when resilience policy fails.
/// </summary>
public class ResilienceException : Exception
{
    public ResilienceException(string message) : base(message) { }
    public ResilienceException(string message, Exception innerException) : base(message, innerException) { }
}
