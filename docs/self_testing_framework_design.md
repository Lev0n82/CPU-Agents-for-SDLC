# Self-Testing Framework for the Autonomous AI Agent

## 1. Introduction

To ensure the reliability, robustness, and integrity of the autonomous AI agent, a comprehensive self-testing framework is essential. This framework is designed to be an integral part of the agent's startup sequence and ongoing operations, providing a multi-level validation of its core components and capabilities. The primary goal is to create a self-aware agent that can verify its own health and functionality before beginning its tasks, ensuring a high degree of confidence in its operations.

This document outlines the architecture and design of this built-in self-testing framework, which will be initiated automatically upon agent startup.

### 1.1 Design Goals

-   **Comprehensive Validation:** The framework must test all critical components of the agent, from low-level functions to high-level system integrations.
-   **Multi-Level Granularity:** Testing must occur at four distinct levels: function, class, module, and system.
-   **Automation:** The entire self-test sequence must be fully automated and run without human intervention upon agent startup.
-   **Clear Reporting:** The framework must produce a clear, concise report of the test results, indicating the health of each component.
-   **Fail-Fast Principle:** If a critical component fails its self-test, the agent should not proceed with its normal operations and should clearly report the failure.
-   **Extensibility:** The framework should be designed to easily accommodate new tests as new agent capabilities are developed.

---

## 2. Self-Testing Architecture

The self-testing framework is designed as a dedicated module within the agent's orchestration layer. It is the first module to be invoked after the agent's core services are initialized.

### 2.1 High-Level Diagram

```
┌──────────────────────┐   1. Initiate Self-Test   ┌────────────────────────┐
│   Agent Startup      │──────────────────────────►│   Self-Test Manager    │
│      Sequence        │                          └────────────┬───────────┘
└──────────────────────┘                                        │ 2. Execute Tests
                                                                │   Sequentially
                                                                ▼
┌───────────────────────────────────────────────────────────────────────────┐
│                             Test Levels                                   │
│                                                                           │
│  ┌───────────────┐   ┌───────────────┐   ┌───────────────┐   ┌──────────┐ │
│  │ Function-Level│──►│  Class-Level  │──►│ Module-Level  │──►│ System-  │ │
│  │     Tests     │   │     Tests     │   │     Tests     │   │ Level    │ │
│  │ (Unit Tests)  │   │(Integration)  │   │(Component)    │   │ Tests    │ │
│  └───────────────┘   └───────────────┘   └───────────────┘   └──────────┘ │
│                                                                           │
└───────────────────────────────────────────────────────────────────────────┘
                                                                │ 3. Aggregate
                                                                │    Results
                                                                ▼
┌──────────────────────┐      4. Log & Report       ┌──────────────────────┐
│   Agent Main Loop    │◄────────────────────────────│   Test Result Logger   │
│ (Proceed if all pass)│                            └──────────────────────┘
└──────────────────────┘
```

### 2.2 Components

-   **Self-Test Manager:** The central orchestrator of the self-testing process. It is responsible for invoking the tests at each level in the correct order and aggregating the results.
-   **Test Levels:** A series of test suites, each targeting a different level of granularity within the agent's architecture.
-   **Test Result Logger:** A component that captures the output of each test, formats it into a readable report, and logs it to both the console and a persistent log file.

---

## 3. Multi-Level Validation Strategy

The framework's core strength lies in its multi-level approach, ensuring that validation is both deep and broad.

### 3.1 Level 1: Function-Level Tests (Unit Tests)

-   **Scope:** The smallest units of code (individual methods and functions).
-   **Goal:** To verify that each function behaves as expected in isolation.
-   **Implementation:** These are classic unit tests, likely written using a framework like xUnit or NUnit for .NET.
-   **Examples:**
    -   Test a utility function that formats dates.
    -   Test a parsing function for a specific data structure.
    -   Test a calculation within a larger algorithm.

### 3.2 Level 2: Class-Level Tests (Integration Tests)

-   **Scope:** Individual classes and their immediate dependencies.
-   **Goal:** To verify that the methods within a class work together correctly and that the class integrates properly with its direct collaborators (e.g., other classes it instantiates or methods it calls).
-   **Implementation:** These tests involve instantiating the class under test and providing mock or stub implementations for its external dependencies.
-   **Examples:**
    -   Test a `RequirementParser` class by providing it with mock file content and verifying the structured JSON output.
    -   Test a `DatabaseClient` class by connecting it to an in-memory or test database instance and verifying CRUD operations.

### 3.3 Level 3: Module-Level Tests (Component Tests)

-   **Scope:** A complete module or a major component of the agent (e.g., the Cognitive Core, the Tooling Layer).
-   **Goal:** To verify that a full module can perform its end-to-end function correctly. This involves testing the public API of the module.
-   **Implementation:** These tests treat the module as a black box, sending inputs to its public interface and validating the outputs.
-   **Examples:**
    -   **Cognitive Core Test:** Provide the Planning module with a complex goal and verify that it produces a valid, multi-step plan.
    -   **Tooling Layer Test:** Send a request to the `Tool Abstraction API` to execute a specific tool (e.g., `FileSystem.ReadFile`) and verify the result.
    -   **Memory System Test:** Test the full lifecycle of a memory item: write to working memory, transfer to short-term, and finally archive to long-term vector storage.

### 3.4 Level 4: System-Level Tests (End-to-End Tests)

-   **Scope:** The entire agent system, including its integration with external systems like Azure DevOps.
-   **Goal:** To verify that the agent can successfully complete a simple, representative end-to-end workflow.
-   **Implementation:** These tests simulate a real user request and check the final output.
-   **Examples:**
    -   **Azure DevOps Connectivity Test:**
        1.  Connect to the configured Azure DevOps organization.
        2.  Fetch a specific (pre-defined) work item.
        3.  Verify that the work item's title is read correctly.
    -   **LLM Engine Health Check:**
        1.  Load the configured SLM model into memory via `llama.cpp`.
        2.  Send a simple prompt (e.g., "What is 2+2?").
        3.  Verify that the response is coherent and correct (e.g., contains the number "4").
    -   **Database Connectivity Test:**
        1.  Connect to the PostgreSQL and Oracle databases.
        2.  Execute a simple `SELECT 1;` query.
        3.  Verify that the connection is successful and the query returns the expected result.

---

## 4. Startup Self-Test Sequence

The `Self-Test Manager` will execute the following sequence every time the agent starts:

1.  **Log Start:** Write a log entry indicating that the self-test sequence is beginning.
2.  **Execute Level 1 Tests:** Run all function-level unit tests.
3.  **Execute Level 2 Tests:** Run all class-level integration tests.
4.  **Execute Level 3 Tests:** Run all module-level component tests.
5.  **Execute Level 4 Tests:** Run all system-level end-to-end tests.
6.  **Aggregate Results:** Collect the pass/fail status and duration of each test.
7.  **Generate Report:** Create a summary report.
8.  **Decision Point:**
    -   **If all tests pass:** Log a success message and allow the agent's main operational loop to begin.
    -   **If any test fails:**
        -   Log a critical error message detailing which test(s) failed.
        -   Prevent the agent's main loop from starting.
        -   Enter a 
safe mode where it only reports its failed status and awaits intervention.

---

## 5. Implementation Details

-   **Test Discovery:** The `Self-Test Manager` will use reflection to discover all methods in the codebase that are decorated with custom attributes like `[FunctionTest]`, `[ClassTest]`, `[ModuleTest]`, and `[SystemTest]`.
-   **Test Runner:** A simple, custom test runner will be built to execute the discovered tests in the correct order.
-   **Assertions:** A lightweight assertion library will be used to check conditions and throw exceptions on failure.

**Example Test Method:**

```csharp
public class LlmEngineTests
{
    [SystemTest(Description = "Verify LLM engine can load a model and respond to a prompt.")]
    public async Task LlmEngineHealthCheck()
    {
        var llmEngine = new LlmEngine();
        await llmEngine.LoadModelAsync("phi-3-mini-4k-instruct-q4.gguf");
        
        var response = await llmEngine.GenerateResponseAsync("What is the capital of France?");
        
        Assert.IsNotNull(response, "LLM response should not be null.");
        Assert.Contains(response, "Paris", "LLM response should contain 'Paris'.");
    }
}
```

---

**End of Document**
