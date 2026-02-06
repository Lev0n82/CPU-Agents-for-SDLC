#Inspired by  GRACE Autonomous Testing System
## Detailed Design Document (DDD) v4.0

**Document Version:** 4.0.0  
**Document Date:** January 31, 2026  
**Classification:** Technical Specification - CONFIDENTIAL  
**System:** Playwright/TestDriverMCP/AutomationTestingProgram/GRACE  
**Repository:** github.com/Lev0n82/AskMarilyn

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [System Overview](#2-system-overview)
3. [Global Architecture](#3-global-architecture)
4. [The Three ABT Layers](#4-the-three-abt-layers)
5. [Decision Trees and Design Rationale](#5-decision-trees-and-design-rationale)
6. [Component Specifications](#6-component-specifications)
7. [Software Version Registry](#7-software-version-registry)
8. [Database Schema](#8-database-schema)
9. [API Specifications](#9-api-specifications)
10. [Code Structure and Implementation](#10-code-structure-and-implementation)
11. [Installation and Configuration](#11-installation-and-configuration)
12. [Security Specifications](#12-security-specifications)
13. [Testing and Quality Assurance](#13-testing-and-quality-assurance)
14. [Deployment Architecture](#14-deployment-architecture)
15. [Appendices](#15-appendices)

---

## 1. Executive Summary

### 1.1 Purpose

This Detailed Design Document (DDD) provides the complete technical specification for the **GRACE (Governed Resilient Autonomous Certification for Enterprises) Autonomous Testing System v4**. The document is designed to enable any AI agent or human developer to replicate the entire system without variance, including all architectural decisions, code structures, configuration requirements, and the rationale behind every design choice.

### 1.2 System Definition

GRACE is an enterprise-grade, AI-powered autonomous testing platform that implements the **Action-Based Testing (ABT)** methodology. The system combines:

- **Playwright** for browser automation and cross-browser testing
- **TestDriver MCP** (Model Context Protocol) for AI-driven test orchestration
- **AutomationTestingProgram** for legacy test case integration
- **Human-in-the-Loop** oversight for critical decision points

### 1.3 Core Objectives

| Objective | Description | Success Criteria |
|-----------|-------------|------------------|
| **Autonomous Execution** | Execute tests without human intervention | 95% of test runs complete autonomously |
| **Self-Healing** | Automatically adapt to UI changes | 80% of locator failures self-corrected |
| **Intelligent Analysis** | AI-powered root cause analysis | Mean time to diagnosis < 5 minutes |
| **Scalable Distribution** | Parallel execution across nodes | Linear scaling up to 1000 nodes |
| **Human Oversight** | Maintain human control over critical paths | 100% of security tests require human approval |

### 1.4 Naming Conventions

The system employs specific naming conventions that MUST be preserved:

| Term | Definition | Context |
|------|------------|---------|
| **Test Modules** | Top layer - Business-level test cases | Maps to user stories and acceptance criteria |
| **Actions** | Middle layer - Reusable test operations | "ClickButton", "EnterText", "VerifyElement" |
| **Interface** | Bottom layer - Technical implementation | WebDriver, API connectors, database hooks |
| **Action Runner** | Central dispatcher for action execution | Composition over inheritance pattern |
| **GRACE Portal** | Web-based mission control interface | Dashboard, test management, configuration |
| **Judge Model** | AI model that evaluates test results | Monitors accuracy and recommends improvements |

---

## 2. System Overview

### 2.1 High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              GRACE PORTAL (Web UI)                               │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│  │  Dashboard  │  │    Test     │  │   Results   │  │     Configuration       │ │
│  │             │  │  Management │  │   Analysis  │  │                         │ │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  └───────────┬─────────────┘ │
└─────────┼────────────────┼────────────────┼─────────────────────┼───────────────┘
          │                │                │                     │
          ▼                ▼                ▼                     ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           API GATEWAY (tRPC + REST)                              │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │                    Authentication & Authorization                        │   │
│  │                         (JWT + OAuth 2.0)                                │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────────┘
          │                │                │                     │
          ▼                ▼                ▼                     ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              CORE SERVICES LAYER                                 │
│                                                                                  │
│  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐    │
│  │   NLP &       │  │   Action      │  │   AI &        │  │   Analysis    │    │
│  │   Intent      │  │   Runner      │  │   Learning    │  │   & Reporting │    │
│  │   Engine      │  │   Engine      │  │   Engine      │  │   Engine      │    │
│  └───────┬───────┘  └───────┬───────┘  └───────┬───────┘  └───────┬───────┘    │
│          │                  │                  │                  │             │
│          └──────────────────┴──────────────────┴──────────────────┘             │
│                                      │                                           │
└──────────────────────────────────────┼───────────────────────────────────────────┘
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                         DISTRIBUTED EXECUTION LAYER                              │
│                                                                                  │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │                    PostgreSQL Job Queue (Central)                        │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│          │                  │                  │                  │             │
│          ▼                  ▼                  ▼                  ▼             │
│  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐    │
│  │  Execution    │  │  Execution    │  │  Execution    │  │  Execution    │    │
│  │  Node 1       │  │  Node 2       │  │  Node 3       │  │  Node N       │    │
│  │  (Playwright) │  │  (Playwright) │  │  (Playwright) │  │  (Playwright) │    │
│  └───────────────┘  └───────────────┘  └───────────────┘  └───────────────┘    │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           DATA & KNOWLEDGE LAYER                                 │
│                                                                                  │
│  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐    │
│  │  PostgreSQL   │  │    Oracle     │  │   Vector DB   │  │   Azure Git   │    │
│  │  (Primary)    │  │  (Test Cases) │  │  (Embeddings) │  │  (Artifacts)  │    │
│  └───────────────┘  └───────────────┘  └───────────────┘  └───────────────┘    │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 System Boundaries

The GRACE system operates within the following boundaries:

| Boundary | Internal | External |
|----------|----------|----------|
| **Test Execution** | Playwright browser automation | System Under Test (SUT) |
| **Data Storage** | PostgreSQL, Oracle, Vector DB | Azure Git for artifacts |
| **AI Processing** | Local Ollama models | Optional cloud AI (Claude, GPT) |
| **User Interface** | GRACE Portal | External authentication (OAuth) |
| **Notifications** | Internal alerting | Email, Slack, Teams webhooks |

### 2.3 Stakeholder Roles

| Role | Responsibilities | System Access |
|------|------------------|---------------|
| **QA Engineer** | Create/execute tests, analyze results | Full Portal access |
| **Developer** | Review failures, fix defects | Results & analysis access |
| **QA Manager** | Monitor progress, generate reports | Dashboard & reporting |
| **Admin** | Configure system, manage users | Full admin access |
| **AI Operator** | Monitor AI models, tune parameters | AI configuration access |

---

## 3. Global Architecture

### 3.1 The Five Core Layers

The GRACE system is built on five interconnected layers, each with specific responsibilities:

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                                                                                  │
│    LAYER 5: KNOWLEDGE & MEMORY                                                   │
│    ┌─────────────────────────────────────────────────────────────────────────┐  │
│    │  Vector Database (Embeddings) │ Historical Test Data │ Defect Patterns  │  │
│    └─────────────────────────────────────────────────────────────────────────┘  │
│                                       ▲                                          │
│                                       │ Learns From                              │
│                                       │                                          │
│    LAYER 4: AI & LEARNING                                                        │
│    ┌─────────────────────────────────────────────────────────────────────────┐  │
│    │  Judge Model │ Prompt Engineer │ Self-Healing │ Root Cause Analysis     │  │
│    └─────────────────────────────────────────────────────────────────────────┘  │
│                                       ▲                                          │
│                                       │ Analyzes                                 │
│                                       │                                          │
│    LAYER 3: ANALYSIS & REPORTING                                                 │
│    ┌─────────────────────────────────────────────────────────────────────────┐  │
│    │  Result Aggregation │ Trend Analysis │ Performance Metrics │ Alerts     │  │
│    └─────────────────────────────────────────────────────────────────────────┘  │
│                                       ▲                                          │
│                                       │ Reports To                               │
│                                       │                                          │
│    LAYER 2: EXECUTION                                                            │
│    ┌─────────────────────────────────────────────────────────────────────────┐  │
│    │  Action Runner │ Playwright │ TestDriver MCP │ Distributed Nodes        │  │
│    └─────────────────────────────────────────────────────────────────────────┘  │
│                                       ▲                                          │
│                                       │ Executes                                 │
│                                       │                                          │
│    LAYER 1: NLP & INTENT                                                         │
│    ┌─────────────────────────────────────────────────────────────────────────┐  │
│    │  Test Case Parser │ Intent Recognition │ Action Mapping │ Data Binding  │  │
│    └─────────────────────────────────────────────────────────────────────────┘  │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Layer Specifications

#### Layer 1: NLP & Intent Engine

**Purpose:** Transform human-readable test cases into executable action sequences.

| Component | Function | Technology |
|-----------|----------|------------|
| Test Case Parser | Parse Excel/Gherkin/Natural Language | Python + spaCy |
| Intent Recognition | Identify test intent from description | LLM (Claude/Ollama) |
| Action Mapping | Map intents to Action catalog | Rule engine + ML |
| Data Binding | Inject test data into actions | Template engine |

**Input Format Support:**
- Excel spreadsheets (legacy AutomationTestingProgram format)
- Gherkin/Cucumber feature files
- Natural language descriptions
- JSON/YAML test definitions

#### Layer 2: Execution Engine

**Purpose:** Execute test actions against the System Under Test.

| Component | Function | Technology |
|-----------|----------|------------|
| Action Runner | Central dispatcher for all actions | TypeScript/Rust |
| Playwright Engine | Browser automation | Playwright 1.50.x |
| TestDriver MCP | AI-driven test orchestration | MCP Protocol |
| Node Manager | Distribute work across nodes | PostgreSQL queue |

**Execution Modes:**
- **Sequential:** Actions execute one after another
- **Parallel:** Multiple tests run simultaneously on different nodes
- **Hybrid:** Sequential within test, parallel across tests

#### Layer 3: Analysis & Reporting Engine

**Purpose:** Aggregate results and provide actionable insights.

| Component | Function | Technology |
|-----------|----------|------------|
| Result Aggregator | Collect results from all nodes | Event streaming |
| Trend Analyzer | Identify patterns over time | Time-series DB |
| Performance Monitor | Track execution metrics | Prometheus/Grafana |
| Alert Manager | Notify stakeholders of issues | Webhook integrations |

#### Layer 4: AI & Learning Engine

**Purpose:** Provide intelligent analysis and self-improvement capabilities.

| Component | Function | Technology |
|-----------|----------|------------|
| Judge Model | Evaluate test quality and accuracy | LLM (up to 10 models) |
| Prompt Engineer | Generate optimal AI prompts | Template + ML |
| Self-Healing | Auto-fix broken locators | Computer vision + ML |
| Root Cause Analyzer | Diagnose failure causes | Knowledge graph |

**Judge Model Capabilities:**
- Monitor and assess accuracy of all AI models
- Recommend model training improvements
- Generate synthetic training data
- Decide when to replace underperforming models

#### Layer 5: Knowledge & Memory Layer

**Purpose:** Store and retrieve historical knowledge for continuous improvement.

| Component | Function | Technology |
|-----------|----------|------------|
| Vector Database | Store test embeddings | Pinecone/Weaviate |
| Historical Store | Archive all test results | PostgreSQL |
| Defect Pattern DB | Known failure signatures | Graph database |
| Learning Cache | Recent model improvements | Redis |

---

## 4. The Three ABT Layers

### 4.1 ABT Philosophy Overview

Action-Based Testing (ABT) is the foundational methodology of the GRACE system. It separates test automation into three distinct layers, each with specific responsibilities:

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                                                                                  │
│                              TEST MODULES (Layer 1)                              │
│                                                                                  │
│    ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│    │   Login     │  │   Search    │  │  Add to     │  │  Checkout   │          │
│    │   Test      │  │  Function   │  │  Cart       │  │  Process    │          │
│    └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘          │
│           │                │                │                │                  │
│           │    Uses Reusable Actions        │                │                  │
│           ▼                ▼                ▼                ▼                  │
│                                                                                  │
│                                ACTIONS (Layer 2)                                 │
│                                                                                  │
│    ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│    │   Enter     │  │   Click     │  │   Select    │  │   Verify    │          │
│    │   Text      │  │   Button    │  │   Dropdown  │  │   Text      │          │
│    └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘          │
│           │                │                │                │                  │
│           │    Executes Technical Commands  │                │                  │
│           ▼                ▼                ▼                ▼                  │
│                                                                                  │
│                               INTERFACE (Layer 3)                                │
│                                                                                  │
│    ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│    │   Web       │  │   API       │  │  Database   │  │   Mobile    │          │
│    │   Driver    │  │  Connector  │  │   Access    │  │  Emulator   │          │
│    └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘          │
│                                                                                  │
│                           SYSTEM UNDER TEST (SUT)                                │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Layer 1: Test Modules

**Definition:** High-level, business-readable test cases that describe what to test.

**Characteristics:**
- Written in business language
- Maps directly to user stories and acceptance criteria
- Contains no technical implementation details
- Easily understood by non-technical stakeholders

**Example Test Module:**
```yaml
Test Module: User Login Verification
  Description: Verify that a registered user can log in successfully
  Preconditions:
    - User account exists with valid credentials
  Steps:
    1. Navigate to login page
    2. Enter username
    3. Enter password
    4. Click login button
    5. Verify dashboard is displayed
  Expected Result: User is logged in and sees dashboard
```

### 4.3 Layer 2: Actions

**Definition:** Reusable, parameterized operations that implement test steps.

**Characteristics:**
- Technology-agnostic at the action level
- Highly reusable across multiple test modules
- Parameterized for flexibility
- Self-documenting through clear naming

**Action Catalog (Core Actions):**

| Action Name | Parameters | Description |
|-------------|------------|-------------|
| `NavigateToUrl` | `url: string` | Navigate browser to specified URL |
| `EnterText` | `locator: string, text: string` | Enter text into an input field |
| `ClickElement` | `locator: string` | Click on an element |
| `SelectDropdown` | `locator: string, value: string` | Select dropdown option |
| `VerifyText` | `locator: string, expected: string` | Verify element contains text |
| `VerifyVisible` | `locator: string` | Verify element is visible |
| `WaitForElement` | `locator: string, timeout: number` | Wait for element to appear |
| `CaptureScreenshot` | `name: string` | Capture screenshot |
| `ExecuteSQL` | `query: string` | Execute database query |
| `CallAPI` | `method: string, url: string, body: object` | Make API call |

**Action Implementation Pattern:**
```typescript
interface Action {
  name: string;
  parameters: Record<string, unknown>;
  execute(context: ExecutionContext): Promise<ActionResult>;
  validate(): ValidationResult;
  rollback?(): Promise<void>;
}

class EnterTextAction implements Action {
  name = 'EnterText';
  parameters: { locator: string; text: string };
  
  async execute(context: ExecutionContext): Promise<ActionResult> {
    const element = await context.page.locator(this.parameters.locator);
    await element.fill(this.parameters.text);
    return { success: true, duration: Date.now() - startTime };
  }
  
  validate(): ValidationResult {
    if (!this.parameters.locator) {
      return { valid: false, error: 'Locator is required' };
    }
    return { valid: true };
  }
}
```

### 4.4 Layer 3: Interface

**Definition:** Technical implementation that connects actions to the System Under Test.

**Characteristics:**
- Technology-specific implementations
- Handles all low-level interactions
- Abstracts complexity from higher layers
- Supports multiple interface types

**Interface Types:**

| Interface | Technology | Use Case |
|-----------|------------|----------|
| WebDriver | Playwright | Web browser automation |
| APIConnector | Axios/Fetch | REST/GraphQL API testing |
| DatabaseAccess | pg/mysql2 | Database operations |
| MobileEmulator | Appium | Mobile app testing |
| DesktopHooks | WinAppDriver | Desktop application testing |
| ProtocolAdapters | Custom | Legacy system integration |

---

## 5. Decision Trees and Design Rationale

### 5.1 Decision Tree 1: Browser Automation Framework

**Decision:** Which browser automation framework to use?

```
                    ┌─────────────────────────────┐
                    │  Browser Automation Choice  │
                    └──────────────┬──────────────┘
                                   │
           ┌───────────────────────┼───────────────────────┐
           │                       │                       │
           ▼                       ▼                       ▼
    ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
    │  Selenium   │         │  Playwright │         │   Cypress   │
    │  WebDriver  │         │             │         │             │
    └──────┬──────┘         └──────┬──────┘         └──────┬──────┘
           │                       │                       │
           ▼                       ▼                       ▼
    ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
    │ - Mature    │         │ - Modern    │         │ - Fast      │
    │ - Wide      │         │ - Auto-wait │         │ - Dev-      │
    │   support   │         │ - Multi-    │         │   friendly  │
    │ - Slow      │         │   browser   │         │ - Chrome    │
    │ - Flaky     │         │ - Fast      │         │   only      │
    │ - No auto-  │         │ - Reliable  │         │ - Limited   │
    │   wait      │         │ - Trace     │         │   scope     │
    └──────┬──────┘         └──────┬──────┘         └──────┬──────┘
           │                       │                       │
           ▼                       ▼                       ▼
    ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
    │  REJECTED   │         │  ★ WINNER ★ │         │  REJECTED   │
    │             │         │             │         │             │
    │ Reason:     │         │ Reason:     │         │ Reason:     │
    │ Reliability │         │ Best        │         │ Browser     │
    │ issues,     │         │ combination │         │ limitation, │
    │ no auto-    │         │ of speed,   │         │ not         │
    │ waiting     │         │ reliability,│         │ enterprise  │
    │             │         │ features    │         │ ready       │
    └─────────────┘         └─────────────┘         └─────────────┘
```

**Explored Options:**

| Option | Pros | Cons | Score |
|--------|------|------|-------|
| **Selenium WebDriver** | Mature, wide browser support, large community | Slow, flaky, no auto-waiting, complex setup | 5/10 |
| **Playwright** | Modern, auto-waiting, multi-browser, fast, reliable, built-in tracing | Newer (less legacy support) | 9/10 |
| **Cypress** | Fast, developer-friendly, good debugging | Chrome-only (limited), not enterprise-ready | 6/10 |
| **Puppeteer** | Fast, Chrome-focused, Google-backed | Chrome-only, limited cross-browser | 5/10 |

**Winner: Playwright**

**Rationale:**
1. **Auto-waiting:** Eliminates 80% of flaky test issues
2. **Multi-browser:** Chrome, Firefox, Safari, Edge support
3. **Speed:** 2-3x faster than Selenium
4. **Reliability:** Built-in retry mechanisms
5. **Tracing:** Full trace capture for debugging
6. **Modern API:** Async/await, TypeScript support

### 5.2 Decision Tree 2: AI Model Architecture

**Decision:** How to orchestrate AI models for test intelligence?

```
                    ┌─────────────────────────────┐
                    │    AI Model Architecture    │
                    └──────────────┬──────────────┘
                                   │
           ┌───────────────────────┼───────────────────────┐
           │                       │                       │
           ▼                       ▼                       ▼
    ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
    │ Single      │         │ Multi-Model │         │ Hybrid      │
    │ Cloud Model │         │ Local       │         │ Local+Cloud │
    └──────┬──────┘         └──────┬──────┘         └──────┬──────┘
           │                       │                       │
           ▼                       ▼                       ▼
    ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
    │ - Simple    │         │ - Privacy   │         │ - Best of   │
    │ - Powerful  │         │ - Control   │         │   both      │
    │ - Expensive │         │ - Limited   │         │ - Complex   │
    │ - Privacy   │         │ - Slower    │         │ - Flexible  │
    │   concerns  │         │             │         │             │
    └──────┬──────┘         └──────┬──────┘         └──────┬──────┘
           │                       │                       │
           ▼                       ▼                       ▼
    ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
    │  REJECTED   │         │  REJECTED   │         │  ★ WINNER ★ │
    │             │         │             │         │             │
    │ Reason:     │         │ Reason:     │         │ Reason:     │
    │ Privacy,    │         │ Capability  │         │ Balances    │
    │ cost,       │         │ limitations │         │ privacy,    │
    │ dependency  │         │ for complex │         │ capability, │
    │             │         │ analysis    │         │ cost        │
    └─────────────┘         └─────────────┘         └─────────────┘
```

**Winner: Hybrid Local+Cloud Architecture**

**Implementation:**
- **Local (Ollama):** Routine analysis, sensitive data processing
- **Cloud (Claude/GPT):** Complex reasoning, advanced analysis
- **Judge Model:** Monitors all models, recommends improvements

### 5.3 Decision Tree 3: Data Storage Strategy

**Decision:** How to store test cases, results, and knowledge?

```
                    ┌─────────────────────────────┐
                    │   Data Storage Strategy     │
                    └──────────────┬──────────────┘
                                   │
    ┌──────────────┬───────────────┼───────────────┬──────────────┐
    │              │               │               │              │
    ▼              ▼               ▼               ▼              ▼
┌────────┐   ┌────────┐      ┌────────┐      ┌────────┐    ┌────────┐
│ Single │   │ NoSQL  │      │ Hybrid │      │ Graph  │    │ Multi- │
│ RDBMS  │   │ Only   │      │ SQL+   │      │ DB     │    │ Store  │
│        │   │        │      │ NoSQL  │      │ Only   │    │        │
└───┬────┘   └───┬────┘      └───┬────┘      └───┬────┘    └───┬────┘
    │            │               │               │             │
    ▼            ▼               ▼               ▼             ▼
┌────────┐   ┌────────┐      ┌────────┐      ┌────────┐    ┌────────┐
│REJECTED│   │REJECTED│      │REJECTED│      │REJECTED│    │★WINNER★│
└────────┘   └────────┘      └────────┘      └────────┘    └────────┘
```

**Winner: Multi-Store Architecture**

| Store | Technology | Purpose |
|-------|------------|---------|
| Primary Relational | PostgreSQL | User data, test results, job queue |
| Test Case Store | Oracle | Legacy test case storage, fast lookups |
| Vector Store | Pinecone/Weaviate | Embeddings for AI similarity search |
| Artifact Store | Azure Git | Screenshots, videos, logs |
| Cache | Redis | Session data, recent results |

**Rationale:**
1. **PostgreSQL:** ACID compliance for critical data
2. **Oracle:** Existing investment, fast test case lookups
3. **Vector DB:** Essential for AI-powered similarity search
4. **Azure Git:** Scalable artifact storage with versioning
5. **Redis:** High-performance caching for real-time operations

### 5.4 Decision Tree 4: Containerization Strategy

**Decision:** Which container runtime to use?

```
                    ┌─────────────────────────────┐
                    │  Container Runtime Choice   │
                    └──────────────┬──────────────┘
                                   │
           ┌───────────────────────┼───────────────────────┐
           │                       │                       │
           ▼                       ▼                       ▼
    ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
    │   Docker    │         │   Podman    │         │ Kubernetes  │
    │             │         │             │         │   Native    │
    └──────┬──────┘         └──────┬──────┘         └──────┬──────┘
           │                       │                       │
           ▼                       ▼                       ▼
    ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
    │ - Popular   │         │ - Rootless  │         │ - Scalable  │
    │ - Ecosystem │         │ - Daemonless│         │ - Complex   │
    │ - Daemon    │         │ - OCI       │         │ - Overkill  │
    │   required  │         │   compliant │         │   for some  │
    │ - Root      │         │ - Open      │         │   use cases │
    │   needed    │         │   source    │         │             │
    └──────┬──────┘         └──────┬──────┘         └──────┬──────┘
           │                       │                       │
           ▼                       ▼                       ▼
    ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
    │  REJECTED   │         │  ★ WINNER ★ │         │  OPTIONAL   │
    │             │         │             │         │             │
    │ Reason:     │         │ Reason:     │         │ Reason:     │
    │ Security    │         │ Security,   │         │ Use for     │
    │ concerns,   │         │ open source,│         │ large-scale │
    │ daemon      │         │ daemonless  │         │ deployments │
    │ dependency  │         │             │         │ only        │
    └─────────────┘         └─────────────┘         └─────────────┘
```

**Winner: Podman (Primary) + Kubernetes (Scale)**

**Rationale:**
1. **Rootless:** Enhanced security without root privileges
2. **Daemonless:** No background daemon required
3. **OCI Compliant:** Full compatibility with Docker images
4. **Open Source:** Aligns with enterprise open-source strategy

### 5.5 Decision Tree 5: Test Case Format

**Decision:** What format for defining test cases?

```
                    ┌─────────────────────────────┐
                    │   Test Case Format Choice   │
                    └──────────────┬──────────────┘
                                   │
    ┌──────────────┬───────────────┼───────────────┬──────────────┐
    │              │               │               │              │
    ▼              ▼               ▼               ▼              ▼
┌────────┐   ┌────────┐      ┌────────┐      ┌────────┐    ┌────────┐
│ Excel  │   │Gherkin │      │  JSON  │      │  YAML  │    │ Multi- │
│ Only   │   │ Only   │      │  Only  │      │  Only  │    │ Format │
└───┬────┘   └───┬────┘      └───┬────┘      └───┬────┘    └───┬────┘
    │            │               │               │             │
    ▼            ▼               ▼               ▼             ▼
┌────────┐   ┌────────┐      ┌────────┐      ┌────────┐    ┌────────┐
│REJECTED│   │REJECTED│      │REJECTED│      │REJECTED│    │★WINNER★│
└────────┘   └────────┘      └────────┘      └────────┘    └────────┘
```

**Winner: Multi-Format Support**

**Supported Formats:**
1. **Excel (.xlsx):** Legacy AutomationTestingProgram compatibility
2. **Gherkin (.feature):** BDD-style test definitions
3. **YAML (.yaml):** Human-readable structured format
4. **JSON (.json):** Programmatic generation
5. **Natural Language:** AI-interpreted free-form text

---

## 6. Component Specifications

### 6.1 Action Runner Engine

**Purpose:** Central dispatcher that executes actions using composition over inheritance.

**Architecture:**
```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              ACTION RUNNER ENGINE                                │
│                                                                                  │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │                           Action Registry                                │   │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐           │   │
│  │  │Navigate │ │EnterText│ │  Click  │ │ Verify  │ │ Custom  │           │   │
│  │  │  ToUrl  │ │         │ │ Element │ │  Text   │ │ Actions │           │   │
│  │  └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘           │   │
│  │       │           │           │           │           │                 │   │
│  │       └───────────┴───────────┴───────────┴───────────┘                 │   │
│  │                               │                                          │   │
│  └───────────────────────────────┼──────────────────────────────────────────┘   │
│                                  │                                               │
│  ┌───────────────────────────────▼──────────────────────────────────────────┐   │
│  │                           Dispatcher                                      │   │
│  │                                                                           │   │
│  │   1. Receive action request                                               │   │
│  │   2. Validate parameters                                                  │   │
│  │   3. Resolve action from registry                                         │   │
│  │   4. Execute action with context                                          │   │
│  │   5. Capture result and metrics                                           │   │
│  │   6. Handle errors and retries                                            │   │
│  │   7. Return result to caller                                              │   │
│  │                                                                           │   │
│  └───────────────────────────────┬──────────────────────────────────────────┘   │
│                                  │                                               │
│  ┌───────────────────────────────▼──────────────────────────────────────────┐   │
│  │                        Execution Context                                  │   │
│  │                                                                           │   │
│  │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │   │
│  │   │  Playwright │  │   Test      │  │  Variables  │  │   Logger    │    │   │
│  │   │    Page     │  │   Data      │  │   Store     │  │             │    │   │
│  │   └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │   │
│  │                                                                           │   │
│  └───────────────────────────────────────────────────────────────────────────┘   │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

**Key Interfaces:**

```typescript
// Action interface - all actions must implement this
interface IAction {
  readonly name: string;
  readonly version: string;
  readonly category: ActionCategory;
  
  validate(params: ActionParams): ValidationResult;
  execute(context: ExecutionContext, params: ActionParams): Promise<ActionResult>;
  rollback?(context: ExecutionContext): Promise<void>;
}

// Execution context passed to all actions
interface ExecutionContext {
  page: PlaywrightPage;
  browser: PlaywrightBrowser;
  testData: TestDataStore;
  variables: VariableStore;
  logger: Logger;
  config: RuntimeConfig;
  metrics: MetricsCollector;
}

// Action result returned by all actions
interface ActionResult {
  success: boolean;
  duration: number;
  error?: Error;
  screenshot?: string;
  data?: Record<string, unknown>;
}

// Action categories for organization
enum ActionCategory {
  NAVIGATION = 'navigation',
  INPUT = 'input',
  VERIFICATION = 'verification',
  WAIT = 'wait',
  DATA = 'data',
  API = 'api',
  DATABASE = 'database',
  CUSTOM = 'custom'
}
```

### 6.2 TestDriver MCP Integration

**Purpose:** Model Context Protocol integration for AI-driven test orchestration.

**Architecture:**
```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           TESTDRIVER MCP INTEGRATION                             │
│                                                                                  │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │                         MCP Server (Local)                               │   │
│  │                                                                          │   │
│  │   ┌─────────────────────────────────────────────────────────────────┐   │   │
│  │   │                    Tool Definitions                              │   │   │
│  │   │                                                                  │   │   │
│  │   │   browser_install    - Install browser for testing               │   │   │
│  │   │   browser_navigate   - Navigate to URL                           │   │   │
│  │   │   browser_click      - Click element                             │   │   │
│  │   │   browser_type       - Type text                                 │   │   │
│  │   │   browser_screenshot - Capture screenshot                        │   │   │
│  │   │   browser_evaluate   - Execute JavaScript                        │   │   │
│  │   │   browser_wait       - Wait for condition                        │   │   │
│  │   │                                                                  │   │   │
│  │   └─────────────────────────────────────────────────────────────────┘   │   │
│  │                                                                          │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                      │                                          │
│                                      │ MCP Protocol                             │
│                                      ▼                                          │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │                         AI Orchestrator                                  │   │
│  │                                                                          │   │
│  │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐   │   │
│  │   │   Claude    │  │    GPT      │  │   Ollama    │  │   Custom    │   │   │
│  │   │   Model     │  │   Model     │  │   Local     │  │   Model     │   │   │
│  │   └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘   │   │
│  │                                                                          │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

**MCP Tool Specifications:**

| Tool | Parameters | Returns | Description |
|------|------------|---------|-------------|
| `browser_install` | `browser: string` | `{ success: boolean }` | Install specified browser |
| `browser_navigate` | `url: string` | `{ title: string, url: string }` | Navigate to URL |
| `browser_click` | `selector: string` | `{ clicked: boolean }` | Click element |
| `browser_type` | `selector: string, text: string` | `{ typed: boolean }` | Type text |
| `browser_screenshot` | `name: string` | `{ path: string }` | Capture screenshot |
| `browser_evaluate` | `script: string` | `{ result: any }` | Execute JavaScript |
| `browser_wait` | `selector: string, timeout: number` | `{ found: boolean }` | Wait for element |

### 6.3 Distributed Execution Architecture

**Purpose:** Scale test execution across multiple nodes for massive parallelism.

**Architecture:**
```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                      DISTRIBUTED EXECUTION ARCHITECTURE                          │
│                                                                                  │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │                    PostgreSQL Job Queue (Central)                        │   │
│  │                                                                          │   │
│  │   ┌─────────────────────────────────────────────────────────────────┐   │   │
│  │   │  Job Table                                                       │   │   │
│  │   │  ─────────────────────────────────────────────────────────────  │   │   │
│  │   │  id │ test_id │ status │ node_id │ priority │ created_at       │   │   │
│  │   │  ─────────────────────────────────────────────────────────────  │   │   │
│  │   │  1  │ TC-001  │ queued │ null    │ high     │ 2026-01-31 10:00 │   │   │
│  │   │  2  │ TC-002  │ running│ node-1  │ medium   │ 2026-01-31 10:01 │   │   │
│  │   │  3  │ TC-003  │ done   │ node-2  │ low      │ 2026-01-31 10:02 │   │   │
│  │   └─────────────────────────────────────────────────────────────────┘   │   │
│  │                                                                          │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                      │                                          │
│              ┌───────────────────────┼───────────────────────┐                 │
│              │                       │                       │                 │
│              ▼                       ▼                       ▼                 │
│  ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐          │
│  │  Execution      │     │  Execution      │     │  Execution      │          │
│  │  Node 1         │     │  Node 2         │     │  Node N         │          │
│  │                 │     │                 │     │                 │          │
│  │  ┌───────────┐  │     │  ┌───────────┐  │     │  ┌───────────┐  │          │
│  │  │ Playwright│  │     │  │ Playwright│  │     │  │ Playwright│  │          │
│  │  │ Instance  │  │     │  │ Instance  │  │     │  │ Instance  │  │          │
│  │  └───────────┘  │     │  └───────────┘  │     │  └───────────┘  │          │
│  │                 │     │                 │     │                 │          │
│  │  ┌───────────┐  │     │  ┌───────────┐  │     │  ┌───────────┐  │          │
│  │  │  Action   │  │     │  │  Action   │  │     │  │  Action   │  │          │
│  │  │  Runner   │  │     │  │  Runner   │  │     │  │  Runner   │  │          │
│  │  └───────────┘  │     │  └───────────┘  │     │  └───────────┘  │          │
│  │                 │     │                 │     │                 │          │
│  │  Capacity: 4    │     │  Capacity: 4    │     │  Capacity: 4    │          │
│  │  Status: Active │     │  Status: Active │     │  Status: Active │          │
│  │                 │     │                 │     │                 │          │
│  └─────────────────┘     └─────────────────┘     └─────────────────┘          │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

**Job Queue Schema:**

```sql
CREATE TABLE execution_jobs (
    id SERIAL PRIMARY KEY,
    test_id VARCHAR(64) NOT NULL,
    test_suite_id VARCHAR(64),
    status VARCHAR(20) NOT NULL DEFAULT 'queued',
    priority VARCHAR(10) NOT NULL DEFAULT 'medium',
    node_id VARCHAR(64),
    payload JSONB NOT NULL,
    result JSONB,
    error_message TEXT,
    retry_count INT DEFAULT 0,
    max_retries INT DEFAULT 3,
    created_at TIMESTAMP DEFAULT NOW(),
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    
    CONSTRAINT valid_status CHECK (status IN ('queued', 'running', 'done', 'failed', 'cancelled')),
    CONSTRAINT valid_priority CHECK (priority IN ('critical', 'high', 'medium', 'low'))
);

CREATE INDEX idx_jobs_status ON execution_jobs(status);
CREATE INDEX idx_jobs_priority ON execution_jobs(priority);
CREATE INDEX idx_jobs_node ON execution_jobs(node_id);
```

### 6.4 GRACE Portal

**Purpose:** Web-based mission control for the entire quality operation.

**Component Hierarchy:**
```
GRACE Portal
├── Dashboard
│   ├── Health Overview
│   │   ├── Test Pass Rate (gauge)
│   │   ├── Performance Metrics (line chart)
│   │   └── Security Alerts (list)
│   ├── Recent Activity
│   │   ├── Latest Test Runs
│   │   ├── Recent Failures
│   │   └── Pending Reviews
│   └── Quick Actions
│       ├── Run Test Suite
│       ├── View Reports
│       └── Configure Settings
│
├── Test Management
│   ├── Test Suites
│   │   ├── Suite List
│   │   ├── Suite Editor
│   │   └── Suite Scheduler
│   ├── Test Cases
│   │   ├── Case Browser
│   │   ├── Case Editor
│   │   └── Case Import/Export
│   └── Test Data
│       ├── Data Sets
│       ├── Data Generator
│       └── Data Masking
│
├── Results Analysis
│   ├── Run History
│   │   ├── Run List
│   │   ├── Run Details
│   │   └── Run Comparison
│   ├── Failure Analysis
│   │   ├── Failure Browser
│   │   ├── Root Cause View
│   │   └── Screenshot/Video
│   └── Trend Reports
│       ├── Pass Rate Trends
│       ├── Performance Trends
│       └── Coverage Trends
│
├── Configuration
│   ├── Execution Nodes
│   │   ├── Node List
│   │   ├── Node Health
│   │   └── Node Configuration
│   ├── AI Settings
│   │   ├── Model Selection
│   │   ├── Prompt Templates
│   │   └── Judge Model Config
│   └── Integrations
│       ├── CI/CD Webhooks
│       ├── Notification Channels
│       └── External Tools
│
└── Administration
    ├── User Management
    ├── Role Permissions
    ├── Audit Logs
    └── System Settings
```

---

## 7. Software Version Registry

### 7.1 Core Runtime Dependencies

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| Node.js | 22.13.0 | JavaScript runtime | MIT |
| TypeScript | 5.9.3 | Type-safe JavaScript | Apache-2.0 |
| Rust | 1.75.0 | High-performance components | MIT/Apache-2.0 |
| Python | 3.11.0 | NLP and ML components | PSF |

### 7.2 Browser Automation

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| Playwright | 1.50.1 | Browser automation | Apache-2.0 |
| @playwright/test | 1.50.1 | Test runner | Apache-2.0 |

### 7.3 Web Framework

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| React | 19.2.1 | UI framework | MIT |
| Next.js | 15.1.0 | React framework | MIT |
| Express | 4.21.2 | Node.js web server | MIT |
| tRPC | 11.6.0 | Type-safe API | MIT |

### 7.4 Database & Storage

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| PostgreSQL | 16.2 | Primary database | PostgreSQL |
| Oracle | 19c | Test case storage | Commercial |
| Drizzle ORM | 0.44.5 | TypeScript ORM | MIT |
| Redis | 7.2.4 | Caching | BSD-3 |
| Pinecone | 2.0.0 | Vector database | Commercial |

### 7.5 AI & ML

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| Ollama | 0.1.27 | Local LLM runtime | MIT |
| LangChain | 0.1.0 | LLM orchestration | MIT |
| spaCy | 3.7.0 | NLP processing | MIT |
| Transformers | 4.36.0 | ML models | Apache-2.0 |

### 7.6 DevOps & Infrastructure

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| Podman | 4.9.0 | Container runtime | Apache-2.0 |
| Kubernetes | 1.29.0 | Container orchestration | Apache-2.0 |
| Prometheus | 2.48.0 | Metrics collection | Apache-2.0 |
| Grafana | 10.2.0 | Metrics visualization | AGPL-3.0 |

### 7.7 Testing & Quality

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| Vitest | 2.1.4 | Unit testing | MIT |
| ESLint | 9.17.0 | Code linting | MIT |
| Prettier | 3.6.2 | Code formatting | MIT |

### 7.8 UI Components

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| Tailwind CSS | 4.1.14 | Utility CSS | MIT |
| shadcn/ui | 2.1.0 | UI components | MIT |
| Radix UI | 1.1.0 | Headless components | MIT |
| Lucide React | 0.453.0 | Icons | ISC |
| Recharts | 2.15.2 | Charts | MIT |

---

## 8. Database Schema

### 8.1 Entity Relationship Diagram

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│     users       │       │   test_suites   │       │   test_cases    │
├─────────────────┤       ├─────────────────┤       ├─────────────────┤
│ id (PK)         │       │ id (PK)         │       │ id (PK)         │
│ openId          │       │ name            │       │ suite_id (FK)   │
│ name            │◄──────│ created_by (FK) │       │ name            │
│ email           │       │ description     │       │ description     │
│ role            │       │ status          │       │ steps (JSON)    │
│ created_at      │       │ created_at      │       │ expected_result │
│ updated_at      │       │ updated_at      │       │ priority        │
└─────────────────┘       └────────┬────────┘       │ tags            │
                                   │                │ created_at      │
                                   │                └────────┬────────┘
                                   │                         │
                                   ▼                         ▼
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│ execution_nodes │       │  test_runs      │       │  test_results   │
├─────────────────┤       ├─────────────────┤       ├─────────────────┤
│ id (PK)         │       │ id (PK)         │       │ id (PK)         │
│ name            │       │ suite_id (FK)   │◄──────│ run_id (FK)     │
│ hostname        │       │ status          │       │ case_id (FK)    │
│ capacity        │       │ started_at      │       │ status          │
│ status          │       │ completed_at    │       │ duration        │
│ last_heartbeat  │       │ triggered_by    │       │ error_message   │
│ config (JSON)   │       │ node_id (FK)    │       │ screenshot_url  │
└─────────────────┘       └─────────────────┘       │ video_url       │
                                                    │ created_at      │
                                                    └─────────────────┘
                          
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│    actions      │       │ action_results  │       │   ai_models     │
├─────────────────┤       ├─────────────────┤       ├─────────────────┤
│ id (PK)         │       │ id (PK)         │       │ id (PK)         │
│ name            │       │ result_id (FK)  │       │ name            │
│ category        │       │ action_id (FK)  │       │ type            │
│ description     │       │ status          │       │ endpoint        │
│ parameters      │       │ duration        │       │ config (JSON)   │
│ implementation  │       │ input (JSON)    │       │ is_active       │
│ version         │       │ output (JSON)   │       │ performance     │
│ is_active       │       │ error           │       │ created_at      │
└─────────────────┘       │ created_at      │       └─────────────────┘
                          └─────────────────┘
```

### 8.2 Complete Schema Definition

```sql
-- Users table (authentication)
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    open_id VARCHAR(64) NOT NULL UNIQUE,
    name TEXT,
    email VARCHAR(320),
    login_method VARCHAR(64),
    role VARCHAR(20) NOT NULL DEFAULT 'user',
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    last_signed_in TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_role CHECK (role IN ('user', 'admin', 'operator'))
);

-- Test suites table
CREATE TABLE test_suites (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    created_by INT REFERENCES users(id),
    status VARCHAR(20) NOT NULL DEFAULT 'active',
    config JSONB DEFAULT '{}',
    tags TEXT[],
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_status CHECK (status IN ('active', 'archived', 'draft'))
);

-- Test cases table
CREATE TABLE test_cases (
    id SERIAL PRIMARY KEY,
    suite_id INT REFERENCES test_suites(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    steps JSONB NOT NULL,
    expected_result TEXT,
    priority VARCHAR(20) NOT NULL DEFAULT 'medium',
    tags TEXT[],
    preconditions JSONB,
    test_data JSONB,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_priority CHECK (priority IN ('critical', 'high', 'medium', 'low'))
);

-- Execution nodes table
CREATE TABLE execution_nodes (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    hostname VARCHAR(255) NOT NULL,
    ip_address INET,
    capacity INT NOT NULL DEFAULT 4,
    current_load INT DEFAULT 0,
    status VARCHAR(20) NOT NULL DEFAULT 'offline',
    config JSONB DEFAULT '{}',
    last_heartbeat TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_status CHECK (status IN ('online', 'offline', 'busy', 'maintenance'))
);

-- Test runs table
CREATE TABLE test_runs (
    id SERIAL PRIMARY KEY,
    suite_id INT REFERENCES test_suites(id),
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    triggered_by INT REFERENCES users(id),
    node_id INT REFERENCES execution_nodes(id),
    config JSONB DEFAULT '{}',
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_status CHECK (status IN ('pending', 'running', 'passed', 'failed', 'cancelled'))
);

-- Test results table
CREATE TABLE test_results (
    id SERIAL PRIMARY KEY,
    run_id INT REFERENCES test_runs(id) ON DELETE CASCADE,
    case_id INT REFERENCES test_cases(id),
    status VARCHAR(20) NOT NULL,
    duration INT, -- milliseconds
    error_message TEXT,
    error_stack TEXT,
    screenshot_url TEXT,
    video_url TEXT,
    trace_url TEXT,
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_status CHECK (status IN ('passed', 'failed', 'skipped', 'error'))
);

-- Actions catalog table
CREATE TABLE actions (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    category VARCHAR(50) NOT NULL,
    description TEXT,
    parameters JSONB NOT NULL,
    implementation TEXT NOT NULL,
    version VARCHAR(20) NOT NULL DEFAULT '1.0.0',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_category CHECK (category IN (
        'navigation', 'input', 'verification', 'wait', 'data', 'api', 'database', 'custom'
    ))
);

-- Action results table
CREATE TABLE action_results (
    id SERIAL PRIMARY KEY,
    result_id INT REFERENCES test_results(id) ON DELETE CASCADE,
    action_id INT REFERENCES actions(id),
    action_name VARCHAR(255) NOT NULL,
    status VARCHAR(20) NOT NULL,
    duration INT, -- milliseconds
    input JSONB,
    output JSONB,
    error TEXT,
    screenshot_url TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_status CHECK (status IN ('passed', 'failed', 'skipped', 'error'))
);

-- AI models configuration table
CREATE TABLE ai_models (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    type VARCHAR(50) NOT NULL,
    endpoint TEXT,
    api_key_ref VARCHAR(255), -- Reference to secrets manager
    config JSONB DEFAULT '{}',
    is_active BOOLEAN DEFAULT TRUE,
    is_local BOOLEAN DEFAULT FALSE,
    performance_score DECIMAL(5,2),
    last_used TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_type CHECK (type IN ('llm', 'embedding', 'vision', 'judge'))
);

-- AI model evaluations table (Judge Model data)
CREATE TABLE ai_model_evaluations (
    id SERIAL PRIMARY KEY,
    model_id INT REFERENCES ai_models(id),
    evaluation_type VARCHAR(50) NOT NULL,
    input_hash VARCHAR(64),
    expected_output TEXT,
    actual_output TEXT,
    accuracy_score DECIMAL(5,4),
    latency_ms INT,
    tokens_used INT,
    evaluated_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_type CHECK (evaluation_type IN ('accuracy', 'latency', 'cost', 'quality'))
);

-- Vector embeddings table (for similarity search)
CREATE TABLE vector_embeddings (
    id SERIAL PRIMARY KEY,
    entity_type VARCHAR(50) NOT NULL,
    entity_id INT NOT NULL,
    embedding VECTOR(1536), -- OpenAI embedding dimension
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT valid_entity CHECK (entity_type IN ('test_case', 'defect', 'action', 'error'))
);

-- Defect patterns table (for root cause analysis)
CREATE TABLE defect_patterns (
    id SERIAL PRIMARY KEY,
    pattern_hash VARCHAR(64) NOT NULL UNIQUE,
    error_signature TEXT NOT NULL,
    root_cause TEXT,
    resolution TEXT,
    occurrence_count INT DEFAULT 1,
    last_seen TIMESTAMP DEFAULT NOW(),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Audit log table
CREATE TABLE audit_logs (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(id),
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(50),
    entity_id INT,
    old_value JSONB,
    new_value JSONB,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Create indexes for performance
CREATE INDEX idx_test_results_run ON test_results(run_id);
CREATE INDEX idx_test_results_status ON test_results(status);
CREATE INDEX idx_test_runs_suite ON test_runs(suite_id);
CREATE INDEX idx_test_runs_status ON test_runs(status);
CREATE INDEX idx_action_results_result ON action_results(result_id);
CREATE INDEX idx_vector_embeddings_entity ON vector_embeddings(entity_type, entity_id);
CREATE INDEX idx_defect_patterns_hash ON defect_patterns(pattern_hash);
CREATE INDEX idx_audit_logs_user ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_created ON audit_logs(created_at);
```

---

## 9. API Specifications

### 9.1 tRPC Router Structure

```typescript
// Main router structure
const appRouter = router({
  // Authentication
  auth: router({
    me: publicProcedure.query(...),
    logout: publicProcedure.mutation(...),
  }),
  
  // Test Management
  tests: router({
    suites: router({
      list: protectedProcedure.query(...),
      get: protectedProcedure.input(z.object({ id: z.number() })).query(...),
      create: protectedProcedure.input(suiteSchema).mutation(...),
      update: protectedProcedure.input(suiteUpdateSchema).mutation(...),
      delete: protectedProcedure.input(z.object({ id: z.number() })).mutation(...),
    }),
    cases: router({
      list: protectedProcedure.input(z.object({ suiteId: z.number() })).query(...),
      get: protectedProcedure.input(z.object({ id: z.number() })).query(...),
      create: protectedProcedure.input(caseSchema).mutation(...),
      update: protectedProcedure.input(caseUpdateSchema).mutation(...),
      delete: protectedProcedure.input(z.object({ id: z.number() })).mutation(...),
      import: protectedProcedure.input(importSchema).mutation(...),
    }),
  }),
  
  // Execution
  execution: router({
    run: protectedProcedure.input(runSchema).mutation(...),
    cancel: protectedProcedure.input(z.object({ runId: z.number() })).mutation(...),
    status: protectedProcedure.input(z.object({ runId: z.number() })).query(...),
    nodes: router({
      list: protectedProcedure.query(...),
      health: protectedProcedure.input(z.object({ nodeId: z.number() })).query(...),
      configure: adminProcedure.input(nodeConfigSchema).mutation(...),
    }),
  }),
  
  // Results & Analysis
  results: router({
    runs: router({
      list: protectedProcedure.input(runsFilterSchema).query(...),
      get: protectedProcedure.input(z.object({ id: z.number() })).query(...),
      compare: protectedProcedure.input(compareSchema).query(...),
    }),
    failures: router({
      list: protectedProcedure.input(failuresFilterSchema).query(...),
      analyze: protectedProcedure.input(z.object({ resultId: z.number() })).query(...),
      rootCause: protectedProcedure.input(z.object({ resultId: z.number() })).query(...),
    }),
    trends: router({
      passRate: protectedProcedure.input(trendFilterSchema).query(...),
      performance: protectedProcedure.input(trendFilterSchema).query(...),
      coverage: protectedProcedure.input(trendFilterSchema).query(...),
    }),
  }),
  
  // AI & Learning
  ai: router({
    models: router({
      list: adminProcedure.query(...),
      configure: adminProcedure.input(modelConfigSchema).mutation(...),
      evaluate: adminProcedure.input(z.object({ modelId: z.number() })).query(...),
    }),
    judge: router({
      recommendations: adminProcedure.query(...),
      generateTrainingData: adminProcedure.input(trainingDataSchema).mutation(...),
    }),
    selfHeal: router({
      suggest: protectedProcedure.input(z.object({ resultId: z.number() })).query(...),
      apply: protectedProcedure.input(healingSchema).mutation(...),
    }),
  }),
  
  // Administration
  admin: router({
    users: router({
      list: adminProcedure.query(...),
      updateRole: adminProcedure.input(roleUpdateSchema).mutation(...),
    }),
    audit: router({
      logs: adminProcedure.input(auditFilterSchema).query(...),
    }),
    system: router({
      health: adminProcedure.query(...),
      config: adminProcedure.input(systemConfigSchema).mutation(...),
    }),
  }),
});
```

### 9.2 REST API Endpoints (Legacy Support)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/suites` | List test suites | Bearer |
| POST | `/api/v1/suites` | Create test suite | Bearer |
| GET | `/api/v1/suites/:id` | Get test suite | Bearer |
| PUT | `/api/v1/suites/:id` | Update test suite | Bearer |
| DELETE | `/api/v1/suites/:id` | Delete test suite | Bearer |
| POST | `/api/v1/runs` | Start test run | Bearer |
| GET | `/api/v1/runs/:id` | Get run status | Bearer |
| POST | `/api/v1/runs/:id/cancel` | Cancel run | Bearer |
| GET | `/api/v1/results/:runId` | Get run results | Bearer |
| POST | `/api/v1/import/excel` | Import Excel tests | Bearer |
| GET | `/api/v1/nodes` | List execution nodes | Bearer |
| GET | `/api/v1/health` | System health check | None |

### 9.3 WebSocket Events

| Event | Direction | Payload | Description |
|-------|-----------|---------|-------------|
| `run:started` | Server→Client | `{ runId, suiteId }` | Test run started |
| `run:progress` | Server→Client | `{ runId, completed, total }` | Progress update |
| `run:completed` | Server→Client | `{ runId, status, summary }` | Run completed |
| `result:new` | Server→Client | `{ resultId, status }` | New test result |
| `node:status` | Server→Client | `{ nodeId, status }` | Node status change |
| `alert:new` | Server→Client | `{ alertId, severity, message }` | New alert |

---

## 10. Code Structure and Implementation

### 10.1 Project Directory Structure

```
grace-autonomous-testing/
├── apps/
│   ├── portal/                    # GRACE Portal (Next.js)
│   │   ├── src/
│   │   │   ├── pages/            # Next.js pages
│   │   │   ├── components/       # React components
│   │   │   ├── hooks/            # Custom hooks
│   │   │   ├── lib/              # Utilities
│   │   │   └── styles/           # CSS/Tailwind
│   │   ├── public/               # Static assets
│   │   └── package.json
│   │
│   ├── api/                       # API Server (Express + tRPC)
│   │   ├── src/
│   │   │   ├── routers/          # tRPC routers
│   │   │   ├── services/         # Business logic
│   │   │   ├── db/               # Database queries
│   │   │   └── middleware/       # Express middleware
│   │   └── package.json
│   │
│   └── executor/                  # Execution Node (Rust + TypeScript)
│       ├── src/
│       │   ├── runner/           # Action runner
│       │   ├── actions/          # Action implementations
│       │   ├── playwright/       # Playwright integration
│       │   └── mcp/              # TestDriver MCP
│       └── package.json
│
├── packages/
│   ├── shared/                    # Shared types and utilities
│   │   ├── src/
│   │   │   ├── types/            # TypeScript types
│   │   │   ├── schemas/          # Zod schemas
│   │   │   └── utils/            # Shared utilities
│   │   └── package.json
│   │
│   ├── actions/                   # Action library
│   │   ├── src/
│   │   │   ├── core/             # Core actions
│   │   │   ├── web/              # Web actions
│   │   │   ├── api/              # API actions
│   │   │   └── database/         # Database actions
│   │   └── package.json
│   │
│   └── ai/                        # AI components
│       ├── src/
│       │   ├── llm/              # LLM integration
│       │   ├── judge/            # Judge model
│       │   ├── healing/          # Self-healing
│       │   └── embeddings/       # Vector embeddings
│       └── package.json
│
├── services/
│   ├── nlp/                       # NLP Service (Python)
│   │   ├── src/
│   │   │   ├── parser/           # Test case parser
│   │   │   ├── intent/           # Intent recognition
│   │   │   └── mapping/          # Action mapping
│   │   └── requirements.txt
│   │
│   └── analytics/                 # Analytics Service
│       ├── src/
│       │   ├── collectors/       # Data collectors
│       │   ├── processors/       # Data processors
│       │   └── reporters/        # Report generators
│       └── package.json
│
├── infrastructure/
│   ├── docker/                    # Docker/Podman files
│   │   ├── portal.Dockerfile
│   │   ├── api.Dockerfile
│   │   ├── executor.Dockerfile
│   │   └── docker-compose.yml
│   │
│   ├── kubernetes/                # K8s manifests
│   │   ├── deployments/
│   │   ├── services/
│   │   └── configmaps/
│   │
│   └── terraform/                 # Infrastructure as Code
│       ├── modules/
│       └── environments/
│
├── scripts/
│   ├── setup.sh                   # Initial setup
│   ├── migrate.sh                 # Database migrations
│   ├── seed.sh                    # Seed data
│   └── deploy.sh                  # Deployment script
│
├── docs/
│   ├── architecture/              # Architecture docs
│   ├── api/                       # API documentation
│   └── guides/                    # User guides
│
├── tests/
│   ├── unit/                      # Unit tests
│   ├── integration/               # Integration tests
│   └── e2e/                       # End-to-end tests
│
├── drizzle/                       # Database migrations
│   ├── schema.ts
│   └── migrations/
│
├── package.json                   # Root package.json
├── pnpm-workspace.yaml            # pnpm workspace config
├── tsconfig.json                  # TypeScript config
└── README.md
```

### 10.2 Core Action Implementation

```typescript
// packages/actions/src/core/base-action.ts

/**
 * Base Action Class
 * 
 * All actions in the GRACE system inherit from this base class.
 * It provides common functionality for validation, execution,
 * logging, and error handling.
 * 
 * @abstract
 */
export abstract class BaseAction implements IAction {
  /**
   * Unique action name used for registration and lookup
   * Convention: PascalCase, verb-noun format (e.g., "EnterText", "ClickElement")
   */
  abstract readonly name: string;
  
  /**
   * Semantic version of the action implementation
   * Used for compatibility checking and migration
   */
  abstract readonly version: string;
  
  /**
   * Category for organization in the action catalog
   * Determines which interface layer handles execution
   */
  abstract readonly category: ActionCategory;
  
  /**
   * Human-readable description for documentation
   */
  abstract readonly description: string;
  
  /**
   * JSON Schema defining required and optional parameters
   */
  abstract readonly parameterSchema: z.ZodSchema;
  
  /**
   * Validates action parameters before execution
   * 
   * @param params - Parameters to validate
   * @returns ValidationResult with success status and any errors
   * 
   * Implementation notes:
   * - Uses Zod schema for type-safe validation
   * - Returns detailed error messages for debugging
   * - Called automatically by ActionRunner before execute()
   */
  validate(params: ActionParams): ValidationResult {
    const result = this.parameterSchema.safeParse(params);
    
    if (!result.success) {
      return {
        valid: false,
        errors: result.error.errors.map(e => ({
          path: e.path.join('.'),
          message: e.message,
        })),
      };
    }
    
    return { valid: true };
  }
  
  /**
   * Executes the action against the System Under Test
   * 
   * @param context - Execution context with page, browser, data
   * @param params - Validated action parameters
   * @returns Promise<ActionResult> with success status and metrics
   * 
   * Implementation notes:
   * - Must be idempotent where possible
   * - Should capture screenshots on failure
   * - Must handle timeouts gracefully
   * - Should log all significant operations
   */
  abstract execute(
    context: ExecutionContext,
    params: ActionParams
  ): Promise<ActionResult>;
  
  /**
   * Optional rollback method for reversible actions
   * Called when subsequent actions fail and cleanup is needed
   * 
   * @param context - Execution context
   */
  async rollback?(context: ExecutionContext): Promise<void>;
  
  /**
   * Helper method to capture screenshot on failure
   */
  protected async captureFailureScreenshot(
    context: ExecutionContext,
    actionName: string
  ): Promise<string | undefined> {
    try {
      const timestamp = Date.now();
      const filename = `failure-${actionName}-${timestamp}.png`;
      const path = `/tmp/screenshots/${filename}`;
      await context.page.screenshot({ path, fullPage: true });
      return path;
    } catch (error) {
      context.logger.warn('Failed to capture screenshot', { error });
      return undefined;
    }
  }
  
  /**
   * Helper method to create standardized action result
   */
  protected createResult(
    success: boolean,
    startTime: number,
    options?: Partial<ActionResult>
  ): ActionResult {
    return {
      success,
      duration: Date.now() - startTime,
      ...options,
    };
  }
}
```

### 10.3 EnterText Action Implementation

```typescript
// packages/actions/src/web/enter-text.action.ts

import { z } from 'zod';
import { BaseAction, ActionCategory, ExecutionContext, ActionResult } from '../core';

/**
 * EnterText Action
 * 
 * Enters text into an input field identified by a locator.
 * Supports various input types including text, password, and textarea.
 * 
 * @example
 * // Basic usage
 * { action: "EnterText", locator: "#username", text: "testuser" }
 * 
 * // With clear option
 * { action: "EnterText", locator: "#search", text: "query", clear: true }
 * 
 * // With delay for slow typing simulation
 * { action: "EnterText", locator: "#otp", text: "123456", delay: 100 }
 */
export class EnterTextAction extends BaseAction {
  readonly name = 'EnterText';
  readonly version = '1.0.0';
  readonly category = ActionCategory.INPUT;
  readonly description = 'Enter text into an input field';
  
  /**
   * Parameter schema for EnterText action
   * 
   * @property locator - Playwright locator string (CSS, text, role, etc.)
   * @property text - Text to enter into the field
   * @property clear - Whether to clear existing text first (default: true)
   * @property delay - Delay between keystrokes in ms (default: 0)
   * @property timeout - Maximum wait time for element in ms (default: 30000)
   */
  readonly parameterSchema = z.object({
    locator: z.string()
      .min(1, 'Locator is required')
      .describe('Playwright locator for the input element'),
    text: z.string()
      .describe('Text to enter into the field'),
    clear: z.boolean()
      .default(true)
      .describe('Clear existing text before entering'),
    delay: z.number()
      .min(0)
      .max(1000)
      .default(0)
      .describe('Delay between keystrokes in milliseconds'),
    timeout: z.number()
      .min(1000)
      .max(60000)
      .default(30000)
      .describe('Maximum wait time for element'),
  });
  
  /**
   * Execute the EnterText action
   * 
   * Execution flow:
   * 1. Locate the input element using the provided locator
   * 2. Wait for element to be visible and enabled
   * 3. Optionally clear existing text
   * 4. Enter the new text with optional delay
   * 5. Verify text was entered correctly
   * 
   * @param context - Execution context with Playwright page
   * @param params - Validated parameters
   * @returns ActionResult with success status and metrics
   */
  async execute(
    context: ExecutionContext,
    params: z.infer<typeof this.parameterSchema>
  ): Promise<ActionResult> {
    const startTime = Date.now();
    const { locator, text, clear, delay, timeout } = params;
    
    context.logger.info('Executing EnterText', { locator, textLength: text.length });
    
    try {
      // Step 1: Locate the element
      const element = context.page.locator(locator);
      
      // Step 2: Wait for element to be actionable
      await element.waitFor({ state: 'visible', timeout });
      
      // Step 3: Clear existing text if requested
      if (clear) {
        await element.clear();
      }
      
      // Step 4: Enter the text
      if (delay > 0) {
        // Type with delay for realistic simulation
        await element.pressSequentially(text, { delay });
      } else {
        // Fast fill for efficiency
        await element.fill(text);
      }
      
      // Step 5: Verify text was entered
      const actualValue = await element.inputValue();
      if (actualValue !== text) {
        context.logger.warn('Text verification failed', {
          expected: text,
          actual: actualValue,
        });
        
        return this.createResult(false, startTime, {
          error: new Error(`Text verification failed: expected "${text}", got "${actualValue}"`),
          screenshot: await this.captureFailureScreenshot(context, this.name),
        });
      }
      
      context.logger.info('EnterText completed successfully');
      
      return this.createResult(true, startTime, {
        data: { enteredText: text, locator },
      });
      
    } catch (error) {
      context.logger.error('EnterText failed', { error, locator });
      
      return this.createResult(false, startTime, {
        error: error as Error,
        screenshot: await this.captureFailureScreenshot(context, this.name),
      });
    }
  }
  
  /**
   * Rollback by clearing the entered text
   */
  async rollback(context: ExecutionContext): Promise<void> {
    // Note: Rollback requires storing the original value
    // This is a simplified implementation
    context.logger.info('EnterText rollback - clearing field');
  }
}
```

---

## 11. Installation and Configuration

### 11.1 Prerequisites

| Requirement | Version | Verification | Notes |
|-------------|---------|--------------|-------|
| Node.js | 22.x LTS | `node --version` | Required for all JS components |
| pnpm | 10.4.1+ | `pnpm --version` | Package manager |
| Rust | 1.75.0+ | `rustc --version` | High-performance components |
| Python | 3.11+ | `python3 --version` | NLP services |
| PostgreSQL | 16.2+ | `psql --version` | Primary database |
| Podman | 4.9.0+ | `podman --version` | Container runtime |
| Git | 2.x+ | `git --version` | Version control |

### 11.2 Installation Steps

```bash
# Step 1: Clone the repository
git clone https://github.com/Lev0n82/grace-autonomous-testing.git
cd grace-autonomous-testing

# Step 2: Install Node.js dependencies
pnpm install

# Step 3: Install Python dependencies
cd services/nlp
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
cd ../..

# Step 4: Build Rust components
cd apps/executor
cargo build --release
cd ../..

# Step 5: Configure environment
cp .env.example .env
# Edit .env with your configuration

# Step 6: Initialize database
pnpm db:push

# Step 7: Seed initial data
pnpm db:seed

# Step 8: Start development servers
pnpm dev
```

### 11.3 Environment Variables

```env
# =============================================================================
# GRACE Autonomous Testing System - Environment Configuration
# =============================================================================

# -----------------------------------------------------------------------------
# Database Configuration
# -----------------------------------------------------------------------------
# Primary PostgreSQL database for all application data
DATABASE_URL=postgresql://user:password@localhost:5432/grace_db

# Oracle database for legacy test case storage (optional)
ORACLE_CONNECTION_STRING=oracle://user:password@localhost:1521/TESTCASES

# Redis for caching and session management
REDIS_URL=redis://localhost:6379

# Vector database for AI embeddings
PINECONE_API_KEY=your-pinecone-api-key
PINECONE_ENVIRONMENT=us-east-1-aws
PINECONE_INDEX=grace-embeddings

# -----------------------------------------------------------------------------
# Authentication Configuration
# -----------------------------------------------------------------------------
# JWT secret for session tokens (minimum 32 characters)
JWT_SECRET=your-secure-jwt-secret-minimum-32-characters

# OAuth configuration for Manus authentication
VITE_APP_ID=your-manus-app-id
OAUTH_SERVER_URL=https://api.manus.im
VITE_OAUTH_PORTAL_URL=https://manus.im/oauth

# Owner identification
OWNER_OPEN_ID=your-owner-open-id
OWNER_NAME=Your Name

# -----------------------------------------------------------------------------
# AI Model Configuration
# -----------------------------------------------------------------------------
# Local AI (Ollama)
OLLAMA_BASE_URL=http://localhost:11434
OLLAMA_MODEL=llama2:13b

# Cloud AI (optional, for complex analysis)
ANTHROPIC_API_KEY=your-anthropic-api-key
OPENAI_API_KEY=your-openai-api-key

# AI model selection (local, cloud, hybrid)
AI_MODE=hybrid

# -----------------------------------------------------------------------------
# Execution Configuration
# -----------------------------------------------------------------------------
# Maximum parallel test executions per node
MAX_PARALLEL_EXECUTIONS=4

# Default timeout for test actions (milliseconds)
DEFAULT_ACTION_TIMEOUT=30000

# Screenshot capture on failure
CAPTURE_SCREENSHOTS=true
CAPTURE_VIDEOS=true

# -----------------------------------------------------------------------------
# Storage Configuration
# -----------------------------------------------------------------------------
# Azure Git for artifact storage
AZURE_STORAGE_CONNECTION_STRING=your-azure-connection-string
AZURE_CONTAINER_NAME=grace-artifacts

# Local storage path (for development)
LOCAL_STORAGE_PATH=/var/grace/artifacts

# -----------------------------------------------------------------------------
# Notification Configuration
# -----------------------------------------------------------------------------
# Email notifications
SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP_USER=notifications@example.com
SMTP_PASSWORD=your-smtp-password

# Slack integration
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/xxx/yyy/zzz

# -----------------------------------------------------------------------------
# Security Configuration
# -----------------------------------------------------------------------------
# Enable security scanning
SECURITY_SCANNING_ENABLED=true

# SQL injection testing
SQL_INJECTION_TESTING=true

# Penetration testing module
PENTEST_MODULE_ENABLED=true

# -----------------------------------------------------------------------------
# Monitoring Configuration
# -----------------------------------------------------------------------------
# Prometheus metrics endpoint
METRICS_ENABLED=true
METRICS_PORT=9090

# Grafana dashboard
GRAFANA_URL=http://localhost:3001

# -----------------------------------------------------------------------------
# Application Configuration
# -----------------------------------------------------------------------------
# Application title and branding
VITE_APP_TITLE=GRACE Autonomous Testing
VITE_APP_LOGO=/images/grace-logo.png

# Portal port
PORTAL_PORT=3000

# API port
API_PORT=4000

# Executor port
EXECUTOR_PORT=5000
```

### 11.4 Verification Checklist

| Check | Expected Result | Command |
|-------|-----------------|---------|
| Node.js installed | v22.x.x | `node --version` |
| pnpm installed | v10.x.x | `pnpm --version` |
| Database connected | Connection successful | `pnpm db:check` |
| Migrations applied | All migrations up | `pnpm db:status` |
| Portal starts | http://localhost:3000 | `pnpm dev:portal` |
| API starts | http://localhost:4000 | `pnpm dev:api` |
| Executor starts | http://localhost:5000 | `pnpm dev:executor` |
| Tests pass | All green | `pnpm test` |
| Playwright works | Browser launches | `pnpm test:e2e` |
| AI models respond | Response received | `pnpm test:ai` |

---

## 12. Security Specifications

### 12.1 Security Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           SECURITY ARCHITECTURE                                  │
│                                                                                  │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │                    PERIMETER SECURITY                                    │   │
│  │                                                                          │   │
│  │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐   │   │
│  │   │   WAF       │  │   DDoS      │  │   Rate      │  │   IP        │   │   │
│  │   │   Rules     │  │   Protection│  │   Limiting  │  │   Filtering │   │   │
│  │   └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘   │   │
│  │                                                                          │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                      │                                          │
│                                      ▼                                          │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │                    APPLICATION SECURITY                                  │   │
│  │                                                                          │   │
│  │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐   │   │
│  │   │   OAuth     │  │   JWT       │  │   RBAC      │  │   Input     │   │   │
│  │   │   2.0       │  │   Tokens    │  │   Policies  │  │   Validation│   │   │
│  │   └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘   │   │
│  │                                                                          │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                      │                                          │
│                                      ▼                                          │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │                    DATA SECURITY                                         │   │
│  │                                                                          │   │
│  │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐   │   │
│  │   │   Encryption│  │   Data      │  │   Secrets   │  │   Audit     │   │   │
│  │   │   at Rest   │  │   Masking   │  │   Manager   │  │   Logging   │   │   │
│  │   └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘   │   │
│  │                                                                          │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### 12.2 Security Testing Modules

**SQL Injection Module:**
```typescript
// Mandatory security testing module for SQL injection
const sqlInjectionTests = [
  "' OR '1'='1",
  "'; DROP TABLE users; --",
  "1; SELECT * FROM users",
  "' UNION SELECT * FROM passwords --",
  "admin'--",
];
```

**Penetration Testing Module:**
```typescript
// Mandatory penetration testing module
const pentestChecks = [
  'xss_reflected',
  'xss_stored',
  'csrf_token_validation',
  'session_fixation',
  'insecure_direct_object_reference',
  'security_misconfiguration',
];
```

### 12.3 Human-in-the-Loop Requirements

| Operation | Human Approval Required | Reason |
|-----------|------------------------|--------|
| Security test execution | Yes | Potential system impact |
| Production deployment | Yes | Critical operation |
| User role elevation | Yes | Access control |
| AI model replacement | Yes | Quality assurance |
| Data deletion | Yes | Irreversible action |

---

## 13. Testing and Quality Assurance

### 13.1 Test Pyramid

```
                    ┌───────────────┐
                    │     E2E       │  ← 10% (Critical paths)
                    │    Tests      │
                    └───────┬───────┘
                            │
                    ┌───────┴───────┐
                    │  Integration  │  ← 30% (API, DB)
                    │    Tests      │
                    └───────┬───────┘
                            │
            ┌───────────────┴───────────────┐
            │         Unit Tests            │  ← 60% (Functions, classes)
            │                               │
            └───────────────────────────────┘
```

### 13.2 Multi-Level Success Criteria

**Function Level:**
```typescript
// Every function must have:
// 1. Input validation tests
// 2. Happy path tests
// 3. Error handling tests
// 4. Edge case tests

describe('EnterTextAction.execute', () => {
  it('should enter text successfully', async () => { /* ... */ });
  it('should handle missing element', async () => { /* ... */ });
  it('should handle timeout', async () => { /* ... */ });
  it('should handle empty text', async () => { /* ... */ });
});
```

**Class Level:**
```typescript
// Every class must have:
// 1. Constructor tests
// 2. Method interaction tests
// 3. State management tests
// 4. Lifecycle tests

describe('ActionRunner', () => {
  describe('constructor', () => { /* ... */ });
  describe('registerAction', () => { /* ... */ });
  describe('executeAction', () => { /* ... */ });
  describe('cleanup', () => { /* ... */ });
});
```

**Module Level:**
```typescript
// Every module must have:
// 1. Integration tests with dependencies
// 2. API contract tests
// 3. Performance benchmarks
// 4. Security scans

describe('Execution Module', () => {
  describe('integration with Action Runner', () => { /* ... */ });
  describe('API contracts', () => { /* ... */ });
  describe('performance', () => { /* ... */ });
  describe('security', () => { /* ... */ });
});
```

**System Level:**
```typescript
// System must have:
// 1. End-to-end workflow tests
// 2. Load tests
// 3. Chaos engineering tests
// 4. Accessibility tests (WCAG 2.2 AAA)

describe('GRACE System', () => {
  describe('complete test execution workflow', () => { /* ... */ });
  describe('under load', () => { /* ... */ });
  describe('node failure recovery', () => { /* ... */ });
  describe('accessibility compliance', () => { /* ... */ });
});
```

### 13.3 Built-in Self-Testing

```typescript
// Every component must include self-testing capability
interface SelfTestable {
  runSelfTest(): Promise<SelfTestResult>;
  getHealthStatus(): HealthStatus;
}

class ActionRunner implements SelfTestable {
  async runSelfTest(): Promise<SelfTestResult> {
    const results: TestResult[] = [];
    
    // Test 1: Action registration
    results.push(await this.testActionRegistration());
    
    // Test 2: Action execution
    results.push(await this.testActionExecution());
    
    // Test 3: Error handling
    results.push(await this.testErrorHandling());
    
    // Test 4: Performance
    results.push(await this.testPerformance());
    
    return {
      component: 'ActionRunner',
      passed: results.every(r => r.passed),
      results,
      timestamp: new Date(),
    };
  }
}
```

---

## 14. Deployment Architecture

### 14.1 Multi-Stage Deployment Pipeline

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        DEPLOYMENT PIPELINE                                       │
│                                                                                  │
│  ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐       │
│  │   DEV   │───▶│   IST   │───▶│   UAT   │───▶│  STAGE  │───▶│  PROD   │       │
│  │         │    │         │    │         │    │  (10%)  │    │  (90%)  │       │
│  └─────────┘    └─────────┘    └─────────┘    └─────────┘    └─────────┘       │
│       │              │              │              │              │             │
│       ▼              ▼              ▼              ▼              ▼             │
│  ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐       │
│  │  Unit   │    │  Integ  │    │   E2E   │    │ Canary  │    │  Full   │       │
│  │  Tests  │    │  Tests  │    │  Tests  │    │ Monitor │    │ Release │       │
│  └─────────┘    └─────────┘    └─────────┘    └─────────┘    └─────────┘       │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### 14.2 On-Premise Deployment Option

The system MUST support on-premise deployment independent of cloud services:

```yaml
# On-premise deployment configuration
deployment:
  mode: on-premise
  
  components:
    portal:
      replicas: 2
      resources:
        cpu: 2
        memory: 4Gi
    
    api:
      replicas: 3
      resources:
        cpu: 4
        memory: 8Gi
    
    executor:
      replicas: 10
      resources:
        cpu: 4
        memory: 8Gi
    
    database:
      type: postgresql
      replicas: 3
      storage: 500Gi
    
    ai:
      type: ollama
      models:
        - llama2:13b
        - codellama:7b
      gpu: required
```

---

## 15. Appendices

### 15.1 Glossary

| Term | Definition |
|------|------------|
| **ABT** | Action-Based Testing - methodology separating tests into modules, actions, and interface |
| **Action** | Reusable, parameterized test operation |
| **Action Runner** | Central dispatcher for executing actions |
| **GRACE** | Governed Resilient Autonomous Certification for Enterprises |
| **Judge Model** | AI model that evaluates and improves other AI models |
| **MCP** | Model Context Protocol - standard for AI tool integration |
| **Self-Healing** | Automatic adaptation to UI changes |
| **SUT** | System Under Test |
| **TestDriver MCP** | MCP implementation for test automation |
| **Vector DB** | Database optimized for similarity search on embeddings |

### 15.2 References

1. Playwright Documentation: https://playwright.dev
2. Model Context Protocol: https://modelcontextprotocol.io
3. Action-Based Testing (LogiGear): https://www.logigear.com/abt
4. tRPC Documentation: https://trpc.io
5. Drizzle ORM: https://orm.drizzle.team
6. WCAG 2.2 Guidelines: https://www.w3.org/WAI/WCAG22/quickref/
7. Podman Documentation: https://podman.io

### 15.3 Change Log

| Version | Date | Changes |
|---------|------|---------|
| 4.0.0 | 2026-01-31 | Complete DDD with all specifications |
| 3.0.0 | 2025-12-18 | Integrated TestDriver MCP |
| 2.0.0 | 2025-11-20 | Added ABT three-layer model |
| 1.0.0 | 2025-10-15 | Initial architecture concept |

---

**Document End**

*This Detailed Design Document provides the complete specification for the GRACE Autonomous Testing System v4. Any AI agent or human developer can use this document to replicate the entire system without variance. All architectural decisions, code structures, and configuration requirements are fully documented with rationale.*

*Classification: CONFIDENTIAL - For authorized personnel only*

---

# SECTION 3: DECISION TREES AND EXPLORED OPTIONS

## 3.1 Decision Tree Overview

This section documents every major architectural decision made during the development of the GRACE Autonomous Testing System v4. Each decision includes:
- The problem statement
- All options explored
- Evaluation criteria
- The winning decision with rationale
- Implementation implications

---

## 3.2 Decision Tree #1: Core Automation Framework Selection

### Problem Statement
Select the primary browser automation framework that will serve as the foundation for all test execution capabilities.

### Options Explored

| Option | Description | Pros | Cons |
|--------|-------------|------|------|
| **Option A: Selenium WebDriver** | Industry standard, mature ecosystem | Wide browser support, large community, extensive documentation | Slower execution, complex setup, flaky waits |
| **Option B: Cypress** | Modern JavaScript-based framework | Fast execution, excellent DX, built-in retry | Single browser tab limitation, no cross-origin, JS only |
| **Option C: Playwright** | Microsoft's modern automation library | Multi-browser, auto-wait, network interception, codegen | Newer ecosystem, learning curve |
| **Option D: Puppeteer** | Chrome DevTools Protocol wrapper | Fast, lightweight, good for Chrome | Chrome-only, limited cross-browser |

### Evaluation Criteria
1. **Multi-browser support** (Weight: 25%)
2. **Execution speed** (Weight: 20%)
3. **API stability and auto-waiting** (Weight: 20%)
4. **Network interception capabilities** (Weight: 15%)
5. **AI/MCP integration potential** (Weight: 20%)

### Decision Matrix

| Criteria | Weight | Selenium | Cypress | Playwright | Puppeteer |
|----------|--------|----------|---------|------------|-----------|
| Multi-browser | 25% | 9 | 4 | 10 | 3 |
| Execution speed | 20% | 5 | 8 | 9 | 9 |
| API stability | 20% | 6 | 8 | 9 | 7 |
| Network interception | 15% | 4 | 7 | 10 | 8 |
| AI/MCP integration | 20% | 5 | 6 | 10 | 7 |
| **Weighted Score** | 100% | **5.85** | **6.45** | **9.60** | **6.55** |

### **WINNER: Option C - Playwright**

### Rationale
Playwright was selected as the core automation framework for the following reasons:

1. **Native MCP Support**: Playwright has first-class Model Context Protocol integration through `@anthropic/mcp-playwright`, enabling direct AI agent control of browser sessions.

2. **Auto-Wait Intelligence**: Playwright's actionability checks eliminate flaky tests by automatically waiting for elements to be visible, enabled, stable, and not obscured.

3. **Multi-Browser Parity**: Single API works identically across Chromium, Firefox, and WebKit, ensuring cross-browser test coverage.

4. **Network Interception**: Built-in `page.route()` enables sophisticated API mocking, request modification, and response stubbing essential for isolated testing.

5. **Trace Viewer**: Built-in debugging with screenshots, DOM snapshots, and network logs at each step.

### Implementation Specification
```
Package: playwright
Version: 1.40.0
Installation: npm install playwright @playwright/test
Browser Binaries: npx playwright install chromium firefox webkit
```

---

## 3.3 Decision Tree #2: AI Integration Protocol

### Problem Statement
Select the protocol/interface for enabling AI agents to control and orchestrate test automation.

### Options Explored

| Option | Description | Pros | Cons |
|--------|-------------|------|------|
| **Option A: Custom REST API** | Build proprietary AI-to-automation bridge | Full control, custom optimization | High development cost, maintenance burden |
| **Option B: LangChain Tools** | Use LangChain's tool abstraction | Python ecosystem, agent frameworks | Python-centric, overhead, vendor lock-in |
| **Option C: Model Context Protocol (MCP)** | Anthropic's standardized AI-tool protocol | Standardized, multi-model support, growing ecosystem | Newer standard, evolving spec |
| **Option D: OpenAI Function Calling** | Use OpenAI's native function calling | Simple integration, well-documented | OpenAI-only, no local model support |

### Evaluation Criteria
1. **Model agnosticism** (Weight: 30%)
2. **Standardization and longevity** (Weight: 25%)
3. **Ecosystem and tooling** (Weight: 20%)
4. **Implementation complexity** (Weight: 15%)
5. **Human-in-the-loop support** (Weight: 10%)

### Decision Matrix

| Criteria | Weight | Custom REST | LangChain | MCP | OpenAI FC |
|----------|--------|-------------|-----------|-----|-----------|
| Model agnosticism | 30% | 10 | 7 | 10 | 2 |
| Standardization | 25% | 3 | 6 | 9 | 7 |
| Ecosystem | 20% | 2 | 8 | 7 | 8 |
| Implementation | 15% | 4 | 6 | 8 | 9 |
| Human-in-loop | 10% | 8 | 5 | 9 | 4 |
| **Weighted Score** | 100% | **5.35** | **6.55** | **8.65** | **5.55** |

### **WINNER: Option C - Model Context Protocol (MCP)**

### Rationale
MCP was selected as the AI integration protocol for the following reasons:

1. **Model Agnosticism**: MCP works with Claude, GPT-4, Gemini, Llama, and any future LLM, preventing vendor lock-in.

2. **Standardized Interface**: The protocol defines clear schemas for tools, resources, and prompts, enabling interoperability.

3. **Human-in-the-Loop Native**: MCP's design includes approval workflows and confirmation steps, essential for autonomous testing with human oversight.

4. **TestDriver MCP Availability**: Pre-built `@anthropic/mcp-playwright` and `testdriver-mcp` packages provide immediate integration.

5. **Bidirectional Communication**: MCP supports both AI-to-tool and tool-to-AI communication, enabling sophisticated feedback loops.

### Implementation Specification
```
Package: @anthropic/mcp-playwright
Version: 0.1.0
Protocol Version: MCP 1.0
Transport: stdio (local) or SSE (remote)
```

---

## 3.4 Decision Tree #3: Test Organization Paradigm

### Problem Statement
Select the architectural pattern for organizing test logic, actions, and interfaces.

### Options Explored

| Option | Description | Pros | Cons |
|--------|-------------|------|------|
| **Option A: Page Object Model (POM)** | Encapsulate page elements in classes | Industry standard, good encapsulation | Rigid hierarchy, duplication across pages |
| **Option B: Screenplay Pattern** | Actor-task-ability abstraction | Highly readable, flexible | Complex setup, steep learning curve |
| **Option C: Action-Based Testing (ABT)** | Three-layer architecture (Modules/Actions/Interface) | Maximum reusability, clear separation | Requires upfront design investment |
| **Option D: Keyword-Driven Testing** | External keyword tables drive execution | Non-technical authoring, data-driven | Limited flexibility, maintenance overhead |

### Evaluation Criteria
1. **Reusability of components** (Weight: 30%)
2. **Maintainability at scale** (Weight: 25%)
3. **AI agent comprehension** (Weight: 20%)
4. **Human readability** (Weight: 15%)
5. **Onboarding complexity** (Weight: 10%)

### Decision Matrix

| Criteria | Weight | POM | Screenplay | ABT | Keyword |
|----------|--------|-----|------------|-----|---------|
| Reusability | 30% | 6 | 8 | 10 | 7 |
| Maintainability | 25% | 6 | 7 | 9 | 5 |
| AI comprehension | 20% | 5 | 6 | 9 | 8 |
| Human readability | 15% | 7 | 9 | 8 | 9 |
| Onboarding | 10% | 8 | 4 | 6 | 7 |
| **Weighted Score** | 100% | **6.15** | **7.00** | **8.85** | **7.00** |

### **WINNER: Option C - Action-Based Testing (ABT)**

### Rationale
The ABT three-layer architecture was selected for the following reasons:

1. **Maximum Reusability**: Actions are atomic, composable units that can be shared across hundreds of test modules without duplication.

2. **Clear Abstraction Boundaries**: 
   - **Test Modules**: Business logic and test scenarios
   - **Actions**: Reusable behavioral components
   - **Interface**: Technical implementation details

3. **AI-Friendly Structure**: The hierarchical organization maps naturally to how LLMs understand and generate test logic.

4. **Maintainability**: Changes to UI selectors only affect the Interface layer; business logic remains untouched.

5. **Scalability**: Proven to scale to 10,000+ test cases in enterprise environments.

### Implementation Specification
```
Directory Structure:
├── test-modules/          # Business test scenarios
│   ├── checkout/
│   ├── user-management/
│   └── search/
├── actions/               # Reusable action components
│   ├── navigation.ts
│   ├── authentication.ts
│   ├── forms.ts
│   └── verification.ts
└── interface/             # Technical adapters
    ├── web-driver.ts
    ├── api-connector.ts
    └── database-access.ts
```

---

## 3.5 Decision Tree #4: Distributed Execution Architecture

### Problem Statement
Select the architecture for distributing test execution across multiple nodes for parallel processing.

### Options Explored

| Option | Description | Pros | Cons |
|--------|-------------|------|------|
| **Option A: Selenium Grid** | Hub-and-node architecture | Mature, widely adopted | Complex setup, resource heavy |
| **Option B: Kubernetes Jobs** | Container orchestration | Scalable, cloud-native | Infrastructure complexity, cost |
| **Option C: PostgreSQL Job Queue** | Database-backed job distribution | Simple, transactional, observable | Single point of failure, polling overhead |
| **Option D: Redis + Bull Queue** | In-memory job queue | Fast, real-time, pub/sub | Data persistence concerns, complexity |
| **Option E: AWS Step Functions** | Serverless orchestration | Managed, scalable, visual | Vendor lock-in, cold starts, cost |

### Evaluation Criteria
1. **Simplicity of implementation** (Weight: 25%)
2. **Observability and debugging** (Weight: 20%)
3. **Transactional integrity** (Weight: 20%)
4. **Scalability** (Weight: 20%)
5. **Cost efficiency** (Weight: 15%)

### Decision Matrix

| Criteria | Weight | Selenium Grid | K8s Jobs | PostgreSQL | Redis/Bull | AWS Step |
|----------|--------|---------------|----------|------------|------------|----------|
| Simplicity | 25% | 4 | 3 | 9 | 6 | 5 |
| Observability | 20% | 5 | 6 | 9 | 7 | 8 |
| Transactional | 20% | 3 | 5 | 10 | 4 | 7 |
| Scalability | 20% | 7 | 10 | 7 | 9 | 10 |
| Cost efficiency | 15% | 5 | 4 | 9 | 7 | 3 |
| **Weighted Score** | 100% | **4.75** | **5.50** | **8.75** | **6.50** | **6.55** |

### **WINNER: Option C - PostgreSQL Job Queue**

### Rationale
PostgreSQL-based job queue was selected for the following reasons:

1. **Transactional Guarantees**: ACID compliance ensures no job is lost or duplicated, critical for test result integrity.

2. **Built-in Observability**: SQL queries provide instant visibility into job status, history, and performance metrics.

3. **Simplicity**: No additional infrastructure required; leverages existing database.

4. **SKIP LOCKED Pattern**: PostgreSQL's `SELECT FOR UPDATE SKIP LOCKED` enables efficient concurrent job claiming without contention.

5. **Audit Trail**: Complete history of all test executions stored in queryable format.

### Implementation Specification
```sql
-- Job Queue Table Schema
CREATE TABLE test_jobs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    status VARCHAR(20) DEFAULT 'pending',
    priority INTEGER DEFAULT 5,
    test_module_id VARCHAR(100) NOT NULL,
    input_parameters JSONB,
    assigned_node VARCHAR(100),
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    result JSONB,
    error_message TEXT,
    retry_count INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Index for efficient job claiming
CREATE INDEX idx_jobs_status_priority 
ON test_jobs(status, priority DESC, created_at ASC)
WHERE status = 'pending';
```

---

## 3.6 Decision Tree #5: AI Model Selection for Test Intelligence

### Problem Statement
Select the primary AI model(s) for powering autonomous test generation, analysis, and decision-making.

### Options Explored

| Option | Description | Pros | Cons |
|--------|-------------|------|------|
| **Option A: GPT-4 Turbo** | OpenAI's flagship model | Strong reasoning, large context | Cost, rate limits, no local option |
| **Option B: Claude 3.5 Sonnet** | Anthropic's balanced model | Excellent coding, MCP native, safety | API-only, Anthropic dependency |
| **Option C: Gemini 1.5 Pro** | Google's multimodal model | Large context, multimodal | Newer, less testing ecosystem |
| **Option D: Llama 3 70B** | Meta's open-source model | Local deployment, no API costs | Resource intensive, less capable |
| **Option E: Hybrid Approach** | Multiple models for different tasks | Best of each model, redundancy | Complexity, inconsistency |

### Evaluation Criteria
1. **Code generation quality** (Weight: 25%)
2. **MCP/Tool use capability** (Weight: 25%)
3. **Context window size** (Weight: 15%)
4. **Cost efficiency** (Weight: 15%)
5. **Deployment flexibility** (Weight: 20%)

### Decision Matrix

| Criteria | Weight | GPT-4 | Claude 3.5 | Gemini 1.5 | Llama 3 | Hybrid |
|----------|--------|-------|------------|------------|---------|--------|
| Code generation | 25% | 9 | 10 | 8 | 7 | 9 |
| MCP/Tool use | 25% | 7 | 10 | 6 | 5 | 8 |
| Context window | 15% | 7 | 8 | 10 | 7 | 8 |
| Cost efficiency | 15% | 5 | 7 | 6 | 10 | 6 |
| Deployment flex | 20% | 4 | 5 | 5 | 10 | 7 |
| **Weighted Score** | 100% | **6.60** | **8.25** | **6.85** | **7.35** | **7.65** |

### **WINNER: Option B - Claude 3.5 Sonnet (Primary) with Option E - Hybrid Fallback**

### Rationale
Claude 3.5 Sonnet was selected as the primary model with hybrid fallback for the following reasons:

1. **Native MCP Support**: Claude was designed alongside MCP, providing seamless tool integration.

2. **Superior Code Generation**: Benchmarks show Claude 3.5 Sonnet excels at TypeScript/JavaScript code generation, essential for Playwright tests.

3. **Safety and Alignment**: Claude's constitutional AI approach reduces harmful or incorrect test generation.

4. **Hybrid Fallback**: GPT-4 serves as fallback for specific tasks; Llama 3 for offline/air-gapped environments.

### Implementation Specification
```typescript
// Model Configuration
const AI_CONFIG = {
  primary: {
    provider: 'anthropic',
    model: 'claude-3-5-sonnet-20241022',
    maxTokens: 8192,
    temperature: 0.1  // Low temperature for deterministic test generation
  },
  fallback: {
    provider: 'openai',
    model: 'gpt-4-turbo-preview',
    maxTokens: 4096,
    temperature: 0.1
  },
  offline: {
    provider: 'ollama',
    model: 'llama3:70b',
    maxTokens: 4096,
    temperature: 0.1
  }
};
```

---

## 3.7 Decision Tree #6: Knowledge Storage and Retrieval

### Problem Statement
Select the storage system for test knowledge, patterns, and historical data to enable AI learning and retrieval.

### Options Explored

| Option | Description | Pros | Cons |
|--------|-------------|------|------|
| **Option A: PostgreSQL + pgvector** | Vector extension for PostgreSQL | Single database, SQL familiar | Limited vector operations |
| **Option B: Pinecone** | Managed vector database | Fast, scalable, managed | Cost, vendor lock-in |
| **Option C: ChromaDB** | Open-source embedding database | Simple, local-first, Python native | Less mature, limited scale |
| **Option D: Weaviate** | Open-source vector search | GraphQL API, hybrid search | Complex setup, resource heavy |
| **Option E: Qdrant** | High-performance vector DB | Fast, Rust-based, filtering | Newer ecosystem |

### Evaluation Criteria
1. **Integration simplicity** (Weight: 25%)
2. **Query performance** (Weight: 20%)
3. **Hybrid search (vector + metadata)** (Weight: 20%)
4. **Self-hosted option** (Weight: 20%)
5. **Cost** (Weight: 15%)

### Decision Matrix

| Criteria | Weight | pgvector | Pinecone | ChromaDB | Weaviate | Qdrant |
|----------|--------|----------|----------|----------|----------|--------|
| Integration | 25% | 10 | 7 | 8 | 5 | 7 |
| Performance | 20% | 6 | 10 | 6 | 8 | 9 |
| Hybrid search | 20% | 8 | 7 | 6 | 9 | 9 |
| Self-hosted | 20% | 10 | 0 | 10 | 8 | 10 |
| Cost | 15% | 10 | 3 | 10 | 7 | 9 |
| **Weighted Score** | 100% | **8.80** | **5.55** | **7.90** | **7.25** | **8.65** |

### **WINNER: Option A - PostgreSQL + pgvector**

### Rationale
PostgreSQL with pgvector extension was selected for the following reasons:

1. **Unified Data Layer**: All data (jobs, results, vectors) in single database simplifies operations.

2. **SQL Familiarity**: Team can use existing SQL skills for vector queries.

3. **Hybrid Queries**: Combine vector similarity with traditional WHERE clauses in single query.

4. **Zero Additional Infrastructure**: Leverages existing PostgreSQL deployment.

5. **ACID Compliance**: Transactional guarantees for knowledge updates.

### Implementation Specification
```sql
-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Knowledge Base Table
CREATE TABLE test_knowledge (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    content_type VARCHAR(50) NOT NULL,  -- 'action', 'pattern', 'failure', 'selector'
    content TEXT NOT NULL,
    embedding vector(1536),  -- OpenAI ada-002 dimensions
    metadata JSONB,
    usage_count INTEGER DEFAULT 0,
    success_rate DECIMAL(5,4),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Vector similarity index
CREATE INDEX idx_knowledge_embedding 
ON test_knowledge 
USING ivfflat (embedding vector_cosine_ops)
WITH (lists = 100);

-- Hybrid search query example
SELECT content, metadata,
       1 - (embedding <=> $1::vector) as similarity
FROM test_knowledge
WHERE content_type = 'action'
  AND (metadata->>'domain')::text = 'e-commerce'
ORDER BY embedding <=> $1::vector
LIMIT 10;
```

---

## 3.8 Decision Tree #7: Human-in-the-Loop Implementation

### Problem Statement
Design the mechanism for human oversight, approval, and intervention in autonomous test execution.

### Options Explored

| Option | Description | Pros | Cons |
|--------|-------------|------|------|
| **Option A: Approval Queue** | All AI actions require human approval | Maximum safety | Slow, defeats automation purpose |
| **Option B: Confidence Threshold** | Auto-execute above threshold, queue below | Balanced, adaptive | Threshold tuning complexity |
| **Option C: Risk-Based Routing** | Route by action risk level | Targeted oversight | Risk classification overhead |
| **Option D: Audit-Only** | Execute all, human reviews after | Fast execution | Potential damage before review |
| **Option E: Hybrid Confidence + Risk** | Combine confidence and risk factors | Comprehensive, nuanced | Implementation complexity |

### Evaluation Criteria
1. **Safety assurance** (Weight: 30%)
2. **Execution efficiency** (Weight: 25%)
3. **Adaptability** (Weight: 20%)
4. **User experience** (Weight: 15%)
5. **Implementation complexity** (Weight: 10%)

### Decision Matrix

| Criteria | Weight | Approval Queue | Confidence | Risk-Based | Audit-Only | Hybrid |
|----------|--------|----------------|------------|------------|------------|--------|
| Safety | 30% | 10 | 7 | 8 | 3 | 9 |
| Efficiency | 25% | 2 | 8 | 7 | 10 | 7 |
| Adaptability | 20% | 3 | 8 | 7 | 5 | 9 |
| User experience | 15% | 4 | 7 | 6 | 8 | 7 |
| Implementation | 10% | 9 | 7 | 5 | 9 | 4 |
| **Weighted Score** | 100% | **5.55** | **7.45** | **6.90** | **6.35** | **7.85** |

### **WINNER: Option E - Hybrid Confidence + Risk**

### Rationale
The hybrid approach combining confidence scoring and risk classification was selected:

1. **Nuanced Decision Making**: High-confidence, low-risk actions execute automatically; others require review.

2. **Adaptive Learning**: System learns from human decisions to improve future confidence scores.

3. **Risk Categories**:
   - **Low Risk**: Read-only operations, assertions, navigation
   - **Medium Risk**: Form submissions, data entry, clicks
   - **High Risk**: Deletions, payments, authentication changes

4. **Confidence Thresholds**:
   - **Auto-execute**: Confidence ≥ 0.95 AND Risk = Low
   - **Auto-execute with logging**: Confidence ≥ 0.85 AND Risk ≤ Medium
   - **Human approval required**: Confidence < 0.85 OR Risk = High

### Implementation Specification
```typescript
interface ActionDecision {
  action: string;
  confidence: number;  // 0.0 - 1.0
  riskLevel: 'low' | 'medium' | 'high';
  reasoning: string;
  requiresApproval: boolean;
}

function evaluateAction(action: ActionDecision): ExecutionMode {
  const { confidence, riskLevel } = action;
  
  // Decision matrix
  if (riskLevel === 'high') {
    return 'REQUIRE_APPROVAL';
  }
  
  if (confidence >= 0.95 && riskLevel === 'low') {
    return 'AUTO_EXECUTE';
  }
  
  if (confidence >= 0.85 && riskLevel !== 'high') {
    return 'AUTO_EXECUTE_WITH_LOGGING';
  }
  
  return 'REQUIRE_APPROVAL';
}
```

---

## 3.9 Decision Tree #8: Test Result Analysis Engine

### Problem Statement
Select the approach for analyzing test results, identifying patterns, and generating insights.

### Options Explored

| Option | Description | Pros | Cons |
|--------|-------------|------|------|
| **Option A: Rule-Based Analysis** | Predefined rules for failure categorization | Predictable, explainable | Limited to known patterns |
| **Option B: ML Classification** | Train models on historical failures | Learns new patterns | Training data requirement |
| **Option C: LLM-Powered Analysis** | Use LLM to analyze failures | Flexible, contextual | Cost, latency |
| **Option D: Hybrid Rules + LLM** | Rules for common cases, LLM for complex | Best of both, cost-effective | Integration complexity |

### Evaluation Criteria
1. **Accuracy of analysis** (Weight: 30%)
2. **Handling novel failures** (Weight: 25%)
3. **Explainability** (Weight: 20%)
4. **Cost efficiency** (Weight: 15%)
5. **Latency** (Weight: 10%)

### Decision Matrix

| Criteria | Weight | Rule-Based | ML Classification | LLM-Powered | Hybrid |
|----------|--------|------------|-------------------|-------------|--------|
| Accuracy | 30% | 7 | 8 | 9 | 9 |
| Novel failures | 25% | 3 | 6 | 10 | 8 |
| Explainability | 20% | 10 | 5 | 7 | 8 |
| Cost efficiency | 15% | 10 | 7 | 4 | 7 |
| Latency | 10% | 10 | 8 | 4 | 7 |
| **Weighted Score** | 100% | **7.15** | **6.75** | **7.55** | **8.15** |

### **WINNER: Option D - Hybrid Rules + LLM**

### Rationale
The hybrid approach was selected for optimal balance:

1. **Fast Path for Known Patterns**: Rule engine handles 80% of failures instantly (timeout, element not found, assertion failed).

2. **LLM for Complex Analysis**: Novel or ambiguous failures escalate to LLM for deep analysis.

3. **Cost Optimization**: LLM calls only for cases that truly need intelligence.

4. **Continuous Improvement**: LLM insights can be codified into new rules over time.

### Implementation Specification
```typescript
// Failure Analysis Pipeline
async function analyzeFailure(failure: TestFailure): Promise<FailureAnalysis> {
  // Step 1: Rule-based classification
  const ruleResult = applyFailureRules(failure);
  
  if (ruleResult.confidence >= 0.9) {
    return {
      category: ruleResult.category,
      rootCause: ruleResult.rootCause,
      suggestedFix: ruleResult.suggestedFix,
      analysisMethod: 'rule-based',
      confidence: ruleResult.confidence
    };
  }
  
  // Step 2: LLM analysis for complex cases
  const llmResult = await invokeLLM({
    messages: [
      { role: 'system', content: FAILURE_ANALYSIS_PROMPT },
      { role: 'user', content: formatFailureContext(failure) }
    ],
    response_format: { type: 'json_schema', json_schema: FAILURE_SCHEMA }
  });
  
  // Step 3: Learn from LLM analysis
  await storeAnalysisPattern(failure, llmResult);
  
  return {
    ...llmResult,
    analysisMethod: 'llm-powered',
    confidence: llmResult.confidence
  };
}

// Rule definitions
const FAILURE_RULES = [
  {
    pattern: /TimeoutError.*waiting for selector/i,
    category: 'ELEMENT_NOT_FOUND',
    rootCause: 'Selector may have changed or element not rendered',
    suggestedFix: 'Verify selector accuracy, add explicit wait, check page load'
  },
  {
    pattern: /net::ERR_CONNECTION_REFUSED/i,
    category: 'NETWORK_ERROR',
    rootCause: 'Target server not responding',
    suggestedFix: 'Verify server is running, check URL, validate network access'
  },
  // ... additional rules
];
```

---

## 3.10 Decision Tree Summary

| Decision | Winner | Key Rationale |
|----------|--------|---------------|
| Core Automation Framework | **Playwright** | Native MCP support, auto-wait, multi-browser |
| AI Integration Protocol | **MCP (Model Context Protocol)** | Model agnostic, standardized, human-in-loop native |
| Test Organization | **Action-Based Testing (ABT)** | Maximum reusability, clear abstraction layers |
| Distributed Execution | **PostgreSQL Job Queue** | Transactional, observable, simple |
| AI Model | **Claude 3.5 Sonnet + Hybrid** | Best MCP support, superior code generation |
| Knowledge Storage | **PostgreSQL + pgvector** | Unified data layer, hybrid search |
| Human-in-the-Loop | **Hybrid Confidence + Risk** | Balanced safety and efficiency |
| Result Analysis | **Hybrid Rules + LLM** | Fast common cases, intelligent complex analysis |


---

# SECTION 4: DETAILED CODE SPECIFICATIONS

## 4.1 Code Architecture Overview

The GRACE Autonomous Testing System v4 follows a modular architecture with clear separation of concerns. This section provides line-by-line explanations of every critical code component.

---

## 4.2 Core Module: Action Runner

The Action Runner is the heart of the ABT implementation, responsible for executing atomic actions and composing them into test flows.

### File: `src/core/action-runner.ts`

```typescript
/**
 * ActionRunner - Generic Dispatcher for Composition-Based Testing
 * 
 * PURPOSE: Provides a unified interface for executing atomic test actions
 * while maintaining composability, error handling, and observability.
 * 
 * DESIGN RATIONALE: The Action Runner implements the Command Pattern,
 * where each action is an encapsulated command that can be executed,
 * logged, retried, and composed with other actions.
 */

import { Page, BrowserContext } from 'playwright';           // Line 1: Import Playwright types for browser automation
import { ActionResult, ActionConfig, ActionContext } from './types';  // Line 2: Import internal type definitions
import { Logger } from '../utils/logger';                    // Line 3: Import logging utility for observability
import { MetricsCollector } from '../utils/metrics';         // Line 4: Import metrics for performance tracking
import { RetryPolicy } from '../utils/retry';                // Line 5: Import retry logic for resilience

/**
 * ActionRegistry - Singleton pattern for action registration
 * 
 * WHY SINGLETON: Ensures all parts of the system reference the same
 * action definitions, preventing inconsistencies and enabling
 * dynamic action discovery.
 */
class ActionRegistry {
  private static instance: ActionRegistry;                   // Line 6: Static instance for singleton
  private actions: Map<string, ActionDefinition> = new Map(); // Line 7: Map storing action name -> definition

  private constructor() {}                                   // Line 8: Private constructor prevents direct instantiation

  /**
   * getInstance - Returns the singleton instance
   * 
   * THREAD SAFETY: JavaScript is single-threaded, so no mutex needed.
   * In multi-threaded environments, this would require synchronization.
   */
  static getInstance(): ActionRegistry {                     // Line 9: Public accessor for singleton
    if (!ActionRegistry.instance) {                          // Line 10: Lazy initialization check
      ActionRegistry.instance = new ActionRegistry();        // Line 11: Create instance if not exists
    }
    return ActionRegistry.instance;                          // Line 12: Return singleton instance
  }

  /**
   * register - Adds a new action to the registry
   * 
   * @param name - Unique identifier for the action (e.g., 'click', 'fill', 'navigate')
   * @param definition - The action's implementation and metadata
   * 
   * VALIDATION: Throws if action already exists to prevent silent overwrites
   * which could cause hard-to-debug issues in test execution.
   */
  register(name: string, definition: ActionDefinition): void {
    if (this.actions.has(name)) {                            // Line 13: Check for duplicate registration
      throw new Error(`Action "${name}" already registered`); // Line 14: Fail fast on duplicates
    }
    this.actions.set(name, definition);                      // Line 15: Store action in registry
    Logger.debug(`Action registered: ${name}`);              // Line 16: Log for debugging
  }

  /**
   * get - Retrieves an action definition by name
   * 
   * RETURNS: ActionDefinition or undefined (caller must handle missing actions)
   */
  get(name: string): ActionDefinition | undefined {          // Line 17: Retrieve action by name
    return this.actions.get(name);                           // Line 18: Return from map (O(1) lookup)
  }

  /**
   * list - Returns all registered action names
   * 
   * USE CASE: Used by AI agents to discover available actions
   * and by documentation generators.
   */
  list(): string[] {                                         // Line 19: List all action names
    return Array.from(this.actions.keys());                  // Line 20: Convert map keys to array
  }
}

/**
 * ActionDefinition - Schema for defining an action
 * 
 * DESIGN: Each action is self-describing, containing:
 * - execute: The actual implementation
 * - validate: Pre-execution validation
 * - schema: JSON Schema for parameters (used by AI for generation)
 * - riskLevel: Used by human-in-the-loop system
 */
interface ActionDefinition {
  name: string;                                              // Line 21: Human-readable action name
  description: string;                                       // Line 22: Description for AI understanding
  category: 'navigation' | 'interaction' | 'assertion' | 'data' | 'utility';  // Line 23: Categorization
  riskLevel: 'low' | 'medium' | 'high';                     // Line 24: Risk classification for HITL
  schema: JSONSchema;                                        // Line 25: Parameter schema for validation
  validate: (params: unknown) => ValidationResult;           // Line 26: Pre-execution validator
  execute: (ctx: ActionContext, params: unknown) => Promise<ActionResult>;  // Line 27: Implementation
  rollback?: (ctx: ActionContext, params: unknown) => Promise<void>;  // Line 28: Optional rollback for transactions
}

/**
 * ActionContext - Execution context passed to every action
 * 
 * CONTAINS: All resources an action might need, avoiding
 * global state and enabling parallel execution.
 */
interface ActionContext {
  page: Page;                                                // Line 29: Playwright page instance
  context: BrowserContext;                                   // Line 30: Browser context for cookies, storage
  logger: Logger;                                            // Line 31: Scoped logger for this execution
  metrics: MetricsCollector;                                 // Line 32: Metrics collector
  variables: Map<string, unknown>;                           // Line 33: Test variables for data passing
  config: ActionConfig;                                      // Line 34: Configuration options
}

/**
 * ActionRunner - Main class for executing actions
 * 
 * RESPONSIBILITIES:
 * 1. Validate action parameters before execution
 * 2. Execute actions with proper error handling
 * 3. Collect metrics and logs
 * 4. Handle retries according to policy
 * 5. Support action composition (sequences)
 */
export class ActionRunner {
  private registry: ActionRegistry;                          // Line 35: Reference to action registry
  private logger: Logger;                                    // Line 36: Logger instance
  private metrics: MetricsCollector;                         // Line 37: Metrics collector
  private retryPolicy: RetryPolicy;                          // Line 38: Retry configuration

  /**
   * Constructor - Initializes the ActionRunner
   * 
   * @param config - Configuration options for the runner
   * 
   * DEPENDENCY INJECTION: All dependencies passed via config,
   * enabling testing with mocks and different configurations.
   */
  constructor(config: ActionRunnerConfig) {
    this.registry = ActionRegistry.getInstance();            // Line 39: Get singleton registry
    this.logger = config.logger ?? new Logger('ActionRunner'); // Line 40: Use provided or default logger
    this.metrics = config.metrics ?? new MetricsCollector(); // Line 41: Use provided or default metrics
    this.retryPolicy = config.retryPolicy ?? RetryPolicy.default(); // Line 42: Use provided or default retry
  }

  /**
   * execute - Executes a single action
   * 
   * @param actionName - Name of the action to execute
   * @param params - Parameters for the action
   * @param ctx - Execution context
   * @returns ActionResult with success/failure and data
   * 
   * FLOW:
   * 1. Look up action in registry
   * 2. Validate parameters
   * 3. Execute with retry logic
   * 4. Collect metrics
   * 5. Return result
   */
  async execute(
    actionName: string,                                      // Line 43: Action identifier
    params: unknown,                                         // Line 44: Action parameters (validated later)
    ctx: ActionContext                                       // Line 45: Execution context
  ): Promise<ActionResult> {
    const startTime = Date.now();                            // Line 46: Record start time for metrics
    
    // Step 1: Retrieve action definition
    const action = this.registry.get(actionName);            // Line 47: Look up action
    if (!action) {                                           // Line 48: Check if action exists
      return {                                               // Line 49: Return error result
        success: false,                                      // Line 50: Mark as failed
        error: `Unknown action: ${actionName}`,              // Line 51: Error message
        duration: Date.now() - startTime                     // Line 52: Execution duration
      };
    }

    // Step 2: Validate parameters
    const validation = action.validate(params);              // Line 53: Run validation
    if (!validation.valid) {                                 // Line 54: Check validation result
      this.logger.error(`Validation failed for ${actionName}`, validation.errors);  // Line 55: Log errors
      return {                                               // Line 56: Return validation error
        success: false,
        error: `Validation failed: ${validation.errors.join(', ')}`,
        duration: Date.now() - startTime
      };
    }

    // Step 3: Execute with retry logic
    let lastError: Error | null = null;                      // Line 57: Track last error for reporting
    let attempts = 0;                                        // Line 58: Track attempt count
    
    while (attempts < this.retryPolicy.maxAttempts) {        // Line 59: Retry loop
      attempts++;                                            // Line 60: Increment attempt counter
      
      try {
        this.logger.info(`Executing ${actionName} (attempt ${attempts})`);  // Line 61: Log execution start
        
        const result = await action.execute(ctx, params);    // Line 62: Execute the action
        
        // Step 4: Collect metrics on success
        this.metrics.recordAction({                          // Line 63: Record success metrics
          name: actionName,
          duration: Date.now() - startTime,
          success: true,
          attempts
        });
        
        this.logger.info(`${actionName} completed successfully`);  // Line 64: Log success
        return result;                                       // Line 65: Return successful result
        
      } catch (error) {
        lastError = error as Error;                          // Line 66: Store error for reporting
        this.logger.warn(`${actionName} failed (attempt ${attempts}): ${lastError.message}`);  // Line 67: Log failure
        
        // Check if error is retryable
        if (!this.retryPolicy.isRetryable(lastError)) {      // Line 68: Check retryability
          break;                                             // Line 69: Exit loop if not retryable
        }
        
        // Wait before retry (exponential backoff)
        if (attempts < this.retryPolicy.maxAttempts) {       // Line 70: Check if more attempts remain
          const delay = this.retryPolicy.getDelay(attempts); // Line 71: Calculate backoff delay
          await this.sleep(delay);                           // Line 72: Wait before retry
        }
      }
    }

    // Step 5: All retries exhausted - return failure
    this.metrics.recordAction({                              // Line 73: Record failure metrics
      name: actionName,
      duration: Date.now() - startTime,
      success: false,
      attempts,
      error: lastError?.message
    });

    return {                                                 // Line 74: Return failure result
      success: false,
      error: lastError?.message ?? 'Unknown error',
      duration: Date.now() - startTime,
      attempts
    };
  }

  /**
   * executeSequence - Executes multiple actions in order
   * 
   * @param actions - Array of action specifications
   * @param ctx - Shared execution context
   * @returns Array of results, one per action
   * 
   * BEHAVIOR: Stops on first failure unless continueOnError is true.
   * This enables both strict sequences and best-effort execution.
   */
  async executeSequence(
    actions: ActionSpec[],                                   // Line 75: Array of actions to execute
    ctx: ActionContext,                                      // Line 76: Shared context
    options: { continueOnError?: boolean } = {}              // Line 77: Execution options
  ): Promise<ActionResult[]> {
    const results: ActionResult[] = [];                      // Line 78: Collect results
    
    for (const actionSpec of actions) {                      // Line 79: Iterate through actions
      const result = await this.execute(                     // Line 80: Execute each action
        actionSpec.name,
        actionSpec.params,
        ctx
      );
      
      results.push(result);                                  // Line 81: Store result
      
      // Check if we should stop on failure
      if (!result.success && !options.continueOnError) {     // Line 82: Check failure handling
        this.logger.error(`Sequence stopped at ${actionSpec.name}`);  // Line 83: Log stop
        break;                                               // Line 84: Exit loop
      }
    }
    
    return results;                                          // Line 85: Return all results
  }

  /**
   * sleep - Utility for async delays
   * 
   * WHY NOT setTimeout: This returns a Promise for clean async/await usage.
   */
  private sleep(ms: number): Promise<void> {                 // Line 86: Sleep utility
    return new Promise(resolve => setTimeout(resolve, ms));  // Line 87: Promise-wrapped timeout
  }
}

/**
 * ActionSpec - Specification for an action in a sequence
 */
interface ActionSpec {
  name: string;                                              // Line 88: Action name
  params: unknown;                                           // Line 89: Action parameters
  description?: string;                                      // Line 90: Optional description for logging
}

// Export singleton registry for action registration
export const actionRegistry = ActionRegistry.getInstance();  // Line 91: Export registry singleton
```

---

## 4.3 Core Module: MCP Server Implementation

The MCP Server exposes the testing capabilities to AI agents through the Model Context Protocol.

### File: `src/mcp/server.ts`

```typescript
/**
 * GRACE MCP Server - Model Context Protocol Implementation
 * 
 * PURPOSE: Provides a standardized interface for AI agents to
 * discover and invoke testing capabilities.
 * 
 * PROTOCOL: Implements MCP 1.0 specification with:
 * - Tool discovery (list available actions)
 * - Tool invocation (execute actions)
 * - Resource access (test results, knowledge base)
 * - Prompt templates (common testing patterns)
 */

import { Server } from '@modelcontextprotocol/sdk/server/index.js';     // Line 1: MCP SDK server
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';  // Line 2: stdio transport
import {
  CallToolRequestSchema,                                     // Line 3: Schema for tool calls
  ListToolsRequestSchema,                                    // Line 4: Schema for tool listing
  ListResourcesRequestSchema,                                // Line 5: Schema for resource listing
  ReadResourceRequestSchema,                                 // Line 6: Schema for resource reading
  Tool,                                                      // Line 7: Tool type definition
  TextContent,                                               // Line 8: Text content type
  ImageContent                                               // Line 9: Image content type
} from '@modelcontextprotocol/sdk/types.js';

import { ActionRunner, actionRegistry } from '../core/action-runner';   // Line 10: Import action system
import { BrowserManager } from '../browser/manager';         // Line 11: Browser lifecycle management
import { KnowledgeBase } from '../knowledge/base';           // Line 12: Vector knowledge store
import { HumanInTheLoop } from '../hitl/controller';         // Line 13: Human approval system
import { Logger } from '../utils/logger';                    // Line 14: Logging utility

/**
 * GraceMCPServer - Main MCP server class
 * 
 * LIFECYCLE:
 * 1. Initialize with configuration
 * 2. Register all tools (actions)
 * 3. Start listening for requests
 * 4. Process requests and return responses
 * 5. Shutdown cleanly on termination
 */
export class GraceMCPServer {
  private server: Server;                                    // Line 15: MCP server instance
  private actionRunner: ActionRunner;                        // Line 16: Action execution engine
  private browserManager: BrowserManager;                    // Line 17: Browser lifecycle manager
  private knowledgeBase: KnowledgeBase;                      // Line 18: Knowledge storage
  private hitl: HumanInTheLoop;                              // Line 19: Human-in-the-loop controller
  private logger: Logger;                                    // Line 20: Logger instance

  /**
   * Constructor - Initializes the MCP server
   * 
   * @param config - Server configuration options
   */
  constructor(config: MCPServerConfig) {
    this.logger = new Logger('GraceMCPServer');              // Line 21: Create scoped logger
    
    // Initialize MCP server with metadata
    this.server = new Server(                                // Line 22: Create MCP server
      {
        name: 'grace-autonomous-testing',                    // Line 23: Server name for discovery
        version: '4.0.0',                                    // Line 24: Version number
      },
      {
        capabilities: {                                      // Line 25: Declare server capabilities
          tools: {},                                         // Line 26: We provide tools
          resources: {},                                     // Line 27: We provide resources
          prompts: {},                                       // Line 28: We provide prompt templates
        },
      }
    );

    // Initialize subsystems
    this.actionRunner = new ActionRunner({                   // Line 29: Create action runner
      logger: this.logger,
      retryPolicy: config.retryPolicy
    });
    
    this.browserManager = new BrowserManager(config.browser); // Line 30: Create browser manager
    this.knowledgeBase = new KnowledgeBase(config.database); // Line 31: Create knowledge base
    this.hitl = new HumanInTheLoop(config.hitl);             // Line 32: Create HITL controller

    this.registerHandlers();                                 // Line 33: Register request handlers
  }

  /**
   * registerHandlers - Sets up all MCP request handlers
   * 
   * HANDLERS:
   * - tools/list: Returns available testing actions
   * - tools/call: Executes a testing action
   * - resources/list: Returns available resources
   * - resources/read: Reads a specific resource
   */
  private registerHandlers(): void {
    // Handler: List available tools
    this.server.setRequestHandler(                           // Line 34: Register list tools handler
      ListToolsRequestSchema,                                // Line 35: Schema for validation
      async () => {                                          // Line 36: Handler function
        const actions = actionRegistry.list();               // Line 37: Get all registered actions
        
        const tools: Tool[] = actions.map(actionName => {    // Line 38: Convert to MCP tools
          const action = actionRegistry.get(actionName)!;    // Line 39: Get action definition
          
          return {                                           // Line 40: Return tool definition
            name: actionName,                                // Line 41: Tool name
            description: action.description,                 // Line 42: Human-readable description
            inputSchema: action.schema,                      // Line 43: JSON Schema for parameters
          };
        });
        
        this.logger.debug(`Listed ${tools.length} tools`);   // Line 44: Log tool count
        return { tools };                                    // Line 45: Return tools array
      }
    );

    // Handler: Execute a tool (action)
    this.server.setRequestHandler(                           // Line 46: Register call tool handler
      CallToolRequestSchema,                                 // Line 47: Schema for validation
      async (request) => {                                   // Line 48: Handler function
        const { name, arguments: params } = request.params;  // Line 49: Extract tool name and params
        
        this.logger.info(`Tool call: ${name}`, params);      // Line 50: Log the call
        
        // Step 1: Check if action requires human approval
        const action = actionRegistry.get(name);             // Line 51: Get action definition
        if (!action) {                                       // Line 52: Check action exists
          return this.errorResponse(`Unknown tool: ${name}`); // Line 53: Return error
        }

        // Step 2: Evaluate human-in-the-loop requirements
        const hitlDecision = await this.hitl.evaluate({      // Line 54: Evaluate HITL
          action: name,
          params,
          riskLevel: action.riskLevel,
          confidence: params._confidence ?? 0.5              // Line 55: AI confidence score
        });

        if (hitlDecision.requiresApproval) {                 // Line 56: Check if approval needed
          const approved = await this.hitl.requestApproval({ // Line 57: Request approval
            action: name,
            params,
            reasoning: hitlDecision.reasoning
          });
          
          if (!approved) {                                   // Line 58: Check approval result
            return this.errorResponse('Action rejected by human reviewer');  // Line 59: Return rejection
          }
        }

        // Step 3: Get or create browser context
        const ctx = await this.browserManager.getContext();  // Line 60: Get browser context
        
        // Step 4: Execute the action
        const result = await this.actionRunner.execute(      // Line 61: Execute action
          name,
          params,
          {
            page: ctx.page,
            context: ctx.context,
            logger: this.logger,
            metrics: ctx.metrics,
            variables: ctx.variables,
            config: ctx.config
          }
        );

        // Step 5: Store execution in knowledge base for learning
        await this.knowledgeBase.recordExecution({           // Line 62: Record for learning
          action: name,
          params,
          result,
          timestamp: new Date()
        });

        // Step 6: Return result to AI agent
        if (result.success) {                                // Line 63: Check success
          return {                                           // Line 64: Return success response
            content: [
              {
                type: 'text',                                // Line 65: Text content type
                text: JSON.stringify(result.data ?? { success: true }, null, 2)  // Line 66: Result data
              } as TextContent
            ]
          };
        } else {
          return this.errorResponse(result.error ?? 'Action failed');  // Line 67: Return error
        }
      }
    );

    // Handler: List available resources
    this.server.setRequestHandler(                           // Line 68: Register list resources handler
      ListResourcesRequestSchema,
      async () => {
        return {                                             // Line 69: Return resource list
          resources: [
            {
              uri: 'grace://test-results/latest',            // Line 70: Latest test results
              name: 'Latest Test Results',
              description: 'Most recent test execution results',
              mimeType: 'application/json'
            },
            {
              uri: 'grace://knowledge/actions',              // Line 71: Action knowledge base
              name: 'Action Knowledge Base',
              description: 'Learned patterns and best practices for actions',
              mimeType: 'application/json'
            },
            {
              uri: 'grace://knowledge/selectors',            // Line 72: Selector knowledge
              name: 'Selector Knowledge Base',
              description: 'Known selectors and their reliability scores',
              mimeType: 'application/json'
            },
            {
              uri: 'grace://metrics/summary',                // Line 73: Metrics summary
              name: 'Execution Metrics',
              description: 'Performance and reliability metrics',
              mimeType: 'application/json'
            }
          ]
        };
      }
    );

    // Handler: Read a specific resource
    this.server.setRequestHandler(                           // Line 74: Register read resource handler
      ReadResourceRequestSchema,
      async (request) => {
        const { uri } = request.params;                      // Line 75: Extract resource URI
        
        this.logger.debug(`Reading resource: ${uri}`);       // Line 76: Log resource access
        
        // Route to appropriate resource handler
        if (uri === 'grace://test-results/latest') {         // Line 77: Latest results
          const results = await this.knowledgeBase.getLatestResults();  // Line 78: Fetch results
          return {
            contents: [{
              uri,
              mimeType: 'application/json',
              text: JSON.stringify(results, null, 2)
            }]
          };
        }
        
        if (uri === 'grace://knowledge/actions') {           // Line 79: Action knowledge
          const knowledge = await this.knowledgeBase.getActionKnowledge();  // Line 80: Fetch knowledge
          return {
            contents: [{
              uri,
              mimeType: 'application/json',
              text: JSON.stringify(knowledge, null, 2)
            }]
          };
        }
        
        if (uri === 'grace://knowledge/selectors') {         // Line 81: Selector knowledge
          const selectors = await this.knowledgeBase.getSelectorKnowledge();  // Line 82: Fetch selectors
          return {
            contents: [{
              uri,
              mimeType: 'application/json',
              text: JSON.stringify(selectors, null, 2)
            }]
          };
        }
        
        if (uri === 'grace://metrics/summary') {             // Line 83: Metrics
          const metrics = await this.knowledgeBase.getMetricsSummary();  // Line 84: Fetch metrics
          return {
            contents: [{
              uri,
              mimeType: 'application/json',
              text: JSON.stringify(metrics, null, 2)
            }]
          };
        }
        
        throw new Error(`Unknown resource: ${uri}`);         // Line 85: Unknown resource error
      }
    );
  }

  /**
   * errorResponse - Creates a standardized error response
   * 
   * @param message - Error message to return
   * @returns MCP-compliant error response
   */
  private errorResponse(message: string) {                   // Line 86: Error response helper
    return {
      content: [
        {
          type: 'text',
          text: JSON.stringify({ error: message }, null, 2)
        } as TextContent
      ],
      isError: true                                          // Line 87: Mark as error
    };
  }

  /**
   * start - Starts the MCP server
   * 
   * TRANSPORT: Uses stdio for local execution.
   * For remote deployment, SSE transport would be used.
   */
  async start(): Promise<void> {                             // Line 88: Start server
    const transport = new StdioServerTransport();            // Line 89: Create stdio transport
    await this.server.connect(transport);                    // Line 90: Connect server to transport
    this.logger.info('GRACE MCP Server started');            // Line 91: Log startup
  }

  /**
   * shutdown - Gracefully shuts down the server
   * 
   * CLEANUP: Closes browser, flushes metrics, disconnects.
   */
  async shutdown(): Promise<void> {                          // Line 92: Shutdown method
    this.logger.info('Shutting down GRACE MCP Server');      // Line 93: Log shutdown
    await this.browserManager.closeAll();                    // Line 94: Close all browsers
    await this.knowledgeBase.flush();                        // Line 95: Flush pending writes
    await this.server.close();                               // Line 96: Close MCP server
  }
}

/**
 * MCPServerConfig - Configuration interface
 */
interface MCPServerConfig {
  browser: BrowserConfig;                                    // Line 97: Browser configuration
  database: DatabaseConfig;                                  // Line 98: Database configuration
  hitl: HITLConfig;                                          // Line 99: HITL configuration
  retryPolicy?: RetryPolicy;                                 // Line 100: Optional retry policy
}
```

---

## 4.4 Core Module: Human-in-the-Loop Controller

### File: `src/hitl/controller.ts`

```typescript
/**
 * HumanInTheLoop Controller - Manages Human Oversight
 * 
 * PURPOSE: Implements the hybrid confidence + risk decision system
 * for determining when human approval is required.
 * 
 * DESIGN: Uses a state machine pattern for approval workflows,
 * with configurable timeouts and escalation paths.
 */

import { EventEmitter } from 'events';                       // Line 1: Event emitter for async notifications
import { Logger } from '../utils/logger';                    // Line 2: Logging utility
import { NotificationService } from '../notifications/service';  // Line 3: Notification delivery

/**
 * Risk levels for actions
 * 
 * LOW: Read-only operations, navigation, assertions
 * MEDIUM: Form submissions, clicks, data entry
 * HIGH: Deletions, payments, authentication changes
 */
type RiskLevel = 'low' | 'medium' | 'high';                  // Line 4: Risk level type

/**
 * Execution modes based on HITL evaluation
 */
type ExecutionMode =                                         // Line 5: Execution mode type
  | 'AUTO_EXECUTE'                                           // Line 6: Execute without approval
  | 'AUTO_EXECUTE_WITH_LOGGING'                              // Line 7: Execute but log for audit
  | 'REQUIRE_APPROVAL';                                      // Line 8: Require human approval

/**
 * ActionEvaluation - Input for HITL evaluation
 */
interface ActionEvaluation {
  action: string;                                            // Line 9: Action name
  params: unknown;                                           // Line 10: Action parameters
  riskLevel: RiskLevel;                                      // Line 11: Risk classification
  confidence: number;                                        // Line 12: AI confidence (0-1)
}

/**
 * HITLDecision - Result of HITL evaluation
 */
interface HITLDecision {
  mode: ExecutionMode;                                       // Line 13: Determined execution mode
  requiresApproval: boolean;                                 // Line 14: Whether approval needed
  reasoning: string;                                         // Line 15: Explanation for decision
  timeout?: number;                                          // Line 16: Approval timeout in ms
}

/**
 * ApprovalRequest - Request for human approval
 */
interface ApprovalRequest {
  id: string;                                                // Line 17: Unique request ID
  action: string;                                            // Line 18: Action name
  params: unknown;                                           // Line 19: Action parameters
  reasoning: string;                                         // Line 20: Why approval needed
  timestamp: Date;                                           // Line 21: Request timestamp
  status: 'pending' | 'approved' | 'rejected' | 'timeout';   // Line 22: Request status
}

/**
 * HumanInTheLoop - Main HITL controller class
 */
export class HumanInTheLoop extends EventEmitter {
  private logger: Logger;                                    // Line 23: Logger instance
  private notifications: NotificationService;                // Line 24: Notification service
  private pendingApprovals: Map<string, ApprovalRequest>;    // Line 25: Pending approval requests
  private config: HITLConfig;                                // Line 26: Configuration

  /**
   * Constructor - Initializes HITL controller
   * 
   * @param config - HITL configuration options
   */
  constructor(config: HITLConfig) {
    super();                                                 // Line 27: Call EventEmitter constructor
    this.logger = new Logger('HumanInTheLoop');              // Line 28: Create logger
    this.notifications = new NotificationService(config.notifications);  // Line 29: Create notification service
    this.pendingApprovals = new Map();                       // Line 30: Initialize pending map
    this.config = config;                                    // Line 31: Store config
  }

  /**
   * evaluate - Evaluates whether an action requires human approval
   * 
   * @param evaluation - Action evaluation input
   * @returns HITLDecision with mode and reasoning
   * 
   * ALGORITHM:
   * 1. High risk actions ALWAYS require approval
   * 2. Low confidence (<0.85) requires approval
   * 3. High confidence (>=0.95) + low risk = auto-execute
   * 4. Medium confidence + medium risk = auto-execute with logging
   */
  evaluate(evaluation: ActionEvaluation): HITLDecision {
    const { action, riskLevel, confidence } = evaluation;    // Line 32: Destructure input
    
    this.logger.debug(`Evaluating: ${action}`, {             // Line 33: Log evaluation
      riskLevel,
      confidence
    });

    // Rule 1: High risk always requires approval
    if (riskLevel === 'high') {                              // Line 34: Check high risk
      return {                                               // Line 35: Return approval required
        mode: 'REQUIRE_APPROVAL',
        requiresApproval: true,
        reasoning: `High-risk action "${action}" requires human approval regardless of confidence`,
        timeout: this.config.highRiskTimeout ?? 300000       // Line 36: 5 minute timeout
      };
    }

    // Rule 2: Very high confidence + low risk = auto-execute
    if (confidence >= 0.95 && riskLevel === 'low') {         // Line 37: Check auto-execute condition
      return {                                               // Line 38: Return auto-execute
        mode: 'AUTO_EXECUTE',
        requiresApproval: false,
        reasoning: `High confidence (${confidence}) and low risk - auto-executing`
      };
    }

    // Rule 3: Good confidence + not high risk = auto-execute with logging
    if (confidence >= 0.85 && riskLevel !== 'high') {        // Line 39: Check logging condition
      return {                                               // Line 40: Return auto with logging
        mode: 'AUTO_EXECUTE_WITH_LOGGING',
        requiresApproval: false,
        reasoning: `Moderate confidence (${confidence}) - auto-executing with audit log`
      };
    }

    // Rule 4: Low confidence = require approval
    return {                                                 // Line 41: Default to approval required
      mode: 'REQUIRE_APPROVAL',
      requiresApproval: true,
      reasoning: `Low confidence (${confidence}) requires human verification`,
      timeout: this.config.defaultTimeout ?? 120000          // Line 42: 2 minute timeout
    };
  }

  /**
   * requestApproval - Requests human approval for an action
   * 
   * @param request - Approval request details
   * @returns Promise<boolean> - true if approved, false if rejected/timeout
   * 
   * FLOW:
   * 1. Create approval request record
   * 2. Send notification to human reviewer
   * 3. Wait for response or timeout
   * 4. Return decision
   */
  async requestApproval(request: Omit<ApprovalRequest, 'id' | 'timestamp' | 'status'>): Promise<boolean> {
    const id = this.generateId();                            // Line 43: Generate unique ID
    
    const approvalRequest: ApprovalRequest = {               // Line 44: Create full request
      id,
      ...request,
      timestamp: new Date(),
      status: 'pending'
    };
    
    this.pendingApprovals.set(id, approvalRequest);          // Line 45: Store pending request
    
    this.logger.info(`Approval requested: ${id}`, {          // Line 46: Log request
      action: request.action,
      reasoning: request.reasoning
    });

    // Send notification to human reviewer
    await this.notifications.send({                          // Line 47: Send notification
      type: 'approval_request',
      title: `Action Approval Required: ${request.action}`,
      body: request.reasoning,
      data: {
        requestId: id,
        action: request.action,
        params: request.params
      },
      actions: [                                             // Line 48: Notification actions
        { id: 'approve', label: 'Approve', type: 'button' },
        { id: 'reject', label: 'Reject', type: 'button' }
      ]
    });

    // Wait for response or timeout
    return new Promise((resolve) => {                        // Line 49: Return promise
      const timeout = setTimeout(() => {                     // Line 50: Set timeout
        const pending = this.pendingApprovals.get(id);
        if (pending && pending.status === 'pending') {
          pending.status = 'timeout';
          this.logger.warn(`Approval timeout: ${id}`);       // Line 51: Log timeout
          this.emit('approval_timeout', pending);            // Line 52: Emit timeout event
          resolve(false);                                    // Line 53: Reject on timeout
        }
      }, this.config.defaultTimeout ?? 120000);

      // Listen for approval response
      const handler = (response: { requestId: string; approved: boolean }) => {  // Line 54: Response handler
        if (response.requestId === id) {
          clearTimeout(timeout);                             // Line 55: Clear timeout
          const pending = this.pendingApprovals.get(id);
          if (pending) {
            pending.status = response.approved ? 'approved' : 'rejected';  // Line 56: Update status
            this.logger.info(`Approval ${pending.status}: ${id}`);  // Line 57: Log result
          }
          this.removeListener('approval_response', handler); // Line 58: Remove listener
          resolve(response.approved);                        // Line 59: Resolve with decision
        }
      };

      this.on('approval_response', handler);                 // Line 60: Register listener
    });
  }

  /**
   * handleApprovalResponse - Processes approval response from human
   * 
   * @param requestId - ID of the approval request
   * @param approved - Whether the action was approved
   * 
   * CALLED BY: Notification service webhook or UI callback
   */
  handleApprovalResponse(requestId: string, approved: boolean): void {
    this.emit('approval_response', { requestId, approved }); // Line 61: Emit response event
  }

  /**
   * generateId - Generates a unique request ID
   * 
   * FORMAT: hitl_<timestamp>_<random>
   */
  private generateId(): string {                             // Line 62: ID generator
    return `hitl_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;  // Line 63: Generate ID
  }

  /**
   * getPendingApprovals - Returns all pending approval requests
   * 
   * USE CASE: Admin UI to view pending approvals
   */
  getPendingApprovals(): ApprovalRequest[] {                 // Line 64: Get pending approvals
    return Array.from(this.pendingApprovals.values())        // Line 65: Convert to array
      .filter(r => r.status === 'pending');                  // Line 66: Filter pending only
  }

  /**
   * getApprovalHistory - Returns approval history
   * 
   * @param limit - Maximum number of records to return
   */
  getApprovalHistory(limit: number = 100): ApprovalRequest[] {  // Line 67: Get history
    return Array.from(this.pendingApprovals.values())        // Line 68: Convert to array
      .sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime())  // Line 69: Sort by time
      .slice(0, limit);                                      // Line 70: Limit results
  }
}

/**
 * HITLConfig - Configuration interface
 */
interface HITLConfig {
  defaultTimeout?: number;                                   // Line 71: Default approval timeout
  highRiskTimeout?: number;                                  // Line 72: High risk approval timeout
  notifications: NotificationConfig;                         // Line 73: Notification configuration
}
```

---

## 4.5 Core Module: Knowledge Base with Vector Search

### File: `src/knowledge/base.ts`

```typescript
/**
 * KnowledgeBase - Vector-Enabled Knowledge Storage
 * 
 * PURPOSE: Stores and retrieves test patterns, selectors, and
 * execution history using vector similarity search.
 * 
 * IMPLEMENTATION: Uses PostgreSQL with pgvector extension for
 * unified storage of structured data and embeddings.
 */

import { Pool } from 'pg';                                   // Line 1: PostgreSQL client
import { Logger } from '../utils/logger';                    // Line 2: Logging utility

/**
 * KnowledgeEntry - A single knowledge item
 */
interface KnowledgeEntry {
  id: string;                                                // Line 3: Unique identifier
  contentType: 'action' | 'pattern' | 'failure' | 'selector';  // Line 4: Entry type
  content: string;                                           // Line 5: The actual content
  embedding?: number[];                                      // Line 6: Vector embedding
  metadata: Record<string, unknown>;                         // Line 7: Additional metadata
  usageCount: number;                                        // Line 8: How often used
  successRate: number;                                       // Line 9: Success rate (0-1)
  createdAt: Date;                                           // Line 10: Creation timestamp
  updatedAt: Date;                                           // Line 11: Last update timestamp
}

/**
 * ExecutionRecord - Record of an action execution
 */
interface ExecutionRecord {
  action: string;                                            // Line 12: Action name
  params: unknown;                                           // Line 13: Parameters used
  result: {                                                  // Line 14: Execution result
    success: boolean;
    data?: unknown;
    error?: string;
    duration: number;
  };
  timestamp: Date;                                           // Line 15: Execution timestamp
}

/**
 * KnowledgeBase - Main knowledge storage class
 */
export class KnowledgeBase {
  private pool: Pool;                                        // Line 16: Database connection pool
  private logger: Logger;                                    // Line 17: Logger instance
  private embeddingService: EmbeddingService;                // Line 18: Embedding generation service

  /**
   * Constructor - Initializes knowledge base
   * 
   * @param config - Database configuration
   */
  constructor(config: DatabaseConfig) {
    this.pool = new Pool({                                   // Line 19: Create connection pool
      connectionString: config.connectionString,
      max: config.maxConnections ?? 10,                      // Line 20: Max pool size
      idleTimeoutMillis: 30000,                              // Line 21: Idle timeout
      connectionTimeoutMillis: 2000                          // Line 22: Connection timeout
    });
    
    this.logger = new Logger('KnowledgeBase');               // Line 23: Create logger
    this.embeddingService = new EmbeddingService(config.embedding);  // Line 24: Create embedding service
  }

  /**
   * recordExecution - Records an action execution for learning
   * 
   * @param record - Execution record to store
   * 
   * LEARNING: This data is used to:
   * 1. Track action success rates
   * 2. Identify common failure patterns
   * 3. Improve selector reliability scores
   * 4. Train the AI on effective patterns
   */
  async recordExecution(record: ExecutionRecord): Promise<void> {
    const client = await this.pool.connect();                // Line 25: Get connection from pool
    
    try {
      // Insert execution record
      await client.query(`                                   
        INSERT INTO execution_history (
          action_name,
          parameters,
          success,
          result_data,
          error_message,
          duration_ms,
          executed_at
        ) VALUES ($1, $2, $3, $4, $5, $6, $7)
      `, [                                                   // Line 26: Insert query
        record.action,                                       // Line 27: Action name
        JSON.stringify(record.params),                       // Line 28: Serialized params
        record.result.success,                               // Line 29: Success flag
        JSON.stringify(record.result.data),                  // Line 30: Result data
        record.result.error,                                 // Line 31: Error message
        record.result.duration,                              // Line 32: Duration
        record.timestamp                                     // Line 33: Timestamp
      ]);

      // Update action statistics
      await client.query(`                                   
        INSERT INTO action_statistics (action_name, total_executions, successful_executions)
        VALUES ($1, 1, $2)
        ON CONFLICT (action_name) DO UPDATE SET
          total_executions = action_statistics.total_executions + 1,
          successful_executions = action_statistics.successful_executions + $2,
          updated_at = NOW()
      `, [record.action, record.result.success ? 1 : 0]);    // Line 34: Update statistics

      this.logger.debug(`Recorded execution: ${record.action}`);  // Line 35: Log recording
      
    } finally {
      client.release();                                      // Line 36: Release connection
    }
  }

  /**
   * findSimilarPatterns - Finds similar patterns using vector search
   * 
   * @param query - Natural language query
   * @param contentType - Type of content to search
   * @param limit - Maximum results to return
   * @returns Array of similar knowledge entries
   * 
   * ALGORITHM:
   * 1. Generate embedding for query
   * 2. Perform cosine similarity search
   * 3. Filter by content type
   * 4. Return top matches
   */
  async findSimilarPatterns(
    query: string,                                           // Line 37: Search query
    contentType?: string,                                    // Line 38: Optional type filter
    limit: number = 10                                       // Line 39: Result limit
  ): Promise<KnowledgeEntry[]> {
    // Generate embedding for query
    const embedding = await this.embeddingService.generate(query);  // Line 40: Generate embedding
    
    const client = await this.pool.connect();                // Line 41: Get connection
    
    try {
      // Perform vector similarity search
      const result = await client.query(`                    
        SELECT 
          id,
          content_type,
          content,
          metadata,
          usage_count,
          success_rate,
          created_at,
          updated_at,
          1 - (embedding <=> $1::vector) as similarity
        FROM test_knowledge
        WHERE ($2::text IS NULL OR content_type = $2)
        ORDER BY embedding <=> $1::vector
        LIMIT $3
      `, [                                                   // Line 42: Vector search query
        `[${embedding.join(',')}]`,                          // Line 43: Embedding as vector
        contentType,                                         // Line 44: Type filter
        limit                                                // Line 45: Result limit
      ]);

      return result.rows.map(row => ({                       // Line 46: Map to KnowledgeEntry
        id: row.id,
        contentType: row.content_type,
        content: row.content,
        metadata: row.metadata,
        usageCount: row.usage_count,
        successRate: parseFloat(row.success_rate),
        createdAt: row.created_at,
        updatedAt: row.updated_at
      }));
      
    } finally {
      client.release();                                      // Line 47: Release connection
    }
  }

  /**
   * storePattern - Stores a new pattern in the knowledge base
   * 
   * @param entry - Knowledge entry to store
   * 
   * EMBEDDING: Automatically generates embedding for the content
   * to enable future similarity searches.
   */
  async storePattern(entry: Omit<KnowledgeEntry, 'id' | 'embedding' | 'createdAt' | 'updatedAt'>): Promise<string> {
    // Generate embedding for content
    const embedding = await this.embeddingService.generate(entry.content);  // Line 48: Generate embedding
    
    const client = await this.pool.connect();                // Line 49: Get connection
    
    try {
      const result = await client.query(`                    
        INSERT INTO test_knowledge (
          content_type,
          content,
          embedding,
          metadata,
          usage_count,
          success_rate
        ) VALUES ($1, $2, $3::vector, $4, $5, $6)
        RETURNING id
      `, [                                                   // Line 50: Insert query
        entry.contentType,                                   // Line 51: Content type
        entry.content,                                       // Line 52: Content
        `[${embedding.join(',')}]`,                          // Line 53: Embedding vector
        JSON.stringify(entry.metadata),                      // Line 54: Metadata
        entry.usageCount,                                    // Line 55: Usage count
        entry.successRate                                    // Line 56: Success rate
      ]);

      const id = result.rows[0].id;                          // Line 57: Get generated ID
      this.logger.info(`Stored pattern: ${id}`);             // Line 58: Log storage
      return id;                                             // Line 59: Return ID
      
    } finally {
      client.release();                                      // Line 60: Release connection
    }
  }

  /**
   * getLatestResults - Gets the most recent test results
   * 
   * @param limit - Maximum results to return
   */
  async getLatestResults(limit: number = 50): Promise<ExecutionRecord[]> {
    const client = await this.pool.connect();                // Line 61: Get connection
    
    try {
      const result = await client.query(`                    
        SELECT 
          action_name,
          parameters,
          success,
          result_data,
          error_message,
          duration_ms,
          executed_at
        FROM execution_history
        ORDER BY executed_at DESC
        LIMIT $1
      `, [limit]);                                           // Line 62: Query latest results

      return result.rows.map(row => ({                       // Line 63: Map to ExecutionRecord
        action: row.action_name,
        params: JSON.parse(row.parameters),
        result: {
          success: row.success,
          data: row.result_data ? JSON.parse(row.result_data) : undefined,
          error: row.error_message,
          duration: row.duration_ms
        },
        timestamp: row.executed_at
      }));
      
    } finally {
      client.release();                                      // Line 64: Release connection
    }
  }

  /**
   * getActionKnowledge - Gets knowledge about actions
   */
  async getActionKnowledge(): Promise<KnowledgeEntry[]> {
    return this.findSimilarPatterns('', 'action', 100);      // Line 65: Get all action knowledge
  }

  /**
   * getSelectorKnowledge - Gets knowledge about selectors
   */
  async getSelectorKnowledge(): Promise<KnowledgeEntry[]> {
    return this.findSimilarPatterns('', 'selector', 100);    // Line 66: Get all selector knowledge
  }

  /**
   * getMetricsSummary - Gets aggregated metrics
   */
  async getMetricsSummary(): Promise<MetricsSummary> {
    const client = await this.pool.connect();                // Line 67: Get connection
    
    try {
      const result = await client.query(`                    
        SELECT 
          COUNT(*) as total_executions,
          SUM(CASE WHEN success THEN 1 ELSE 0 END) as successful_executions,
          AVG(duration_ms) as avg_duration,
          MAX(executed_at) as last_execution
        FROM execution_history
        WHERE executed_at > NOW() - INTERVAL '24 hours'
      `);                                                    // Line 68: Metrics query

      const row = result.rows[0];                            // Line 69: Get result row
      return {                                               // Line 70: Return metrics
        totalExecutions: parseInt(row.total_executions),
        successfulExecutions: parseInt(row.successful_executions),
        successRate: row.total_executions > 0 
          ? row.successful_executions / row.total_executions 
          : 0,
        avgDuration: parseFloat(row.avg_duration) || 0,
        lastExecution: row.last_execution
      };
      
    } finally {
      client.release();                                      // Line 71: Release connection
    }
  }

  /**
   * flush - Flushes any pending writes
   */
  async flush(): Promise<void> {
    // No-op for PostgreSQL (writes are immediate)
    this.logger.debug('Knowledge base flushed');             // Line 72: Log flush
  }

  /**
   * close - Closes database connections
   */
  async close(): Promise<void> {
    await this.pool.end();                                   // Line 73: Close pool
    this.logger.info('Knowledge base closed');               // Line 74: Log close
  }
}

/**
 * EmbeddingService - Generates vector embeddings
 */
class EmbeddingService {
  private apiKey: string;                                    // Line 75: API key for embedding service
  private model: string;                                     // Line 76: Embedding model name

  constructor(config: EmbeddingConfig) {
    this.apiKey = config.apiKey;                             // Line 77: Store API key
    this.model = config.model ?? 'text-embedding-ada-002';   // Line 78: Default to ada-002
  }

  /**
   * generate - Generates embedding for text
   * 
   * @param text - Text to embed
   * @returns Vector embedding (1536 dimensions for ada-002)
   */
  async generate(text: string): Promise<number[]> {
    const response = await fetch('https://api.openai.com/v1/embeddings', {  // Line 79: Call OpenAI API
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.apiKey}`,            // Line 80: Auth header
        'Content-Type': 'application/json'                   // Line 81: Content type
      },
      body: JSON.stringify({                                 // Line 82: Request body
        input: text,
        model: this.model
      })
    });

    const data = await response.json();                      // Line 83: Parse response
    return data.data[0].embedding;                           // Line 84: Return embedding vector
  }
}

/**
 * MetricsSummary - Aggregated metrics interface
 */
interface MetricsSummary {
  totalExecutions: number;                                   // Line 85: Total execution count
  successfulExecutions: number;                              // Line 86: Successful count
  successRate: number;                                       // Line 87: Success rate (0-1)
  avgDuration: number;                                       // Line 88: Average duration in ms
  lastExecution: Date;                                       // Line 89: Last execution time
}
```


---

# SECTION 5: INSTALLATION AND CONFIGURATION SPECIFICATIONS

## 5.1 System Requirements

### 5.1.1 Hardware Requirements

| Component | Minimum | Recommended | Production |
|-----------|---------|-------------|------------|
| **CPU** | 4 cores | 8 cores | 16+ cores |
| **RAM** | 8 GB | 16 GB | 32+ GB |
| **Storage** | 50 GB SSD | 100 GB SSD | 500 GB+ NVMe |
| **Network** | 100 Mbps | 1 Gbps | 10 Gbps |

### 5.1.2 Operating System Requirements

| OS | Version | Support Level |
|----|---------|---------------|
| **Ubuntu** | 22.04 LTS | Primary (Recommended) |
| **Ubuntu** | 20.04 LTS | Supported |
| **Debian** | 12 (Bookworm) | Supported |
| **macOS** | 13+ (Ventura) | Development Only |
| **Windows** | 11 + WSL2 | Development Only |

### 5.1.3 Software Prerequisites

| Software | Version | Purpose | Installation Command |
|----------|---------|---------|---------------------|
| **Node.js** | 22.13.0 | Runtime environment | `nvm install 22.13.0` |
| **pnpm** | 10.4.1 | Package manager | `npm install -g pnpm@10.4.1` |
| **PostgreSQL** | 16.x | Primary database | `apt install postgresql-16` |
| **pgvector** | 0.6.0 | Vector extension | `apt install postgresql-16-pgvector` |
| **Chromium** | Latest | Browser automation | `npx playwright install chromium` |
| **Firefox** | Latest | Browser automation | `npx playwright install firefox` |
| **WebKit** | Latest | Browser automation | `npx playwright install webkit` |

---

## 5.2 Complete Software Version Registry

### 5.2.1 Core Runtime Dependencies

```json
{
  "runtime": {
    "node": "22.13.0",
    "pnpm": "10.4.1",
    "typescript": "5.9.3"
  }
}
```

### 5.2.2 Playwright and Browser Automation

| Package | Version | Purpose |
|---------|---------|---------|
| `playwright` | 1.40.0 | Core browser automation |
| `@playwright/test` | 1.40.0 | Test runner and assertions |
| `@anthropic/mcp-playwright` | 0.1.0 | MCP integration for Playwright |

### 5.2.3 MCP (Model Context Protocol) Stack

| Package | Version | Purpose |
|---------|---------|---------|
| `@modelcontextprotocol/sdk` | 1.0.0 | MCP SDK for server/client |
| `@anthropic/mcp-playwright` | 0.1.0 | Playwright MCP server |
| `testdriver-mcp` | 0.2.0 | TestDriver MCP integration |

### 5.2.4 Database and ORM

| Package | Version | Purpose |
|---------|---------|---------|
| `drizzle-orm` | 0.44.5 | TypeScript ORM |
| `drizzle-kit` | 0.31.4 | Schema migrations |
| `pg` | 8.11.3 | PostgreSQL client |
| `mysql2` | 3.15.0 | MySQL client (optional) |

### 5.2.5 AI and LLM Integration

| Package | Version | Purpose |
|---------|---------|---------|
| `@anthropic-ai/sdk` | 0.20.0 | Claude API client |
| `openai` | 4.28.0 | OpenAI API client |
| `langchain` | 0.1.25 | LLM orchestration (optional) |

### 5.2.6 Web Framework Stack

| Package | Version | Purpose |
|---------|---------|---------|
| `express` | 4.21.2 | HTTP server |
| `@trpc/server` | 11.6.0 | tRPC server |
| `@trpc/client` | 11.6.0 | tRPC client |
| `@trpc/react-query` | 11.6.0 | React tRPC hooks |
| `@tanstack/react-query` | 5.90.2 | Data fetching |

### 5.2.7 Frontend Framework

| Package | Version | Purpose |
|---------|---------|---------|
| `react` | 19.2.1 | UI framework |
| `react-dom` | 19.2.1 | React DOM renderer |
| `wouter` | 3.3.5 | Client-side routing |
| `tailwindcss` | 4.1.14 | CSS framework |

### 5.2.8 UI Component Libraries

| Package | Version | Purpose |
|---------|---------|---------|
| `@radix-ui/react-dialog` | 1.1.15 | Modal dialogs |
| `@radix-ui/react-dropdown-menu` | 2.1.16 | Dropdown menus |
| `@radix-ui/react-tabs` | 1.1.13 | Tab navigation |
| `@radix-ui/react-tooltip` | 1.2.8 | Tooltips |
| `@radix-ui/react-progress` | 1.1.7 | Progress indicators |
| `lucide-react` | 0.453.0 | Icon library |
| `recharts` | 2.15.2 | Chart library |

### 5.2.9 Utility Libraries

| Package | Version | Purpose |
|---------|---------|---------|
| `zod` | 4.1.12 | Schema validation |
| `superjson` | 1.13.3 | JSON serialization |
| `date-fns` | 4.1.0 | Date manipulation |
| `nanoid` | 5.1.5 | ID generation |
| `jose` | 6.1.0 | JWT handling |
| `axios` | 1.12.0 | HTTP client |

### 5.2.10 Development Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `vitest` | 2.1.4 | Test framework |
| `vite` | 7.1.7 | Build tool |
| `esbuild` | 0.25.0 | Bundler |
| `tsx` | 4.19.1 | TypeScript execution |
| `prettier` | 3.6.2 | Code formatting |

---

## 5.3 Installation Procedure

### 5.3.1 Step 1: System Preparation

```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Install essential build tools
sudo apt install -y build-essential git curl wget

# Install Node.js via nvm
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash
source ~/.bashrc
nvm install 22.13.0
nvm use 22.13.0
nvm alias default 22.13.0

# Verify Node.js installation
node --version  # Should output: v22.13.0

# Install pnpm
npm install -g pnpm@10.4.1
pnpm --version  # Should output: 10.4.1
```

### 5.3.2 Step 2: Database Setup

```bash
# Install PostgreSQL 16
sudo sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo apt-key add -
sudo apt update
sudo apt install -y postgresql-16 postgresql-16-pgvector

# Start PostgreSQL service
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Create database and user
sudo -u postgres psql << EOF
CREATE USER grace_admin WITH PASSWORD 'your_secure_password_here';
CREATE DATABASE grace_testing OWNER grace_admin;
GRANT ALL PRIVILEGES ON DATABASE grace_testing TO grace_admin;
\c grace_testing
CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
EOF

# Verify pgvector installation
sudo -u postgres psql -d grace_testing -c "SELECT extversion FROM pg_extension WHERE extname = 'vector';"
```

### 5.3.3 Step 3: Clone and Install Project

```bash
# Clone the repository
git clone https://github.com/Lev0n82/AskMarilyn.git
cd AskMarilyn

# Install dependencies
pnpm install

# Install Playwright browsers
npx playwright install chromium firefox webkit
npx playwright install-deps

# Verify Playwright installation
npx playwright --version  # Should output: Version 1.40.0
```

### 5.3.4 Step 4: Environment Configuration

Create `.env` file in project root:

```bash
# Database Configuration
DATABASE_URL=postgresql://grace_admin:your_secure_password_here@localhost:5432/grace_testing

# JWT Configuration
JWT_SECRET=your_jwt_secret_minimum_32_characters_long

# OAuth Configuration (Manus)
VITE_APP_ID=your_manus_app_id
OAUTH_SERVER_URL=https://api.manus.im
VITE_OAUTH_PORTAL_URL=https://auth.manus.im

# AI Configuration
ANTHROPIC_API_KEY=sk-ant-your_anthropic_api_key
OPENAI_API_KEY=sk-your_openai_api_key

# MCP Configuration
MCP_SERVER_PORT=3001
MCP_TRANSPORT=stdio

# Human-in-the-Loop Configuration
HITL_DEFAULT_TIMEOUT=120000
HITL_HIGH_RISK_TIMEOUT=300000

# Logging Configuration
LOG_LEVEL=info
LOG_FORMAT=json

# Feature Flags
ENABLE_VECTOR_SEARCH=true
ENABLE_AI_ANALYSIS=true
ENABLE_HITL=true
```

### 5.3.5 Step 5: Database Migration

```bash
# Generate migration files
pnpm db:push

# Verify tables were created
sudo -u postgres psql -d grace_testing -c "\dt"

# Expected output:
#              List of relations
#  Schema |        Name         | Type  |    Owner    
# --------+---------------------+-------+-------------
#  public | action_statistics   | table | grace_admin
#  public | execution_history   | table | grace_admin
#  public | test_knowledge      | table | grace_admin
#  public | test_jobs           | table | grace_admin
#  public | users               | table | grace_admin
#  public | grace_modules       | table | grace_admin
#  public | grace_quiz_questions| table | grace_admin
#  public | grace_user_progress | table | grace_admin
#  public | grace_certificates  | table | grace_admin
```

### 5.3.6 Step 6: Seed Initial Data

```bash
# Run the GRACE Academy seed script
node scripts/seed-grace-modules.mjs

# Verify seeding
sudo -u postgres psql -d grace_testing -c "SELECT COUNT(*) FROM grace_modules;"
# Expected: 30 modules

sudo -u postgres psql -d grace_testing -c "SELECT COUNT(*) FROM grace_quiz_questions;"
# Expected: 150+ questions
```

### 5.3.7 Step 7: Start Development Server

```bash
# Start the development server
pnpm dev

# Expected output:
# [2026-01-31T12:00:00.000Z] Server running on http://localhost:3000/
# [2026-01-31T12:00:00.000Z] [OAuth] Initialized with baseURL: https://api.manus.im

# Verify server is running
curl http://localhost:3000/api/health
# Expected: {"status":"ok","version":"4.0.0"}
```

### 5.3.8 Step 8: Start MCP Server

```bash
# In a separate terminal, start the MCP server
node src/mcp/server.ts

# Or use the MCP CLI
manus-mcp-cli tool list --server playwright

# Expected output:
# Available tools:
# - navigate
# - click
# - fill
# - screenshot
# - evaluate
# ... (additional tools)
```

---

## 5.4 Configuration Reference

### 5.4.1 Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `DATABASE_URL` | Yes | - | PostgreSQL connection string |
| `JWT_SECRET` | Yes | - | Secret for JWT signing (min 32 chars) |
| `VITE_APP_ID` | Yes | - | Manus OAuth application ID |
| `OAUTH_SERVER_URL` | Yes | - | Manus OAuth server URL |
| `ANTHROPIC_API_KEY` | Yes | - | Anthropic API key for Claude |
| `OPENAI_API_KEY` | No | - | OpenAI API key (for embeddings) |
| `MCP_SERVER_PORT` | No | 3001 | Port for MCP server |
| `MCP_TRANSPORT` | No | stdio | MCP transport (stdio/sse) |
| `HITL_DEFAULT_TIMEOUT` | No | 120000 | Default approval timeout (ms) |
| `HITL_HIGH_RISK_TIMEOUT` | No | 300000 | High-risk approval timeout (ms) |
| `LOG_LEVEL` | No | info | Logging level (debug/info/warn/error) |
| `LOG_FORMAT` | No | json | Log format (json/text) |
| `ENABLE_VECTOR_SEARCH` | No | true | Enable vector similarity search |
| `ENABLE_AI_ANALYSIS` | No | true | Enable AI-powered analysis |
| `ENABLE_HITL` | No | true | Enable human-in-the-loop |

### 5.4.2 Browser Configuration

```typescript
// config/browser.ts
export const browserConfig = {
  // Default browser to use
  defaultBrowser: 'chromium',  // 'chromium' | 'firefox' | 'webkit'
  
  // Browser launch options
  launchOptions: {
    headless: true,           // Run in headless mode
    slowMo: 0,                // Slow down operations (ms)
    timeout: 30000,           // Default timeout (ms)
    args: [
      '--no-sandbox',
      '--disable-setuid-sandbox',
      '--disable-dev-shm-usage',
      '--disable-accelerated-2d-canvas',
      '--disable-gpu'
    ]
  },
  
  // Context options
  contextOptions: {
    viewport: { width: 1920, height: 1080 },
    userAgent: 'GRACE-Testing-Agent/4.0',
    locale: 'en-US',
    timezoneId: 'America/New_York',
    permissions: ['geolocation', 'notifications'],
    recordVideo: {
      dir: './test-results/videos',
      size: { width: 1920, height: 1080 }
    },
    recordHar: {
      path: './test-results/har',
      mode: 'minimal'
    }
  },
  
  // Screenshot options
  screenshotOptions: {
    fullPage: true,
    type: 'png',
    path: './test-results/screenshots'
  }
};
```

### 5.4.3 Retry Policy Configuration

```typescript
// config/retry.ts
export const retryConfig = {
  // Maximum retry attempts
  maxAttempts: 3,
  
  // Initial delay between retries (ms)
  initialDelay: 1000,
  
  // Maximum delay between retries (ms)
  maxDelay: 10000,
  
  // Backoff multiplier
  backoffMultiplier: 2,
  
  // Errors that should trigger retry
  retryableErrors: [
    'TimeoutError',
    'NetworkError',
    'ElementNotFound',
    'StaleElementReference'
  ],
  
  // Errors that should NOT trigger retry
  nonRetryableErrors: [
    'AssertionError',
    'ValidationError',
    'AuthenticationError'
  ]
};
```

### 5.4.4 Human-in-the-Loop Configuration

```typescript
// config/hitl.ts
export const hitlConfig = {
  // Enable/disable HITL
  enabled: true,
  
  // Confidence thresholds
  thresholds: {
    autoExecute: 0.95,        // Auto-execute if confidence >= this
    autoExecuteWithLog: 0.85, // Auto-execute with logging if >= this
    requireApproval: 0.0      // Require approval if < autoExecuteWithLog
  },
  
  // Risk level overrides
  riskOverrides: {
    high: 'always_approve',   // Always require approval for high risk
    medium: 'threshold',      // Use threshold for medium risk
    low: 'threshold'          // Use threshold for low risk
  },
  
  // Timeout settings (ms)
  timeouts: {
    default: 120000,          // 2 minutes
    highRisk: 300000,         // 5 minutes
    critical: 600000          // 10 minutes
  },
  
  // Notification settings
  notifications: {
    channels: ['email', 'slack', 'webhook'],
    escalation: {
      afterMinutes: 5,
      escalateTo: ['admin@example.com']
    }
  }
};
```

---

## 5.5 Verification Checklist

### 5.5.1 Pre-Installation Verification

| Check | Command | Expected Result |
|-------|---------|-----------------|
| Node.js version | `node --version` | v22.13.0 |
| pnpm version | `pnpm --version` | 10.4.1 |
| PostgreSQL running | `sudo systemctl status postgresql` | active (running) |
| pgvector installed | `psql -c "SELECT extversion FROM pg_extension WHERE extname = 'vector';"` | 0.6.0 |

### 5.5.2 Post-Installation Verification

| Check | Command | Expected Result |
|-------|---------|-----------------|
| Dependencies installed | `pnpm list --depth=0` | No errors |
| Playwright browsers | `npx playwright --version` | Version 1.40.0 |
| Database connection | `pnpm db:push` | Migrations complete |
| Dev server starts | `pnpm dev` | Server running on port 3000 |
| Health endpoint | `curl localhost:3000/api/health` | {"status":"ok"} |
| Tests pass | `pnpm test` | All tests pass |

### 5.5.3 Integration Verification

```bash
# Run integration test suite
pnpm test:integration

# Expected output:
# ✓ Database connection established
# ✓ MCP server responds to tool list
# ✓ Playwright browser launches successfully
# ✓ Action runner executes navigate action
# ✓ Knowledge base stores and retrieves patterns
# ✓ HITL controller evaluates actions correctly
# 
# All integration tests passed!
```

---

# SECTION 6: ARCHITECTURAL DIAGRAMS

## 6.1 High-Level System Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        GRACE AUTONOMOUS TESTING SYSTEM v4                        │
│                     AI-Powered Autonomous Testing Platform                       │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                              PRESENTATION LAYER                                  │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                  │
│  │   GRACE Portal  │  │   Admin Panel   │  │   API Gateway   │                  │
│  │   (React 19)    │  │   (Dashboard)   │  │   (Express 4)   │                  │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘                  │
└───────────┼─────────────────────┼─────────────────────┼──────────────────────────┘
            │                     │                     │
            └─────────────────────┼─────────────────────┘
                                  │
┌─────────────────────────────────┼───────────────────────────────────────────────┐
│                          ORCHESTRATION LAYER                                     │
│                                 │                                                │
│  ┌──────────────────────────────▼──────────────────────────────────────────┐    │
│  │                        tRPC Router (v11.6.0)                             │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │    │
│  │  │   Auth      │  │   Modules   │  │   Progress  │  │   Admin     │    │    │
│  │  │   Router    │  │   Router    │  │   Router    │  │   Router    │    │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │    │
│  └──────────────────────────────────────────────────────────────────────────┘    │
│                                                                                  │
│  ┌──────────────────────────────────────────────────────────────────────────┐    │
│  │                     MCP Server (Model Context Protocol)                   │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │    │
│  │  │   Tools     │  │  Resources  │  │   Prompts   │  │  Transport  │    │    │
│  │  │   Handler   │  │   Handler   │  │   Handler   │  │   (stdio)   │    │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │    │
│  └──────────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                           EXECUTION LAYER (ABT)                                  │
│                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────┐     │
│  │                         TEST MODULES LAYER                              │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                 │     │
│  │  │   Login      │  │   Checkout   │  │   Search     │                 │     │
│  │  │   Test       │  │   Test       │  │   Test       │  ...            │     │
│  │  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘                 │     │
│  └─────────┼─────────────────┼─────────────────┼─────────────────────────┘     │
│            │                 │                 │                                 │
│  ┌─────────▼─────────────────▼─────────────────▼─────────────────────────┐     │
│  │                          ACTIONS LAYER                                 │     │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐│     │
│  │  │ Navigate │  │  Click   │  │   Fill   │  │  Assert  │  │ Screenshot││     │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘│     │
│  └───────┼─────────────┼─────────────┼─────────────┼─────────────┼──────┘     │
│          │             │             │             │             │             │
│  ┌───────▼─────────────▼─────────────▼─────────────▼─────────────▼──────┐     │
│  │                        INTERFACE LAYER                                │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                │     │
│  │  │  Playwright  │  │     API      │  │   Database   │                │     │
│  │  │   Driver     │  │   Connector  │  │    Access    │                │     │
│  │  └──────────────┘  └──────────────┘  └──────────────┘                │     │
│  └──────────────────────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                          INTELLIGENCE LAYER                                      │
│                                                                                  │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                  │
│  │  Claude 3.5     │  │  Human-in-the-  │  │   Failure       │                  │
│  │  Sonnet (AI)    │  │  Loop Controller│  │   Analyzer      │                  │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘                  │
│           │                    │                    │                            │
│  ┌────────▼────────────────────▼────────────────────▼────────┐                  │
│  │                    Knowledge Base                          │                  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │                  │
│  │  │   Vector    │  │   Pattern   │  │   Selector  │        │                  │
│  │  │   Store     │  │   Library   │  │   Registry  │        │                  │
│  │  │  (pgvector) │  │             │  │             │        │                  │
│  │  └─────────────┘  └─────────────┘  └─────────────┘        │                  │
│  └───────────────────────────────────────────────────────────┘                  │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                           DATA LAYER                                             │
│                                                                                  │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                     PostgreSQL 16 + pgvector                             │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │    │
│  │  │   Users     │  │   Modules   │  │   Progress  │  │   Jobs      │    │    │
│  │  │   Table     │  │   Table     │  │   Table     │  │   Queue     │    │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │    │
│  │  │   Quiz      │  │ Certificates│  │  Knowledge  │  │  Execution  │    │    │
│  │  │  Questions  │  │   Table     │  │   Vectors   │  │   History   │    │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## 6.2 ABT Three-Layer Architecture Detail

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│              ACTION-BASED TESTING (ABT) THREE-LAYER ARCHITECTURE                 │
└─────────────────────────────────────────────────────────────────────────────────┘

                              ┌─────────────────────────────────────┐
                              │         ABSTRACTION                  │
                              │   Focus on High-Level Business Logic │
                              └─────────────────────────────────────┘
                                              │
    ┌─────────────────────────────────────────┼─────────────────────────────────────┐
    │                                         │                                      │
    │                          TEST MODULES LAYER                                    │
    │                                         │                                      │
    │    ┌────────────────┐    ┌────────────────┐    ┌────────────────┐             │
    │    │  Login Test    │    │ Search Function│    │ Add to Cart    │             │
    │    │                │    │                │    │                │             │
    │    │ 1. Navigate    │    │ 1. Navigate    │    │ 1. Navigate    │             │
    │    │ 2. Enter creds │    │ 2. Enter query │    │ 2. Select item │             │
    │    │ 3. Click login │    │ 3. Click search│    │ 3. Click add   │             │
    │    │ 4. Verify      │    │ 4. Verify      │    │ 4. Verify cart │             │
    │    └───────┬────────┘    └───────┬────────┘    └───────┬────────┘             │
    │            │                     │                     │                       │
    │            └─────────────────────┼─────────────────────┘                       │
    │                                  │                                             │
    │                      Uses Reusable Actions                                     │
    │                                  │                                             │
    └──────────────────────────────────┼─────────────────────────────────────────────┘
                                       │
    ┌──────────────────────────────────┼─────────────────────────────────────────────┐
    │                                  │                                              │
    │                           ACTIONS LAYER                                         │
    │                                  │                                              │
    │    ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐       │
    │    │ Navigate │  │  Enter   │  │  Click   │  │  Select  │  │  Verify  │       │
    │    │  to URL  │  │   Text   │  │  Button  │  │   from   │  │   Text   │       │
    │    │          │  │          │  │          │  │ Dropdown │  │          │       │
    │    └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘       │
    │         │             │             │             │             │              │
    │    ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐       │
    │    │  Wait    │  │  Drag    │  │ Capture  │  │ Selefy   │  │  Hover   │       │
    │    │   for    │  │   and    │  │Screenshot│  │   Text   │  │   Over   │       │
    │    │ Element  │  │   Drop   │  │          │  │          │  │          │       │
    │    └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘       │
    │         │             │             │             │             │              │
    │         └─────────────┴─────────────┴─────────────┴─────────────┘              │
    │                                     │                                          │
    │                         Returns Results & Status                               │
    │                                     │                                          │
    └─────────────────────────────────────┼──────────────────────────────────────────┘
                                          │
    ┌─────────────────────────────────────┼──────────────────────────────────────────┐
    │                                     │                                           │
    │                           INTERFACE LAYER                                       │
    │                                     │                                           │
    │    ┌────────────────────────────────▼────────────────────────────────────┐     │
    │    │                                                                      │     │
    │    │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │     │
    │    │  │  Web Driver  │  │     API      │  │  Desktop Application     │  │     │
    │    │  │  (Playwright)│  │   Connector  │  │        Hooks             │  │     │
    │    │  └──────────────┘  └──────────────┘  └──────────────────────────┘  │     │
    │    │                                                                      │     │
    │    │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │     │
    │    │  │   Database   │  │    Mobile    │  │     Protocol             │  │     │
    │    │  │    Access    │  │   Emulator   │  │     Adapters             │  │     │
    │    │  └──────────────┘  └──────────────┘  └──────────────────────────┘  │     │
    │    │                                                                      │     │
    │    └──────────────────────────────────────────────────────────────────────┘     │
    │                                     │                                           │
    │                      Executes Technical Commands                                │
    │                                     │                                           │
    └─────────────────────────────────────┼───────────────────────────────────────────┘
                                          │
                                          ▼
                              ┌─────────────────────────────────────┐
                              │      SYSTEM UNDER TEST (SUT)        │
                              │   Web App / Mobile App / Desktop    │
                              └─────────────────────────────────────┘
```

## 6.3 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              DATA FLOW DIAGRAM                                   │
│                    GRACE Autonomous Testing System v4                            │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────┐                                                           ┌─────────┐
│   AI    │                                                           │  Human  │
│  Agent  │                                                           │ Reviewer│
│(Claude) │                                                           │         │
└────┬────┘                                                           └────┬────┘
     │                                                                     │
     │ 1. Request test execution                                           │
     │    via MCP protocol                                                 │
     ▼                                                                     │
┌─────────────────────────────────────────────────────────────────────────┐│
│                           MCP SERVER                                     ││
│  ┌─────────────────────────────────────────────────────────────────┐   ││
│  │                    Tool Handler                                  │   ││
│  │                                                                  │   ││
│  │  2. Parse tool call ──────────────────────────────────────────┐ │   ││
│  │                                                                │ │   ││
│  │  3. Validate parameters ◄─────────────────────────────────────┘ │   ││
│  │                                                                  │   ││
│  └──────────────────────────────┬───────────────────────────────────┘   ││
└─────────────────────────────────┼───────────────────────────────────────┘│
                                  │                                        │
                                  │ 4. Evaluate HITL                       │
                                  ▼                                        │
┌─────────────────────────────────────────────────────────────────────────┐│
│                      HUMAN-IN-THE-LOOP CONTROLLER                        ││
│                                                                          ││
│  ┌────────────────────────────────────────────────────────────────┐    ││
│  │  5. Calculate confidence + risk                                 │    ││
│  │                                                                 │    ││
│  │     IF confidence >= 0.95 AND risk = LOW:                      │    ││
│  │        → AUTO_EXECUTE                                          │    ││
│  │     ELSE IF confidence >= 0.85 AND risk != HIGH:               │    ││
│  │        → AUTO_EXECUTE_WITH_LOGGING                             │    ││
│  │     ELSE:                                                       │    ││
│  │        → REQUIRE_APPROVAL ─────────────────────────────────────┼────┼┘
│  │                                                                 │    │
│  │  6. Wait for approval (if required) ◄───────────────────────────┼────┘
│  │                                                                 │
│  └─────────────────────────────────────────────────────────────────┘
│                                  │
└──────────────────────────────────┼──────────────────────────────────────┘
                                   │
                                   │ 7. Execute action
                                   ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          ACTION RUNNER                                   │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  8. Look up action in registry                                   │   │
│  │                                                                  │   │
│  │  9. Execute with retry logic:                                    │   │
│  │     FOR attempt = 1 TO maxAttempts:                             │   │
│  │        TRY:                                                      │   │
│  │           result = action.execute(context, params)              │   │
│  │           IF success: RETURN result                             │   │
│  │        CATCH error:                                             │   │
│  │           IF retryable: WAIT(backoff) AND CONTINUE              │   │
│  │           ELSE: BREAK                                           │   │
│  │                                                                  │   │
│  └──────────────────────────────┬───────────────────────────────────┘   │
└─────────────────────────────────┼───────────────────────────────────────┘
                                  │
                                  │ 10. Interface call
                                  ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         PLAYWRIGHT DRIVER                                │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  11. Execute browser command:                                    │   │
│  │      - page.goto(url)                                           │   │
│  │      - page.click(selector)                                     │   │
│  │      - page.fill(selector, value)                               │   │
│  │      - page.screenshot()                                        │   │
│  │                                                                  │   │
│  │  12. Return result with:                                        │   │
│  │      - success: boolean                                         │   │
│  │      - data: any                                                │   │
│  │      - duration: number                                         │   │
│  │      - screenshots: string[]                                    │   │
│  │                                                                  │   │
│  └──────────────────────────────┬───────────────────────────────────┘   │
└─────────────────────────────────┼───────────────────────────────────────┘
                                  │
                                  │ 13. Store execution
                                  ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         KNOWLEDGE BASE                                   │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  14. Record execution history:                                   │   │
│  │      INSERT INTO execution_history (                            │   │
│  │        action_name, parameters, success,                        │   │
│  │        result_data, duration_ms, executed_at                    │   │
│  │      )                                                          │   │
│  │                                                                  │   │
│  │  15. Update action statistics:                                  │   │
│  │      UPDATE action_statistics                                   │   │
│  │      SET total_executions = total_executions + 1,               │   │
│  │          successful_executions = successful_executions + ?      │   │
│  │                                                                  │   │
│  │  16. Generate embedding (if new pattern):                       │   │
│  │      embedding = OpenAI.embed(content)                          │   │
│  │      INSERT INTO test_knowledge (content, embedding, ...)       │   │
│  │                                                                  │   │
│  └──────────────────────────────┬───────────────────────────────────┘   │
└─────────────────────────────────┼───────────────────────────────────────┘
                                  │
                                  │ 17. Return to AI
                                  ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          MCP RESPONSE                                    │
│                                                                          │
│  {                                                                       │
│    "content": [                                                          │
│      {                                                                   │
│        "type": "text",                                                   │
│        "text": "{\"success\": true, \"data\": {...}}"                   │
│      }                                                                   │
│    ]                                                                     │
│  }                                                                       │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

# SECTION 7: APPENDICES

## Appendix A: Glossary of Terms

| Term | Definition |
|------|------------|
| **ABT** | Action-Based Testing - A three-layer test architecture (Modules/Actions/Interface) |
| **Action** | An atomic, reusable test operation (e.g., click, fill, navigate) |
| **GRACE** | Governed Resilient Autonomous Certification for Enterprises |
| **HITL** | Human-in-the-Loop - System for human oversight of AI decisions |
| **MCP** | Model Context Protocol - Standardized AI-tool communication protocol |
| **pgvector** | PostgreSQL extension for vector similarity search |
| **Playwright** | Microsoft's browser automation library |
| **Test Module** | A business-level test scenario composed of actions |
| **tRPC** | TypeScript RPC framework for type-safe API calls |

## Appendix B: Reference Documents

1. Playwright Documentation: https://playwright.dev/docs/intro
2. MCP Specification: https://modelcontextprotocol.io/docs
3. pgvector Documentation: https://github.com/pgvector/pgvector
4. tRPC Documentation: https://trpc.io/docs
5. Drizzle ORM Documentation: https://orm.drizzle.team/docs/overview

## Appendix C: Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 4.0.0 | 2026-01-31 | GRACE Team | Initial DDD release |

---

**END OF DOCUMENT**

*Document Version: 4.0.0*
*Last Updated: January 31, 2026*
*Classification: Technical Specification*

