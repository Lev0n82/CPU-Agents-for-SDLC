using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Phase3.AzureDevOps.Telemetry;

public static class TelemetryUtilities
{
    // Meter for application metrics
    public static Meter Meter = new Meter("AzureDevOpsAgent", "1.0.0");

    // Counters
    public static Counter<int> WorkItemsProcessedCounter = Meter.CreateCounter<int>(
        "workitems.processed", 
        "items", 
        "Number of work items processed");

    public static Counter<int> AuthenticationAttemptsCounter = Meter.CreateCounter<int>(
        "authentication.attempts", 
        "attempts", 
        "Number of authentication attempts");

    public static Counter<int> ApiCallsCounter = Meter.CreateCounter<int>(
        "api.calls", 
        "calls", 
        "Number of API calls made");

    // Gauges
    public static ObservableGauge<int> ActiveWorkItemsGauge = Meter.CreateObservableGauge<int>(
        "workitems.active", 
        () => GetActiveWorkItemsCount(), 
        "items", 
        "Number of active work items");

    public static ObservableGauge<double> MemoryUsageGauge = Meter.CreateObservableGauge<double>(
        "memory.usage", 
        () => GetMemoryUsage(), 
        "MB", 
        "Current memory usage");

    // Histograms
    public static Histogram<double> ApiResponseTimeHistogram = Meter.CreateHistogram<double>(
        "api.response_time", 
        "ms", 
        "API response times");

    private static int GetActiveWorkItemsCount()
    {
        // Implementation would track active work items
        return 0; // Placeholder
    }

    private static double GetMemoryUsage()
    {
        using var process = Process.GetCurrentProcess();
        return process.WorkingSet64 / 1024.0 / 1024.0;
    }

    // Activity helpers
    public static ActivitySource ActivitySource = new ActivitySource("AzureDevOpsAgent");

    public static Activity? StartWorkItemProcessingActivity(string workItemId)
    {
        return ActivitySource.StartActivity("process.workitem", ActivityKind.Server, workItemId);
    }

    public static Activity? StartAuthenticationActivity(string method)
    {
        return ActivitySource.StartActivity("authenticate", ActivityKind.Client, method);
    }

    public static Activity? StartApiCallActivity(string operation)
    {
        return ActivitySource.StartActivity("api.call", ActivityKind.Client, operation);
    }
}