# Rituals & Cadence Scheduling — AUTONOMOUS.ML Agent Host

**Document Version:** 1.0  
**Status:** Architecture Design  
**Repository:** CPU-Agents-for-SDLC

---

## Overview

In an autonomous ecosystem like AUTONOMOUS.ML, scheduling is a balance between **Rigid Orchestration** (the clock) and **Dynamic Autonomy** (the trigger). This document defines the Hybrid Schedule Store architecture, the `appsettings.json` schema for static pulse intervals, the database schema for dynamic rituals, Quartz.NET integration patterns, and the opportunistic scheduling algorithm that allows agents to act on resource windows rather than waiting for the next clock tick.

The Omni Command Center (OCC) serves as the single configuration surface for all scheduling concerns. Changes made in the OCC **Rituals & Cadence** panel are propagated to the Agent Host via the WebSocket control bus, updating Quartz.NET job triggers in real time without requiring a service restart.

---

## 1. Architecture: The Hybrid Schedule Store

The Agent Host uses two complementary layers to define when work happens.

### 1.1 Static Layer — `appsettings.json`

The static layer defines the **Heartbeat**: how often the agent wakes up to poll for new work. This is a low-frequency background tick that drives the main polling loop. It is intentionally simple and does not change at runtime.

```json
{
  "AgentHost": {
    "Heartbeat": {
      "IntervalSeconds": 60,
      "JitterSeconds": 5,
      "Description": "How often the agent wakes up to check for any available work. Jitter prevents thundering herd across distributed nodes."
    },
    "Scheduling": {
      "Engine": "Quartz.NET",
      "PersistenceMode": "Database",
      "MisfireThresholdSeconds": 300,
      "MaxConcurrentJobs": 4,
      "TimeZone": "UTC"
    }
  }
}
```

The `JitterSeconds` field adds a random offset (±N seconds) to the heartbeat interval, preventing multiple distributed agent nodes from waking simultaneously and creating load spikes on Azure DevOps APIs.

### 1.2 Dynamic Layer — Database (Rituals Table)

The dynamic layer stores **Rituals**: named, scheduled interventions with full cron expressions, context payloads, and execution history. Rituals are stored in the Agent Host's SQL Server / PostgreSQL database and loaded by Quartz.NET at startup, then updated in real time via the OCC.

#### Rituals Table Schema

```sql
CREATE TABLE AgentRituals (
    Id              UNIQUEIDENTIFIER    PRIMARY KEY DEFAULT NEWID(),
    Name            NVARCHAR(200)       NOT NULL,
    Description     NVARCHAR(1000)      NULL,
    CronExpression  NVARCHAR(100)       NOT NULL,
    TaskType        NVARCHAR(50)        NOT NULL,   -- DefectTriage | RequirementReview | CodeFeedback | Maintenance | Custom
    TriggerMode     NVARCHAR(20)        NOT NULL DEFAULT 'Scheduled',  -- Scheduled | EventDriven | Opportunistic
    IsEnabled       BIT                 NOT NULL DEFAULT 1,
    ContextJson     NVARCHAR(MAX)       NULL,       -- JSON payload passed to the job at execution time
    ConfidenceFloor DECIMAL(4,3)        NOT NULL DEFAULT 0.85,
    LastRunAt       DATETIMEOFFSET      NULL,
    NextRunAt       DATETIMEOFFSET      NULL,
    LastRunStatus   NVARCHAR(20)        NULL,       -- Completed | Failed | Skipped | Running
    LastRunDurationMs BIGINT            NULL,
    CreatedAt       DATETIMEOFFSET      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIMEOFFSET      NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy       NVARCHAR(100)       NOT NULL DEFAULT 'system'
);

CREATE INDEX IX_AgentRituals_TaskType ON AgentRituals(TaskType);
CREATE INDEX IX_AgentRituals_IsEnabled ON AgentRituals(IsEnabled);
```

---

## 2. Recommended Scheduling Strategy by Task Type

The following table defines the recommended frequency, placement, and trigger logic for each of the four core task categories.

| Task Type | Recommended Frequency | Placement | Trigger Logic |
|---|---|---|---|
| **Defect Triage** | Daily at 08:00 local | Agent Host (Quartz.NET Cron) | "Morning Sync" — agent gathers all overnight bugs, categorises by severity, and presents a Triage Summary card in the OCC Interventions panel. |
| **Requirement Review** | Continuous / Event-driven | Azure DevOps Webhook | Triggered whenever a User Story moves to "Proposed" or "Ready for Review." The agent does not wait for a schedule; it responds within 30 seconds of the state change. |
| **Code / Test Feedback** | Weekly (Friday 16:00) | OCC Control Panel Ritual | "Weekly Health Audit" — Omni analyses the delta between code merged and tests covered during the sprint, flagging gaps for the Monday sprint start. |
| **System Maintenance** | Monthly (1st Sunday 02:00) | Distributed Network Hub | "Cleanup Cycle" — pruning old test containers, rotating API keys, archiving stale work items, and running model performance benchmarks. |

### Pre-Built Ritual Templates (JSON)

The following templates are shipped with the Agent Host and can be imported directly into the Rituals table or activated via the OCC.

```json
[
  {
    "name": "Daily Defect Triage",
    "description": "Gather all overnight defects, categorise by severity, and surface a Triage Summary card in the OCC.",
    "cronExpression": "0 0 8 * * MON-FRI",
    "taskType": "DefectTriage",
    "triggerMode": "Scheduled",
    "isEnabled": true,
    "confidenceFloor": 0.85,
    "contextJson": {
      "severityFilter": ["Critical", "High"],
      "maxItems": 50,
      "summaryFormat": "OCC_CARD"
    }
  },
  {
    "name": "Weekly Code Quality Health Audit",
    "description": "Analyse code-to-test coverage delta for the current sprint and flag gaps before Monday standup.",
    "cronExpression": "0 0 16 * * FRI",
    "taskType": "CodeFeedback",
    "triggerMode": "Scheduled",
    "isEnabled": true,
    "confidenceFloor": 0.80,
    "contextJson": {
      "coverageThreshold": 0.80,
      "reportFormat": "OCC_CARD",
      "includeSecurityScan": true
    }
  },
  {
    "name": "Monthly System Maintenance",
    "description": "Prune test containers, rotate API keys, archive stale work items, and benchmark AI model performance.",
    "cronExpression": "0 0 2 1 * SUN",
    "taskType": "Maintenance",
    "triggerMode": "Opportunistic",
    "isEnabled": true,
    "confidenceFloor": 0.90,
    "contextJson": {
      "pruneContainersOlderThanDays": 30,
      "rotateApiKeys": true,
      "archiveWorkItemsOlderThanDays": 90,
      "runModelBenchmarks": true
    }
  },
  {
    "name": "Requirement Gap Analysis",
    "description": "Daily scan for User Stories missing acceptance criteria or test coverage links.",
    "cronExpression": "0 0 8 * * *",
    "taskType": "RequirementReview",
    "triggerMode": "Scheduled",
    "isEnabled": true,
    "confidenceFloor": 0.88,
    "contextJson": {
      "workItemStates": ["Proposed", "Active"],
      "missingCriteriaAlert": true
    }
  }
]
```

---

## 3. Quartz.NET Integration (.NET 8.0 WorkerService)

### 3.1 Package References

```xml
<PackageReference Include="Quartz" Version="3.13.0" />
<PackageReference Include="Quartz.Extensions.Hosting" Version="3.13.0" />
<PackageReference Include="Quartz.Serialization.Json" Version="3.13.0" />
```

### 3.2 Service Registration

```csharp
// Program.cs
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore(); // Replace with UsePersistentStore for production

    // Load rituals from database on startup
    q.UseJobAutoInterrupt(options => options.DefaultMaxRunTime = TimeSpan.FromMinutes(30));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
    options.AwaitApplicationStarted = true;
});

builder.Services.AddScoped<IRitualScheduler, RitualScheduler>();
```

### 3.3 Ritual Scheduler Service

```csharp
public interface IRitualScheduler
{
    Task LoadRitualsFromDatabaseAsync(CancellationToken ct = default);
    Task UpdateRitualAsync(string ritualId, string cronExpression, CancellationToken ct = default);
    Task EnableRitualAsync(string ritualId, bool enabled, CancellationToken ct = default);
    Task TriggerRitualNowAsync(string ritualId, CancellationToken ct = default);
}

public class RitualScheduler : IRitualScheduler
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IRitualRepository _ritualRepo;
    private readonly IThoughtEmitter _thoughtEmitter;

    public RitualScheduler(
        ISchedulerFactory schedulerFactory,
        IRitualRepository ritualRepo,
        IThoughtEmitter thoughtEmitter)
    {
        _schedulerFactory = schedulerFactory;
        _ritualRepo = ritualRepo;
        _thoughtEmitter = thoughtEmitter;
    }

    public async Task UpdateRitualAsync(string ritualId, string cronExpression, CancellationToken ct = default)
    {
        // 1. Validate cron expression
        if (!CronExpression.IsValidExpression(cronExpression))
            throw new ArgumentException($"Invalid cron expression: {cronExpression}");

        // 2. Update database entry
        await _ritualRepo.UpdateCronExpressionAsync(ritualId, cronExpression, ct);

        // 3. Reschedule job in Quartz.NET
        var scheduler = await _schedulerFactory.GetScheduler(ct);
        var triggerKey = new TriggerKey($"ritual-{ritualId}", "rituals");
        var existingTrigger = await scheduler.GetTrigger(triggerKey, ct);

        if (existingTrigger != null)
        {
            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithCronSchedule(cronExpression, x => x.InTimeZone(TimeZoneInfo.Utc))
                .Build();

            await scheduler.RescheduleJob(triggerKey, newTrigger, ct);
        }

        // 4. Emit thought to Live Monitor
        await _thoughtEmitter.EmitAsync(new AgentThought
        {
            Tag = ThoughtTag.Execute,
            Content = $"I've adjusted ritual '{ritualId}' to schedule: {cronExpression}.",
            Confidence = 1.0
        }, ct);
    }
}
```

### 3.4 OCC tRPC Procedure (Server-Side)

```typescript
// server/routers/omni.ts
updateRitual: protectedProcedure
  .input(z.object({
    ritualId: z.string().uuid(),
    cronExpression: z.string(),
    isEnabled: z.boolean().optional(),
    contextJson: z.record(z.unknown()).optional(),
  }))
  .mutation(async ({ input, ctx }) => {
    // Validate cron expression
    if (!isValidCron(input.cronExpression)) {
      throw new TRPCError({ code: 'BAD_REQUEST', message: 'Invalid cron expression' });
    }

    // Persist to database
    await db.updateRitual(input.ritualId, {
      cronExpression: input.cronExpression,
      isEnabled: input.isEnabled,
      contextJson: input.contextJson,
      updatedAt: new Date(),
    });

    // Push to Agent Host via WebSocket control bus
    agentControlBus.emit('ritual:update', {
      ritualId: input.ritualId,
      cronExpression: input.cronExpression,
      isEnabled: input.isEnabled,
    });

    return { success: true };
  }),
```

---

## 4. Opportunistic Scheduling Algorithm

A truly autonomous agent should not merely follow a clock. When system resources are idle, the Agent Host should move scheduled rituals forward to take advantage of the quiet window.

### 4.1 Resource Monitor

```csharp
public class OpportunisticScheduler : BackgroundService
{
    private const double IdleThreshold = 0.05; // 5% CPU usage triggers opportunistic window
    private const int CheckIntervalSeconds = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var cpuUsage = await GetSystemCpuUsageAsync();
            var networkPressure = await GetNetworkPressureAsync();

            if (cpuUsage < IdleThreshold && networkPressure < 0.20)
            {
                await TryAccelerateOpportunisticRitualsAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), stoppingToken);
        }
    }

    private async Task TryAccelerateOpportunisticRitualsAsync(CancellationToken ct)
    {
        var opportunisticRituals = await _ritualRepo
            .GetRitualsAsync(r => r.TriggerMode == "Opportunistic" && r.IsEnabled, ct);

        foreach (var ritual in opportunisticRituals)
        {
            var timeUntilNext = ritual.NextRunAt - DateTimeOffset.UtcNow;

            // If the ritual is within 3 days of its scheduled time, run it now
            if (timeUntilNext <= TimeSpan.FromDays(3))
            {
                await _thoughtEmitter.EmitAsync(new AgentThought
                {
                    Tag = ThoughtTag.Plan,
                    Content = $"[OPPORTUNISTIC] System load is < 5%. Initiating '{ritual.Name}' " +
                              $"{timeUntilNext.Days} days early to optimise resource window.",
                    Confidence = 0.95
                }, ct);

                await _ritualScheduler.TriggerRitualNowAsync(ritual.Id.ToString(), ct);
            }
        }
    }
}
```

### 4.2 Opportunistic Scheduling Decision Matrix

| System CPU | Network Pressure | Action |
|---|---|---|
| < 5% | < 20% | Accelerate all Opportunistic rituals within 3-day window |
| 5–30% | < 40% | Accelerate only Critical-priority Opportunistic rituals |
| 30–70% | Any | Follow scheduled times; no acceleration |
| > 70% | Any | Defer non-critical rituals; emit warning thought |
| Any | > 80% | Defer all rituals; emit network pressure alert to OCC |

---

## 5. Event-Driven Triggers (Azure DevOps Webhooks)

Requirement Review rituals should not wait for a schedule. The Agent Host exposes an HTTP endpoint that Azure DevOps calls via Service Hook whenever a work item state changes.

```csharp
// WebhookController.cs
[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    [HttpPost("azure-devops/work-item-updated")]
    public async Task<IActionResult> OnWorkItemUpdated(
        [FromBody] AzureDevOpsWebhookPayload payload,
        CancellationToken ct)
    {
        // Validate HMAC signature
        if (!_webhookValidator.IsValid(Request, payload))
            return Unauthorized();

        var newState = payload.Resource?.Fields?["System.State"]?.NewValue;
        var workItemType = payload.Resource?.Fields?["System.WorkItemType"]?.NewValue;

        if (workItemType == "User Story" && newState is "Proposed" or "Ready for Review")
        {
            await _ritualScheduler.TriggerRitualNowAsync("requirement-review", ct);

            await _thoughtEmitter.EmitAsync(new AgentThought
            {
                Tag = ThoughtTag.Observe,
                Content = $"[WEBHOOK] User Story #{payload.Resource?.Id} moved to '{newState}'. " +
                          "Triggering Requirement Gap Analysis immediately.",
                Confidence = 1.0
            }, ct);
        }

        return Ok();
    }
}
```

---

## 6. OCC Control Bus Message Contract

The OCC pushes scheduling changes to the Agent Host over the existing WebSocket control bus using the following message types.

```typescript
// Shared message types (shared/types.ts)
export type RitualUpdateMessage = {
  type: 'ritual:update';
  ritualId: string;
  cronExpression?: string;
  isEnabled?: boolean;
  contextJson?: Record<string, unknown>;
};

export type HeartbeatUpdateMessage = {
  type: 'heartbeat:update';
  intervalSeconds: number;
  jitterSeconds: number;
};

export type RitualTriggerMessage = {
  type: 'ritual:trigger';
  ritualId: string;
  reason: 'manual' | 'opportunistic' | 'emergency';
};
```

---

## 7. Acceptance Criteria

The following criteria must be satisfied before the scheduling system is considered production-ready.

| ID | Criterion | Verification Method |
|---|---|---|
| SC-01 | Heartbeat interval can be changed via OCC without service restart | Integration test: update interval, verify next poll time changes within 5 seconds |
| SC-02 | Ritual cron expression update propagates to Quartz.NET within 10 seconds | Integration test: update cron, verify `NextRunAt` changes in DB and scheduler |
| SC-03 | Opportunistic scheduler triggers rituals when CPU < 5% | Unit test: mock CPU monitor, verify ritual is triggered |
| SC-04 | Azure DevOps webhook triggers Requirement Review within 30 seconds | E2E test: POST mock webhook payload, verify thought emitted |
| SC-05 | Disabled rituals are not executed | Unit test: disable ritual, advance clock, verify no execution |
| SC-06 | Ritual execution history is persisted with duration and status | Integration test: run ritual, verify `LastRunAt`, `LastRunStatus`, `LastRunDurationMs` populated |
| SC-07 | OCC Rituals panel reflects live status (Running / Completed / Scheduled) | UI test: start demo, verify status badges update in real time |
| SC-08 | Invalid cron expressions are rejected with a descriptive error | Unit test: submit invalid cron, verify `BAD_REQUEST` with message |
| SC-09 | Concurrent ritual execution is capped at `MaxConcurrentJobs` | Load test: trigger 10 rituals simultaneously, verify max 4 run concurrently |
| SC-10 | Network pressure > 80% defers all rituals and emits OCC alert | Unit test: mock high network pressure, verify deferral thought emitted |

---

## 8. References

- [Quartz.NET Documentation](https://www.quartz-scheduler.net/documentation/) — Job scheduling library for .NET
- [Azure DevOps Service Hooks](https://learn.microsoft.com/en-us/azure/devops/service-hooks/overview) — Webhook integration for work item state changes
- [Cron Expression Reference](https://www.quartz-scheduler.net/documentation/quartz-3.x/how-tos/crontrigger.html) — Quartz.NET cron syntax (6-field format)
- [OMNI_COMMAND_CENTER_DESIGN.md](./OMNI_COMMAND_CENTER_DESIGN.md) — OCC architecture and integration contracts
