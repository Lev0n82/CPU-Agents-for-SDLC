# Azure DevOps Integration Summary

## Overview

The autonomous AI agent architecture has been updated to make **Azure DevOps** the primary platform for requirements and test case management. This document summarizes the key integration points and changes.

## Primary Integration Points

### 1. Azure Boards (Requirements Management)

Azure Boards serves as the **primary source** for requirements management through work items:

- **User Stories**: Primary source for test case generation
- **Features**: Feature-level test planning and organization
- **Epics**: Strategic planning context
- **Tasks**: Implementation tracking
- **Bugs**: Automatically linked to failed test results

**Key Capabilities:**
- Query work items using WIQL (Work Item Query Language)
- Extract acceptance criteria from work item descriptions
- Manage work item links and relationships
- Track requirement status and state transitions
- Bidirectional traceability through work item links

### 2. Azure Test Plans (Test Case Management)

Azure Test Plans is the **primary platform** for test case storage and management:

- **Test Plans**: Container for organizing test suites
- **Test Suites**: Groups of test cases (static, requirements-based, query-based)
- **Test Cases**: Individual test scenarios with steps
- **Shared Steps**: Reusable test steps across multiple test cases
- **Shared Parameters**: Reusable test data

**Key Capabilities:**
- Agent automatically creates test cases in Azure Test Plans
- Requirements-based test suites automatically link test cases to requirements
- Test results published directly to Azure DevOps
- Test configurations for multi-environment testing
- Rich diagnostic data collection (screenshots, logs, recordings)

### 3. Azure Repos (Artifact Storage)

Azure Repos stores generated test artifacts and automation code:

- Test automation code (Playwright scripts)
- Generated test plans and documentation
- Accessibility remediation code
- Test data files

**Key Capabilities:**
- Version control for all test artifacts
- Pull request integration for test case reviews
- Branch policies for test quality gates
- Automatic commit and push of generated artifacts

## Integration Architecture

```
┌─────────────────────────────────────────────────────────┐
│              Autonomous AI Agent                         │
│  ┌───────────────────────────────────────────────────┐  │
│  │         Requirements Management Toolkit            │  │
│  │  - Parse Azure DevOps work items                   │  │
│  │  - Extract User Stories, Features, Epics           │  │
│  │  - Analyze acceptance criteria                     │  │
│  │  - Generate test cases                             │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                    Azure DevOps                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Azure Boards │  │ Azure Test   │  │ Azure Repos  │  │
│  │              │  │ Plans        │  │              │  │
│  │ - User       │  │ - Test Plans │  │ - Test       │  │
│  │   Stories    │  │ - Test Cases │  │   Artifacts  │  │
│  │ - Features   │  │ - Test Runs  │  │ - Automation │  │
│  │ - Epics      │  │ - Results    │  │   Code       │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## Workflow Example

### End-to-End Test Case Generation Workflow

1. **Requirement Creation** (Azure Boards)
   - User creates a User Story in Azure Boards
   - Story includes title, description, and acceptance criteria

2. **Agent Detection** (Observer Agent)
   - Agent monitors Azure DevOps for new/updated work items
   - Identifies User Story as requiring test case generation

3. **Requirements Analysis** (Requirements Analysis Agent)
   - Fetches User Story via Azure DevOps REST API
   - Parses acceptance criteria
   - Assesses testability
   - Identifies test scenarios

4. **Test Case Generation** (Test Generation Agent)
   - Generates functional test cases (positive, negative, boundary)
   - Generates non-functional test cases (performance, security)
   - Creates test cases in Azure Test Plans via REST API
   - Automatically links test cases to User Story

5. **Test Plan Creation** (Test Plan Agent)
   - Creates or updates test plan in Azure Test Plans
   - Organizes test cases into test suites
   - Creates requirements-based test suite for automatic linking

6. **Test Execution** (Execution Agent)
   - Executes automated tests using Playwright
   - Publishes test results to Azure Test Plans
   - Links failed tests to bugs in Azure Boards

7. **Traceability** (Automatic)
   - Azure DevOps maintains bidirectional links
   - Requirements Traceability Matrix generated on demand
   - Coverage reports available through Azure DevOps dashboards

## API Integration Details

### Authentication

All Azure DevOps API calls use Personal Access Token (PAT) authentication:

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Basic", 
        Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}")));
```

### Key API Endpoints

**Azure Boards (Work Items):**
- `GET https://dev.azure.com/{org}/{project}/_apis/wit/workitems/{id}?api-version=7.1`
- `POST https://dev.azure.com/{org}/{project}/_apis/wit/wiql?api-version=7.1`
- `PATCH https://dev.azure.com/{org}/{project}/_apis/wit/workitems/{id}?api-version=7.1`

**Azure Test Plans:**
- `POST https://dev.azure.com/{org}/{project}/_apis/testplan/plans?api-version=7.1`
- `POST https://dev.azure.com/{org}/{project}/_apis/wit/workitems/$Test Case?api-version=7.1`
- `POST https://dev.azure.com/{org}/{project}/_apis/test/runs/{runId}/results?api-version=7.1`

**Azure Repos:**
- `POST https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repoId}/pushes?api-version=7.1`

## Configuration

Azure DevOps integration is configured in `config.json`:

```json
{
  "enterprise": {
    "azure_devops": {
      "organization": "enterprise-org",
      "project": "MyProject",
      "pat": "***",
      "boards": {
        "enabled": true,
        "area_path": "MyProject\\MyTeam"
      },
      "test_plans": {
        "enabled": true,
        "default_plan_id": 12345
      },
      "repos": {
        "enabled": true,
        "repository_id": "test-artifacts-repo"
      }
    }
  }
}
```

## Benefits

### Unified Platform
- Single source of truth for requirements, test cases, and results
- Seamless traceability from requirements to test cases to results
- Integrated reporting and analytics through Azure DevOps dashboards

### Automated Workflows
- Agent automatically creates test cases in Azure Test Plans
- Test results published directly to Azure DevOps
- Requirements-based test suites automatically update when requirements change
- Work item links maintain bidirectional traceability

### Enterprise Features
- Role-based access control (RBAC)
- Audit logging for compliance
- Integration with Azure Pipelines for CI/CD
- Power BI integration for advanced analytics

## Secondary Integrations

While Azure DevOps is the primary platform, the agent maintains secondary integrations:

- **PostgreSQL**: Agent execution logs and learning data
- **Oracle**: Test case cache for fast lookup (mirrors Azure Test Plans data)
- **Playwright**: Automated test execution
- **axe-core/Pa11y**: Accessibility testing

## Migration Path

For organizations currently using other tools (Jama, Jira), the agent supports:

1. **Dual-mode operation**: Read from legacy systems, write to Azure DevOps
2. **Bulk migration**: Import requirements and test cases from legacy systems into Azure DevOps
3. **Gradual transition**: Maintain both systems during transition period

---

**Document Version:** 1.0  
**Last Updated:** January 31, 2026
