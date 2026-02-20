# Phase 3: Azure DevOps Integration - Research Notes

## Research Date
February 19, 2026

## Overview
Research findings for integrating the CPU Agents autonomous system with Azure DevOps Services for requirements management, test case storage, and result reporting.

## Azure DevOps REST API Overview

### API Structure
- **Base URL Pattern**: `https://dev.azure.com/{organization}/_apis[/{area}]/{resource}?api-version={version}`
- **Current API Version**: 7.2 (latest stable)
- **Authentication**: Personal Access Tokens (PAT) with Base64 encoding
- **Response Format**: JSON

### Key API Areas for Phase 3

#### 1. Work Item Tracking (Azure Boards)
- **Endpoint**: `/_apis/wit/workitems`
- **Purpose**: Requirements management (User Stories, Features, Epics)
- **Operations**: GET, POST, PATCH, DELETE
- **Key Features**:
  - WIQL (Work Item Query Language) for bulk retrieval
  - Work item links for traceability
  - Custom fields support
  - Area paths and iterations

#### 2. Test Plans API
- **Endpoint**: `/_apis/testplan/testplans`
- **Purpose**: Test case storage and management
- **Operations**: GET, POST, PATCH, DELETE
- **Key Features**:
  - Test plans, test suites, test cases
  - Test results publishing
  - Requirements-based test suites
  - Test attachments (screenshots, logs)

#### 3. Git Repos API
- **Endpoint**: `/_apis/git/repositories`
- **Purpose**: Artifact storage (test code, documentation)
- **Operations**: GET, POST, PATCH
- **Key Features**:
  - Repository management
  - Pull requests
  - Commits and pushes
  - File content management

## Authentication Methods

### Personal Access Tokens (Recommended for Phase 3)
- Simple and secure
- Scoped permissions
- Easy to revoke
- Example header: `Authorization: Basic {Base64(:{PAT})}`

### Alternative Methods (Future Consideration)
- OAuth 2.0 (for user-facing applications)
- MSAL (Microsoft Authentication Library)
- Service Principals (for server-to-server)

## API Request Pattern

### C# Example Structure
```csharp
using (HttpClient client = new HttpClient())
{
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
        Convert.ToBase64String(
            ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}")));
    
    using (HttpResponseMessage response = await client.GetAsync(url))
    {
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        // Process response
    }
}
```

## Key Findings

### 1. API Versioning
- API version must be specified in every request
- Format: `api-version=7.2`
- Backward compatibility maintained
- Preview APIs available for new features

### 2. Rate Limiting
- Need to research specific limits
- Implement retry logic with exponential backoff
- Consider batch operations for efficiency

### 3. Error Handling
- Standard HTTP status codes
- Detailed error messages in response body
- Need to implement comprehensive error handling

### 4. Data Models
- Work items have flexible schema
- Custom fields supported
- Relationships via links
- State transitions configurable

## Next Steps for Research

1. ✅ Understand basic API structure
2. ⏳ Research Work Item Tracking API details
3. ⏳ Research Test Plans API details
4. ⏳ Research Git Repos API details
5. ⏳ Investigate .NET client libraries
6. ⏳ Study authentication best practices
7. ⏳ Review rate limiting and quotas
8. ⏳ Examine error handling patterns

## References
- [Azure DevOps REST API Reference](https://learn.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-7.2)
- [Authentication Guidance](https://learn.microsoft.com/en-us/azure/devops/integrate/get-started/authentication/authentication-guidance)


## Detailed API Research

### Work Items API (Azure Boards)

**Base Endpoint**: `https://dev.azure.com/{organization}/{project}/_apis/wit/workitems`

**Available Operations**:
1. **Create** - Creates a single work item
   - Method: POST
   - Endpoint: `/{type}?api-version=7.1`
   - Body: JSON Patch document with field values

2. **Get Work Item** - Returns a single work item
   - Method: GET
   - Endpoint: `/{id}?api-version=7.1`
   - Supports field expansion

3. **Get Work Items Batch** - Gets multiple work items (Maximum 200)
   - Method: POST
   - Endpoint: `?ids={ids}&api-version=7.1`
   - Efficient for bulk retrieval

4. **List** - Returns a list of work items (Maximum 200)
   - Method: GET
   - Query parameters for filtering

5. **Update** - Updates a single work item
   - Method: PATCH
   - Body: JSON Patch document

6. **Delete** - Soft delete to Recycle Bin
   - Method: DELETE
   - Can be restored if needed

7. **Delete Work Items** - Bulk delete operation
   - Method: DELETE
   - Supports destroy parameter for permanent deletion

**Key Capabilities for Phase 3**:
- Create User Stories, Features, Epics programmatically
- Query requirements using WIQL
- Update work item states and fields
- Create work item links for traceability
- Support for custom fields
- Area paths and iterations management

### Test Plans API

**Base Endpoint**: `https://dev.azure.com/{organization}/{project}/_apis/testplan/plans`

**Available Operations**:
1. **Create** - Create a test plan
   - Method: POST
   - Endpoint: `?api-version=7.1`

2. **Get** - Get a test plan by ID
   - Method: GET
   - Endpoint: `/{planId}?api-version=7.1`

3. **List** - Get a list of test plans
   - Method: GET
   - Supports filtering

4. **Update** - Update a test plan
   - Method: PATCH

5. **Delete** - Delete a test plan
   - Method: DELETE

**Related APIs**:
- **Test Suites** - Organize test cases
- **Test Cases** - Individual test definitions
- **Test Results** - Publish test execution results
- **Test Points** - Test case instances in suites
- **Variables** - Test configuration variables

**Key Capabilities for Phase 3**:
- Create test plans programmatically
- Organize test cases into suites
- Link test cases to requirements
- Publish test results with attachments
- Support for automated and manual tests
- Test configuration management

## Integration Architecture Decisions

### 1. Authentication Strategy
**Decision**: Use Personal Access Tokens (PAT)
**Rationale**:
- Simple to implement and manage
- Scoped permissions for security
- Easy to revoke if compromised
- Suitable for server-to-server communication

**Implementation**:
```csharp
var pat = configuration["AzureDevOps:PersonalAccessToken"];
var authHeader = Convert.ToBase64String(
    ASCIIEncoding.ASCII.GetBytes($":{pat}"));
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Basic", authHeader);
```

### 2. API Client Design
**Decision**: Create dedicated service classes for each API area
**Rationale**:
- Separation of concerns
- Easier to test and maintain
- Follows single responsibility principle
- Enables mock implementations

**Proposed Structure**:
- `IAzureDevOpsWorkItemService` - Work Items operations
- `IAzureDevOpsTestPlanService` - Test Plans operations
- `IAzureDevOpsGitService` - Git Repos operations
- `AzureDevOpsClientFactory` - Centralized client creation

### 3. Error Handling Strategy
**Decision**: Implement retry logic with exponential backoff
**Rationale**:
- Handle transient network failures
- Respect rate limiting
- Improve reliability

**Implementation Approach**:
- Use Polly library for resilience
- Retry on 429 (Too Many Requests) and 5xx errors
- Exponential backoff: 1s, 2s, 4s, 8s, 16s
- Circuit breaker for persistent failures

### 4. Data Caching Strategy
**Decision**: Cache work items and test plans locally with TTL
**Rationale**:
- Reduce API calls
- Improve performance
- Offline capability

**Implementation**:
- Use IMemoryCache for short-term caching
- PostgreSQL for persistent caching
- Configurable TTL (default: 5 minutes)
- Cache invalidation on updates

## .NET Client Libraries Research

### Microsoft.TeamFoundationServer.Client
- Official .NET client library
- Comprehensive API coverage
- Strongly typed models
- Built-in authentication

**Recommendation**: Evaluate for Phase 3 implementation
- Pros: Official support, type safety, comprehensive
- Cons: Larger dependency, may be overkill for our needs

### Alternative: Direct REST API calls
- More control over requests
- Smaller footprint
- Easier to debug

**Recommendation**: Start with direct REST, migrate to client library if needed

## Next Research Tasks

1. ✅ Understand Work Items API structure
2. ✅ Understand Test Plans API structure  
3. ⏳ Research WIQL (Work Item Query Language) syntax
4. ⏳ Research Test Results API for publishing results
5. ⏳ Research work item links for traceability
6. ⏳ Research rate limiting and quotas
7. ⏳ Evaluate Microsoft.TeamFoundationServer.Client library
8. ⏳ Design data models for Phase 3

## Phase 0 Status
**Progress**: 60% complete
**Next Step**: Continue research on WIQL, Test Results API, and rate limiting
