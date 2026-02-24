namespace Phase3.AzureDevOps.Interfaces;

using System.Diagnostics;

/// <summary>
/// Provides telemetry operations for observability.
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Records a custom metric.
    /// </summary>
    /// <param name="name">Metric name.</param>
    /// <param name="value">Metric value.</param>
    /// <param name="tags">Optional tags.</param>
    void RecordMetric(string name, double value, IDictionary<string, object>? tags = null);

    /// <summary>
    /// Increments a counter metric.
    /// </summary>
    /// <param name="name">Counter name.</param>
    /// <param name="increment">Increment value (default 1).</param>
    /// <param name="tags">Optional tags.</param>
    void IncrementCounter(string name, long increment = 1, IDictionary<string, object>? tags = null);

    /// <summary>
    /// Records a histogram value.
    /// </summary>
    /// <param name="name">Histogram name.</param>
    /// <param name="value">Value to record.</param>
    /// <param name="tags">Optional tags.</param>
    void RecordHistogram(string name, double value, IDictionary<string, object>? tags = null);

    /// <summary>
    /// Sets a gauge value.
    /// </summary>
    /// <param name="name">Gauge name.</param>
    /// <param name="value">Gauge value.</param>
    /// <param name="tags">Optional tags.</param>
    void SetGauge(string name, double value, IDictionary<string, object>? tags = null);

    /// <summary>
    /// Starts a distributed trace activity.
    /// </summary>
    /// <param name="name">Activity name.</param>
    /// <param name="kind">Activity kind.</param>
    /// <returns>Activity instance.</returns>
    Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal);

    /// <summary>
    /// Records an exception with context.
    /// </summary>
    /// <param name="exception">Exception to record.</param>
    /// <param name="tags">Optional tags.</param>
    void RecordException(Exception exception, IDictionary<string, object>? tags = null);

    /// <summary>
    /// Adds a tag to the current activity.
    /// </summary>
    /// <param name="key">Tag key.</param>
    /// <param name="value">Tag value.</param>
    void AddTag(string key, object value);

    /// <summary>
    /// Adds an event to the current activity.
    /// </summary>
    /// <param name="name">Event name.</param>
    /// <param name="tags">Optional tags.</param>
    void AddEvent(string name, IDictionary<string, object>? tags = null);
}
