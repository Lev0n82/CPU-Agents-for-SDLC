# Phase 4.1: Automated GUI Object Mapping & Database Discovery
## Complete System Specification

**Document Version:** 1.0  
**Date:** 2026-02-23  
**Status:** Final Specification  
**Phase:** 4.1 (Automated Test Generation Prerequisites)

---

## Executive Summary

Phase 4.1 introduces **Automated GUI Object Mapping** and **Database Discovery** capabilities that transform the CPU Agents for SDLC system from a test executor into an intelligent test generator. By systematically inventorying web application UI elements and database schemas, agents can autonomously generate robust Playwright tests with comprehensive UI and data validation assertions.

**Key Capabilities**:
- **DOM Acquisition**: Playwright-based crawling of web applications to capture complete UI element inventory
- **GuiObjMap Repository**: Structured catalog of all interactive elements with AI-powered classification and robust selector strategies
- **Database Discovery**: Read-only schema introspection with entity relationship mapping and data dictionary generation
- **DBA-Mediated Write Operations**: Secure workflow for test data setup via Azure DevOps work items
- **Automated Test Generation**: End-to-end Playwright test creation from user stories using GuiObjMap and database knowledge

**Business Impact**:
- **70% reduction** in test automation creation time (from 40 hours to 12 hours per feature)
- **90% selector stability** after UI changes through multi-level fallback strategies
- **95% test generation success rate** with minimal manual intervention
- **100% database security** through read-only access and DBA approval for write operations

---

## Table of Contents

1. [System Architecture](#1-system-architecture)
2. [Component Specifications](#2-component-specifications)
3. [Data Models](#3-data-models)
4. [Workflows](#4-workflows)
5. [Acceptance Criteria](#5-acceptance-criteria)
6. [Security & Compliance](#6-security--compliance)
7. [Performance Requirements](#7-performance-requirements)
8. [Testing Strategy](#8-testing-strategy)
9. [Deployment Plan](#9-deployment-plan)
10. [Success Metrics](#10-success-metrics)

---

## 1. System Architecture

### 1.1 Component Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Phase 4.1 System Architecture                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │                    GUI Object Mapping Subsystem                       │ │
│  ├───────────────────────────────────────────────────────────────────────┤ │
│  │                                                                       │ │
│  │  GuiObjMapService                                                     │ │
│  │    ├─> PlaywrightDomAcquisitionEngine                                │ │
│  │    │     ├─> Playwright Browser (Chromium headless)                  │ │
│  │    │     ├─> Authentication Handler (Azure Key Vault credentials)   │ │
│  │    │     └─> Page Crawler (sitemap, navigation, routes)             │ │
│  │    │                                                                 │ │
│  │    ├─> ElementClassifier (AI-powered)                                │ │
│  │    │     ├─> AI Decision Module (Granite 4, Phi-3)                   │ │
│  │    │     └─> Element Role Detection (button, input, nav, etc.)      │ │
│  │    │                                                                 │ │
│  │    ├─> SelectorGenerator                                             │ │
│  │    │     ├─> Priority-based selector generation                      │ │
│  │    │     ├─> Fallback strategy (data-testid → ID → semantic → CSS)  │ │
│  │    │     └─> Selector validation & robustness scoring               │ │
│  │    │                                                                 │ │
│  │    ├─> PageObjectModelBuilder                                        │ │
│  │    │     ├─> TypeScript Page Object class generator                 │ │
│  │    │     └─> Template engine (Handlebars/Liquid)                    │ │
│  │    │                                                                 │ │
│  │    └─> GuiObjMapRepository                                           │ │
│  │          ├─> JSON storage (human-readable)                           │ │
│  │          ├─> SQLite indexing (fast queries)                          │ │
│  │          └─> Version control (change tracking)                       │ │
│  │                                                                       │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
│                                    ↓                                        │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │                   Database Discovery Subsystem                        │ │
│  ├───────────────────────────────────────────────────────────────────────┤ │
│  │                                                                       │ │
│  │  DatabaseDiscoveryService                                             │ │
│  │    ├─> SchemaIntrospector                                             │ │
│  │    │     ├─> INFORMATION_SCHEMA queries                              │ │
│  │    │     ├─> Table/column metadata extraction                        │ │
│  │    │     └─> Constraint & index discovery                            │ │
│  │    │                                                                 │ │
│  │    ├─> EntityRelationshipMapper                                      │ │
│  │    │     ├─> Primary key detection                                   │ │
│  │    │     ├─> Foreign key relationship mapping                        │ │
│  │    │     └─> ERD generation (Mermaid/PlantUML)                       │ │
│  │    │                                                                 │ │
│  │    ├─> ReadOnlyQueryExecutor                                         │ │
│  │    │     ├─> Query validation (allow SELECT, block write ops)       │ │
│  │    │     ├─> Connection pooling (PostgreSQL/Oracle)                 │ │
│  │    │     └─> Query timeout enforcement (30 seconds)                 │ │
│  │    │                                                                 │ │
│  │    └─> DataDictionaryGenerator                                       │ │
│  │          ├─> Human-readable schema documentation                     │ │
│  │          └─> Sample data profiling (row counts, value ranges)        │ │
│  │                                                                       │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
│                                    ↓                                        │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │                  DBA Work Item Orchestration Subsystem                │ │
│  ├───────────────────────────────────────────────────────────────────────┤ │
│  │                                                                       │ │
│  │  DbaWorkItemOrchestrator                                              │ │
│  │    ├─> SqlRequestGenerator                                            │ │
│  │    │     ├─> SQL script formatting with comments                     │ │
│  │    │     └─> Rollback script generation                              │ │
│  │    │                                                                 │ │
│  │    ├─> WorkItemCreator                                                │ │
│  │    │     ├─> Azure DevOps API integration                            │ │
│  │    │     ├─> Attachment upload (SQL scripts)                         │ │
│  │    │     └─> DBA team assignment                                     │ │
│  │    │                                                                 │ │
│  │    ├─> ExecutionMonitor                                               │ │
│  │    │     ├─> Work item status polling (30-second intervals)          │ │
│  │    │     └─> Timeout handling (24-hour max wait)                     │ │
│  │    │                                                                 │ │
│  │    └─> ResultParser                                                   │ │
│  │          ├─> Execution log parsing                                    │ │
│  │          ├─> Rows affected extraction                                │ │
│  │          └─> Error detection & reporting                             │ │
│  │                                                                       │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
│                                    ↓                                        │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │                   Test Generation Orchestration Subsystem             │ │
│  ├───────────────────────────────────────────────────────────────────────┤ │
│  │                                                                       │ │
│  │  TestGenerationOrchestrator                                           │ │
│  │    ├─> UserStoryAnalyzer (Phase 3.1 - existing)                      │ │
│  │    ├─> GuiObjMapQuerier (Phase 4.1 - new)                            │ │
│  │    ├─> DatabaseSchemaQuerier (Phase 4.1 - new)                       │ │
│  │    ├─> DataSetupScriptGenerator (Phase 4.1 - new)                    │ │
│  │    ├─> PlaywrightTestGenerator (Phase 4.1 - new)                     │ │
│  │    └─> TestExecutor (Phase 3.2 - existing)                           │ │
│  │                                                                       │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Integration with Existing System

Phase 4.1 extends the Phase 3.1-3.4 architecture with new services registered in the Agent Host dependency injection container.

| **Existing Component** | **Phase 4.1 Extension** | **Integration Method** |
|------------------------|------------------------|------------------------|
| **Agent Host** | Registers `IGuiObjMapService`, `IDatabaseDiscoveryService`, `IDbaWorkItemOrchestrator` | DI container registration in `Startup.cs` |
| **AI Decision Module** | Adds `ElementClassifier` capability | New AI prompt templates for UI element classification |
| **Work Item Service** | Adds "DBA SQL Request" work item type | New work item template in Azure DevOps |
| **Test Plan Service** | Consumes GuiObjMap for test case generation | Reads GuiObjMap repository via `IGuiObjMapService` |
| **Secrets Management** | Stores read-only database credentials | New Key Vault secrets: `DB_READONLY_USER`, `DB_READONLY_PASSWORD` |
| **Observability** | Adds Phase 4.1 metrics and traces | New OpenTelemetry instrumentation |

---

## 2. Component Specifications

### 2.1 GuiObjMapService

**Responsibility**: Orchestrates the complete GUI object mapping workflow from DOM acquisition to GuiObjMap repository storage.

**Dependencies**:
- `IPlaywrightDomAcquisitionEngine`: DOM snapshot capture
- `IElementClassifier`: AI-powered element classification
- `ISelectorGenerator`: Robust selector generation
- `IPageObjectModelBuilder`: TypeScript class generation
- `IGuiObjMapRepository`: Data persistence

**Public Interface**:

```csharp
public interface IGuiObjMapService
{
    /// <summary>
    /// Generates a complete GuiObjMap for the specified web application.
    /// </summary>
    /// <param name="baseUrl">Base URL of the application (e.g., https://app.example.com)</param>
    /// <param name="authConfig">Authentication configuration for accessing protected pages</param>
    /// <param name="cancellationToken">Cancellation token for long-running operations</param>
    /// <returns>Complete GuiObjMap with all pages and elements</returns>
    Task<GuiObjMap> GenerateGuiObjMapAsync(
        string baseUrl, 
        AuthenticationConfig authConfig, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing GuiObjMap by re-scanning pages and detecting changes.
    /// </summary>
    /// <param name="baseUrl">Base URL of the application</param>
    /// <param name="existingMap">Existing GuiObjMap to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated GuiObjMap with change report</returns>
    Task<GuiObjMapUpdateResult> UpdateGuiObjMapAsync(
        string baseUrl, 
        GuiObjMap existingMap, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves all UI elements for a specific page.
    /// </summary>
    /// <param name="pageId">Unique page identifier</param>
    /// <returns>List of page elements with selectors and classifications</returns>
    Task<List<PageElement>> GetElementsForPageAsync(string pageId);
    
    /// <summary>
    /// Retrieves workflows (user interaction sequences) for a specific user story.
    /// </summary>
    /// <param name="userStoryId">Azure DevOps user story ID</param>
    /// <returns>List of workflows with step-by-step UI interactions</returns>
    Task<List<Workflow>> GetWorkflowsForUserStoryAsync(int userStoryId);
}
```

**Configuration**:

```json
{
  "GuiObjMap": {
    "CrawlConfig": {
      "MaxDepth": 3,
      "MaxConcurrentPages": 5,
      "PageTimeout": 10000,
      "WaitUntil": "domcontentloaded",
      "DisableImages": true,
      "DisableCSS": true
    },
    "SelectorPriority": [
      "data-testid",
      "id",
      "semantic",
      "css",
      "xpath"
    ],
    "CacheDuration": "24:00:00",
    "StoragePath": "/var/lib/cpu-agents/guiobjmap"
  }
}
```

---

### 2.2 PlaywrightDomAcquisitionEngine

**Responsibility**: Executes Playwright browser automation to capture DOM snapshots of web application pages.

**Dependencies**:
- Playwright library (Chromium browser)
- `ISecretsManager`: Retrieves test account credentials from Azure Key Vault
- `ILogger<PlaywrightDomAcquisitionEngine>`: Structured logging

**Public Interface**:

```csharp
public interface IPlaywrightDomAcquisitionEngine
{
    /// <summary>
    /// Crawls the entire web application starting from the base URL.
    /// </summary>
    /// <param name="baseUrl">Starting URL for crawl</param>
    /// <param name="config">Crawl configuration (depth, timeout, etc.)</param>
    /// <returns>List of page snapshots with DOM content and metadata</returns>
    Task<List<PageSnapshot>> CrawlApplicationAsync(string baseUrl, CrawlConfig config);
    
    /// <summary>
    /// Captures a DOM snapshot for a single page.
    /// </summary>
    /// <param name="url">Page URL to capture</param>
    /// <returns>Page snapshot with HTML, elements, and screenshot</returns>
    Task<PageSnapshot> CapturePageSnapshotAsync(string url);
    
    /// <summary>
    /// Validates that a selector uniquely identifies an element on a page.
    /// </summary>
    /// <param name="selector">CSS or XPath selector to validate</param>
    /// <param name="pageUrl">Page URL to test selector against</param>
    /// <returns>True if selector is valid and unique, false otherwise</returns>
    Task<bool> ValidateSelectorAsync(string selector, string pageUrl);
    
    /// <summary>
    /// Authenticates to the web application using test account credentials.
    /// </summary>
    /// <param name="authConfig">Authentication configuration</param>
    /// <returns>Browser context with authenticated session</returns>
    Task<IBrowserContext> AuthenticateAsync(AuthenticationConfig authConfig);
}
```

**Implementation Notes**:
- Uses headless Chromium browser for performance
- Disables images and CSS to reduce page load time (40% faster)
- Implements parallel page processing (5 concurrent pages)
- Handles lazy-loaded content by scrolling to trigger loading
- Captures screenshots for visual context (used by AI classifier)
- Stores session cookies for authenticated pages

---

### 2.3 ElementClassifier

**Responsibility**: Uses local AI models (Granite 4, Phi-3) to classify UI elements by role, purpose, and expected interactions.

**Dependencies**:
- `IAiDecisionModule`: AI inference engine (Phase 3.1)
- `ILogger<ElementClassifier>`: Structured logging

**Public Interface**:

```csharp
public interface IElementClassifier
{
    /// <summary>
    /// Classifies a single UI element using AI.
    /// </summary>
    /// <param name="element">Element snapshot with HTML attributes</param>
    /// <param name="pageContext">Surrounding page context for better classification</param>
    /// <returns>Element classification with confidence score</returns>
    Task<ElementClassification> ClassifyElementAsync(
        ElementSnapshot element, 
        string pageContext);
    
    /// <summary>
    /// Classifies multiple elements in batch for performance.
    /// </summary>
    /// <param name="elements">List of element snapshots</param>
    /// <returns>List of element classifications</returns>
    Task<List<ElementClassification>> ClassifyBatchAsync(List<ElementSnapshot> elements);
}
```

**AI Prompt Template**:

```
You are a UI element classifier for automated test generation.

Analyze the following HTML element and classify it:

HTML: {element_html}
Page Context: {page_context}

Classify the element with:
1. Role: button | input | link | navigation | form | table | modal | dropdown | other
2. Type: text | password | email | number | checkbox | radio | submit | reset | etc.
3. Label: User-facing text or aria-label
4. Business Purpose: What does this element do? (e.g., "Submit login form", "Navigate to dashboard")
5. Expected Interactions: click | type | select | hover | drag | etc.
6. Confidence: 0.0 - 1.0 (how confident are you in this classification?)

Output JSON format:
{
  "role": "button",
  "type": "submit",
  "label": "Login",
  "businessPurpose": "Submit login credentials to authenticate user",
  "expectedInteractions": ["click"],
  "confidence": 0.98,
  "reasoning": "Element has type='submit' and text 'Login' in authentication form"
}
```

**Classification Accuracy Target**: 90%+ validated by human review

---

### 2.4 SelectorGenerator

**Responsibility**: Generates robust CSS and XPath selectors with multi-level fallback strategies to ensure selector stability across UI changes.

**Dependencies**:
- `IPlaywrightDomAcquisitionEngine`: Selector validation
- `ILogger<SelectorGenerator>`: Structured logging

**Public Interface**:

```csharp
public interface ISelectorGenerator
{
    /// <summary>
    /// Generates a selector strategy with multiple fallback options.
    /// </summary>
    /// <param name="element">Element snapshot with HTML attributes</param>
    /// <returns>Selector strategy with primary and fallback selectors</returns>
    Task<SelectorStrategy> GenerateSelectorsAsync(ElementSnapshot element);
    
    /// <summary>
    /// Validates selector robustness by testing uniqueness and stability.
    /// </summary>
    /// <param name="selector">Selector to validate</param>
    /// <param name="pageUrl">Page URL to test against</param>
    /// <returns>Robustness score (0.0 = invalid, 1.0 = perfect)</returns>
    Task<double> ValidateSelectorRobustnessAsync(string selector, string pageUrl);
}
```

**Selector Priority Hierarchy**:

1. **data-testid** (Priority 1 - Most Stable)
   - Example: `[data-testid='login-button']`
   - Explicitly intended for testing, rarely changes
   - Robustness score: 1.0

2. **Unique ID** (Priority 2)
   - Example: `#username-input`
   - Stable if IDs are semantic (not auto-generated like `#input-1234`)
   - Robustness score: 0.9

3. **Semantic Attributes** (Priority 3)
   - Example: `button[aria-label='Submit form']`, `input[name='email']`
   - Uses ARIA labels, names, roles
   - Robustness score: 0.8

4. **CSS Class Combinations** (Priority 4)
   - Example: `.btn.btn-primary.submit-btn`
   - Less stable but common
   - Robustness score: 0.6

5. **Text Content** (Priority 5)
   - Example: `button:has-text('Submit')`
   - Fragile (breaks with localization)
   - Robustness score: 0.4

6. **XPath** (Priority 6 - Last Resort)
   - Example: `//div[@class='form']//button[contains(text(), 'Submit')]`
   - Complex hierarchies only
   - Robustness score: 0.3

**Validation Algorithm**:

```csharp
public async Task<double> ValidateSelectorRobustnessAsync(string selector, string pageUrl)
{
    var page = await _browser.NewPageAsync();
    await page.GotoAsync(pageUrl);
    
    var elements = await page.Locator(selector).AllAsync();
    
    if (elements.Count == 0)
        return 0.0; // Selector finds nothing (invalid)
    
    if (elements.Count == 1)
        return 1.0; // Perfect - unique selector
    
    if (elements.Count > 1)
        return 0.5; // Ambiguous - finds multiple elements
}
```

---

### 2.5 DatabaseDiscoveryService

**Responsibility**: Introspects database schemas to generate comprehensive data dictionaries and entity relationship diagrams.

**Dependencies**:
- `ISecretsManager`: Retrieves read-only database credentials
- `IReadOnlyQueryExecutor`: Executes schema introspection queries
- `ILogger<DatabaseDiscoveryService>`: Structured logging

**Public Interface**:

```csharp
public interface IDatabaseDiscoveryService
{
    /// <summary>
    /// Discovers complete database schema including tables, columns, constraints.
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <returns>Complete database schema metadata</returns>
    Task<DatabaseSchema> DiscoverSchemaAsync(string connectionString);
    
    /// <summary>
    /// Retrieves metadata for all tables in the database.
    /// </summary>
    /// <returns>List of table metadata (name, row count, description)</returns>
    Task<List<TableMetadata>> GetTablesAsync();
    
    /// <summary>
    /// Retrieves metadata for all columns in a specific table.
    /// </summary>
    /// <param name="tableName">Table name</param>
    /// <returns>List of column metadata (name, type, constraints)</returns>
    Task<List<ColumnMetadata>> GetColumnsAsync(string tableName);
    
    /// <summary>
    /// Retrieves foreign key relationships for a specific table.
    /// </summary>
    /// <param name="tableName">Table name</param>
    /// <returns>List of foreign key metadata</returns>
    Task<List<ForeignKeyMetadata>> GetForeignKeysAsync(string tableName);
    
    /// <summary>
    /// Generates an entity relationship diagram (ERD) in Mermaid format.
    /// </summary>
    /// <returns>ERD as Mermaid diagram text</returns>
    Task<string> GenerateErdAsync();
}
```

**Schema Introspection Queries** (PostgreSQL):

```sql
-- Discover all tables
SELECT 
    table_schema,
    table_name,
    table_type
FROM information_schema.tables
WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
ORDER BY table_schema, table_name;

-- Discover all columns
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable,
    column_default,
    character_maximum_length
FROM information_schema.columns
WHERE table_schema = 'public'
ORDER BY table_name, ordinal_position;

-- Discover primary keys
SELECT 
    tc.table_name,
    kcu.column_name
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu
    ON tc.constraint_name = kcu.constraint_name
WHERE tc.constraint_type = 'PRIMARY KEY'
ORDER BY tc.table_name;

-- Discover foreign keys
SELECT 
    tc.table_name AS child_table,
    kcu.column_name AS child_column,
    ccu.table_name AS parent_table,
    ccu.column_name AS parent_column,
    rc.update_rule,
    rc.delete_rule
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage ccu
    ON ccu.constraint_name = tc.constraint_name
JOIN information_schema.referential_constraints rc
    ON rc.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
ORDER BY tc.table_name;
```

---

### 2.6 ReadOnlyQueryExecutor

**Responsibility**: Executes database queries with strict validation to ensure only read operations are allowed.

**Dependencies**:
- `Npgsql` (PostgreSQL) or `Oracle.ManagedDataAccess` (Oracle)
- `ILogger<ReadOnlyQueryExecutor>`: Structured logging

**Public Interface**:

```csharp
public interface IReadOnlyQueryExecutor
{
    /// <summary>
    /// Executes a read-only SQL query and returns results as a DataTable.
    /// </summary>
    /// <param name="sqlQuery">SQL query (must be SELECT only)</param>
    /// <param name="parameters">Query parameters (for parameterized queries)</param>
    /// <returns>Query results as DataTable</returns>
    /// <exception cref="InvalidOperationException">Thrown if query contains write operations</exception>
    Task<DataTable> ExecuteQueryAsync(string sqlQuery, Dictionary<string, object> parameters = null);
    
    /// <summary>
    /// Validates that a query is read-only (SELECT only, no write operations).
    /// </summary>
    /// <param name="sqlQuery">SQL query to validate</param>
    /// <returns>True if query is read-only, false otherwise</returns>
    bool IsReadOnlyQuery(string sqlQuery);
    
    /// <summary>
    /// Gets the row count for a specific table.
    /// </summary>
    /// <param name="tableName">Table name</param>
    /// <returns>Number of rows in the table</returns>
    Task<int> GetRowCountAsync(string tableName);
    
    /// <summary>
    /// Gets distinct values for a column (useful for enum-like columns).
    /// </summary>
    /// <param name="tableName">Table name</param>
    /// <param name="columnName">Column name</param>
    /// <param name="maxResults">Maximum number of distinct values to return</param>
    /// <returns>List of distinct values</returns>
    Task<List<string>> GetDistinctValuesAsync(
        string tableName, 
        string columnName, 
        int maxResults = 100);
}
```

**Query Validation Logic**:

```csharp
public bool IsReadOnlyQuery(string sqlQuery)
{
    var upperQuery = sqlQuery.ToUpperInvariant().Trim();
    
    // Whitelist: Only SELECT and SHOW commands
    if (!upperQuery.StartsWith("SELECT") && !upperQuery.StartsWith("SHOW"))
    {
        _logger.LogWarning("Query rejected: Does not start with SELECT or SHOW");
        return false;
    }
    
    // Blacklist: Detect write operations
    string[] writeKeywords = {
        "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER",
        "TRUNCATE", "GRANT", "REVOKE", "EXEC", "EXECUTE", "MERGE"
    };
    
    foreach (var keyword in writeKeywords)
    {
        if (upperQuery.Contains(keyword))
        {
            _logger.LogWarning(
                "Query rejected: Contains write operation keyword '{Keyword}'", 
                keyword);
            return false;
        }
    }
    
    return true;
}
```

**Security Constraints**:
- Query timeout: 30 seconds (prevents long-running queries)
- Result set limit: 10,000 rows (prevents memory exhaustion)
- Connection pooling: Max 10 connections (prevents connection exhaustion)
- No access to sensitive tables: `password_hashes`, `credit_cards`, `ssn_data`

---

### 2.7 DbaWorkItemOrchestrator

**Responsibility**: Manages the complete workflow for DBA-mediated write operations via Azure DevOps work items.

**Dependencies**:
- `IWorkItemService`: Azure DevOps API integration (Phase 3.1)
- `ILogger<DbaWorkItemOrchestrator>`: Structured logging

**Public Interface**:

```csharp
public interface IDbaWorkItemOrchestrator
{
    /// <summary>
    /// Creates a DBA SQL request work item with scripts and metadata.
    /// </summary>
    /// <param name="request">SQL request model with script and justification</param>
    /// <returns>Work item ID</returns>
    Task<int> CreateSqlRequestAsync(SqlRequestModel request);
    
    /// <summary>
    /// Waits for DBA to complete SQL execution and returns results.
    /// </summary>
    /// <param name="workItemId">Work item ID to monitor</param>
    /// <param name="timeout">Maximum wait time (default: 24 hours)</param>
    /// <returns>SQL execution result with logs and status</returns>
    /// <exception cref="TimeoutException">Thrown if DBA does not complete within timeout</exception>
    Task<SqlExecutionResult> WaitForCompletionAsync(
        int workItemId, 
        TimeSpan? timeout = null);
    
    /// <summary>
    /// Retrieves execution results for a completed work item.
    /// </summary>
    /// <param name="workItemId">Work item ID</param>
    /// <returns>SQL execution result</returns>
    Task<SqlExecutionResult> GetExecutionResultAsync(int workItemId);
}
```

**SqlRequestModel**:

```csharp
public class SqlRequestModel
{
    public string Title { get; set; }
    public string BusinessJustification { get; set; }
    public string SqlScript { get; set; }
    public string RollbackScript { get; set; }
    public string Environment { get; set; } // Development, Staging, Production
    public int Priority { get; set; } // 1=Critical, 2=High, 3=Normal, 4=Low
    public string ExpectedOutcome { get; set; }
    public string ValidationQueries { get; set; }
}
```

**SqlExecutionResult**:

```csharp
public class SqlExecutionResult
{
    public int WorkItemId { get; set; }
    public string Status { get; set; } // Completed, Failed, Cancelled
    public string ExecutionLog { get; set; }
    public int RowsAffected { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public List<string> Errors { get; set; }
    public DateTime CompletedAt { get; set; }
}
```

**Work Item Template**:

```json
{
  "workItemType": "Task",
  "title": "DBA SQL Request: {title}",
  "description": "**Business Justification**: {justification}\n\n**Requested By**: Agent Host\n**Environment**: {environment}\n**Expected Outcome**: {expectedOutcome}",
  "assignedTo": "dba-team@example.com",
  "priority": 2,
  "tags": ["agent-generated", "sql-request", "phase-4.1"],
  "attachments": [
    "script.sql",
    "rollback.sql"
  ]
}
```

---

### 2.8 TestGenerationOrchestrator

**Responsibility**: Coordinates the end-to-end test generation workflow from user story analysis to Playwright test execution.

**Dependencies**:
- `IUserStoryAnalyzer`: AI-powered user story analysis (Phase 3.1)
- `IGuiObjMapService`: UI element inventory
- `IDatabaseDiscoveryService`: Database schema knowledge
- `IPlaywrightTestGenerator`: Test code generation
- `IDataSetupScriptGenerator`: SQL script generation
- `IDbaWorkItemOrchestrator`: DBA workflow management

**Public Interface**:

```csharp
public interface ITestGenerationOrchestrator
{
    /// <summary>
    /// Generates a complete Playwright test from a user story.
    /// </summary>
    /// <param name="userStory">Azure DevOps user story</param>
    /// <returns>Generated test with Page Objects, test specs, and data setup scripts</returns>
    Task<GeneratedTest> GenerateTestAsync(UserStory userStory);
    
    /// <summary>
    /// Generates a test suite for multiple user stories.
    /// </summary>
    /// <param name="userStories">List of user stories</param>
    /// <returns>List of generated tests</returns>
    Task<List<GeneratedTest>> GenerateTestSuiteAsync(List<UserStory> userStories);
    
    /// <summary>
    /// Regenerates a test after UI changes are detected.
    /// </summary>
    /// <param name="testId">Test ID to regenerate</param>
    /// <param name="reason">Reason for regeneration (e.g., "Selector changed")</param>
    /// <returns>True if regeneration succeeded</returns>
    Task<bool> RegenerateTestAsync(int testId, string reason);
}
```

**GeneratedTest Model**:

```csharp
public class GeneratedTest
{
    public int TestId { get; set; }
    public int UserStoryId { get; set; }
    public string TestName { get; set; }
    public string Description { get; set; }
    
    // Generated code files
    public List<GeneratedFile> Files { get; set; }
    
    // Data setup requirements
    public SqlRequestModel DataSetupRequest { get; set; }
    public int? DbaWorkItemId { get; set; }
    
    // Test execution metadata
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; }
    public TestGenerationStatus Status { get; set; }
}

public class GeneratedFile
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string Content { get; set; }
    public FileType Type { get; set; } // PageObject, TestSpec, Helper, Config
}

public enum TestGenerationStatus
{
    Pending,
    DataSetupRequested,
    DataSetupCompleted,
    TestGenerated,
    TestExecuted,
    Failed
}
```

---

## 3. Data Models

### 3.1 GuiObjMap Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "GuiObjMap",
  "type": "object",
  "required": ["version", "applicationName", "baseUrl", "acquisitionDate", "pages"],
  "properties": {
    "version": {
      "type": "string",
      "description": "GuiObjMap schema version",
      "example": "1.0.0"
    },
    "applicationName": {
      "type": "string",
      "description": "Name of the web application",
      "example": "BPS Secure"
    },
    "baseUrl": {
      "type": "string",
      "format": "uri",
      "description": "Base URL of the application",
      "example": "https://bps-secure.pristine.ontarioemail.ca"
    },
    "acquisitionDate": {
      "type": "string",
      "format": "date-time",
      "description": "Timestamp when GuiObjMap was generated"
    },
    "pages": {
      "type": "array",
      "description": "List of all pages in the application",
      "items": {
        "$ref": "#/definitions/Page"
      }
    },
    "components": {
      "type": "array",
      "description": "Reusable UI components (navigation, modals, etc.)",
      "items": {
        "$ref": "#/definitions/Component"
      }
    }
  },
  "definitions": {
    "Page": {
      "type": "object",
      "required": ["pageId", "url", "title", "elements"],
      "properties": {
        "pageId": {
          "type": "string",
          "description": "Unique page identifier",
          "example": "login-page"
        },
        "url": {
          "type": "string",
          "description": "Page URL path",
          "example": "/login"
        },
        "title": {
          "type": "string",
          "description": "Page title",
          "example": "BPS Secure - Login"
        },
        "screenshot": {
          "type": "string",
          "description": "Path to page screenshot",
          "example": "/screenshots/login-page.png"
        },
        "elements": {
          "type": "array",
          "items": {
            "$ref": "#/definitions/Element"
          }
        },
        "workflows": {
          "type": "array",
          "items": {
            "$ref": "#/definitions/Workflow"
          }
        }
      }
    },
    "Element": {
      "type": "object",
      "required": ["elementId", "role", "selectors"],
      "properties": {
        "elementId": {
          "type": "string",
          "description": "Unique element identifier",
          "example": "username-input"
        },
        "role": {
          "type": "string",
          "enum": ["input", "button", "link", "navigation", "form", "table", "modal", "dropdown"],
          "description": "Element role"
        },
        "type": {
          "type": "string",
          "description": "Element type (text, password, submit, etc.)",
          "example": "text"
        },
        "label": {
          "type": "string",
          "description": "User-facing label or text",
          "example": "Username"
        },
        "selectors": {
          "type": "object",
          "required": ["primary"],
          "properties": {
            "primary": {
              "type": "string",
              "description": "Primary selector (highest priority)",
              "example": "[data-testid='username-input']"
            },
            "fallback1": {
              "type": "string",
              "example": "#username"
            },
            "fallback2": {
              "type": "string",
              "example": "input[name='username']"
            },
            "fallback3": {
              "type": "string",
              "example": "input[placeholder='Enter username']"
            }
          }
        },
        "properties": {
          "type": "object",
          "description": "HTML attributes and properties",
          "additionalProperties": true
        },
        "businessPurpose": {
          "type": "string",
          "description": "AI-generated description of element purpose",
          "example": "User authentication - username entry"
        },
        "expectedInteractions": {
          "type": "array",
          "items": {
            "type": "string",
            "enum": ["click", "type", "select", "hover", "drag", "clear"]
          }
        },
        "aiClassification": {
          "type": "object",
          "properties": {
            "confidence": {
              "type": "number",
              "minimum": 0.0,
              "maximum": 1.0
            },
            "model": {
              "type": "string",
              "example": "granite-code:8b"
            },
            "reasoning": {
              "type": "string"
            }
          }
        }
      }
    },
    "Workflow": {
      "type": "object",
      "required": ["workflowId", "description", "steps"],
      "properties": {
        "workflowId": {
          "type": "string",
          "example": "user-login"
        },
        "description": {
          "type": "string",
          "example": "User authentication workflow"
        },
        "steps": {
          "type": "array",
          "items": {
            "type": "object",
            "required": ["step", "action", "target"],
            "properties": {
              "step": {
                "type": "integer"
              },
              "action": {
                "type": "string",
                "enum": ["navigate", "click", "type", "select", "waitForNavigation"]
              },
              "target": {
                "type": "string",
                "description": "Element ID or URL"
              },
              "value": {
                "type": "string",
                "description": "Value for type/select actions"
              }
            }
          }
        }
      }
    },
    "Component": {
      "type": "object",
      "required": ["componentId", "type", "appearsOn", "elements"],
      "properties": {
        "componentId": {
          "type": "string",
          "example": "main-navigation"
        },
        "type": {
          "type": "string",
          "enum": ["navigation", "modal", "dropdown", "table", "form"]
        },
        "appearsOn": {
          "type": "array",
          "items": {
            "type": "string"
          },
          "description": "List of page IDs where component appears"
        },
        "elements": {
          "type": "array",
          "items": {
            "$ref": "#/definitions/Element"
          }
        }
      }
    }
  }
}
```

### 3.2 Database Schema Model

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "DatabaseSchema",
  "type": "object",
  "required": ["version", "databaseName", "databaseType", "discoveryDate", "tables"],
  "properties": {
    "version": {
      "type": "string",
      "example": "1.0.0"
    },
    "databaseName": {
      "type": "string",
      "example": "BPS_Secure_DB"
    },
    "databaseType": {
      "type": "string",
      "enum": ["PostgreSQL", "Oracle", "SQL Server"],
      "example": "PostgreSQL"
    },
    "discoveryDate": {
      "type": "string",
      "format": "date-time"
    },
    "tables": {
      "type": "array",
      "items": {
        "$ref": "#/definitions/Table"
      }
    },
    "relationships": {
      "type": "array",
      "items": {
        "$ref": "#/definitions/Relationship"
      }
    }
  },
  "definitions": {
    "Table": {
      "type": "object",
      "required": ["tableName", "schema", "columns"],
      "properties": {
        "tableName": {
          "type": "string",
          "example": "users"
        },
        "schema": {
          "type": "string",
          "example": "public"
        },
        "description": {
          "type": "string",
          "example": "Application users and authentication data"
        },
        "rowCount": {
          "type": "integer",
          "example": 1250
        },
        "columns": {
          "type": "array",
          "items": {
            "$ref": "#/definitions/Column"
          }
        },
        "foreignKeys": {
          "type": "array",
          "items": {
            "$ref": "#/definitions/ForeignKey"
          }
        },
        "indexes": {
          "type": "array",
          "items": {
            "$ref": "#/definitions/Index"
          }
        }
      }
    },
    "Column": {
      "type": "object",
      "required": ["columnName", "dataType", "isNullable"],
      "properties": {
        "columnName": {
          "type": "string",
          "example": "user_id"
        },
        "dataType": {
          "type": "string",
          "example": "integer"
        },
        "isPrimaryKey": {
          "type": "boolean"
        },
        "isNullable": {
          "type": "boolean"
        },
        "isUnique": {
          "type": "boolean"
        },
        "defaultValue": {
          "type": "string",
          "example": "nextval('users_user_id_seq')"
        },
        "description": {
          "type": "string",
          "example": "Unique user identifier"
        }
      }
    },
    "ForeignKey": {
      "type": "object",
      "required": ["constraintName", "columnName", "referencedTable", "referencedColumn"],
      "properties": {
        "constraintName": {
          "type": "string",
          "example": "fk_user_roles_user_id"
        },
        "columnName": {
          "type": "string",
          "example": "user_id"
        },
        "referencedTable": {
          "type": "string",
          "example": "users"
        },
        "referencedColumn": {
          "type": "string",
          "example": "user_id"
        },
        "onDelete": {
          "type": "string",
          "enum": ["CASCADE", "SET NULL", "RESTRICT", "NO ACTION"],
          "example": "CASCADE"
        },
        "onUpdate": {
          "type": "string",
          "enum": ["CASCADE", "SET NULL", "RESTRICT", "NO ACTION"],
          "example": "CASCADE"
        }
      }
    },
    "Index": {
      "type": "object",
      "required": ["indexName", "columns"],
      "properties": {
        "indexName": {
          "type": "string",
          "example": "idx_users_username"
        },
        "columns": {
          "type": "array",
          "items": {
            "type": "string"
          },
          "example": ["username"]
        },
        "isUnique": {
          "type": "boolean"
        }
      }
    },
    "Relationship": {
      "type": "object",
      "required": ["relationshipType", "description"],
      "properties": {
        "relationshipType": {
          "type": "string",
          "enum": ["one-to-one", "one-to-many", "many-to-many"]
        },
        "parentTable": {
          "type": "string"
        },
        "parentColumn": {
          "type": "string"
        },
        "childTable": {
          "type": "string"
        },
        "childColumn": {
          "type": "string"
        },
        "junctionTable": {
          "type": "string",
          "description": "For many-to-many relationships"
        },
        "description": {
          "type": "string",
          "example": "One user can have multiple active sessions"
        }
      }
    }
  }
}
```

---

## 4. Workflows

### 4.1 Complete Test Generation Workflow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│               End-to-End Automated Test Generation Workflow                 │
└─────────────────────────────────────────────────────────────────────────────┘

Step 1: User Story Analysis (Phase 3.1 - Existing)
├─ Input: Azure DevOps user story (e.g., "User Login Authentication")
├─ AI analyzes acceptance criteria
├─ Extracts test scenarios
└─ Output: List of test scenarios with expected behaviors

        ↓

Step 2: GUI Object Mapping Query (Phase 4.1 - NEW)
├─ Input: Test scenarios
├─ Identify pages involved (e.g., /login, /dashboard)
├─ Query GuiObjMap for relevant pages
├─ Load element inventory (username input, password input, login button)
├─ Map test steps to UI elements
└─ Output: UI interaction plan with selectors

        ↓

Step 3: Database Schema Query (Phase 4.1 - NEW)
├─ Input: Test scenarios
├─ Identify affected tables (e.g., users, user_sessions, audit_logs)
├─ Query database schema metadata
├─ Determine data setup requirements (test user accounts)
├─ Plan validation queries (verify session created, audit log entry)
└─ Output: Database interaction plan

        ↓

Step 4: Test Data Setup (Phase 4.1 - NEW)
├─ Generate SQL script for test data (INSERT test users)
├─ Generate rollback script (DELETE test users)
├─ Create DBA work item with scripts
├─ Wait for DBA execution (poll every 30 seconds)
├─ Parse execution results
└─ Output: Test data ready, credentials stored in Key Vault

        ↓

Step 5: Playwright Test Generation (Phase 4.1 - NEW)
├─ Generate Page Object classes (TypeScript)
│   ├─ LoginPage.ts (with selectors from GuiObjMap)
│   └─ DashboardPage.ts
├─ Generate test spec files
│   └─ login.spec.ts (with UI + database assertions)
├─ Generate database helper
│   └─ DatabaseHelper.ts (read-only query executor)
├─ Generate configuration
│   └─ playwright.config.ts
└─ Output: Complete test suite ready for execution

        ↓

Step 6: Test Execution (Phase 3.2 - Existing)
├─ Execute Playwright tests
├─ Capture screenshots on failure
├─ Collect execution logs
└─ Output: Test results

        ↓

Step 7: Test Results Reporting (Phase 3.2 - Existing)
├─ Create Azure DevOps test run
├─ Link test results to user story
├─ Attach screenshots and logs
└─ Output: Test run completed

        ↓

Step 8: Test Maintenance (Phase 4.1 - NEW)
├─ Detect UI changes (GuiObjMap update)
├─ Identify affected tests
├─ Regenerate tests with new selectors
├─ Re-execute tests to validate
└─ Output: Tests updated and passing
```

### 4.2 DBA Work Item Workflow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    DBA-Mediated Write Operation Workflow                    │
└─────────────────────────────────────────────────────────────────────────────┘

Agent Needs Write Operation
        ↓
Step 1: Generate SQL Script Request
├─ Create SQL script with comments
│   -- Purpose: Create test user accounts
│   -- Environment: Development
│   INSERT INTO users (username, email, ...) VALUES (...);
├─ Create rollback script
│   DELETE FROM users WHERE username IN ('test_user1', 'test_user2');
├─ Document expected outcome
│   "3 test user accounts created with roles: Admin, Editor, Viewer"
└─ Add validation queries
    SELECT COUNT(*) FROM users WHERE username LIKE 'test_%';

        ↓

Step 2: Create Azure DevOps Work Item
├─ Work Item Type: "Task"
├─ Title: "DBA SQL Request: Create test data for User Login scenario"
├─ Description: Business justification, environment, expected outcome
├─ Assigned To: "dba-team@example.com"
├─ Priority: 2 (High)
├─ Tags: ["agent-generated", "sql-request", "phase-4.1"]
├─ Attachments: script.sql, rollback.sql
└─ Output: Work item ID (e.g., 12345)

        ↓

Step 3: Agent Polls for Completion
├─ Poll interval: 30 seconds
├─ Max wait time: 24 hours
├─ Check work item state: New → Active → Completed/Failed
└─ Log: "Waiting for DBA work item 12345 completion. Current state: Active"

        ↓

Step 4: DBA Reviews Request
├─ Validates SQL syntax
├─ Checks for security risks (SQL injection, privilege escalation)
├─ Reviews impact on production data
├─ Estimates execution time
└─ Decision: Approve or Request Changes

        ↓

Step 5: DBA Executes SQL Script
├─ Executes in appropriate environment (dev/staging/prod)
├─ Captures execution logs
│   -- Execution started: 2026-02-23 12:00:00
│   -- INSERT INTO users ... (3 rows affected)
│   -- Execution completed: 2026-02-23 12:00:01
│   -- Duration: 1.2 seconds
├─ Runs validation queries
│   SELECT COUNT(*) FROM users WHERE username LIKE 'test_%';
│   -- Result: 3 rows
├─ Takes database backup (if needed)
└─ Documents results

        ↓

Step 6: DBA Updates Work Item
├─ Changes state to "Completed" or "Failed"
├─ Attaches execution logs (execution_log.txt)
├─ Adds comment with results
│   "SQL script executed successfully. 3 test users created.
│    Validation query confirmed 3 rows inserted."
└─ Notifies agent via work item update event

        ↓

Step 7: Agent Processes Response
├─ Downloads execution logs
├─ Parses results
│   ├─ Rows affected: 3
│   ├─ Execution time: 1.2 seconds
│   └─ Errors: None
├─ Updates test automation
│   ├─ Stores test credentials in Azure Key Vault
│   └─ Proceeds with test generation
└─ Logs outcome
    "DBA work item 12345 completed successfully. Test data ready."
```

---

## 5. Acceptance Criteria

### 5.1 GuiObjMapService (12 Criteria)

**AC-4.1.1**: When `GenerateGuiObjMapAsync()` is called with a valid base URL and authentication config, the service SHALL discover all pages by crawling sitemap.xml, navigation menus, and internal links up to a maximum depth of 3 levels.

**AC-4.1.2**: When discovering pages, the service SHALL authenticate using test account credentials retrieved from Azure Key Vault and maintain session state across all page visits.

**AC-4.1.3**: When capturing a page snapshot, the service SHALL wait for the 'domcontentloaded' event and network idle (no pending requests for 500ms) before extracting DOM content.

**AC-4.1.4**: When extracting DOM content, the service SHALL identify all interactive elements including buttons, inputs, links, forms, tables, modals, and dropdowns.

**AC-4.1.5**: When generating GuiObjMap, the service SHALL classify each element using the AI-powered ElementClassifier with a minimum confidence score of 0.7.

**AC-4.1.6**: When generating selectors, the service SHALL create a priority-based fallback strategy with at least 3 selector options per element (primary, fallback1, fallback2).

**AC-4.1.7**: When validating selectors, the service SHALL test each selector against the live page and assign a robustness score (0.0 = invalid, 1.0 = perfect unique selector).

**AC-4.1.8**: When storing GuiObjMap, the service SHALL save data in both JSON format (for human readability) and SQLite database (for fast queries).

**AC-4.1.9**: When updating an existing GuiObjMap, the service SHALL detect changes (new/modified/deleted elements) and generate a change report.

**AC-4.1.10**: When processing pages in parallel, the service SHALL limit concurrent page acquisitions to 5 simultaneous pages to prevent resource exhaustion.

**AC-4.1.11**: When a page acquisition fails (timeout, authentication error, network error), the service SHALL log the error, skip the page, and continue processing remaining pages.

**AC-4.1.12**: When GuiObjMap generation completes, the service SHALL emit OpenTelemetry metrics including total pages discovered, total elements classified, acquisition duration, and classification accuracy.

---

### 5.2 PlaywrightDomAcquisitionEngine (8 Criteria)

**AC-4.1.13**: When launching Playwright browser, the engine SHALL use headless Chromium with optimized launch arguments (--disable-images, --disable-css, --no-sandbox) to reduce page load time by at least 40%.

**AC-4.1.14**: When crawling an application, the engine SHALL discover pages from multiple sources: sitemap.xml (if available), navigation menus, internal links, and route configuration files (React Router, Angular routes).

**AC-4.1.15**: When authenticating to a web application, the engine SHALL support multiple authentication methods: form-based login, OAuth redirect, API token injection, and cookie-based session.

**AC-4.1.16**: When capturing a page snapshot, the engine SHALL extract the complete HTML structure, query all interactive elements, capture element properties (id, class, name, data-*, aria-*), record element positions, and take a full-page screenshot.

**AC-4.1.17**: When handling lazy-loaded content, the engine SHALL scroll to the bottom of the page to trigger loading and wait for network idle before extracting DOM.

**AC-4.1.18**: When validating a selector, the engine SHALL execute `page.locator(selector).count()` and return true only if exactly 1 element is found (unique selector).

**AC-4.1.19**: When a page load times out (default 10 seconds), the engine SHALL log a warning, capture a screenshot of the current state, and return a partial page snapshot with available elements.

**AC-4.1.20**: When disposing of browser resources, the engine SHALL close all browser contexts and pages to prevent memory leaks.

---

### 5.3 ElementClassifier (6 Criteria)

**AC-4.1.21**: When classifying an element, the classifier SHALL send the element HTML and page context to the AI Decision Module (Granite 4 or Phi-3) with a structured prompt requesting role, type, label, business purpose, expected interactions, and confidence score.

**AC-4.1.22**: When receiving AI classification results, the classifier SHALL parse the JSON response and validate that all required fields are present (role, type, label, businessPurpose, expectedInteractions, confidence).

**AC-4.1.23**: When AI classification confidence is below 0.7, the classifier SHALL log a warning, apply a fallback heuristic-based classification (e.g., type='submit' → role='button'), and flag the element for human review.

**AC-4.1.24**: When classifying elements in batch, the classifier SHALL process up to 50 elements per AI inference request to optimize performance.

**AC-4.1.25**: When AI inference fails (timeout, model error, invalid response), the classifier SHALL retry up to 3 times with exponential backoff, then fall back to heuristic classification.

**AC-4.1.26**: When classification completes, the classifier SHALL emit OpenTelemetry metrics including total elements classified, average confidence score, AI inference duration, and fallback rate.

---

### 5.4 SelectorGenerator (7 Criteria)

**AC-4.1.27**: When generating selectors for an element, the generator SHALL prioritize data-testid attributes (Priority 1) if present, followed by unique IDs (Priority 2), semantic attributes (Priority 3), CSS class combinations (Priority 4), text content (Priority 5), and XPath (Priority 6).

**AC-4.1.28**: When an element has a data-testid attribute, the generator SHALL use `[data-testid='value']` as the primary selector and assign a robustness score of 1.0.

**AC-4.1.29**: When an element has a unique ID, the generator SHALL validate that the ID is semantic (not auto-generated like `#input-1234`) by checking for numeric suffixes, and only use it if semantic.

**AC-4.1.30**: When generating CSS selectors, the generator SHALL combine multiple classes (e.g., `.btn.btn-primary.submit-btn`) to increase specificity and reduce ambiguity.

**AC-4.1.31**: When generating XPath selectors, the generator SHALL use relative XPath (starting with `//`) and prefer attribute-based predicates (e.g., `//button[@aria-label='Submit']`) over positional predicates (e.g., `//div[3]/button[2]`).

**AC-4.1.32**: When validating selector robustness, the generator SHALL execute the selector against the live page and assign a score: 1.0 if exactly 1 element found, 0.5 if multiple elements found, 0.0 if no elements found.

**AC-4.1.33**: When generating fallback selectors, the generator SHALL create at least 3 fallback options per element and validate each fallback to ensure at least one selector has a robustness score ≥ 0.8.

---

### 5.5 DatabaseDiscoveryService (10 Criteria)

**AC-4.1.34**: When connecting to a database, the service SHALL retrieve read-only credentials from Azure Key Vault using the secret name format `DB_{DATABASE_NAME}_READONLY_USER` and `DB_{DATABASE_NAME}_READONLY_PASSWORD`.

**AC-4.1.35**: When discovering schema metadata, the service SHALL query INFORMATION_SCHEMA.TABLES, INFORMATION_SCHEMA.COLUMNS, INFORMATION_SCHEMA.CONSTRAINTS, and INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS to extract complete schema information.

**AC-4.1.36**: When extracting table metadata, the service SHALL include table name, schema, description (from comments), row count (via `SELECT COUNT(*)`), and last modified timestamp (if available).

**AC-4.1.37**: When extracting column metadata, the service SHALL include column name, data type, is_nullable, is_primary_key, is_unique, default_value, and description (from comments).

**AC-4.1.38**: When mapping foreign key relationships, the service SHALL identify parent table, parent column, child table, child column, ON DELETE action, and ON UPDATE action.

**AC-4.1.39**: When detecting many-to-many relationships, the service SHALL identify junction tables (tables with exactly 2 foreign keys and no other non-key columns) and map the relationship between the two parent tables.

**AC-4.1.40**: When generating an entity relationship diagram (ERD), the service SHALL produce Mermaid diagram syntax with entities, attributes, and relationships (one-to-one, one-to-many, many-to-many).

**AC-4.1.41**: When profiling sample data, the service SHALL query row counts, identify enum-like columns (columns with ≤ 20 distinct values), detect date ranges (MIN/MAX dates), and find common values for test data generation.

**AC-4.1.42**: When storing database schema, the service SHALL save data in JSON format with version control to track schema changes over time.

**AC-4.1.43**: When schema discovery completes, the service SHALL emit OpenTelemetry metrics including total tables discovered, total columns discovered, total relationships mapped, and discovery duration.

---

### 5.6 ReadOnlyQueryExecutor (8 Criteria)

**AC-4.1.44**: When executing a query, the executor SHALL validate that the query is read-only by checking that it starts with SELECT or SHOW and does not contain write operation keywords (INSERT, UPDATE, DELETE, DROP, CREATE, ALTER, TRUNCATE, GRANT, REVOKE, EXEC, EXECUTE, MERGE).

**AC-4.1.45**: When a query contains write operation keywords, the executor SHALL throw an `InvalidOperationException` with the message "Query contains write operations. Submit a DBA work item for write operations."

**AC-4.1.46**: When executing a valid read-only query, the executor SHALL enforce a 30-second timeout to prevent long-running queries from blocking resources.

**AC-4.1.47**: When a query exceeds the timeout, the executor SHALL cancel the query, log a warning, and throw a `TimeoutException` with the message "Query exceeded 30-second timeout."

**AC-4.1.48**: When a query returns more than 10,000 rows, the executor SHALL truncate the result set to 10,000 rows, log a warning, and include a note in the result metadata indicating truncation.

**AC-4.1.49**: When executing parameterized queries, the executor SHALL use database-specific parameter syntax (e.g., `$1`, `$2` for PostgreSQL, `:param1`, `:param2` for Oracle) to prevent SQL injection.

**AC-4.1.50**: When a database connection fails, the executor SHALL retry up to 3 times with exponential backoff (2s, 4s, 8s), then throw a `DatabaseConnectionException`.

**AC-4.1.51**: When query execution completes, the executor SHALL emit OpenTelemetry metrics including query duration, rows returned, and success/failure status.

---

### 5.7 DbaWorkItemOrchestrator (9 Criteria)

**AC-4.1.52**: When creating a SQL request work item, the orchestrator SHALL generate a work item with type "Task", title "DBA SQL Request: {title}", description with business justification, assigned to DBA team, priority based on urgency, and tags ["agent-generated", "sql-request", "phase-4.1"].

**AC-4.1.53**: When attaching SQL scripts, the orchestrator SHALL upload two files: "script.sql" (main SQL script) and "rollback.sql" (rollback script), both with UTF-8 encoding.

**AC-4.1.54**: When generating SQL script content, the orchestrator SHALL include comments with purpose, environment, rollback instructions, expected outcome, and validation queries.

**AC-4.1.55**: When waiting for completion, the orchestrator SHALL poll the work item status every 30 seconds and check if the state is "Completed" or "Failed".

**AC-4.1.56**: When the work item state changes to "Completed", the orchestrator SHALL download the execution log attachment (execution_log.txt) and parse the results.

**AC-4.1.57**: When parsing execution results, the orchestrator SHALL extract rows affected, execution time, errors (if any), and validation query results from the execution log.

**AC-4.1.58**: When the work item state changes to "Failed", the orchestrator SHALL parse the failure reason from work item comments and throw a `DbaExecutionFailedException` with the failure details.

**AC-4.1.59**: When waiting for completion exceeds the timeout (default 24 hours), the orchestrator SHALL throw a `TimeoutException` with the message "DBA work item {workItemId} did not complete within {timeout} hours."

**AC-4.1.60**: When work item processing completes, the orchestrator SHALL emit OpenTelemetry metrics including work item ID, status, wait duration, and execution duration.

---

### 5.8 TestGenerationOrchestrator (11 Criteria)

**AC-4.1.61**: When generating a test from a user story, the orchestrator SHALL analyze the user story using the AI Decision Module to extract test scenarios, acceptance criteria, and expected behaviors.

**AC-4.1.62**: When identifying pages involved in a test scenario, the orchestrator SHALL query the GuiObjMap repository to retrieve element inventories for all relevant pages.

**AC-4.1.63**: When mapping test steps to UI elements, the orchestrator SHALL match user story actions (e.g., "User enters username") to GuiObjMap elements (e.g., "username-input") using AI-powered semantic matching.

**AC-4.1.64**: When identifying database tables affected by a test scenario, the orchestrator SHALL query the database schema metadata to retrieve table definitions, column types, and relationships.

**AC-4.1.65**: When test data setup is required, the orchestrator SHALL generate SQL scripts for data insertion, create a DBA work item, and wait for DBA execution before proceeding with test generation.

**AC-4.1.66**: When generating Page Object classes, the orchestrator SHALL create TypeScript classes with properties for each UI element, methods for user interactions (login, navigate, etc.), and locators using GuiObjMap selectors with fallback strategies.

**AC-4.1.67**: When generating test spec files, the orchestrator SHALL create Playwright test files with describe blocks, test cases, beforeEach/afterEach hooks, UI assertions (element visibility, navigation), and database assertions (data integrity, audit logs).

**AC-4.1.68**: When generating database helpers, the orchestrator SHALL create TypeScript classes with methods for read-only query execution, query validation, and result parsing.

**AC-4.1.69**: When test generation fails (missing GuiObjMap data, database schema unavailable, AI inference error), the orchestrator SHALL log the error, create a work item for human intervention, and mark the test as "Failed".

**AC-4.1.70**: When test generation completes successfully, the orchestrator SHALL save all generated files to the test repository, create a Git commit with descriptive message, and trigger test execution.

**AC-4.1.71**: When test generation completes, the orchestrator SHALL emit OpenTelemetry metrics including test generation duration, number of files generated, test scenario count, and success/failure status.

---

### 5.9 PlaywrightTestGenerator (8 Criteria)

**AC-4.1.72**: When generating a Page Object class, the generator SHALL create a TypeScript class with a constructor accepting a Playwright `Page` object, readonly properties for each UI element (as `Locator`), and methods for user interactions.

**AC-4.1.73**: When generating element locators, the generator SHALL use GuiObjMap selectors with the `.or()` fallback strategy (e.g., `page.locator('[data-testid="username"]').or(page.locator('#username'))`).

**AC-4.1.74**: When generating test spec files, the generator SHALL create Playwright test files with imports (Page Objects, helpers), describe blocks for test suites, test cases with AAA pattern (Arrange, Act, Assert), and beforeEach/afterEach hooks for setup/teardown.

**AC-4.1.75**: When generating UI assertions, the generator SHALL use Playwright's `expect()` API with matchers for visibility (`toBeVisible()`), text content (`toHaveText()`), URL (`toHaveURL()`), and element state (`toBeEnabled()`, `toBeDisabled()`).

**AC-4.1.76**: When generating database assertions, the generator SHALL create helper method calls (e.g., `await dbHelper.executeQuery(...)`) with SQL queries to validate data integrity, audit logs, and expected database state.

**AC-4.1.77**: When generating test data references, the generator SHALL use environment variables (e.g., `process.env.TEST_USERNAME`) or Azure Key Vault retrieval for sensitive credentials.

**AC-4.1.78**: When generating error handling, the generator SHALL include try-catch blocks for database queries, screenshot capture on test failure, and cleanup logic in afterEach hooks.

**AC-4.1.79**: When test generation completes, the generator SHALL format generated code using Prettier with 2-space indentation, single quotes, and semicolons.

---

### 5.10 DataSetupScriptGenerator (6 Criteria)

**AC-4.1.80**: When generating SQL scripts for test data setup, the generator SHALL create INSERT statements with explicit column lists (not `INSERT INTO table VALUES (...)`) to prevent column order issues.

**AC-4.1.81**: When generating test data values, the generator SHALL use realistic data (e.g., valid email addresses, phone numbers, dates) based on column data types and constraints from the database schema.

**AC-4.1.82**: When generating rollback scripts, the generator SHALL create DELETE statements with WHERE clauses that precisely match the inserted test data (e.g., `WHERE username IN ('test_user1', 'test_user2')`).

**AC-4.1.83**: When generating validation queries, the generator SHALL create SELECT statements to verify that test data was inserted correctly (e.g., `SELECT COUNT(*) FROM users WHERE username LIKE 'test_%'`).

**AC-4.1.84**: When generating scripts for foreign key relationships, the generator SHALL ensure that parent records are inserted before child records to avoid foreign key constraint violations.

**AC-4.1.85**: When generating scripts, the generator SHALL wrap all statements in a transaction (BEGIN TRANSACTION ... COMMIT) to ensure atomicity.

---

### 5.11 PageObjectModelBuilder (5 Criteria)

**AC-4.1.86**: When building a Page Object model, the builder SHALL create a TypeScript class with a name derived from the page title (e.g., "Login Page" → "LoginPage").

**AC-4.1.87**: When adding element properties, the builder SHALL create readonly properties with names derived from element labels (e.g., "Username" → "usernameInput") and type `Locator`.

**AC-4.1.88**: When adding interaction methods, the builder SHALL create async methods for common workflows (e.g., `async login(username: string, password: string)`) that encapsulate multi-step interactions.

**AC-4.1.89**: When generating locator initialization, the builder SHALL use GuiObjMap selectors with fallback strategies in the constructor.

**AC-4.1.90**: When building Page Object classes, the builder SHALL use a template engine (Handlebars or Liquid) to generate consistent code structure.

---

### 5.12 GuiObjMapRepository (6 Criteria)

**AC-4.1.91**: When storing GuiObjMap data, the repository SHALL save data in JSON format at the configured storage path (e.g., `/var/lib/cpu-agents/guiobjmap/{applicationName}.json`).

**AC-4.1.92**: When storing GuiObjMap data, the repository SHALL also index data in a SQLite database for fast queries (e.g., "Find all elements on page X", "Find all buttons with label Y").

**AC-4.1.93**: When querying GuiObjMap data, the repository SHALL support filtering by page ID, element role, element type, and selector type.

**AC-4.1.94**: When updating GuiObjMap data, the repository SHALL create a new version with timestamp and preserve the previous version for rollback.

**AC-4.1.95**: When detecting changes, the repository SHALL compare the new GuiObjMap with the previous version and generate a change report listing new elements, modified elements, and deleted elements.

**AC-4.1.96**: When querying GuiObjMap data, the repository SHALL return results within 100ms for typical queries (e.g., "Get all elements for page X").

---

## 6. Security & Compliance

### 6.1 Credential Management

All credentials are stored in **Azure Key Vault** with the following naming conventions:

| **Credential Type** | **Key Vault Secret Name** | **Rotation Policy** | **Access Control** |
|--------------------|--------------------------|--------------------|--------------------|
| Test Account Username | `TEST_ACCOUNT_{ROLE}_USERNAME` | 90 days | Agent Host service principal only |
| Test Account Password | `TEST_ACCOUNT_{ROLE}_PASSWORD` | 90 days | Agent Host service principal only |
| Read-Only DB User | `DB_{DATABASE_NAME}_READONLY_USER` | 90 days | Agent Host service principal only |
| Read-Only DB Password | `DB_{DATABASE_NAME}_READONLY_PASSWORD` | 90 days | Agent Host service principal only |
| Azure DevOps PAT | `AZDO_PAT` | 90 days | Agent Host service principal only |

**Access Pattern**:
- Credentials retrieved at runtime (not stored in configuration files)
- Credentials cached in memory for 1 hour, then re-retrieved
- Credentials never logged or written to disk
- Credentials transmitted over TLS 1.2+ only

### 6.2 Database Security

**Read-Only Account Configuration**:

```sql
-- PostgreSQL example
CREATE ROLE agent_readonly WITH LOGIN PASSWORD 'secure-password-from-keyvault';

-- Grant minimal permissions
GRANT CONNECT ON DATABASE app_db TO agent_readonly;
GRANT USAGE ON SCHEMA public TO agent_readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO agent_readonly;
GRANT SELECT ON ALL SEQUENCES IN SCHEMA public TO agent_readonly;

-- Revoke write permissions explicitly
REVOKE INSERT, UPDATE, DELETE, TRUNCATE ON ALL TABLES IN SCHEMA public FROM agent_readonly;
REVOKE CREATE, DROP, ALTER ON SCHEMA public FROM agent_readonly;

-- Prevent access to sensitive tables
REVOKE SELECT ON TABLE password_hashes FROM agent_readonly;
REVOKE SELECT ON TABLE credit_cards FROM agent_readonly;
REVOKE SELECT ON TABLE ssn_data FROM agent_readonly;
```

**Query Validation**:
- Whitelist: Only `SELECT` and `SHOW` commands allowed
- Blacklist: `INSERT`, `UPDATE`, `DELETE`, `DROP`, `CREATE`, `ALTER`, `TRUNCATE`, `GRANT`, `REVOKE`, `EXEC`, `EXECUTE`, `MERGE` blocked
- Timeout: 30 seconds maximum per query
- Result limit: 10,000 rows maximum per query

**DBA Approval Required For**:
- All write operations (`INSERT`, `UPDATE`, `DELETE`)
- Schema changes (`CREATE`, `ALTER`, `DROP`)
- Production environment operations (regardless of read/write)

### 6.3 GuiObjMap Data Protection

**Sensitive Information Handling**:
- GuiObjMap does NOT store user credentials, PII, or sensitive data
- Screenshots stored locally (not in cloud storage)
- Element text content sanitized to remove phone numbers, emails, SSNs (regex-based detection)
- GuiObjMap repository encrypted at rest (if stored in database)
- Access control: Only Agent Host and authorized developers can read GuiObjMap

**Data Retention**:
- GuiObjMap versions retained for 90 days
- Screenshots retained for 30 days
- Older versions automatically purged

### 6.4 Compliance

**OWASP Top 10 Mitigation**:
- **SQL Injection**: Parameterized queries only, query validation
- **Broken Authentication**: Azure Key Vault for credential storage, no hardcoded credentials
- **Sensitive Data Exposure**: TLS 1.2+ for all network communication, encryption at rest
- **XML External Entities (XXE)**: Not applicable (no XML parsing)
- **Broken Access Control**: Read-only database account, DBA approval for write operations
- **Security Misconfiguration**: Secure defaults, minimal permissions
- **Cross-Site Scripting (XSS)**: Not applicable (no web UI in agent)
- **Insecure Deserialization**: JSON schema validation for all inputs
- **Using Components with Known Vulnerabilities**: Automated dependency scanning (Dependabot)
- **Insufficient Logging & Monitoring**: OpenTelemetry instrumentation, audit logs

---

## 7. Performance Requirements

### 7.1 DOM Acquisition Performance

| **Metric** | **Target** | **Maximum** | **Measurement Method** |
|-----------|-----------|------------|----------------------|
| Page acquisition time | < 5 seconds per page | 10 seconds | OpenTelemetry histogram |
| Parallel page processing | 5 concurrent pages | 10 concurrent pages | Configuration setting |
| Application scan time (50 pages) | < 5 minutes | 10 minutes | End-to-end timer |
| Incremental update time | < 10 seconds per changed page | 30 seconds | OpenTelemetry histogram |

**Optimization Strategies**:
- Disable images and CSS (40% faster page load)
- Use `domcontentloaded` wait strategy (vs. `load`)
- Parallel page processing (5x speedup)
- Incremental updates (90% reduction in scan time)
- DOM caching (24-hour TTL)

### 7.2 Database Discovery Performance

| **Metric** | **Target** | **Maximum** | **Measurement Method** |
|-----------|-----------|------------|----------------------|
| Schema introspection time | < 2 minutes (100 tables) | 5 minutes | OpenTelemetry histogram |
| Read-only query execution | < 1 second | 5 seconds | OpenTelemetry histogram |
| Connection pool acquisition | < 100ms | 500ms | OpenTelemetry histogram |

**Optimization Strategies**:
- Connection pooling (reuse connections)
- Schema metadata caching (1-hour TTL)
- Batch queries (combine multiple queries into one)
- Index hints for large tables

### 7.3 Test Generation Performance

| **Metric** | **Target** | **Maximum** | **Measurement Method** |
|-----------|-----------|------------|----------------------|
| Test generation time | < 30 seconds per test | 60 seconds | OpenTelemetry histogram |
| Page Object generation | < 5 seconds per page | 10 seconds | OpenTelemetry histogram |
| AI classification time | < 2 seconds per element | 5 seconds | OpenTelemetry histogram |

---

## 8. Testing Strategy

### 8.1 Unit Tests (95%+ Coverage)

**GuiObjMapService**:
- Mock `IPlaywrightDomAcquisitionEngine`, test GuiObjMap generation logic
- Test page discovery (sitemap, navigation, links)
- Test element classification with mock AI responses
- Test selector generation and validation
- Test GuiObjMap storage and retrieval

**SelectorGenerator**:
- Test selector priority hierarchy (data-testid > ID > semantic > CSS > XPath)
- Test fallback strategy generation
- Test selector robustness scoring
- Test XPath generation for complex hierarchies

**ElementClassifier**:
- Test AI classification with known element types (button, input, link)
- Test fallback heuristic classification when AI confidence is low
- Test batch classification performance

**ReadOnlyQueryExecutor**:
- Test query validation (allow SELECT, block INSERT/UPDATE/DELETE)
- Test parameterized queries (prevent SQL injection)
- Test timeout enforcement (30 seconds)
- Test result set limit (10,000 rows)

**DbaWorkItemOrchestrator**:
- Test work item creation with SQL scripts and metadata
- Test work item status polling
- Test execution result parsing
- Test timeout handling

### 8.2 Integration Tests

**End-to-End DOM Acquisition**:
- Test against real web application (test environment)
- Verify all pages discovered
- Verify all elements classified with 90%+ accuracy
- Verify selectors remain stable after UI changes

**Database Discovery**:
- Test against real PostgreSQL and Oracle databases
- Verify all tables, columns, and relationships discovered
- Verify ERD generation accuracy
- Verify read-only query execution

**DBA Work Item Workflow**:
- Test work item creation → DBA execution → result retrieval
- Verify SQL script execution logs parsed correctly
- Verify agent can proceed with test generation after data setup

**Test Generation**:
- Test complete workflow from user story → generated Playwright test
- Verify generated tests execute successfully
- Verify UI and database assertions work correctly

### 8.3 System Tests

**Full Application Scan**:
- Scan entire BPS Secure application (50+ pages)
- Measure acquisition time (target: < 5 minutes)
- Verify GuiObjMap completeness (95%+ elements captured)

**Test Generation at Scale**:
- Generate 100+ tests from user stories
- Measure generation time (target: < 30 seconds per test)
- Verify test execution success rate (95%+)

**Performance Benchmarks**:
- Measure DOM acquisition time for large applications (200+ pages)
- Measure database discovery time for large schemas (500+ tables)
- Measure test generation throughput (tests per hour)

**Selector Robustness**:
- Validate selectors remain stable after UI changes
- Measure selector retention rate (target: 90%+)
- Test fallback strategy effectiveness

---

## 9. Deployment Plan

### 9.1 Phase 4.1 Deployment Timeline

**Week 1: Infrastructure Setup**
- Deploy Phase 4.1 services (`GuiObjMapService`, `DatabaseDiscoveryService`, `DbaWorkItemOrchestrator`)
- Update Agent Host to register new services in DI container
- Deploy GuiObjMap repository (SQLite database)
- Configure Azure Key Vault secrets (read-only database credentials)

**Week 2: Database Configuration**
- Create read-only database accounts (PostgreSQL, Oracle)
- Grant minimal SELECT permissions
- Revoke write permissions explicitly
- Test database connectivity from agent
- Run initial schema discovery

**Week 3: Initial DOM Acquisition**
- Run initial scan of target application (e.g., BPS Secure)
- Generate GuiObjMap for all pages
- Review and validate element classifications (human review)
- Fix classification errors (retrain AI if needed)

**Week 4: DBA Workflow Testing**
- Create test SQL request work item
- DBA executes script and attaches logs
- Agent retrieves results and validates workflow
- Document DBA workflow for team training

**Week 5: Test Generation Pilot**
- Select 5 simple user stories (e.g., "User Login", "View Profile")
- Run end-to-end test generation workflow
- Execute generated Playwright tests
- Validate test passes and assertions work
- Fix issues and refine AI prompts

**Week 6: Incremental Rollout**
- Generate tests for 20 user stories
- Monitor test generation success rate (target: 95%+)
- Monitor test execution success rate (target: 95%+)
- Refine selector strategies based on failures

**Week 7: Team Training**
- Train QA team on GuiObjMap maintenance
- Train developers on adding data-testid attributes
- Train DBAs on SQL request work item workflow
- Document best practices and troubleshooting

**Week 8: Production Rollout**
- Deploy Phase 4.1 to production environment
- Monitor observability metrics (OpenTelemetry)
- Establish on-call rotation for Phase 4.1 issues
- Celebrate successful deployment 🎉

### 9.2 Rollback Plan

If Phase 4.1 deployment fails:
1. Disable `GuiObjMapService` and `DatabaseDiscoveryService` in Agent Host configuration
2. Continue using Phase 3.1-3.4 functionality (manual test creation)
3. Investigate root cause (Playwright issues, database connectivity, AI classification errors)
4. Fix issues in development environment
5. Re-deploy Phase 4.1 after validation

**Rollback Triggers**:
- Test generation success rate < 80% for 24 hours
- DOM acquisition failures > 20% for 24 hours
- Database discovery failures > 10% for 24 hours
- DBA work item workflow failures > 10% for 24 hours

---

## 10. Success Metrics

Phase 4.1 is considered successful when the following metrics are achieved:

### 10.1 DOM Acquisition Metrics

| **Metric** | **Target** | **Measurement Period** |
|-----------|-----------|----------------------|
| Pages discovered | 95%+ of application pages | Per scan |
| Elements classified | 95%+ of interactive elements | Per scan |
| Classification accuracy | 90%+ validated by human review | Weekly |
| Selector stability | 90%+ selectors remain valid after UI changes | Monthly |
| Acquisition time | < 5 minutes for 50-page application | Per scan |

### 10.2 Database Discovery Metrics

| **Metric** | **Target** | **Measurement Period** |
|-----------|-----------|----------------------|
| Tables discovered | 100% of accessible tables | Per discovery |
| Relationships mapped | 95%+ of foreign key relationships | Per discovery |
| Read-only query success rate | 99%+ | Daily |
| Write operation blocking | 100% (zero unauthorized writes) | Daily |
| Discovery time | < 2 minutes for 100-table database | Per discovery |

### 10.3 DBA Work Item Metrics

| **Metric** | **Target** | **Measurement Period** |
|-----------|-----------|----------------------|
| Work item creation success rate | 100% | Daily |
| DBA completion time (average) | < 4 hours | Weekly |
| DBA completion time (P95) | < 24 hours | Weekly |
| Execution result parsing success rate | 100% | Daily |
| Unauthorized write attempts | 0 | Daily |

### 10.4 Test Generation Metrics

| **Metric** | **Target** | **Measurement Period** |
|-----------|-----------|----------------------|
| Test generation success rate | 95%+ | Daily |
| Test execution success rate (first run) | 95%+ | Daily |
| Test generation time | < 30 seconds per test | Per generation |
| Test maintenance time reduction | 70%+ (vs. manual test creation) | Monthly |
| Automated test coverage | 80%+ of user stories | Monthly |

---

## Appendix A: Configuration Examples

### A.1 appsettings.json (Phase 4.1 Configuration)

```json
{
  "Phase4.1": {
    "GuiObjMap": {
      "Enabled": true,
      "CrawlConfig": {
        "MaxDepth": 3,
        "MaxConcurrentPages": 5,
        "PageTimeout": 10000,
        "WaitUntil": "domcontentloaded",
        "DisableImages": true,
        "DisableCSS": true
      },
      "SelectorPriority": [
        "data-testid",
        "id",
        "semantic",
        "css",
        "xpath"
      ],
      "CacheDuration": "24:00:00",
      "StoragePath": "/var/lib/cpu-agents/guiobjmap"
    },
    "DatabaseDiscovery": {
      "Enabled": true,
      "ConnectionStrings": {
        "BPS_Secure_DB": "Server=db.example.com;Database=BPS_Secure_DB;User Id=agent_readonly;Password=${KEYVAULT:DB_BPS_SECURE_DB_READONLY_PASSWORD}"
      },
      "QueryTimeout": 30,
      "MaxResultRows": 10000,
      "CacheDuration": "01:00:00"
    },
    "DbaWorkItem": {
      "Enabled": true,
      "DbaTeamEmail": "dba-team@example.com",
      "PollInterval": "00:00:30",
      "MaxWaitTime": "24:00:00",
      "WorkItemType": "Task",
      "WorkItemTags": ["agent-generated", "sql-request", "phase-4.1"]
    },
    "TestGeneration": {
      "Enabled": true,
      "OutputPath": "/var/lib/cpu-agents/generated-tests",
      "TemplateEngine": "Handlebars",
      "CodeFormatter": "Prettier",
      "GitAutoCommit": true
    }
  }
}
```

---

## Appendix B: Example Generated Test

### B.1 Generated Page Object Class (LoginPage.ts)

```typescript
// Generated by Phase 4.1 GUI Object Mapping Service
// Page: Login Page
// URL: /login
// Generated: 2026-02-23T11:30:00Z

import { Page, Locator } from '@playwright/test';

export class LoginPage {
  readonly page: Page;
  
  // Elements from GuiObjMap
  readonly usernameInput: Locator;
  readonly passwordInput: Locator;
  readonly loginButton: Locator;
  readonly errorMessage: Locator;
  readonly forgotPasswordLink: Locator;
  
  constructor(page: Page) {
    this.page = page;
    
    // Using GuiObjMap selectors with fallback strategy
    this.usernameInput = page.locator('[data-testid="username-input"]')
      .or(page.locator('#username'))
      .or(page.locator('input[name="username"]'))
      .or(page.locator('input[placeholder="Enter username"]'));
    
    this.passwordInput = page.locator('[data-testid="password-input"]')
      .or(page.locator('#password'))
      .or(page.locator('input[type="password"]'))
      .or(page.locator('input[name="password"]'));
    
    this.loginButton = page.locator('[data-testid="login-button"]')
      .or(page.locator('button[type="submit"]'))
      .or(page.locator('button:has-text("Login")'))
      .or(page.locator('.login-form button.primary'));
    
    this.errorMessage = page.locator('[data-testid="error-message"]')
      .or(page.locator('.error-message'))
      .or(page.locator('[role="alert"]'));
    
    this.forgotPasswordLink = page.locator('[data-testid="forgot-password-link"]')
      .or(page.locator('a:has-text("Forgot Password")'));
  }
  
  async navigate() {
    await this.page.goto('/login');
    await this.page.waitForLoadState('domcontentloaded');
  }
  
  async login(username: string, password: string) {
    await this.usernameInput.fill(username);
    await this.passwordInput.fill(password);
    await this.loginButton.click();
  }
  
  async getErrorMessage(): Promise<string> {
    return await this.errorMessage.textContent() || '';
  }
  
  async clickForgotPassword() {
    await this.forgotPasswordLink.click();
  }
}
```

### B.2 Generated Test Spec (login.spec.ts)

```typescript
// Generated by Phase 4.1 Test Generation Orchestrator
// User Story: User Login Authentication
// Test Scenario: Successful login with valid credentials
// Generated: 2026-02-23T11:30:00Z

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { DashboardPage } from '../pages/DashboardPage';
import { DatabaseHelper } from '../helpers/DatabaseHelper';

test.describe('User Login Authentication', () => {
  let loginPage: LoginPage;
  let dashboardPage: DashboardPage;
  let dbHelper: DatabaseHelper;
  
  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    dashboardPage = new DashboardPage(page);
    dbHelper = new DatabaseHelper();
    
    // Navigate to login page
    await loginPage.navigate();
  });
  
  test('TC001: Successful login with valid credentials', async ({ page }) => {
    // Arrange: Get test credentials from Azure Key Vault
    const username = process.env.TEST_USERNAME || 'test_admin';
    const password = process.env.TEST_PASSWORD || 'secure_password';
    
    // Act: Perform login
    await loginPage.login(username, password);
    
    // Assert: Verify navigation to dashboard
    await expect(page).toHaveURL('/dashboard');
    await expect(dashboardPage.welcomeMessage).toBeVisible();
    await expect(dashboardPage.welcomeMessage).toHaveText(`Welcome, ${username}!`);
    
    // Assert: Verify database session created
    const sessionResult = await dbHelper.executeQuery(
      `SELECT COUNT(*) as count FROM user_sessions 
       WHERE username = $1 AND logout_time IS NULL`,
      { username }
    );
    expect(sessionResult.rows[0].count).toBeGreaterThan(0);
    
    // Assert: Verify audit log entry
    const auditResult = await dbHelper.executeQuery(
      `SELECT * FROM audit_logs 
       WHERE username = $1 AND action = 'LOGIN' 
       ORDER BY created_at DESC LIMIT 1`,
      { username }
    );
    expect(auditResult.rows.length).toBe(1);
    expect(auditResult.rows[0].status).toBe('SUCCESS');
    expect(auditResult.rows[0].ip_address).toBeTruthy();
  });
  
  test('TC002: Failed login with invalid credentials', async ({ page }) => {
    // Arrange
    const username = 'invalid_user';
    const password = 'wrong_password';
    
    // Act
    await loginPage.login(username, password);
    
    // Assert: Verify error message displayed
    await expect(loginPage.errorMessage).toBeVisible();
    const errorText = await loginPage.getErrorMessage();
    expect(errorText).toContain('Invalid username or password');
    
    // Assert: Verify still on login page
    await expect(page).toHaveURL('/login');
    
    // Assert: Verify failed login attempt logged
    const auditResult = await dbHelper.executeQuery(
      `SELECT * FROM audit_logs 
       WHERE username = $1 AND action = 'LOGIN' 
       ORDER BY created_at DESC LIMIT 1`,
      { username }
    );
    expect(auditResult.rows.length).toBe(1);
    expect(auditResult.rows[0].status).toBe('FAILED');
    expect(auditResult.rows[0].failure_reason).toContain('Invalid credentials');
  });
  
  test('TC003: Login with empty credentials', async ({ page }) => {
    // Act
    await loginPage.login('', '');
    
    // Assert: Verify validation errors
    await expect(loginPage.usernameInput).toHaveAttribute('aria-invalid', 'true');
    await expect(loginPage.passwordInput).toHaveAttribute('aria-invalid', 'true');
    
    // Assert: Verify login button disabled
    await expect(loginPage.loginButton).toBeDisabled();
    
    // Assert: Verify no audit log entry (validation prevented submission)
    const auditResult = await dbHelper.executeQuery(
      `SELECT COUNT(*) as count FROM audit_logs 
       WHERE action = 'LOGIN' AND created_at > NOW() - INTERVAL '10 seconds'`
    );
    expect(auditResult.rows[0].count).toBe(0);
  });
  
  test.afterEach(async ({ page }, testInfo) => {
    // Capture screenshot on failure
    if (testInfo.status !== testInfo.expectedStatus) {
      await page.screenshot({ 
        path: `screenshots/${testInfo.title}-failure.png`,
        fullPage: true 
      });
    }
    
    // Cleanup: Logout if logged in
    if (page.url().includes('/dashboard')) {
      await dashboardPage.logout();
    }
    
    // Cleanup: Close database connection
    await dbHelper.disconnect();
  });
});
```

---

**Document Status**: Final Specification  
**Approval Required From**: Product Owner, Technical Lead, DBA Team Lead, Security Team, QA Lead  
**Next Steps**: Implementation (3-4 weeks), Testing (1 week), Deployment (1 week)
