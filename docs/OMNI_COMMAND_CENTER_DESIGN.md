# AUTONOMOUS.ML — Omni Command Center: Design Document

**Document Version:** 1.0  
**Status:** Production Architecture  
**Author:** Manus AI  
**Date:** March 2026  
**Repository:** [CPU-Agents-for-SDLC](https://github.com/Lev0n82/CPU-Agents-for-SDLC)

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
| Operational mode governance (Training/Intern/Autonomous) | Yes | Per-agent mode override |
| Instruction dispatch (Direct and Microsoft Teams) | Yes | Email or SMS delivery |
| Intervention approval workflow | Yes | Automated regression testing |
| Control Panel state persistence | Yes | Multi-tenant configuration management |
| System health monitoring | Yes | Infrastructure provisioning |

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
│  │  Pulse, KPIs)│ │  Models,     │ │  (Direct +   │ │  (Agents,  │ │
│  │              │ │  Prompts)    │ │  Teams)      │ │  Models)   │ │
│  └──────┬───────┘ └──────┬───────┘ └──────┬───────┘ └─────┬──────┘ │
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

The OCC communicates with the Agent Host and AI Decision Module over a **WebSocket control bus** for real-time telemetry and instruction delivery. Configuration changes (model selection, prompt updates, feature toggles) are persisted to the agent's `appsettings.json` via a **tRPC configuration procedure**, which the Agent Host watches for hot-reload. Instruction dispatch to Microsoft Teams uses an **outbound webhook** configured by the operator.

---

## 4. Core Modules

### 4.1 Live Monitor

The Live Monitor provides real-time observability across three panels:

**Active Thoughts Panel** — streams the agent's inner monologue as it processes work items. Each thought entry carries a timestamp, a confidence score (0.00–1.00), and the raw reasoning text produced by the active AI model. The panel auto-scrolls within its own bounded container, never affecting the page scroll position. Thoughts are sourced from the Agent Host's `WorkerService` via the WebSocket control bus.

**SDLC Pulse Panel** — displays the health of each functional capability group (Requirement Clarity, Test Coverage, Quality Assurance, SDLC Automation) as a live bar chart, alongside per-model confidence and activity metrics. The pulse data is aggregated from OpenTelemetry metrics exported by the Agent Host's observability stack.

**Intervention Queue Panel** — lists all pending, approved, and rejected interventions. In Training and Intern modes, each intervention card exposes Approve and Reject controls. In Autonomous mode, cards are marked auto-executed and carry a full audit trail. The panel is mode-aware: the approval controls are rendered conditionally based on the active operational mode.

### 4.2 Control Panel

The Control Panel is the primary configuration interface for all four AUTONOMOUS.ML capability groups. It is organised as a set of expandable feature group cards, each containing:

- A **master enable/disable toggle** that activates or deactivates the entire capability group.
- An **AI model selector** offering the locally available models: Granite 4 (default for code analysis), Phi-3 (default for requirements), Llama 3 8B (default for test generation), Llama 3 70B (high-accuracy mode), and Codestral (specialised code completion).
- A **system prompt editor** — a full-height textarea pre-populated with the group's default system prompt, fully editable by the operator. Prompt changes are validated for minimum length and saved with an explicit Save button to prevent accidental overwrites.
- **Sub-feature toggles** for each individual capability within the group.

The four capability groups and their sub-features are defined as follows:

| Capability Group | Sub-Features |
|---|---|
| **Requirement Clarity** | Requirement Evaluation, Clarifying Questions, Industry-Standard Examples |
| **Test Coverage** | Unit Tests, Class Tests, Module Tests, Integration Tests, E2E Functional Tests, System Integration Tests |
| **Quality Assurance** | Security Scanning, Performance Optimization, WCAG 2.2 AAA Accessibility Certification |
| **SDLC Automation** | Code Reviews, Documentation Updates, Defect Fixes, Test Optimization, Test Automation Generation, Test Execution |

Control Panel state is persisted to `localStorage` under the key `omni-control-panel-v1`. On first load, defaults are applied: all groups enabled, Granite 4 selected for all groups, and default system prompts loaded from the OCC's static configuration. The unsaved-changes indicator (a yellow dot on the Save button) prevents operators from navigating away without saving.

### 4.3 Operational Mode Selector

The Operational Mode Selector governs the autonomy level of the entire AUTONOMOUS.ML system. The active mode is displayed as a persistent badge in the OCC header and drives behaviour across all other modules. Three modes are defined:

**Training Mode** — the AI observes all SDLC events and logs decision patterns without executing any interventions. Every proposed action is recorded in the intervention log with a `pending-review` status. The operator reviews the log to label decisions as correct or incorrect, building a labelled dataset for future model fine-tuning. This mode is recommended during initial deployment and after major system changes.

**Intern Mode** — the AI proposes interventions and waits for explicit human approval before executing each one. The Intervention Queue Panel displays Approve and Reject buttons for every pending card. Rejected interventions are logged with the operator's implicit reason (the act of rejection) and fed back to the AI Decision Module as negative examples. This mode is recommended for new capability groups or when operating in production environments with low error tolerance.

**Autonomous Mode** — the AI executes all enabled interventions independently, subject only to the feature toggles and model configuration set in the Control Panel. Every action is logged with a full audit trail (timestamp, model, confidence score, action taken, outcome). The operator retains the ability to dispatch override instructions at any time. This mode is recommended for stable, well-validated capability groups in non-production environments.

The mode selector persists the active mode to `localStorage` under the key `omni-operational-mode`. Mode changes take effect immediately for all new interventions; in-flight interventions in Intern mode that are awaiting approval are not affected by a mode change until they are resolved.

### 4.4 Instruction Dispatch

The Instruction Dispatch panel allows operators to send natural-language instructions directly to the agent system without modifying configuration files. Instructions are composed using:

- An **instruction type selector**: Command, Query, Configuration Change, Emergency Stop.
- A **target agent selector**: All Agents, Requirement Agent, Test Agent, QA Agent, SDLC Agent.
- A **free-text instruction field** supporting multi-line input (Ctrl+Enter to submit).
- A **delivery channel selector**: Direct (WebSocket to Agent Host), Microsoft Teams (outbound webhook), or Both.

When Microsoft Teams delivery is selected, the operator must configure a Teams Incoming Webhook URL in the Teams Configuration panel. The OCC formats the instruction as an Adaptive Card with the instruction type, target, content, timestamp, and operator identity, then POSTs it to the configured webhook endpoint. The Teams Configuration panel provides a live preview of the Adaptive Card before dispatch.

Every dispatched instruction is recorded in the **Instruction History** log with a status progression: Pending → Dispatched → Acknowledged. Acknowledgement is received when the Agent Host's WebSocket connection confirms receipt of the instruction payload.

### 4.5 System Health Monitor

The System Health Monitor displays the operational status of all registered agents and AI models. For each agent, it shows: name, status (healthy/degraded/offline), confidence score, and activity level. For each AI model, it shows: model name, quantisation level, current load, and last inference latency. Health data is polled from the Agent Host's OpenTelemetry metrics endpoint at a configurable interval (default: 5 seconds).

---

## 5. Integration Contracts

### 5.1 Agent Host Integration

The OCC integrates with the Agent Host (.NET 8.0 `WorkerService`) over two channels:

**WebSocket Control Bus** — a persistent bidirectional WebSocket connection established when the OCC page loads. The Agent Host exposes a WebSocket endpoint at `ws://{host}:5001/omni`. The OCC sends instruction payloads as JSON messages; the Agent Host streams thought entries, intervention proposals, and health metrics back as JSON events. The connection is resilient: the OCC reconnects automatically with exponential backoff on disconnect.

**tRPC Configuration Procedure** — the OCC calls `trpc.omni.updateConfig` to push Control Panel changes (feature toggles, model selection, prompt updates) to the Agent Host. The Agent Host's configuration service watches for these updates and applies them via hot-reload without restarting the service. The procedure is protected and requires operator authentication.

The Agent Host's `appsettings.json` serves as the ground-truth configuration store. The OCC reads the current configuration on page load via `trpc.omni.getConfig` and writes changes back via `trpc.omni.updateConfig`. This ensures that configuration changes made directly to `appsettings.json` (e.g., during deployment) are reflected in the OCC on next load.

### 5.2 AI Decision Module Integration

The AI Decision Module (Ollama / vLLM) is controlled indirectly through the Agent Host. When the operator changes the active model for a capability group in the Control Panel, the OCC sends the updated model identifier to the Agent Host via `trpc.omni.updateConfig`. The Agent Host's `AIDecisionService` reads the new model identifier and routes subsequent inference requests to the corresponding Ollama model endpoint.

The OCC does not communicate directly with Ollama or vLLM. This design preserves the local-first architecture: all model inference remains within the enterprise network, and the OCC never exposes model endpoints to the browser.

### 5.3 Azure DevOps Integration

The OCC does not issue Azure DevOps API calls directly. Instead, it controls the Agent Host's Azure DevOps integration layer through the instruction dispatch mechanism. For example, an operator instruction of type "Command" targeting "SDLC Agent" with content "Pause work item claiming for the next 2 hours" is received by the Agent Host, which sets an internal flag in its `WorkItemClaimService` to suspend claiming until the specified time.

The OCC's SDLC Pulse panel reads aggregated Azure DevOps metrics (work items claimed, test cases executed, pull requests reviewed) from the Agent Host's OpenTelemetry metrics endpoint. These metrics are displayed as read-only KPI tiles and are not editable from the OCC.

### 5.4 Distributed Execution Network Integration

The Distributed Execution Network (Hub + Minions) exposes a REST API on the Hub node. The OCC integrates with this API via the Agent Host's `DistributedExecutionService`, which acts as a proxy. The OCC can dispatch instructions to pause, resume, or reassign test execution jobs across the minion farm. The System Health Monitor displays the status of each registered minion (idle, busy, offline) by polling the Hub's `/api/minions` endpoint through the Agent Host proxy.

### 5.5 AI Model Management & Training Arena Integration (v5.0)

The AI Model Management & Training Arena (v5.0) exposes a model registry and deployment API. The OCC's Control Panel model selector is populated from the registry's list of available, validated models. When the operator selects a new model for a capability group, the OCC verifies that the model is in a `deployed` state in the registry before committing the change. If the selected model is in a `candidate` state (awaiting Arena evaluation), the OCC displays a warning and requires explicit confirmation.

Training Mode data (labelled intervention decisions) is exported from the OCC's instruction history log to the Synthetic Data Generation module via `trpc.omni.exportTrainingData`. This procedure packages the labelled dataset as a JSONL file and uploads it to the Agent Host's S3-compatible storage, from which the Training Arena ingests it for fine-tuning.

---

## 6. Acceptance Criteria

The following acceptance criteria are defined at four levels of granularity, consistent with the AUTONOMOUS.ML self-testing framework.

### 6.1 Function-Level Criteria

| ID | Function | Criterion | Pass Condition |
|---|---|---|---|
| F-OCC-001 | Mode selector | Mode change persists across page refresh | `localStorage['omni-operational-mode']` matches selected mode after reload |
| F-OCC-002 | Control Panel save | Unsaved indicator appears on change | Yellow dot visible on Save button after any field edit |
| F-OCC-003 | Control Panel save | Save clears unsaved indicator | Yellow dot disappears after successful save |
| F-OCC-004 | Thought panel scroll | New thoughts auto-scroll panel only | `window.scrollY` unchanged after thought entry appended |
| F-OCC-005 | Instruction dispatch | Instruction recorded in history | New entry appears in Instruction History log within 500ms of dispatch |
| F-OCC-006 | Teams delivery | Adaptive Card preview renders | Preview card displays instruction type, target, content, and timestamp |
| F-OCC-007 | Intervention approval | Approve button visible in Intern mode | Approve/Reject buttons rendered on intervention cards when mode = Intern |
| F-OCC-008 | Intervention approval | Approve button hidden in Autonomous mode | No Approve/Reject buttons rendered when mode = Autonomous |
| F-OCC-009 | Feature toggle | Disabled group suppresses interventions | No interventions generated for a disabled capability group |
| F-OCC-010 | Model selector | Model change reflected in Agent Host | Agent Host `AIDecisionService` uses new model for next inference after config push |

### 6.2 Class-Level Criteria

| ID | Class | Criterion | Pass Condition |
|---|---|---|---|
| C-OCC-001 | ControlPanel | All four groups render with correct sub-features | 3 sub-features for Requirement Clarity, 6 for Test Coverage, 3 for QA, 6 for SDLC Automation |
| C-OCC-002 | ControlPanel | localStorage round-trip preserves all fields | Saved config reloaded correctly after page refresh |
| C-OCC-003 | InstructionDispatch | All instruction types selectable | Command, Query, Configuration Change, Emergency Stop all available |
| C-OCC-004 | InstructionDispatch | All target agents selectable | All Agents, Requirement Agent, Test Agent, QA Agent, SDLC Agent all available |
| C-OCC-005 | ModeSelector | Mode badge updates in header on change | Header badge text and colour match selected mode within 100ms |
| C-OCC-006 | LiveMonitor | Thought panel bounded scroll | `thoughtsContainerRef.scrollTop` increments; `window.scrollY` stable |
| C-OCC-007 | SystemHealth | Agent status cards render for all registered agents | One card per agent in the health registry |
| C-OCC-008 | TeamsConfig | Webhook URL validated before save | Invalid URL (non-HTTPS, missing host) rejected with inline error |

### 6.3 Module-Level Criteria

| ID | Module | Criterion | Pass Condition |
|---|---|---|---|
| M-OCC-001 | Control Bus | WebSocket reconnects after disconnect | OCC reconnects within 5 seconds of simulated disconnect |
| M-OCC-002 | Control Bus | Instruction acknowledged by Agent Host | Instruction status transitions from Dispatched to Acknowledged within 2 seconds |
| M-OCC-003 | Config Sync | Control Panel reflects Agent Host config on load | All field values match `appsettings.json` on initial page load |
| M-OCC-004 | Config Sync | Config push hot-reloads Agent Host | Agent Host applies new model/prompt without service restart |
| M-OCC-005 | Mode Governance | Training Mode produces no executions | Zero interventions with status `executed` while mode = Training |
| M-OCC-006 | Mode Governance | Intern Mode blocks execution until approved | Interventions remain in `pending` status until operator approves |
| M-OCC-007 | Teams Integration | Adaptive Card delivered to Teams channel | Teams channel receives message within 3 seconds of dispatch |

### 6.4 System-Level Criteria

| ID | Criterion | Pass Condition |
|---|---|---|
| S-OCC-001 | End-to-end instruction flow | Operator instruction dispatched via OCC reaches Agent Host and is logged in audit trail within 5 seconds |
| S-OCC-002 | Mode change propagation | Switching from Autonomous to Intern mode causes all subsequent interventions to require approval |
| S-OCC-003 | Feature disable propagation | Disabling Test Coverage group stops all test generation interventions within one polling cycle |
| S-OCC-004 | Training data export | Labelled intervention dataset exported to S3 and ingested by Training Arena without data loss |
| S-OCC-005 | System health accuracy | OCC health display matches Agent Host OpenTelemetry metrics within one polling interval |
| S-OCC-006 | WCAG 2.2 AAA compliance | All OCC panels pass axe-core AAA audit with zero violations |
| S-OCC-007 | Scroll stability | Page scroll position unchanged across 100 consecutive thought stream updates |

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

## 8. Data Flow: Intervention Lifecycle (Intern Mode)

```
Agent Host AIDecisionService
  │
  │ 1. Analyses work item / code / test result
  │ 2. Generates intervention proposal with confidence score
  │ 3. Sends intervention event via WebSocket:
  │    { id, type, description, confidence, affectedItem, proposedAction }
  │
  ▼
OCC Intervention Queue Panel
  │
  │ 4. Renders intervention card with Approve / Reject buttons
  │ 5. Operator reviews proposed action
  │
  ├──────────────────────────────────────────────────────────────┐
  │ [Approve]                                                    │ [Reject]
  │                                                              │
  ▼                                                              ▼
OCC Frontend                                           OCC Frontend
  │                                                              │
  │ 6a. Sends approval via WebSocket:                            │ 6b. Sends rejection via WebSocket:
  │     { interventionId, decision: "approved" }                 │     { interventionId, decision: "rejected" }
  │                                                              │
  ▼                                                              ▼
Agent Host WorkerService                               Agent Host WorkerService
  │                                                              │
  │ 7a. Executes intervention                                    │ 7b. Logs rejection as negative example
  │ 8a. Updates audit trail: status = executed                   │ 8b. Updates audit trail: status = rejected
  │ 9a. Sends completion event via WebSocket                     │ 9b. Feeds rejection to AI Decision Module
  │                                                              │     for pattern learning
  ▼                                                              ▼
OCC Intervention Queue Panel                           OCC Intervention Queue Panel
  │                                                              │
  │ 10a. Updates card status: Executed ✅                        │ 10b. Updates card status: Rejected ❌
```

---

## 9. Configuration Reference

The following table documents all OCC-managed configuration fields and their corresponding `appsettings.json` paths in the Agent Host:

| OCC Field | appsettings.json Path | Type | Default |
|---|---|---|---|
| Operational Mode | `OmniCommandCenter.OperationalMode` | enum: Training/Intern/Autonomous | `Intern` |
| Requirement Clarity Enabled | `Features.RequirementClarity.Enabled` | bool | `true` |
| Requirement Clarity Model | `Features.RequirementClarity.Model` | string | `granite4` |
| Requirement Clarity Prompt | `Features.RequirementClarity.SystemPrompt` | string | (see defaults) |
| Test Coverage Enabled | `Features.TestCoverage.Enabled` | bool | `true` |
| Test Coverage Model | `Features.TestCoverage.Model` | string | `llama3-8b` |
| Test Coverage Prompt | `Features.TestCoverage.SystemPrompt` | string | (see defaults) |
| QA Enabled | `Features.QualityAssurance.Enabled` | bool | `true` |
| QA Model | `Features.QualityAssurance.Model` | string | `phi3` |
| QA Prompt | `Features.QualityAssurance.SystemPrompt` | string | (see defaults) |
| SDLC Automation Enabled | `Features.SDLCAutomation.Enabled` | bool | `true` |
| SDLC Automation Model | `Features.SDLCAutomation.Model` | string | `granite4` |
| SDLC Automation Prompt | `Features.SDLCAutomation.SystemPrompt` | string | (see defaults) |
| Teams Webhook URL | `OmniCommandCenter.TeamsWebhookUrl` | string | `""` |
| WebSocket Endpoint | `OmniCommandCenter.WebSocketEndpoint` | string | `ws://localhost:5001/omni` |
| Health Poll Interval (ms) | `OmniCommandCenter.HealthPollIntervalMs` | int | `5000` |

---

## 10. Security Considerations

All communication between the OCC and the Agent Host occurs within the enterprise network. The WebSocket control bus endpoint (`ws://{host}:5001/omni`) is not exposed to the public internet. Operator authentication is enforced via the Manus OAuth flow; only authenticated users with the `operator` role can access the OCC and dispatch instructions.

The Teams webhook URL is stored in the Agent Host's secrets management system (Azure Key Vault for production, DPAPI for on-premises) and is never transmitted to the OCC frontend. The OCC frontend stores only a masked representation of the URL (e.g., `https://outlook.office.com/webhook/***`) in `localStorage`.

System prompts stored in `localStorage` and `appsettings.json` must not contain sensitive data (API keys, credentials, PII). The OCC validates prompt content against a blocklist of credential patterns before saving.

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
