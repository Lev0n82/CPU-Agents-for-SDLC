# Phase 3 Integration Tests

**End-to-End Integration Testing for CPU Agents**

This project contains comprehensive integration tests for all Phase 3 modules of the CPU Agents for SDLC system. The tests validate the complete workflow of work item processing, test automation, Git integration, offline synchronization, and resilience patterns.

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Quick Start](#quick-start)
4. [Test Scenarios](#test-scenarios)
5. [Configuration](#configuration)
6. [Running Tests](#running-tests)
7. [Test Reports](#test-reports)
8. [Troubleshooting](#troubleshooting)

---

## Overview

The integration test suite validates the following Phase 3 modules:

**Phase 3.1 (Critical Foundations)**:
- Authentication & Authorization (PAT)
- Concurrency Control (work item claiming)
- Secrets Management (DPAPI)
- Work Item Service (CRUD operations)

**Phase 3.2 (Core Services)**:
- Test Plan Service (test case management)
- Git Service (repository operations)
- Offline Synchronization (conflict resolution)
- Git Workspace Management (dependency caching)

**Phase 3.3 (Operational Resilience)**:
- Resilience Policy (retry, circuit breaker, timeout, bulkhead)
- Telemetry Service (OpenTelemetry)
- Cache Service (in-memory caching)
- Rate Limiter (token bucket)

**Phase 3.4 (Migration & Testing)**:
- Test Lifecycle Manager (obsolete test case detection)
- Migration Service (Phase 2→3 migration)

---

## Prerequisites

### Required Software

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Azure DevOps Account** - [Sign up](https://dev.azure.com)
- **Personal Access Token** - With Work Items (Read, Write, Manage) and Test Management (Read, Write) permissions

### Optional (for observability)

- **Podman** or **Docker** - For running OpenTelemetry stack
- **OpenTelemetry Stack** - See `/home/ubuntu/opentelemetry-podman/`

---

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/CPU-Agents-for-SDLC.git
cd CPU-Agents-for-SDLC/tests/Phase3.IntegrationTests
```

### 2. Configure Azure DevOps

Edit `appsettings.json` with your Azure DevOps details:

```json
{
  "AzureDevOps": {
    "OrganizationUrl": "https://dev.azure.com/YourOrganization",
    "ProjectName": "YourProject",
    "PersonalAccessToken": "your-pat-here"
  }
}
```

**Security Note**: For production, use environment variables instead of storing PAT in `appsettings.json`.

### 3. Set Up Test Data (Optional)

Run the setup script to create test work items:

```bash
export AZURE_DEVOPS_PAT='your-pat-here'
./setup-azure-devops.sh
```

### 4. Run Tests

```bash
./run-tests.sh
```

---

## Test Scenarios

### Scenario 1: End-to-End Work Item Processing

**Purpose**: Validates the complete workflow of querying, claiming, updating, and releasing work items with caching, telemetry, and resilience.

**Steps**:
1. Authenticate with Azure DevOps (MSAL)
2. Query available work items (WorkItemService + Cache + Resilience)
3. Claim a work item (ConcurrencyController)
4. Update work item state (WorkItemService + Telemetry)
5. Verify metrics recorded (TelemetryService)
6. Check cache hit rate (CacheService)
7. Check circuit breaker state (ResiliencePolicy)
8. Release work item claim (ConcurrencyController)

**Expected Results**:
- Work items are queried successfully
- Work item is claimed without conflicts
- State is updated to "Active"
- Metrics are recorded (API duration, work items processed)
- Cache hit rate improves on subsequent queries
- Circuit breaker remains closed

**Test File**: `Scenarios/Scenario1_WorkItemProcessingTests.cs`

---

### Scenario 2: Test Case Automation

**Purpose**: Validates the complete workflow of creating test cases, executing tests, and managing obsolete test cases.

**Steps**:
1. Create a requirement work item (WorkItemService)
2. Create test case linked to requirement (TestPlanService)
3. Execute test and record result (TestPlanService)
4. Remove requirement (WorkItemService)
5. Detect obsolete test case (TestLifecycleManager)
6. Close obsolete test case (TestLifecycleManager)

**Expected Results**:
- Test case is created and linked to requirement
- Test result is recorded as "Passed"
- Obsolete test case is detected after requirement removal
- Test case is closed automatically

**Test File**: `Scenarios/Scenario2_TestCaseAutomationTests.cs`

---

### Scenario 3: Git Integration

**Purpose**: Validates the complete workflow of workspace management, repository cloning, and Git operations.

**Steps**:
1. Create workspace (GitWorkspaceManager)
2. Clone repository (GitService + Secrets)
3. Make changes and commit (GitService)
4. Push changes (GitService + RateLimiter)
5. Verify workspace cached (GitWorkspaceManager)
6. Delete workspace (GitWorkspaceManager)

**Expected Results**:
- Workspace is created successfully
- Repository is cloned with progress tracking
- Changes are committed and pushed
- Workspace is cached for reuse
- Rate limiter enforces token limits

**Test File**: `Scenarios/Scenario3_GitIntegrationTests.cs`

---

### Scenario 4: Offline Synchronization

**Purpose**: Validates the complete workflow of offline mode, conflict detection, and conflict resolution.

**Steps**:
1. Enable offline mode (OfflineSyncService)
2. Queue work item updates while offline
3. Go online and sync (OfflineSyncService)
4. Detect conflicts (OfflineSyncService)
5. Resolve conflicts with policy (OfflineSyncService)

**Expected Results**:
- Offline mode is enabled successfully
- Updates are queued locally
- Sync completes without errors
- Conflicts are detected and resolved according to policy

**Test File**: `Scenarios/Scenario4_OfflineSyncTests.cs`

---

### Scenario 5: Resilience Testing

**Purpose**: Validates retry policies, circuit breaker, timeout handling, and bulkhead isolation.

**Steps**:
1. Simulate transient failures (ResiliencePolicy)
2. Verify retry attempts (ResiliencePolicy)
3. Trigger circuit breaker (5 consecutive failures)
4. Verify circuit opens (ResiliencePolicy)
5. Wait for circuit to close (30 seconds)

**Expected Results**:
- Retry policy attempts 3 times before failing
- Circuit breaker opens after 5 failures
- Circuit breaker closes after 30 seconds
- Bulkhead limits parallel execution to 10

**Test File**: `Scenarios/Scenario5_ResilienceTests.cs`

---

## Configuration

### appsettings.json

Complete configuration reference:

```json
{
  "AzureDevOps": {
    "OrganizationUrl": "https://dev.azure.com/YourOrganization",
    "ProjectName": "YourProject",
    "PersonalAccessToken": "your-pat-here"
  },
  "Authentication": {
    "Method": "PAT"
  },
  "Resilience": {
    "CircuitBreaker": {
      "FailureThreshold": 5,
      "DurationOfBreakSeconds": 30
    },
    "Retry": {
      "MaxAttempts": 3,
      "InitialDelaySeconds": 1
    },
    "Timeout": {
      "Seconds": 30
    },
    "Bulkhead": {
      "MaxParallelization": 10,
      "MaxQueuingActions": 20
    }
  },
  "Telemetry": {
    "ServiceName": "cpu-agents-integration-tests",
    "ServiceVersion": "3.0.0",
    "OtlpEndpoint": "http://localhost:4317",
    "ConsoleExporter": true,
    "SamplingRate": 1.0
  },
  "Cache": {
    "DefaultTtlMinutes": 5
  },
  "RateLimiting": {
    "TokensPerSecond": 10,
    "BurstCapacity": 20
  },
  "IntegrationTests": {
    "CleanupAfterTests": true,
    "CreateTestData": true,
    "TestWorkItemPrefix": "[IntegrationTest]",
    "MaxTestDurationMinutes": 30
  }
}
```

### Environment Variables

For production, use environment variables instead of `appsettings.json`:

```bash
export AzureDevOps__OrganizationUrl="https://dev.azure.com/YourOrganization"
export AzureDevOps__ProjectName="YourProject"
export AzureDevOps__PersonalAccessToken="your-pat-here"
```

---

## Running Tests

### Run All Tests

```bash
./run-tests.sh
```

### Run Specific Scenario

```bash
./run-tests.sh 1  # Scenario 1: Work Item Processing
./run-tests.sh 2  # Scenario 2: Test Case Automation
./run-tests.sh 3  # Scenario 3: Git Integration
./run-tests.sh 4  # Scenario 4: Offline Sync
./run-tests.sh 5  # Scenario 5: Resilience Testing
```

### Run with .NET CLI

```bash
dotnet test --verbosity normal
```

### Run with Code Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run with Detailed Output

```bash
dotnet test --verbosity detailed --logger "console;verbosity=detailed"
```

---

## Test Reports

### Console Output

Tests output detailed step-by-step progress to the console:

```
=== Scenario 1: End-to-End Work Item Processing ===
Organization: https://dev.azure.com/LevonMinasyan2
Project: CPU-Agents-Test

Step 1: Authenticating with Azure DevOps...
Step 2: Querying available work items...
  ✓ Found 15 work items
Step 3: Claiming a work item...
  ✓ Claimed work item 123
...
```

### Test Results

View test results summary:

```bash
dotnet test --logger "trx;LogFileName=test-results.trx"
```

### Code Coverage Report

Generate HTML coverage report:

```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

---

## Troubleshooting

### Authentication Failures

**Error**: `401 Unauthorized`

**Solution**:
- Verify PAT is valid and not expired
- Ensure PAT has required permissions (Work Items: Read, Write, Manage)
- Check organization URL is correct

### Work Items Not Found

**Error**: `No work items available for testing`

**Solution**:
- Run `./setup-azure-devops.sh` to create test work items
- Verify project name is correct
- Check work item query in test code

### Git Clone Failures

**Error**: `Failed to clone repository`

**Solution**:
- Verify Git credentials are configured
- Check repository URL is accessible
- Ensure workspace directory has write permissions

### OpenTelemetry Connection Failures

**Error**: `Failed to connect to OTLP endpoint`

**Solution**:
- Start OpenTelemetry stack: `cd /home/ubuntu/opentelemetry-podman && ./start.sh`
- Verify OTLP endpoint is `http://localhost:4317`
- Check firewall allows port 4317

### Circuit Breaker Open

**Error**: `Circuit breaker is open`

**Solution**:
- Wait 30 seconds for circuit to close
- Check Azure DevOps service status
- Verify network connectivity

---

## Contributing

To add new test scenarios:

1. Create a new test class in `Scenarios/`
2. Inherit from `IClassFixture<IntegrationTestFixture>`
3. Use dependency injection to get services
4. Follow the existing test pattern
5. Update this README with scenario documentation

---

## License

Copyright © 2026 CPU Agents Team. All rights reserved.
