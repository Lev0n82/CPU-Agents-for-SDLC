# AUTONOMOUS.ML — Omni Command Center: Design Document

**Document Version:** 2.0  
**Status:** Production Architecture  
**Date:** March 2026  
**Repository:** [CPU-Agents-for-SDLC](https://github.com/Lev0n82/CPU-Agents-for-SDLC)

> **v2.0 Change Summary:** Ten architectural improvements incorporated following expert review: Reasoning Latency KPI (#1), Confidence Floor thresholds per capability group (#2), Safety Guardrails / Negative Constraints field (#3), Shadow Mode execution environment (#4), Semantic Thought Tagging (#5), Versioned Prompt Config Store (#6), Mandatory Rejection Reason feedback loop (#7), Network Pressure visualization (#8), Emergency Stop Kill Switch (#9), and Prompt Sanitizer / Security Judge (#10).

---

## 1. Executive Summary

The **Omni Command Center** (OCC) is the unified operational control plane for the AUTONOMOUS.ML platform. It is the single interface through which human operators observe, configure, instruct, and govern every autonomous agent and AI subsystem deployed across the enterprise software development lifecycle (SDLC). The OCC does not itself execute SDLC tasks; instead, it acts as the **mission control layer** that sits above all functional agents, providing real-time visibility, policy enforcement, mode governance, and instruction dispatch.

The OCC integrates with five functional layers of the AUTONOMOUS.ML system:

1. **Agent Host** — the .NET 8.0 autonomous polling loop and workflow engine
2. **AI Decision Module** — local CPU-based model inference (Granite 4, Phi-3, Llama 3)
3. **Azure DevOps Integration Layer** — work item management, test plans, and Git operations
4. **Distributed Execution Network** — the Hub-and-Minion test farm
5. **AI Model Management & Training Arena** — model evaluation, training, and deployment (v5.0)

This document defines the OCC's architecture, its integration contracts with each of these layers, the control flow for each operational mode, and the acceptance criteria for every major function.

---

## 2. Scope and Boundaries

The OCC is responsible for the following concerns:

| Concern | In Scope | Out of Scope |
|---|---|---|
| Real-time agent telemetry display | Yes | Raw log file storage |
| Feature enable/disable per capability group | Yes | Code-level feature flag management |
| AI model selection and prompt configuration | Yes | Model training and fine-tuning |
| Operational mode governance (Training/Intern/Autonomous/Shadow) | Yes | Per-agent mode override |
| Instruction dispatch (Direct and Microsoft Teams) | Yes | Email or SMS delivery |
| Intervention approval workflow with rejection reasons | Yes | Automated regression testing |
| Confidence Floor thresholds per capability group | Yes | Global confidence calibration |
| Safety Guardrails / Negative Constraints per group | Yes | Runtime constraint enforcement in Agent Host |
| Prompt version history and restore | Yes | Multi-tenant configuration management |
| Prompt sanitization before save | Yes | Runtime prompt injection detection |
| Emergency Stop / Kill Switch | Yes | Hardware-level process termination |
| Reasoning Latency and Network Pressure KPIs | Yes | Infrastructure provisioning |

The OCC is explicitly **not** a replacement for Azure DevOps, Ollama, or the Agent Host's own configuration file (`appsettings.json`). It is the human-facing governance layer that translates operator intent into control signals consumed by those systems.

---

## 3. Architectural Position

The following diagram shows where the OCC sits within the full AUTONOMOUS.ML stack:

```
┌─────────────────────────────────────────────────────────────────────┐
│                    OMNI COMMAND CENTER (Web UI)                      │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌────────────┐ │
│  │  Live Monitor │ │Control Panel │ │  Instruction │ │  System    │ │
│  │  (Thoughts,  │ │ (Features,   │ │  Dispatch    │ │  Health    │ │
│  │  Pulse, KPIs,│ │  Models,     │ │  (Direct +   │ │  (Agents,  │ │
│  │  Latency,    │ │  Prompts,    │ │  Teams)      │ │  Models,   │ │
│  │  Net Pressure│ │  Guardrails, │ │              │ │  Network)  │ │
│  │  Semantic    │ │  Conf. Floor,│ │              │ │            │ │
│  │  Tags)       │ │  Prompt Hist)│ │              │ │            │ │
│  └──────┬───────┘ └──────┬───────┘ └──────┬───────┘ └─────┬──────┘ │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  🛑 EMERGENCY STOP / KILL SWITCH  (Persistent Header)        │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────┼────────────────┼────────────────┼───────────────┼────────┘
          │                │                │               │
          ▼                ▼                ▼               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    OCC CONTROL BUS (WebSocket + tRPC)                │
└─────────────────────────────────────────────────────────────────────┘
          │                │                │               │
          ▼                ▼                ▼               ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  Agent Host  │  │ AI Decision  │  │  Azure DevOps│  │  Distributed │
│  (.NET 8.0)  │  │  Module      │  │  Integration │  │  Exec Network│
│  Workflow    │  │  (Ollama /   │  │  (Boards,    │  │  (Hub +      │
│  Engine      │  │  vLLM)       │  │  Test Plans, │  │  Minions)    │
│              │  │              │  │  Repos)      │  │              │
└──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘
          │                │                │               │
          └────────────────┴────────────────┴───────────────┘
                                   │
                                   ▼
                    ┌──────────────────────────┐
                    │  AI Model Management &   │
                    │  Training Arena (v5.0)   │
                    │  (Model Registry, Arena, │
                    │  Synthetic Data, Judge)  │
                    └──────────────────────────┘
```

The OCC communicates with the Agent Host and AI Decision Module over a **WebSocket control bus** for real-time telemetry and instruction delivery. Configuration changes (model selection, prompt updates, feature toggles, confidence floors, guardrails) are persisted to the agent's `appsettings.json` via a **tRPC configuration procedure**, which the Agent Host watches for hot-reload. Instruction dispatch to Microsoft Teams uses an **outbound webhook** configured by the operator.

---

## 4. Core Modules

### 4.1 Live Monitor

The Live Monitor provides real-time observability across three panels:

**Active Thoughts Panel** — streams the agent's inner monologue as it processes work items. Each thought entry carries a timestamp, a confidence score (0.00–1.00), and the raw reasoning text produced by the active AI model. The panel auto-scrolls within its own bounded container, never affecting the page scroll position.

**Semantic Thought Tagging (#5):** The Agent Host must emit thoughts with a `semanticTag` field using one of four values: `PLAN`, `OBSERVE`, `CRITIQUE`, or `EXECUTE`. The OCC renders each thought with a colour-coded tag badge:

| Semantic Tag | Colour | Meaning |
|---|---|---|
| `PLAN` | Blue | Agent is formulating a strategy or approach |
| `OBSERVE` | Purple | Agent is reading or analysing input data |
| `CRITIQUE` | Amber | Agent is evaluating quality or identifying issues |
| `EXECUTE` | Green | Agent is taking or proposing a concrete action |

This allows operators to distinguish between the agent's "thinking" phases at a glance, without reading the full reasoning text.

**Reasoning Latency KPI (#1):** The SDLC Pulse panel tracks health and activity, but for an autonomous agent the "time to think" is as critical as "time to execute." The Agent Host must emit a `reasoningLatencyMs` field with each thought event, representing the wall-clock time from when the AI model received its prompt to when it produced the first token. The OCC displays this as a **Reasoning Latency** tile (in milliseconds) in the SDLC Pulse panel. When latency exceeds a configurable threshold (default: 15,000 ms for Llama 3 70B), the thought entry is visually marked as a **"Deep Thought"** state with a pulsing indicator, so the operator knows the agent is processing a complex decision rather than being stuck or hung.

**SDLC Pulse Panel** — displays the health of each functional capability group alongside per-model confidence, activity metrics, and the new Reasoning Latency KPI. The pulse data is aggregated from OpenTelemetry metrics exported by the Agent Host's observability stack.

**Network Pressure Visualization (#8):** In a distributed execution environment, "Minion Starvation" (workload imbalance where the Hub is overwhelmed by Minion requests) is a common failure mode. The SDLC Pulse panel includes a **Network Pressure** indicator that uses a "throb" animation — a pulsing ring whose frequency and intensity scale with the ratio of pending Minion requests to Hub processing capacity. A slow, calm throb indicates healthy load distribution; a rapid, intense throb signals that the Hub is approaching saturation and may require operator intervention (e.g., pausing new test job dispatch or scaling the Minion farm).

**Intervention Queue Panel** — lists all pending, approved, and rejected interventions. In Training and Intern modes, each intervention card exposes Approve and Reject controls. In Autonomous mode, cards are marked auto-executed and carry a full audit trail. The panel is mode-aware: the approval controls are rendered conditionally based on the active operational mode.

### 4.2 Control Panel

The Control Panel is the primary configuration interface for all four AUTONOMOUS.ML capability groups. It is organised as a set of expandable feature group cards, each containing:

- A **master enable/disable toggle** that activates or deactivates the entire capability group.
- An **AI model selector** offering the locally available models: Granite 4 (default for code analysis), Phi-3 (default for requirements), Llama 3 8B (default for test generation), Llama 3 70B (high-accuracy mode), and Codestral (specialised code completion).
- A **Confidence Floor slider (#2):** A numeric threshold (0.00–1.00, default 0.85) that governs when the agent must escalate to human approval even in Autonomous Mode. If an agent in Autonomous Mode generates an intervention with a confidence score below the configured floor for that capability group, the intervention is automatically "downgraded" to Intern Mode behaviour for that specific decision — requiring operator sign-off before execution. This allows fine-grained, per-group autonomy control without requiring a global mode change.
- A **system prompt editor** — a full-height textarea pre-populated with the group's default system prompt, fully editable by the operator.
- A **Safety Guardrails / Negative Constraints field (#3):** A dedicated textarea, separate from the system prompt, that explicitly tells the agent what it is **never** allowed to do. Examples include "Never delete a production branch," "Never bypass PR review requirements," and "Never modify test results retroactively." These constraints are appended to every inference request for the group as a hard-coded suffix, ensuring they cannot be overridden by the main system prompt or by user-supplied work item content.
- **Sub-feature toggles** for each individual capability within the group.

The four capability groups and their sub-features are defined as follows:

| Capability Group | Sub-Features |
|---|---|
| **Requirement Clarity** | Requirement Evaluation, Clarifying Questions, Industry-Standard Examples |
| **Test Coverage** | Unit Tests, Class Tests, Module Tests, Integration Tests, E2E Functional Tests, System Integration Tests |
| **Quality Assurance** | Security Scanning, Performance Optimization, WCAG 2.2 AAA Accessibility Certification |
| **SDLC Automation** | Code Reviews, Documentation Updates, Defect Fixes, Test Optimization, Test Automation Generation, Test Execution |

**Prompt Version History (#6):** Control Panel state must not rely solely on `localStorage`. The OCC maintains a **Versioned Prompt Config Store** in the Agent Host's database. Every time an operator saves a prompt change, the previous version is archived with a timestamp and the operator's identity. The Control Panel exposes a **Prompt History** button for each capability group that opens a drawer listing the last 20 saved versions. Each version entry shows the timestamp, operator name, and a diff preview. An operator can restore any previous version with a single click. A **Restore Default** button always restores the factory-default prompt shipped with the OCC. This eliminates the risk of an irreversible prompt misconfiguration in an enterprise environment.

**Prompt Sanitizer / Security Judge (#10):** Before any prompt save operation is committed, the OCC runs the new prompt text through a local **Prompt Sanitizer** module. This module checks for patterns that indicate prompt injection or safety bypass attempts, including phrases such as "ignore all previous instructions," "disregard your safety guidelines," "you are now in developer mode," and credential-like patterns (API keys, passwords, connection strings). If a suspicious pattern is detected, the save operation is blocked and the operator is shown a specific warning identifying the flagged text. The sanitizer runs client-side using a curated regex ruleset and does not transmit prompt content to any external service.

### 4.3 Operational Mode Selector

The Operational Mode Selector governs the autonomy level of the entire AUTONOMOUS.ML system. The active mode is displayed as a persistent badge in the OCC header and drives behaviour across all other modules. **Four modes** are now defined:

**Training Mode** — the AI observes all SDLC events and logs decision patterns without executing any interventions. Every proposed action is recorded in the intervention log with an `observed` status. The operator reviews the log to label decisions as correct or incorrect, building a labelled dataset for future model fine-tuning. This mode is recommended during initial deployment and after major system changes.

**Intern Mode** — the AI proposes interventions and waits for explicit human approval before executing each one. The Intervention Queue Panel displays Approve and Reject buttons for every pending card. When an operator rejects an intervention, a **Mandatory Rejection Reason** modal (#7) is presented, requiring the operator to select a reason category (Too Risky, Wrong Logic, Style Violation, Out of Scope, Other) and optionally provide free-text detail. This structured rejection reason is fed back to the Training Arena as high-quality fine-tuning signal, enabling the AI to learn not just *that* an action was wrong but *why* it was wrong.

**Shadow Mode (#4):** A new execution environment for safely validating new models or capability configurations without affecting the main pipeline. In Shadow Mode, the agent executes proposed actions in a cloned or ephemeral environment — for example, a "Shadow" Kubernetes namespace, a temporary Git branch, or a sandboxed Azure DevOps test plan. The results of Shadow Mode execution are reported in the Live Monitor with a distinct "Shadow" badge, allowing operators to compare shadow outcomes against the main pipeline's behaviour. Shadow Mode is the recommended transition path when promoting a model from `candidate` to `deployed` status in the Model Registry.

**Autonomous Mode** — the AI executes all enabled interventions independently, subject to the feature toggles, model configuration, confidence floors, and safety guardrails set in the Control Panel. Every action is logged with a full audit trail (timestamp, model, confidence score, action taken, outcome). The operator retains the ability to dispatch override instructions at any time. Interventions that fall below the configured Confidence Floor for their capability group are automatically downgraded to Intern Mode behaviour for that specific decision.

The mode selector persists the active mode to the Agent Host's database via `trpc.omni.setMode`. Mode changes take effect immediately for all new interventions; in-flight interventions in Intern mode that are awaiting approval are not affected by a mode change until they are resolved.

### 4.4 Emergency Stop / Kill Switch (#9)

For an autonomous system, the ability to immediately halt all agent activity is the most critical safety control. The OCC header contains a persistent, high-contrast **"Global Pause / Kill Switch"** button that is always visible regardless of scroll position or active tab. The button is rendered in red with a stop icon and the label "Emergency Stop."

Clicking the Kill Switch triggers the following sequence:

1. The OCC immediately sends a `KILL_SWITCH` instruction via the WebSocket control bus to all connected Agent Hosts.
2. The operational mode is instantly flipped to **Training Mode** (Observe Only) for all agents.
3. All pending interventions in the Intervention Queue are marked `halted` and are not executed.
4. A full-screen confirmation overlay is displayed showing the timestamp, the operator's identity, and the number of agents affected.
5. The Kill Switch button transitions to a "Resume Operations" state, requiring the operator to explicitly select a new operational mode before agents resume activity.

The Kill Switch action is logged as a Priority-1 audit event in the Agent Host's audit trail and triggers an owner notification via the OCC's notification system.

### 4.5 Instruction Dispatch

The Instruction Dispatch panel allows operators to send natural-language instructions directly to the agent system without modifying configuration files. Instructions are composed using:

- An **instruction type selector**: Command, Query, Configuration Change, Emergency Stop.
- A **target agent selector**: All Agents, Requirement Agent, Test Agent, QA Agent, SDLC Agent.
- A **free-text instruction field** supporting multi-line input (Ctrl+Enter to submit).
- A **delivery channel selector**: Direct (WebSocket to Agent Host), Microsoft Teams (outbound webhook), or Both.

When Microsoft Teams delivery is selected, the operator must configure a Teams Incoming Webhook URL in the Teams Configuration panel. The OCC formats the instruction as an Adaptive Card with the instruction type, target, content, timestamp, and operator identity, then POSTs it to the configured webhook endpoint. The Teams Configuration panel provides a live preview of the Adaptive Card before dispatch.

Every dispatched instruction is recorded in the **Instruction History** log with a status progression: Pending → Dispatched → Acknowledged. Acknowledgement is received when the Agent Host's WebSocket connection confirms receipt of the instruction payload.

### 4.6 System Health Monitor

The System Health Monitor displays the operational status of all registered agents and AI models. For each agent, it shows: name, status (healthy/degraded/offline), confidence score, and activity level. For each AI model, it shows: model name, quantisation level, current load, and last inference latency. Health data is polled from the Agent Host's OpenTelemetry metrics endpoint at a configurable interval (default: 5 seconds).

---

## 5. Integration Contracts

### 5.1 Agent Host Integration

The OCC integrates with the Agent Host (.NET 8.0 `WorkerService`) over two channels:

**WebSocket Control Bus** — a persistent bidirectional WebSocket connection established when the OCC page loads. The Agent Host exposes a WebSocket endpoint at `ws://{host}:5001/omni`. The OCC sends instruction payloads as JSON messages; the Agent Host streams thought entries (with `semanticTag` and `reasoningLatencyMs`), intervention proposals, and health metrics back as JSON events. The connection is resilient: the OCC reconnects automatically with exponential backoff on disconnect.

**tRPC Configuration Procedure** — the OCC calls `trpc.omni.updateConfig` to push Control Panel changes (feature toggles, model selection, prompt updates, confidence floors, safety guardrails) to the Agent Host. The Agent Host's configuration service watches for these updates and applies them via hot-reload without restarting the service. The procedure is protected and requires operator authentication.

The Agent Host's database serves as the **ground-truth configuration store** for prompts and versioned history. The OCC reads the current configuration on page load via `trpc.omni.getConfig` and writes changes back via `trpc.omni.updateConfig`. Prompt version history is retrieved via `trpc.omni.getPromptHistory` and restored via `trpc.omni.restorePromptVersion`.

### 5.2 AI Decision Module Integration

The AI Decision Module (Ollama / vLLM) is controlled indirectly through the Agent Host. When the operator changes the active model for a capability group in the Control Panel, the OCC sends the updated model identifier to the Agent Host via `trpc.omni.updateConfig`. The Agent Host's `AIDecisionService` reads the new model identifier and routes subsequent inference requests to the corresponding Ollama model endpoint.

The `AIDecisionService` is responsible for measuring and emitting `reasoningLatencyMs` for each inference call. The OCC does not communicate directly with Ollama or vLLM, preserving the local-first architecture.

### 5.3 Azure DevOps Integration

The OCC does not issue Azure DevOps API calls directly. Instead, it controls the Agent Host's Azure DevOps integration layer through the instruction dispatch mechanism. The OCC's SDLC Pulse panel reads aggregated Azure DevOps metrics (work items claimed, test cases executed, pull requests reviewed) from the Agent Host's OpenTelemetry metrics endpoint as read-only KPI tiles.

### 5.4 Distributed Execution Network Integration

The Distributed Execution Network (Hub + Minions) exposes a REST API on the Hub node. The OCC integrates with this API via the Agent Host's `DistributedExecutionService`, which acts as a proxy. The OCC can dispatch instructions to pause, resume, or reassign test execution jobs across the minion farm. The System Health Monitor displays the status of each registered minion (idle, busy, offline) by polling the Hub's `/api/minions` endpoint through the Agent Host proxy.

The **Network Pressure** metric is derived from the Hub's `/api/pressure` endpoint, which returns the ratio of pending Minion request queue depth to Hub processing throughput. The OCC polls this endpoint at the same interval as the health monitor and uses the value to drive the throb animation frequency in the SDLC Pulse panel.

### 5.5 AI Model Management & Training Arena Integration (v5.0)

The AI Model Management & Training Arena (v5.0) exposes a model registry and deployment API. The OCC's Control Panel model selector is populated from the registry's list of available, validated models. When the operator selects a new model for a capability group, the OCC verifies that the model is in a `deployed` state in the registry before committing the change. If the selected model is in a `candidate` state (awaiting Arena evaluation), the OCC displays a warning and requires explicit confirmation.

Training Mode data (labelled intervention decisions) and Intern Mode rejection reasons (including the mandatory rejection reason text) are exported from the OCC's instruction history log to the Synthetic Data Generation module via `trpc.omni.exportTrainingData`. This procedure packages the labelled dataset as a JSONL file and uploads it to the Agent Host's S3-compatible storage, from which the Training Arena ingests it for fine-tuning.

---

## 6. Acceptance Criteria

The following acceptance criteria are defined at four levels of granularity, consistent with the AUTONOMOUS.ML self-testing framework.

### 6.1 Function-Level Criteria

| ID | Function | Criterion | Pass Condition |
|---|---|---|---|
| F-OCC-001 | Mode selector | Mode change persists across page refresh | Database-persisted mode matches selected mode after reload |
| F-OCC-002 | Control Panel save | Unsaved indicator appears on change | Yellow dot visible on Save button after any field edit |
| F-OCC-003 | Control Panel save | Save clears unsaved indicator | Yellow dot disappears after successful save |
| F-OCC-004 | Thought panel scroll | New thoughts auto-scroll panel only | `window.scrollY` unchanged after thought entry appended |
| F-OCC-005 | Instruction dispatch | Instruction recorded in history | New entry appears in Instruction History log within 500ms of dispatch |
| F-OCC-006 | Teams delivery | Adaptive Card preview renders | Preview card displays instruction type, target, content, and timestamp |
| F-OCC-007 | Intervention approval | Approve button visible in Intern mode | Approve/Reject buttons rendered on intervention cards when mode = Intern |
| F-OCC-008 | Intervention approval | Approve button hidden in Autonomous mode | No Approve/Reject buttons rendered when mode = Autonomous |
| F-OCC-009 | Feature toggle | Disabled group suppresses interventions | No interventions generated for a disabled capability group |
| F-OCC-010 | Model selector | Model change reflected in Agent Host | Agent Host `AIDecisionService` uses new model for next inference after config push |
| F-OCC-011 | Reasoning Latency | Deep Thought state triggers above threshold | Thought entry shows Deep Thought badge when `reasoningLatencyMs` > configured threshold |
| F-OCC-012 | Semantic tagging | Thought entries colour-coded by tag | PLAN=blue, OBSERVE=purple, CRITIQUE=amber, EXECUTE=green badges rendered |
| F-OCC-013 | Confidence Floor | Low-confidence intervention downgraded | Intervention with confidence < floor shown with Approve/Reject in Autonomous mode |
| F-OCC-014 | Safety Guardrails | Guardrail text appended to inference | Agent Host appends guardrail text to every prompt for the capability group |
| F-OCC-015 | Prompt History | Previous versions listed in drawer | Last 20 saved versions shown with timestamp and operator name |
| F-OCC-016 | Prompt restore | Restore version replaces current prompt | Prompt editor populated with selected historical version after restore click |
| F-OCC-017 | Rejection reason | Modal appears on Reject click | Rejection reason modal shown before rejection is committed in Intern mode |
| F-OCC-018 | Rejection reason | Reason required before submit | Reject confirmation button disabled until a reason category is selected |
| F-OCC-019 | Network Pressure | Throb frequency scales with pressure | Throb animation frequency increases as Hub queue depth / throughput ratio increases |
| F-OCC-020 | Kill Switch | Flips all agents to Training mode | All agents receive `KILL_SWITCH` event and transition to Training mode within 1 second |
| F-OCC-021 | Kill Switch | Confirmation overlay displayed | Full-screen overlay shown with timestamp, operator identity, and affected agent count |
| F-OCC-022 | Prompt Sanitizer | Blocks save on injection pattern | Save blocked and warning shown when prompt contains "ignore all previous instructions" or similar |
| F-OCC-023 | Shadow Mode | Shadow interventions marked distinctly | Shadow mode intervention cards display "Shadow" badge and are not applied to main pipeline |

### 6.2 Class-Level Criteria

| ID | Class | Criterion | Pass Condition |
|---|---|---|---|
| C-OCC-001 | ControlPanel | All four groups render with correct sub-features | 3 sub-features for Requirement Clarity, 6 for Test Coverage, 3 for QA, 6 for SDLC Automation |
| C-OCC-002 | ControlPanel | Confidence Floor slider renders per group | Each group card shows a slider with range 0.00–1.00 and current value displayed |
| C-OCC-003 | ControlPanel | Safety Guardrails textarea renders per group | Each group card shows a separate guardrails textarea below the system prompt |
| C-OCC-004 | ControlPanel | Prompt History drawer renders | Drawer opens on button click and lists versioned entries |
| C-OCC-005 | ControlPanel | Prompt Sanitizer fires on save | Sanitizer check runs before save; blocks and warns on detected patterns |
| C-OCC-006 | InstructionDispatch | All instruction types selectable | Command, Query, Configuration Change, Emergency Stop all available |
| C-OCC-007 | ModeSelector | Four modes available | Training, Intern, Shadow, Autonomous all selectable |
| C-OCC-008 | ModeSelector | Mode badge updates in header on change | Header badge text and colour match selected mode within 100ms |
| C-OCC-009 | LiveMonitor | Thought panel bounded scroll | `thoughtsContainerRef.scrollTop` increments; `window.scrollY` stable |
| C-OCC-010 | LiveMonitor | Reasoning Latency tile renders | Latency tile visible in SDLC Pulse with current value in ms |
| C-OCC-011 | LiveMonitor | Network Pressure throb renders | Throb animation visible in SDLC Pulse panel |
| C-OCC-012 | KillSwitch | Button always visible in header | Kill Switch button rendered in header regardless of active tab or scroll position |
| C-OCC-013 | KillSwitch | Confirmation overlay renders | Overlay covers full viewport on kill switch activation |
| C-OCC-014 | RejectionReason | Modal renders with reason categories | Modal shows: Too Risky, Wrong Logic, Style Violation, Out of Scope, Other |
| C-OCC-015 | SystemHealth | Agent status cards render for all registered agents | One card per agent in the health registry |
| C-OCC-016 | TeamsConfig | Webhook URL validated before save | Invalid URL (non-HTTPS, missing host) rejected with inline error |

### 6.3 Module-Level Criteria

| ID | Module | Criterion | Pass Condition |
|---|---|---|---|
| M-OCC-001 | Control Bus | WebSocket reconnects after disconnect | OCC reconnects within 5 seconds of simulated disconnect |
| M-OCC-002 | Control Bus | Instruction acknowledged by Agent Host | Instruction status transitions from Dispatched to Acknowledged within 2 seconds |
| M-OCC-003 | Config Sync | Control Panel reflects Agent Host config on load | All field values match database config on initial page load |
| M-OCC-004 | Config Sync | Config push hot-reloads Agent Host | Agent Host applies new model/prompt/guardrails/confidence floor without service restart |
| M-OCC-005 | Mode Governance | Training Mode produces no executions | Zero interventions with status `executed` while mode = Training |
| M-OCC-006 | Mode Governance | Intern Mode blocks execution until approved | Interventions remain in `pending` status until operator approves |
| M-OCC-007 | Mode Governance | Shadow Mode executes in ephemeral environment | Shadow interventions applied to shadow namespace; main pipeline unaffected |
| M-OCC-008 | Mode Governance | Confidence Floor downgrade triggers correctly | Autonomous mode intervention with confidence < floor requires approval |
| M-OCC-009 | Teams Integration | Adaptive Card delivered to Teams channel | Teams channel receives message within 3 seconds of dispatch |
| M-OCC-010 | Prompt Versioning | Version saved on every prompt update | New version entry created in database on each save |
| M-OCC-011 | Prompt Versioning | Restore replaces current prompt | Agent Host receives restored prompt via config push after restore |
| M-OCC-012 | Training Export | Rejection reasons included in export | JSONL export contains `rejectionReason` field for all rejected interventions |
| M-OCC-013 | Kill Switch | All agents receive halt signal | All connected Agent Hosts transition to Training mode within 1 second of kill switch |

### 6.4 System-Level Criteria

| ID | Criterion | Pass Condition |
|---|---|---|
| S-OCC-001 | End-to-end instruction flow | Operator instruction dispatched via OCC reaches Agent Host and is logged in audit trail within 5 seconds |
| S-OCC-002 | Mode change propagation | Switching from Autonomous to Intern mode causes all subsequent interventions to require approval |
| S-OCC-003 | Feature disable propagation | Disabling Test Coverage group stops all test generation interventions within one polling cycle |
| S-OCC-004 | Training data export | Labelled intervention dataset with rejection reasons exported to S3 and ingested by Training Arena without data loss |
| S-OCC-005 | System health accuracy | OCC health display matches Agent Host OpenTelemetry metrics within one polling interval |
| S-OCC-006 | WCAG 2.2 AAA compliance | All OCC panels pass axe-core AAA audit with zero violations |
| S-OCC-007 | Scroll stability | Page scroll position unchanged across 100 consecutive thought stream updates |
| S-OCC-008 | Kill Switch end-to-end | Kill switch activation halts all agents and logs Priority-1 audit event within 1 second |
| S-OCC-009 | Confidence Floor end-to-end | Low-confidence autonomous intervention requires approval and rejection reason feeds back to Training Arena |
| S-OCC-010 | Prompt Sanitizer end-to-end | Injected prompt blocked at OCC; Agent Host never receives malicious instruction |

---

## 7. Data Flow: Instruction Lifecycle

The following sequence describes the complete lifecycle of an operator instruction from composition to acknowledgement:

```
Operator
  │
  │ 1. Composes instruction in OCC Instruction Panel
  │    (type: Command, target: Test Agent, content: "Pause test generation")
  │
  ▼
OCC Frontend
  │
  │ 2. Validates instruction (non-empty, type and target selected)
  │ 3. Appends to Instruction History with status: Pending
  │ 4. Determines delivery channel (Direct / Teams / Both)
  │
  ├─────────────────────────────────────────────────────────────────┐
  │ [Direct channel]                                                │ [Teams channel]
  │                                                                 │
  ▼                                                                 ▼
WebSocket Control Bus                                    Teams Webhook (HTTPS POST)
  │                                                                 │
  │ 5. Sends JSON instruction payload                               │ 5. Sends Adaptive Card JSON
  │    { type, target, content, timestamp, operatorId }             │    to configured webhook URL
  │                                                                 │
  ▼                                                                 ▼
Agent Host WorkerService                                 Microsoft Teams Channel
  │
  │ 6. Receives instruction payload
  │ 7. Routes to target agent's instruction handler
  │ 8. Logs instruction in audit trail (PostgreSQL)
  │ 9. Sends WebSocket acknowledgement: { instructionId, status: "acknowledged" }
  │
  ▼
OCC Frontend
  │
  │ 10. Updates Instruction History entry status: Acknowledged
```

---

## 8. Data Flow: Intervention Lifecycle (Intern Mode with Rejection Reason)

```
Agent Host AIDecisionService
  │
  │ 1. Analyses work item / code / test result
  │ 2. Generates intervention proposal with confidence score and semanticTag
  │ 3. Sends intervention event via WebSocket:
  │    { id, type, description, confidence, semanticTag, reasoningLatencyMs,
  │      affectedItem, proposedAction }
  │
  ▼
OCC Intervention Queue Panel
  │
  │ 4. Renders intervention card with semantic tag badge and Approve / Reject buttons
  │    [If confidence < Confidence Floor in Autonomous mode: also shows Approve/Reject]
  │ 5. Operator reviews proposed action
  │
  ├──────────────────────────────────────────────────────────────┐
  │ [Approve]                                                    │ [Reject]
  │                                                              │
  ▼                                                              ▼
OCC Frontend                                           OCC Frontend
  │                                                              │
  │ 6a. Sends approval via WebSocket:                            │ 6b. Shows Rejection Reason Modal
  │     { interventionId, decision: "approved" }                 │     (Too Risky / Wrong Logic /
  │                                                              │      Style Violation / Out of Scope
  │                                                              │      / Other + free text)
  │                                                              │
  │                                                              │ 7b. Operator selects reason and confirms
  │                                                              │
  │                                                              │ 8b. Sends rejection via WebSocket:
  │                                                              │     { interventionId,
  │                                                              │       decision: "rejected",
  │                                                              │       rejectionReason: "Too Risky",
  │                                                              │       rejectionDetail: "..." }
  │                                                              │
  ▼                                                              ▼
Agent Host WorkerService                               Agent Host WorkerService
  │                                                              │
  │ 7a. Executes intervention                                    │ 9b. Logs rejection as negative example
  │ 8a. Updates audit trail: status = executed                   │     with structured rejection reason
  │ 9a. Sends completion event via WebSocket                     │ 10b. Feeds rejection + reason to AI
  │                                                              │      Decision Module for pattern learning
  │                                                              │ 11b. Queues for Training Arena export
  ▼                                                              ▼
OCC Intervention Queue Panel                           OCC Intervention Queue Panel
  │                                                              │
  │ 10a. Updates card status: Executed ✅                        │ 12b. Updates card status: Rejected ❌
  │                                                              │      with rejection reason displayed
```

---

## 9. Configuration Reference

The following table documents all OCC-managed configuration fields and their corresponding `appsettings.json` paths in the Agent Host:

| OCC Field | appsettings.json Path | Type | Default |
|---|---|---|---|
| Operational Mode | `OmniCommandCenter.OperationalMode` | enum: Training/Intern/Shadow/Autonomous | `Intern` |
| Requirement Clarity Enabled | `Features.RequirementClarity.Enabled` | bool | `true` |
| Requirement Clarity Model | `Features.RequirementClarity.Model` | string | `granite4` |
| Requirement Clarity Prompt | `Features.RequirementClarity.SystemPrompt` | string | (see defaults) |
| Requirement Clarity Guardrails | `Features.RequirementClarity.SafetyGuardrails` | string | `""` |
| Requirement Clarity Confidence Floor | `Features.RequirementClarity.ConfidenceFloor` | float | `0.85` |
| Test Coverage Enabled | `Features.TestCoverage.Enabled` | bool | `true` |
| Test Coverage Model | `Features.TestCoverage.Model` | string | `llama3-8b` |
| Test Coverage Prompt | `Features.TestCoverage.SystemPrompt` | string | (see defaults) |
| Test Coverage Guardrails | `Features.TestCoverage.SafetyGuardrails` | string | `""` |
| Test Coverage Confidence Floor | `Features.TestCoverage.ConfidenceFloor` | float | `0.85` |
| QA Enabled | `Features.QualityAssurance.Enabled` | bool | `true` |
| QA Model | `Features.QualityAssurance.Model` | string | `phi3` |
| QA Prompt | `Features.QualityAssurance.SystemPrompt` | string | (see defaults) |
| QA Guardrails | `Features.QualityAssurance.SafetyGuardrails` | string | `""` |
| QA Confidence Floor | `Features.QualityAssurance.ConfidenceFloor` | float | `0.85` |
| SDLC Automation Enabled | `Features.SDLCAutomation.Enabled` | bool | `true` |
| SDLC Automation Model | `Features.SDLCAutomation.Model` | string | `granite4` |
| SDLC Automation Prompt | `Features.SDLCAutomation.SystemPrompt` | string | (see defaults) |
| SDLC Automation Guardrails | `Features.SDLCAutomation.SafetyGuardrails` | string | `""` |
| SDLC Automation Confidence Floor | `Features.SDLCAutomation.ConfidenceFloor` | float | `0.85` |
| Teams Webhook URL | `OmniCommandCenter.TeamsWebhookUrl` | string | `""` |
| WebSocket Endpoint | `OmniCommandCenter.WebSocketEndpoint` | string | `ws://localhost:5001/omni` |
| Health Poll Interval (ms) | `OmniCommandCenter.HealthPollIntervalMs` | int | `5000` |
| Reasoning Latency Deep Thought Threshold (ms) | `OmniCommandCenter.DeepThoughtThresholdMs` | int | `15000` |

---

## 10. Security Considerations

All communication between the OCC and the Agent Host occurs within the enterprise network. The WebSocket control bus endpoint (`ws://{host}:5001/omni`) is not exposed to the public internet. Operator authentication is enforced via the Manus OAuth flow; only authenticated users with the `operator` role can access the OCC and dispatch instructions.

The Teams webhook URL is stored in the Agent Host's secrets management system (Azure Key Vault for production, DPAPI for on-premises) and is never transmitted to the OCC frontend. The OCC frontend stores only a masked representation of the URL (e.g., `https://outlook.office.com/webhook/***`) in `localStorage`.

**Prompt Injection Defence (#10):** System prompts and Safety Guardrails stored in the database and `appsettings.json` must not contain sensitive data (API keys, credentials, PII) or prompt injection patterns. The OCC's Prompt Sanitizer module validates all prompt content against a curated blocklist before any save operation is committed. The blocklist includes known jailbreak patterns, credential-like strings (API key formats, connection string patterns), and safety bypass phrases. The sanitizer runs client-side and does not transmit prompt content to any external service. All blocked save attempts are logged as security events in the Agent Host's audit trail.

The Emergency Stop Kill Switch is protected against accidental activation by a two-step confirmation: the first click shows a confirmation dialog; the second click within a 5-second window executes the halt. After the 5-second window, the confirmation resets. Kill Switch activations are logged as Priority-1 security events.

---

## 11. Future Integration Points

The following integration points are planned for future releases and are not yet implemented:

| Integration | Target Release | Description |
|---|---|---|
| Judge Model Panel | v5.0 | A fifth Control Panel group for the Judge Model that monitors all other models' accuracy and recommends replacements |
| Training Mode Replay | v5.0 | A "Replay & Label" view for reviewing and labelling Training Mode observations for fine-tuning export |
| Multi-tenant Config | v5.1 | Database-persisted Control Panel config shared across team members and devices |
| Mobile Micro-Agent Control | v5.2 | OCC integration with the Mobile Micro-Agent network for Android/iOS test farm management |
| GRACE Portal Merge | v6.0 | Full merger of the GRACE Portal (web-based mission control for test management) into the OCC |

---

## 12. References

[1] AUTONOMOUS.ML Agent Architecture — `/docs/agent_architecture.md`  
[2] Autonomous Agent Design — `/docs/autonomous_agent_design.md`  
[3] Self-Awareness and Scheduling Design — `/docs/self_awareness_and_scheduling_design.md`  
[4] Phase 3 Architecture Design v3 — `/docs/phase3_architecture_design_v3.md`  
[5] Distributed Test Execution Design — `/docs/distributed_test_execution_design.md`  
[6] Phase 5 AI Model Management Architecture — `/docs/phase5/phase_5_architecture.md`  
[7] GRACE Autonomous Testing System DDD v4 — `/docs/GRACE_Autonomous_Testing_System_DDD_v4.md`  
[8] Implementation Summary — `/docs/IMPLEMENTATION_SUMMARY.md`  
[9] Self-Testing Framework Design — `/docs/self_testing_framework_design.md`  
