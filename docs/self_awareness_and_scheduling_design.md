# Self-Awareness and Proactive Scheduling System


## 1. Introduction

To elevate the autonomous AI agent from a reactive tool to a truly proactive and self-managing entity, a system for self-awareness and scheduling is required. This system endows the agent with an understanding of its own state and the ability to execute tasks based on a schedule or internal triggers, without direct human command.

This document details the architecture for this system, with a primary focus on implementing a configurable, automated midnight reboot for system maintenance and ensuring a clean operational state. It also lays the groundwork for more advanced self-aware behaviors, enabling the agent to proactively maintain its own health and performance.

### 1.1 Design Goals

-   **Proactive Maintenance:** The agent must be able to perform scheduled maintenance tasks on itself and its host system, such as a nightly reboot.
-   **Configurability:** Core scheduled tasks, like the midnight reboot, must be easily configurable (e.g., enable/disable, change time) without modifying code.
-   **Reliability:** The scheduling mechanism must be robust and ensure that tasks are executed at the correct time.
-   **Self-Awareness:** The agent should monitor its internal state (e.g., uptime, resource consumption, error rates) and have the capability to trigger actions based on this awareness.
-   **Graceful Operation:** Scheduled operations like a reboot must be performed gracefully, ensuring all ongoing tasks are safely terminated and state is saved.

---

## 2. System Architecture

The Self-Awareness and Scheduling System is a background service that runs continuously within the agent's Orchestration Layer. It operates independently of the main task-processing loop but can influence it by queuing tasks or initiating system-level actions.

### 2.1 High-Level Diagram

```
┌────────────────────────┐
│   Agent Configuration  │
│   (appsettings.json)   │
└────────────┬───────────┘
             │ 1. Load Schedule
             ▼
┌────────────────────────┐   2. Monitor Time & Events   ┌────────────────────────┐
│   Scheduling Service   │◄────────────────────────────┤   System Clock &       │
│    (Background Task)   │                             │   Agent State Monitor  │
└────────────┬───────────┘                             └────────────────────────┘
             │ 3. Trigger Task
             │
             ▼
┌────────────────────────┐   4. Execute Action          ┌────────────────────────┐
│      Task Executor     ├─────────────────────────────►│   System Shell / API   │
│                        │                             │ (e.g., shutdown.exe)   │
└────────────────────────┘                             └────────────────────────┘
```

### 2.2 Components

-   **Scheduling Service:** A long-running background service (e.g., a .NET `BackgroundService`) that acts as the heart of the system. It loads the task schedule from the configuration and monitors triggers.
-   **Agent State Monitor:** A component that continuously collects metrics about the agent's health and performance, such as CPU/memory usage, uptime, number of tasks processed, and error rates.
-   **Task Executor:** A component responsible for executing the actions associated with a triggered task. This could range from calling an internal agent function to executing a system-level shell command.
-   **Agent Configuration:** A central configuration file (e.g., `appsettings.json`) where the schedule and self-awareness rules are defined.

---

## 3. Proactive Scheduling

The primary mechanism for proactive behavior is the scheduler, which supports time-based task execution.

### 3.1 Scheduling Mechanism

For a .NET-based agent, a library like **Quartz.NET** [1] provides a powerful and flexible scheduling system. However, for a simple nightly task, a custom implementation using `Task.Delay` within the `BackgroundService` can be sufficient and avoids an external dependency.

**Simplified `BackgroundService` Logic:**

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        var now = DateTime.Now;
        var scheduledTime = DateTime.Today.AddHours(_config.RebootHour);

        if (now > scheduledTime && now < scheduledTime.AddMinutes(1))
        {
            // It's time to run the task
            await _taskExecutor.ExecuteRebootAsync();
        }

        // Wait for 1 minute before checking again
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}
```

### 3.2 Configurable Midnight Reboot

This is the flagship feature of the proactive scheduling system.

**Configuration (`appsettings.json`):**

```json
{
  "Scheduler": {
    "NightlyReboot": {
      "Enabled": true,
      "Hour": 0,
      "Minute": 0
    }
  }
}
```

**Execution Workflow:**

1.  **Trigger:** The `Scheduling Service` detects that the current system time matches the configured reboot time.
2.  **Graceful Shutdown:** The `Task Executor` first initiates a graceful shutdown of the agent itself.
    -   It signals all active modules to stop processing new tasks.
    -   It waits for any in-progress tasks to complete (with a timeout).
    -   It saves any critical state to disk.
    -   It logs that a scheduled reboot is being initiated.
3.  **Execute Reboot Command:** Once the agent is safely shut down, the `Task Executor` executes the system reboot command using the Windows shell.

    ```csharp
    Process.Start("shutdown.exe", "/r /t 0 /f");
    ```
    -   `/r`: Reboot
    -   `/t 0`: No delay
    -   `/f`: Force running applications to close (as a fallback)

4.  **Auto-Restart on Boot:** To ensure the agent starts again after the PC reboots, it must be configured to run on system startup. This is best achieved by installing the agent as a **Windows Service** set to "Automatic" start mode.

---

## 4. Self-Awareness Loop

Beyond simple time-based scheduling, the agent can use its `Agent State Monitor` to make intelligent, proactive decisions.

### 4.1 State Monitoring

The `Agent State Monitor` will track key performance indicators (KPIs):

-   **Resource Usage:** CPU and RAM consumption (e.g., using `System.Diagnostics.PerformanceCounter`).
-   **Uptime:** Time since the agent last started.
-   **Task Throughput:** Number of tasks processed per hour.
-   **Error Rate:** Percentage of tasks that result in failure.
-   **LLM Health:** Average response time and failure rate of the `llama.cpp` engine.

### 4.2 Rule-Based Triggers

The configuration will support simple rule-based triggers for self-aware actions.

**Configuration (`appsettings.json`):**

```json
{
  "SelfAwareness": {
    "Rules": [
      {
        "Condition": "MemoryUsage > 90% for 5m",
        "Action": "TriggerSelfTest",
        "Priority": "Medium"
      },
      {
        "Condition": "ErrorRate > 50% for 1h",
        "Action": "RequestReboot",
        "Priority": "High"
      }
    ]
  }
}
```

**Execution Workflow:**

1.  The `Agent State Monitor` continuously evaluates the rules against the current KPIs.
2.  If a rule's condition is met, it places the corresponding action into a command queue.
3.  The `Task Executor` picks up the action and executes it.
    -   `TriggerSelfTest`: Initiates the full self-testing sequence described in the `self_testing_framework_design.md` document.
    -   `RequestReboot`: Initiates the same graceful reboot procedure as the nightly schedule, but logs a different reason.

This creates a feedback loop where the agent can detect problems and attempt to fix itself, starting with a non-disruptive self-test and escalating to a full reboot if necessary.

---

## 5. References

[1] Quartz.NET. (n.d.). *Quartz.NET - Enterprise Job Scheduler for .NET*. Retrieved from https://www.quartz-scheduler.net/

---

**End of Document**
