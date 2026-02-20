# Phase 3: Azure DevOps Integration - Implementation Specifications

## Document Information

**Version**: 1.0  
**Date**: February 19, 2026  
**Author**: Manus AI  
**Status**: Draft for Review

## Executive Summary

This document provides comprehensive implementation specifications for Phase 3 of the CPU Agents autonomous system. Phase 3 introduces Azure DevOps integration, enabling the agent to retrieve requirements from Azure Boards, publish test cases to Azure Test Plans, report test results, and store test artifacts in Azure Repos.

The implementation follows a service-oriented architecture with four primary service classes, comprehensive error handling with retry logic, two-tier caching for performance optimization, and built-in self-testing at all levels. All components are designed for testability, maintainability, and extensibility.

This specification document complements the Phase 3 Architecture Design document and provides the detailed implementation guidance needed for developers to build the system correctly the first time.

## Implementation Overview

### Development Approach

Phase 3 implementation follows the comprehensive-implementation skill methodology with these key principles:

**Specification-First**: All interfaces, data models, and acceptance criteria are defined before any code is written. This ensures clear contracts and prevents rework.

**Test-Driven Development**: Unit tests are written alongside implementation code. Integration tests validate component interactions. End-to-end tests validate complete workflows.

**Incremental Delivery**: Implementation proceeds in six sub-phases, each delivering a complete, tested increment that builds on previous work.

**Built-In Quality**: Self-testing framework integration ensures continuous validation. Multi-level acceptance criteria provide clear success metrics.

### Technology Stack

**Core Framework**: .NET 8.0 with C# 12.0  
**HTTP Client**: System.Net.Http with HttpClientFactory  
**JSON Serialization**: System.Text.Json with source generators  
**Resilience**: Polly 8.0 for retry and circuit breaker policies  
**Caching**: Microsoft.Extensions.Caching.Memory + PostgreSQL  
**Testing**: xUnit 2.6, Moq 4.20, FluentAssertions 6.12, WireMock.Net 1.5  
**Database**: Npgsql 8.0 for PostgreSQL connectivity

### Project Structure

```
AutonomousAgent.AzureDevOps/
├── src/
│   ├── Core/
│   │   ├── AzureDevOpsClientFactory.cs
│   │   ├── AzureDevOpsConfiguration.cs
│   │   └── Interfaces.cs
│   ├── Services/
│   │   ├── WorkItemService.cs
│   │   ├── TestPlanService.cs
│   │   └── GitService.cs
│   ├── Models/
│   │   ├── WorkItemModel.cs
│   │   ├── TestCaseModel.cs
│   │   └── TestResultModel.cs
│   └── SelfTest/
│       └── AzureDevOpsSelfTests.cs
└── tests/
    ├── Unit/
    ├── Integration/
    └── EndToEnd/
```

## Component Specifications

### 1. Azure DevOps Client Factory

#### Purpose
Centralized factory for creating and configuring HttpClient instances for Azure DevOps API communication.

#### Class: AzureDevOpsClientFactory

**Namespace**: `AutonomousAgent.AzureDevOps.Core`

**Public Methods**:

```csharp
/// <summary>
/// Creates a configured HttpClient for Azure DevOps API calls.
/// </summary>
public HttpClient CreateClient(string clientName = null)
```

**Implementation Details**:

1. Validate configuration (OrganizationUrl, Project, PAT)
2. Create HttpClient using HttpClientFactory
3. Set BaseAddress to `{OrganizationUrl}/{Project}/_apis/`
4. Configure authentication header: `Authorization: Basic {Base64Encode(":" + PAT)}`
5. Set default headers (Accept, User-Agent, api-version)
6. Configure timeout from configuration
7. Log client creation
8. Return configured client

**Acceptance Criteria**:

**Function-Level**:
- ✅ `CreateClient()` returns HttpClient with correct BaseAddress
- ✅ `CreateClient()` sets Authorization header with Base64-encoded PAT
- ✅ `CreateClient()` sets all required default headers
- ✅ `CreateClient()` throws ArgumentException for invalid configuration

**Class-Level**:
- ✅ Factory can create multiple clients without conflicts
- ✅ Factory validates all configuration properties
- ✅ Factory logs all client creation events

### 2. Work Item Service

#### Purpose
Provides operations for managing work items in Azure Boards.

#### Interface: IAzureDevOpsWorkItemService

**Methods**:

```csharp
Task<List<WorkItemModel>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default);
Task<WorkItemModel> GetWorkItemAsync(int id, CancellationToken cancellationToken = default);
Task<List<WorkItemModel>> GetWorkItemsBatchAsync(int[] ids, CancellationToken cancellationToken = default);
Task<WorkItemModel> CreateWorkItemAsync(string workItemType, Dictionary<string, object> fields, CancellationToken cancellationToken = default);
Task<WorkItemModel> UpdateWorkItemAsync(int id, Dictionary<string, object> fields, CancellationToken cancellationToken = default);
Task<WorkItemLinkModel> CreateWorkItemLinkAsync(int sourceId, int targetId, string linkType, CancellationToken cancellationToken = default);
```

**Acceptance Criteria**:

**Function-Level**:
- ✅ `QueryWorkItemsAsync()` returns correct work items for valid WIQL
- ✅ `QueryWorkItemsAsync()` throws ArgumentException for null/empty WIQL
- ✅ `GetWorkItemAsync()` returns correct work item for valid ID
- ✅ `CreateWorkItemAsync()` creates work item with all specified fields
- ✅ `UpdateWorkItemAsync()` updates work item fields correctly
- ✅ `CreateWorkItemLinkAsync()` creates bidirectional link

**Class-Level**:
- ✅ Service handles API errors gracefully with appropriate exceptions
- ✅ Service implements retry logic for transient failures (max 5 retries)
- ✅ Service is thread-safe for concurrent operations

### 3. Test Plan Service

#### Purpose
Manages test plans, test suites, test cases, and test results in Azure Test Plans.

#### Interface: IAzureDevOpsTestPlanService

**Methods**:

```csharp
Task<TestPlanModel> CreateTestPlanAsync(string name, string areaPath, string iteration, CancellationToken cancellationToken = default);
Task<TestCaseModel> CreateTestCaseAsync(TestCaseModel testCase, CancellationToken cancellationToken = default);
Task<TestResultModel> PublishTestResultAsync(TestResultModel result, CancellationToken cancellationToken = default);
Task<string> UploadTestAttachmentAsync(int testRunId, int testResultId, string filePath, string attachmentType, CancellationToken cancellationToken = default);
```

**Acceptance Criteria**:

**Function-Level**:
- ✅ `CreateTestPlanAsync()` creates test plan with correct properties
- ✅ `CreateTestCaseAsync()` creates test case with all steps
- ✅ `PublishTestResultAsync()` publishes result with correct outcome
- ✅ `UploadTestAttachmentAsync()` uploads file and returns attachment URL

## Testing Strategy

### Unit Testing

**Coverage Target**: 95%+ code coverage

**Test Frameworks**:
- xUnit for test execution
- Moq for mocking dependencies
- FluentAssertions for readable assertions

### Integration Testing

**Purpose**: Validate component interactions and API communication using WireMock.Net for mocking Azure DevOps APIs.

### End-to-End Testing

**Purpose**: Validate complete workflows from requirements to test results.

**Test Scenarios**:
1. Requirements ingestion: Query work items → Parse requirements
2. Test case publishing: Generate test cases → Create test plan → Link to requirements
3. Test execution: Execute tests → Publish results with attachments

## System-Level Acceptance Criteria

**End-to-End Workflows**:
- ✅ Agent retrieves requirements from Azure Boards
- ✅ Agent generates test cases from requirements
- ✅ Agent publishes test cases to Azure Test Plans
- ✅ Agent links test cases to requirements
- ✅ Agent publishes test results with attachments
- ✅ All operations complete within performance targets
- ✅ System handles 1000+ requirements without issues

**Non-Functional Requirements**:
- ✅ WCAG 2.2 AAA compliance for any UI components
- ✅ Built-in self-testing validates all components on startup
- ✅ All tests pass (unit, integration, end-to-end)
- ✅ Code coverage ≥ 95%

## References

1. [Azure DevOps Services REST API Reference](https://learn.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-7.2)
2. [Work Item Tracking API](https://learn.microsoft.com/en-us/rest/api/azure/devops/wit/work-items?view=azure-devops-rest-7.1)
3. [Test Plans API](https://learn.microsoft.com/en-us/rest/api/azure/devops/testplan/test-plans?view=azure-devops-rest-7.1)
