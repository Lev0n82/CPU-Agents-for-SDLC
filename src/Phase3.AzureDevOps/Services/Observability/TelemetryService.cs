namespace Phase3.AzureDevOps.Services.Observability;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Phase3.AzureDevOps.Interfaces;
using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <summary>
/// Implements OpenTelemetry-based telemetry service.
/// </summary>
public class TelemetryService : ITelemetryService
{
    private readonly ILogger<TelemetryService> _logger;
    private readonly Meter _meter;
    private readonly ActivitySource _activitySource;
    private readonly Dictionary<string, Counter<long>> _counters = new();
    private readonly Dictionary<string, Histogram<double>> _histograms = new();
    private readonly Dictionary<string, double> _gaugeValues = new();

    public TelemetryService(
        ILogger<TelemetryService> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var serviceName = configuration["Telemetry:ServiceName"] ?? "cpu-agents";
        var serviceVersion = configuration["Telemetry:ServiceVersion"] ?? "3.0.0";

        _meter = new Meter(serviceName, serviceVersion);
        _activitySource = new ActivitySource(serviceName, serviceVersion);

        InitializeStandardMetrics();
    }

    public void RecordMetric(string name, double value, IDictionary<string, object>? tags = null)
    {
        _logger.LogDebug("Recording metric {MetricName}: {Value}", name, value);
        
        // Record as histogram by default
        RecordHistogram(name, value, tags);
    }

    public void IncrementCounter(string name, long increment = 1, IDictionary<string, object>? tags = null)
    {
        if (!_counters.TryGetValue(name, out var counter))
        {
            counter = _meter.CreateCounter<long>(name);
            _counters[name] = counter;
        }

        var tagList = ConvertToTagList(tags);
        counter.Add(increment, tagList);

        _logger.LogDebug("Incremented counter {CounterName} by {Increment}", name, increment);
    }

    public void RecordHistogram(string name, double value, IDictionary<string, object>? tags = null)
    {
        if (!_histograms.TryGetValue(name, out var histogram))
        {
            histogram = _meter.CreateHistogram<double>(name);
            _histograms[name] = histogram;
        }

        var tagList = ConvertToTagList(tags);
        histogram.Record(value, tagList);

        _logger.LogDebug("Recorded histogram {HistogramName}: {Value}", name, value);
    }

    public void SetGauge(string name, double value, IDictionary<string, object>? tags = null)
    {
        // Store gauge value for observable gauge
        _gaugeValues[name] = value;

        // Create observable gauge if it doesn't exist
        if (!_gaugeValues.ContainsKey($"{name}_registered"))
        {
            _meter.CreateObservableGauge<double>(name, () => 
            {
                return _gaugeValues.TryGetValue(name, out var val) ? val : 0;
            });
            _gaugeValues[$"{name}_registered"] = 1;
        }

        _logger.LogDebug("Set gauge {GaugeName}: {Value}", name, value);
    }

    public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var activity = _activitySource.StartActivity(name, kind);
        
        if (activity != null)
        {
            _logger.LogDebug("Started activity {ActivityName} with TraceId {TraceId}", 
                name, activity.TraceId);
        }

        return activity;
    }

    public void RecordException(Exception exception, IDictionary<string, object>? tags = null)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.RecordException(exception);
            
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity.SetTag(tag.Key, tag.Value);
                }
            }
        }

        _logger.LogError(exception, "Exception recorded in telemetry");
    }

    public void AddTag(string key, object value)
    {
        var activity = Activity.Current;
        activity?.SetTag(key, value);
    }

    public void AddEvent(string name, IDictionary<string, object>? tags = null)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            var activityEvent = new ActivityEvent(name);
            if (tags != null)
            {
                var tagList = new ActivityTagsCollection();
                foreach (var tag in tags)
                {
                    tagList.Add(tag.Key, tag.Value);
                }
                activityEvent = new ActivityEvent(name, tags: tagList);
            }
            activity.AddEvent(activityEvent);
        }
    }

    private void InitializeStandardMetrics()
    {
        // Pre-create standard metrics
        IncrementCounter("autonomous_agent.work_items.processed", 0);
        RecordHistogram("autonomous_agent.api_calls.duration", 0);
        SetGauge("autonomous_agent.cache.hit_rate", 0);
        IncrementCounter("autonomous_agent.conflicts.detected", 0);
        RecordHistogram("autonomous_agent.sync.duration", 0);
    }

    private TagList ConvertToTagList(IDictionary<string, object>? tags)
    {
        var tagList = new TagList();
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                tagList.Add(tag.Key, tag.Value);
            }
        }
        return tagList;
    }
}
