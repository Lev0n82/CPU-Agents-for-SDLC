using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AutonomousAgent.Core.Telemetry;

public static class TelemetryUtilities
{
    // Meter for autonomous agent metrics
    public static Meter Meter = new Meter("AutonomousAgent", "1.0.0");

    // Counters
    public static Counter<int> TasksExecutedCounter = Meter.CreateCounter<int>(
        "tasks.executed", 
        "tasks", 
        "Number of tasks executed");

    public static Counter<int> SelfTestsCompletedCounter = Meter.CreateCounter<int>(
        "selftests.completed", 
        "tests", 
        "Number of self-tests completed");

    public static Counter<int> SchedulingIterationsCounter = Meter.CreateCounter<int>(
        "scheduling.iterations", 
        "iterations", 
        "Number of scheduling iterations");

    public static Counter<int> LlmRequestsCounter = Meter.CreateCounter<int>(
        "llm.requests", 
        "requests", 
        "Number of LLM requests made");

    // Gauges
    public static ObservableGauge<int> ActiveTasksGauge = Meter.CreateObservableGauge<int>(
        "tasks.active", 
        () => GetActiveTasksCount(), 
        "tasks", 
        "Number of active tasks");

    public static ObservableGauge<double> MemoryUsageGauge = Meter.CreateObservableGauge<double>(
        "memory.usage", 
        () => GetMemoryUsage(), 
        "MB", 
        "Current memory usage");

    public static ObservableGauge<double> CpuUsageGauge = Meter.CreateObservableGauge<double>(
        "cpu.usage", 
        () => GetCpuUsage(), 
        "%", 
        "Current CPU usage");

    // Histograms
    public static Histogram<double> TaskExecutionTimeHistogram = Meter.CreateHistogram<double>(
        "task.execution_time", 
        "ms", 
        "Task execution times");

    public static Histogram<double> LlmResponseTimeHistogram = Meter.CreateHistogram<double>(
        "llm.response_time", 
        "ms", 
        "LLM response times");

    // Activity helpers
    public static ActivitySource ActivitySource = new ActivitySource("AutonomousAgent");

    public static Activity? StartTaskExecutionActivity(string taskType, string taskId)
    {
        var activity = ActivitySource.StartActivity("execute.task", ActivityKind.Server);
        if (activity != null)
        {
            activity.SetTag("task.type", taskType);
            activity.SetTag("task.id", taskId);
        }
        return activity;
    }

    public static Activity? StartSelfTestActivity(string testName)
    {
        var activity = ActivitySource.StartActivity("execute.selftest", ActivityKind.Internal);
        if (activity != null)
        {
            activity.SetTag("test.name", testName);
        }
        return activity;
    }

    public static Activity? StartSchedulingActivity(string scheduleType)
    {
        var activity = ActivitySource.StartActivity("schedule.tasks", ActivityKind.Internal);
        if (activity != null)
        {
            activity.SetTag("schedule.type", scheduleType);
        }
        return activity;
    }

    public static Activity? StartLlmRequestActivity(string operation)
    {
        var activity = ActivitySource.StartActivity("llm.request", ActivityKind.Client);
        if (activity != null)
        {
            activity.SetTag("llm.operation", operation);
        }
        return activity;
    }

    private static int GetActiveTasksCount()
    {
        // Implementation would track active tasks
        return 0; // Placeholder
    }

    private static double GetMemoryUsage()
    {
        using var process = Process.GetCurrentProcess();
        return process.WorkingSet64 / 1024.0 / 1024.0;
    }

    private static double GetCpuUsage()
    {
        using var process = Process.GetCurrentProcess();
        try
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;
            
            Thread.Sleep(500);
            
            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;
            
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            
            return cpuUsageTotal * 100;
        }
        catch
        {
            return 0.0;
        }
    }
}