# Phase 4: Test Generation - Architecture Design (v2.0)

**Project:** CPU Agents for SDLC  
**Phase:** 4 - Test Generation  
**Date:** February 23, 2026  
**Status:** Architecture Design (Phase 1) - Revised with Feedback  

---

## Document Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-02-23 | Initial architecture design |
| 2.0 | 2026-02-23 | Incorporated comprehensive feedback: AI enhancements, quality gates, self-healing, security, CI/CD integration, enhanced monitoring |

---

## Executive Summary

Phase 4 introduces **intelligent test generation with AI-powered quality assurance and self-healing capabilities** to the CPU Agents system. This phase enables autonomous creation of comprehensive, validated test suites from requirements with built-in quality gates, security controls, and CI/CD integration.

### Key Capabilities

1. **AI-Enhanced Requirements Parsing** - Extract testable requirements using Playwright MCP and specialized testing models
2. **Quality-Gated Test Generation** - Generate validated test cases with automated quality scoring and human-in-the-loop approval
3. **Self-Healing Test Framework** - Automatically repair broken tests using AI-powered analysis
4. **Secure Code Generation** - Generate test code with security validation and content filtering
5. **CI/CD Integration** - Automated test generation in PR workflows with validation gates
6. **Requirements Traceability Matrix** - Bidirectional mapping with impact analysis
7. **Predictive Test Generation** - Risk-based prioritization and change-impact analysis

### Business Value

| Metric | Value |
|--------|-------|
| Test Creation Time Reduction | 70% |
| Requirements Coverage | 95%+ |
| Test Quality Score | 85%+ (automated validation) |
| Test Maintenance Savings | 60% (self-healing) |
| Security Compliance | 100% (automated scanning) |
| Annual Savings per Team | $50K+ (increased from $30K with self-healing) |

---

## 1. System Goals & Objectives

### Primary Goals

1. **Automate High-Quality Test Generation** - Reduce manual effort by 70% while maintaining 85%+ quality score
2. **Ensure Requirements Coverage** - Achieve 95%+ test coverage with automated validation
3. **Enable Self-Healing Tests** - Reduce test maintenance by 60% through AI-powered repair
4. **Maintain Security & Governance** - 100% security compliance with automated scanning and approval workflows
5. **Integrate with CI/CD** - Seamless PR-based test generation with validation gates
6. **Provide Predictive Insights** - Risk-based test prioritization and change-impact analysis

### Success Criteria

**System-Level:**
- ✅ Generate test cases from 100+ user stories with 85%+ quality score
- ✅ Maintain requirements traceability matrix with <1s query time
- ✅ Generate executable test code with 90%+ compilation success rate
- ✅ Self-heal 80%+ of broken tests automatically
- ✅ Achieve 100% security compliance for generated code
- ✅ Process 10+ work items per minute
- ✅ Zero security vulnerabilities in generated code

**Module-Level:**
- ✅ Requirements parser extracts 95%+ of testable requirements with ambiguity detection
- ✅ Test case generator creates comprehensive test cases passing quality gates
- ✅ Self-healing agent repairs 80%+ of broken tests within 5 minutes
- ✅ Security scanner blocks 100% of dangerous patterns
- ✅ CI/CD integration completes test generation within 10-minute PR window

**Performance Targets (Revised):**
- Requirements parsing: <2s per user story
- Test case generation: <15s per requirement (revised from <5s for quality)
- Code generation: <10s per test case
- Self-healing repair: <5 minutes per broken test
- Traceability query: <1s for 1000+ mappings
- Batch processing: 50 concurrent generations

---

## 2. Enhanced High-Level Architecture

### System Components

```
┌──────────────────────────────────────────────────────────────────────┐
│                     Phase 4: Test Generation (Enhanced)               │
├──────────────────────────────────────────────────────────────────────┤
│                                                                        │
│  ┌────────────────────────┐      ┌──────────────────────────┐       │
│  │  Requirements Parser   │─────▶│  Test Case Generator     │       │
│  │  + Playwright MCP      │      │  + Quality Gates         │       │
│  │  + Ambiguity Detection │      │  + AI Validation         │       │
│  │  + Testability Scoring │      │  + Human-in-Loop Approval│       │
│  └────────────────────────┘      └──────────────────────────┘       │
│           │                              │                            │
│           ▼                              ▼                            │
│  ┌────────────────────────┐      ┌──────────────────────────┐       │
│  │ Traceability Matrix    │◀─────│  Test Code Generator     │       │
│  │  + Impact Analysis     │      │  + Security Scanner      │       │
│  │  + Coverage Tracking   │      │  + Code Review           │       │
│  │  + Risk Scoring        │      │  + xUnit + Playwright    │       │
│  └────────────────────────┘      └──────────────────────────┘       │
│           │                              │                            │
│           │      ┌──────────────────────────────┐                    │
│           │      │  Self-Healing Test Agent     │                    │
│           │      │  + Failure Analysis          │                    │
│           │      │  + Auto-Repair Logic         │                    │
│           │      │  + Selector Healing          │                    │
│           │      └──────────────────────────────┘                    │
│           │                      │                                    │
│           └──────────────┬───────┴────────────┐                      │
│                          ▼                     ▼                      │
│                 ┌─────────────────┐   ┌──────────────────┐          │
│                 │  Test Repository│   │  CI/CD Pipeline  │          │
│                 │  + Versioning   │   │  + PR Integration│          │
│                 │  + Audit Trail  │   │  + Quality Gates │          │
│                 └─────────────────┘   └──────────────────┘          │
└──────────────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌──────────────────────────────────────────────────────────────────────┐
│              Phase 3: Foundation (Existing)                           │
├──────────────────────────────────────────────────────────────────────┤
│  Authentication │ Work Items │ AI Service │ Git │ Observability      │
└──────────────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌──────────────────────────────────────────────────────────────────────┐
│                    Azure DevOps + CI/CD                               │
│  Work Items │ Test Plans │ Test Cases │ Git Repos │ Pipelines        │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 3. Enhanced Technology Stack

### Programming Languages

| Language | Usage | Justification |
|----------|-------|---------------|
| **C# (.NET 8.0)** | Core services, xUnit generation, security scanning | Consistent with Phase 3, Azure DevOps SDK, strong typing |
| **TypeScript** | Playwright test generation, MCP integration | Industry standard for browser automation, MCP support |
| **SQL** | Traceability matrix, quality metrics | Efficient relational queries |

### AI/ML Stack (Enhanced)

| Component | Technology | Usage |
|-----------|------------|-------|
| **Local AI** | vLLM or Ollama | Requirements parsing, test generation, self-healing |
| **Models** | Granite 4, Phi-3, Llama 3 | CPU-optimized quantized models (1-7B parameters) |
| **Specialized Models** | CodeLlama, StarCoder | Code generation and repair |
| **Playwright MCP** | MCP Server | Real-time browser interaction for context-aware testing |
| **Prompting** | Structured JSON + Few-Shot | Consistent, validated responses |
| **Validation** | JSON Schema | Output structure validation |

### Quality Assurance Stack (New)

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Code Analysis** | Roslyn Analyzers | C# code quality validation |
| **Security Scanning** | Custom + OWASP Rules | Detect dangerous patterns |
| **Test Validation** | xUnit + Playwright CLI | Compilation and execution validation |
| **Quality Scoring** | Custom ML Model | Test case quality assessment |

### Data Storage

| Data Type | Storage | Justification |
|-----------|---------|---------------|
| **Requirements** | PostgreSQL | Relational data, complex queries, ambiguity flags |
| **Test Cases** | Azure DevOps Test Plans | Native integration, team collaboration |
| **Traceability** | PostgreSQL | Fast lookups, bidirectional queries, impact analysis |
| **Test Code** | Azure Git | Version control, CI/CD integration |
| **Quality Metrics** | PostgreSQL + TimescaleDB | Time-series metrics, trend analysis |
| **Audit Logs** | PostgreSQL | Governance, compliance tracking |

### Integration

| System | Integration Method | Purpose |
|--------|-------------------|---------|
| **Azure DevOps** | REST API + SDK | Work items, test plans, test cases, pipelines |
| **Git** | LibGit2Sharp | Test code version control |
| **PostgreSQL** | Npgsql | Traceability, metrics, audit logs |
| **vLLM/Ollama** | HTTP API | AI-powered generation and repair |
| **Playwright MCP** | MCP Protocol | Browser automation context |
| **CI/CD Pipelines** | Azure Pipelines YAML | Automated test generation on PR |

---

## 4. Enhanced Data Architecture

### Core Data Models

#### 1. Enhanced Requirement

```csharp
public class Requirement
{
    // Core Fields
    public string RequirementId { get; set; }          // REQ-{WorkItemId}-{Index}
    public int WorkItemId { get; set; }                // Source work item
    public string Title { get; set; }
    public string Description { get; set; }
    public RequirementType Type { get; set; }
    public RequirementPriority Priority { get; set; }
    public List<string> AcceptanceCriteria { get; set; }
    
    // Quality Enhancements (NEW)
    public List<string> ValidationRules { get; set; }
    public RequirementQualityScore QualityScore { get; set; }
    public List<string> AmbiguityFlags { get; set; }    // Detected ambiguities
    public bool IsTestable { get; set; }                // Testability assessment
    public double TestabilityScore { get; set; }        // 0.0-1.0
    
    // Metadata
    public DateTime ExtractedAt { get; set; }
    public string ExtractedBy { get; set; }
}

public class RequirementQualityScore
{
    public double Clarity { get; set; }           // 0.0-1.0
    public double Completeness { get; set; }      // 0.0-1.0
    public double Testability { get; set; }       // 0.0-1.0
    public double OverallScore { get; set; }      // Average
    public List<string> IssuesDetected { get; set; }
}
```

#### 2. Enhanced TestCase

```csharp
public class TestCase
{
    // Core Fields
    public string TestCaseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TestCaseType Type { get; set; }
    public TestCasePriority Priority { get; set; }
    public List<string> Preconditions { get; set; }
    public List<TestStep> Steps { get; set; }
    public string ExpectedResult { get; set; }
    public List<string> RequirementIds { get; set; }
    
    // Quality Assurance (NEW)
    public TestQualityScore QualityScore { get; set; }
    public bool PassedQualityGate { get; set; }
    public List<string> QualityIssues { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public string ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Self-Healing (NEW)
    public int FailureCount { get; set; }
    public DateTime? LastFailureAt { get; set; }
    public int SelfHealAttempts { get; set; }
    public bool SelfHealingEnabled { get; set; }
    
    // Metadata
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; }
    public int? AzureTestCaseId { get; set; }
}

public class TestQualityScore
{
    public double Coverage { get; set; }          // Requirements coverage
    public double Maintainability { get; set; }   // Code maintainability
    public int Complexity { get; set; }           // Cyclomatic complexity
    public double Performance { get; set; }       // Execution efficiency
    public double OverallScore { get; set; }      // Weighted average
    public bool MeetsThreshold { get; set; }      // >= 0.85
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    AutoApproved
}
```

#### 3. TestCodeArtifact (Enhanced)

```csharp
public class TestCodeArtifact
{
    // Core Fields
    public string ArtifactId { get; set; }
    public string TestCaseId { get; set; }
    public TestFramework Framework { get; set; }
    public string ClassName { get; set; }
    public string MethodName { get; set; }
    public string SourceCode { get; set; }
    public string FilePath { get; set; }
    
    // Security & Quality (NEW)
    public bool PassedSecurityScan { get; set; }
    public List<string> SecurityIssues { get; set; }
    public bool Compilable { get; set; }
    public List<string> CompilationErrors { get; set; }
    public int ComplexityScore { get; set; }
    public List<string> BannedPatternsDetected { get; set; }
    
    // Self-Healing (NEW)
    public int RepairAttempts { get; set; }
    public DateTime? LastRepairedAt { get; set; }
    public string RepairStrategy { get; set; }
    
    // Metadata
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; }
    public string GitCommitHash { get; set; }
}
```

#### 4. SelfHealingRecord (NEW)

```csharp
public class SelfHealingRecord
{
    public string RecordId { get; set; }
    public string TestCaseId { get; set; }
    public DateTime FailureDetectedAt { get; set; }
    public string FailureReason { get; set; }
    public string FailureContext { get; set; }        // Stack trace, logs
    public string RepairStrategy { get; set; }        // Selector, Flow, Assertion
    public bool RepairSuccessful { get; set; }
    public string RepairedCode { get; set; }
    public DateTime? RepairedAt { get; set; }
    public string RepairedBy { get; set; }            // Agent ID
}
```

#### 5. TestQualityGate (NEW)

```csharp
public class TestQualityGate
{
    public string GateId { get; set; }
    public string Name { get; set; }
    public GateType Type { get; set; }
    public double MinimumThreshold { get; set; }
    public bool IsBlocking { get; set; }
    public List<string> ValidationRules { get; set; }
}

public enum GateType
{
    Coverage,
    Complexity,
    Security,
    Performance,
    Maintainability
}
```

#### 6. SecurityScanResult (NEW)

```csharp
public class SecurityScanResult
{
    public string ScanId { get; set; }
    public string ArtifactId { get; set; }
    public DateTime ScannedAt { get; set; }
    public bool Passed { get; set; }
    public List<SecurityViolation> Violations { get; set; }
}

public class SecurityViolation
{
    public string Pattern { get; set; }
    public int LineNumber { get; set; }
    public string Severity { get; set; }      // Critical, High, Medium, Low
    public string Description { get; set; }
    public string Remediation { get; set; }
}
```

---

## 5. Component Design (Enhanced)

### 5.1 Requirements Parser (Enhanced)

**Responsibility:** Extract testable requirements with quality assessment and ambiguity detection.

**Interfaces:**

```csharp
public interface IRequirementsParser
{
    Task<List<Requirement>> ParseUserStoryAsync(
        int workItemId, 
        CancellationToken cancellationToken = default);
    
    Task<List<Requirement>> ParseAcceptanceCriteriaAsync(
        string acceptanceCriteria, 
        int workItemId,
        CancellationToken cancellationToken = default);
    
    Task<RequirementQualityScore> AssessQualityAsync(
        Requirement requirement,
        CancellationToken cancellationToken = default);
    
    Task<List<string>> DetectAmbiguitiesAsync(
        string requirementText,
        CancellationToken cancellationToken = default);
    
    Task<double> CalculateTestabilityScoreAsync(
        Requirement requirement,
        CancellationToken cancellationToken = default);
}
```

**AI Prompting Strategy (Enhanced):**

```json
{
  "system": "You are a requirements analyst with expertise in test-driven development. Extract testable requirements and identify ambiguities.",
  "user": "Parse this user story and assess quality:\n\n\"\"\"As a user, I want to log in with my email and password so that I can access my account.\n\nAcceptance Criteria:\n- Email must be valid format\n- Password must be 8+ characters\n- Show error message for invalid credentials\n- Redirect to dashboard on success\"\"\"\n\nProvide:\n1. List of testable requirements\n2. Quality assessment (clarity, completeness, testability)\n3. Ambiguities detected\n4. Testability score (0.0-1.0)",
  "response_format": {
    "requirements": [
      {
        "id": "REQ-001",
        "title": "Email validation",
        "description": "System must validate email format",
        "type": "Functional",
        "priority": "Must",
        "acceptance_criteria": ["Email must match RFC 5322 format"],
        "testability_score": 0.95
      }
    ],
    "quality_score": {
      "clarity": 0.9,
      "completeness": 0.85,
      "testability": 0.95,
      "overall": 0.90
    },
    "ambiguities": [
      "\"valid format\" - should specify RFC 5322 or custom validation"
    ]
  }
}
```

**Playwright MCP Integration:**

```typescript
// Use Playwright MCP to capture live browser context for better test generation
import { MCPClient } from '@modelcontextprotocol/sdk';

class PlaywrightMCPIntegration {
  async captureLoginFlow(): Promise<BrowserContext> {
    const mcp = new MCPClient('playwright');
    const context = await mcp.invoke('browser_navigate', {
      url: 'https://app.example.com/login'
    });
    
    // Capture DOM structure, selectors, and user flows
    const pageContext = await mcp.invoke('browser_screenshot', {
      fullPage: true
    });
    
    return {
      dom: context.dom,
      selectors: context.selectors,
      screenshot: pageContext.screenshot
    };
  }
}
```

**Acceptance Criteria:**

**Function-Level:**
- ✅ `ParseUserStoryAsync` extracts 95%+ of testable requirements
- ✅ `AssessQualityAsync` returns quality score with 90%+ accuracy
- ✅ `DetectAmbiguitiesAsync` identifies 85%+ of ambiguous terms
- ✅ `CalculateTestabilityScoreAsync` scores within ±0.1 of manual assessment

**Class-Level:**
- ✅ Parser handles 100+ user stories without performance degradation
- ✅ Quality assessment completes within 2s per user story
- ✅ Ambiguity detection uses NLP techniques (TF-IDF, named entity recognition)
- ✅ Testability scoring considers acceptance criteria completeness

---

### 5.2 Test Case Generator (Enhanced with Quality Gates)

**Responsibility:** Generate comprehensive test cases with automated quality validation and approval workflows.

**Interfaces:**

```csharp
public interface ITestCaseGenerator
{
    Task<TestCase> GenerateTestCaseAsync(
        Requirement requirement,
        CancellationToken cancellationToken = default);
    
    Task<TestQualityScore> ValidateTestCaseQualityAsync(
        TestCase testCase,
        CancellationToken cancellationToken = default);
    
    Task<bool> PassesQualityGatesAsync(
        TestCase testCase,
        List<TestQualityGate> gates,
        CancellationToken cancellationToken = default);
    
    Task<ApprovalStatus> RequestHumanApprovalAsync(
        TestCase testCase,
        CancellationToken cancellationToken = default);
}
```

**Quality Gate Implementation:**

```csharp
public class TestQualityGateValidator
{
    private readonly double _minimumCoverageThreshold = 0.85;
    private readonly int _maximumComplexityScore = 15;
    
    public async Task<bool> ValidateTestCase(TestCase testCase)
    {
        var gates = new List<TestQualityGate>
        {
            new TestQualityGate 
            { 
                Name = "Coverage", 
                Type = GateType.Coverage, 
                MinimumThreshold = 0.85,
                IsBlocking = true
            },
            new TestQualityGate 
            { 
                Name = "Complexity", 
                Type = GateType.Complexity, 
                MinimumThreshold = 15,
                IsBlocking = true
            },
            new TestQualityGate 
            { 
                Name = "Maintainability", 
                Type = GateType.Maintainability, 
                MinimumThreshold = 0.75,
                IsBlocking = false
            }
        };
        
        foreach (var gate in gates.Where(g => g.IsBlocking))
        {
            if (!await PassesGate(testCase, gate))
            {
                testCase.QualityIssues.Add($"Failed {gate.Name} gate");
                return false;
            }
        }
        
        return true;
    }
    
    private async Task<bool> PassesGate(TestCase testCase, TestQualityGate gate)
    {
        return gate.Type switch
        {
            GateType.Coverage => testCase.QualityScore.Coverage >= gate.MinimumThreshold,
            GateType.Complexity => testCase.QualityScore.Complexity <= gate.MinimumThreshold,
            GateType.Maintainability => testCase.QualityScore.Maintainability >= gate.MinimumThreshold,
            _ => true
        };
    }
}
```

**Human-in-the-Loop Approval:**

```csharp
public class HumanApprovalWorkflow
{
    public async Task<ApprovalStatus> RequestApproval(TestCase testCase)
    {
        // Auto-approve if quality score is excellent
        if (testCase.QualityScore.OverallScore >= 0.95)
        {
            return ApprovalStatus.AutoApproved;
        }
        
        // Require manual approval for critical tests or lower quality
        if (testCase.Priority == TestCasePriority.Critical || 
            testCase.QualityScore.OverallScore < 0.85)
        {
            await SendApprovalRequestAsync(testCase);
            return ApprovalStatus.Pending;
        }
        
        return ApprovalStatus.Approved;
    }
}
```

**Acceptance Criteria:**

**Function-Level:**
- ✅ `GenerateTestCaseAsync` creates complete test cases (preconditions, steps, expected results)
- ✅ `ValidateTestCaseQualityAsync` returns quality score within 15s
- ✅ `PassesQualityGatesAsync` validates against all configured gates
- ✅ `RequestHumanApprovalAsync` routes to appropriate approver based on priority

**Class-Level:**
- ✅ Generator produces test cases with 85%+ quality score
- ✅ Quality validation completes within 15s per test case
- ✅ Quality gates block low-quality tests from deployment
- ✅ Approval workflow handles 100+ concurrent requests

---

### 5.3 Self-Healing Test Agent (NEW)

**Responsibility:** Automatically detect and repair broken tests using AI-powered analysis.

**Interfaces:**

```csharp
public interface ISelfHealingTestAgent
{
    Task<SelfHealingRecord> AnalyzeFailureAsync(
        string testCaseId,
        string failureContext,
        CancellationToken cancellationToken = default);
    
    Task<TestCodeArtifact> RepairTestAsync(
        TestCodeArtifact brokenTest,
        SelfHealingRecord failureAnalysis,
        CancellationToken cancellationToken = default);
    
    Task<bool> ValidateRepairAsync(
        TestCodeArtifact repairedTest,
        CancellationToken cancellationToken = default);
    
    Task<List<string>> GetRepairStrategiesAsync(
        string failureType,
        CancellationToken cancellationToken = default);
}
```

**Repair Strategies:**

```csharp
public class SelfHealingStrategies
{
    public enum RepairStrategy
    {
        SelectorRepair,      // Update CSS/XPath selectors
        FlowOptimization,    // Adjust test flow and timing
        AssertionRefinement, // Fix assertion logic
        DataRefresh,         // Update test data
        WaitCondition        // Add/adjust wait conditions
    }
    
    public async Task<TestCodeArtifact> ApplySelectorRepair(
        TestCodeArtifact brokenTest,
        string failureContext)
    {
        // Use Playwright MCP to find updated selectors
        var mcp = new PlaywrightMCPClient();
        var newSelectors = await mcp.FindSelectorsAsync(failureContext);
        
        // Replace old selectors with new ones
        var repairedCode = brokenTest.SourceCode;
        foreach (var (oldSelector, newSelector) in newSelectors)
        {
            repairedCode = repairedCode.Replace(oldSelector, newSelector);
        }
        
        return new TestCodeArtifact
        {
            ArtifactId = Guid.NewGuid().ToString(),
            TestCaseId = brokenTest.TestCaseId,
            SourceCode = repairedCode,
            RepairStrategy = "SelectorRepair",
            RepairAttempts = brokenTest.RepairAttempts + 1
        };
    }
}
```

**AI Prompting for Self-Healing:**

```json
{
  "system": "You are a test automation expert. Analyze test failures and suggest repairs.",
  "user": "Test failed with error:\n\n\"\"\"TimeoutError: Waiting for selector '.login-button' failed: timeout 30000ms exceeded\n\nStack trace:\n  at Page.waitForSelector (page.ts:1234)\n  at LoginTest.testSuccessfulLogin (login.test.ts:45)\"\"\"\n\nTest code:\n\"\"\"await page.click('.login-button');\"\"\"\n\nCurrent page HTML:\n\"\"\"<button class=\"btn-login primary\">Log In</button>\"\"\"\n\nSuggest repair strategy and updated code.",
  "response_format": {
    "failure_analysis": {
      "root_cause": "Selector changed from '.login-button' to '.btn-login'",
      "failure_type": "SelectorNotFound",
      "confidence": 0.95
    },
    "repair_strategy": "SelectorRepair",
    "repaired_code": "await page.click('.btn-login');",
    "alternative_selectors": [
      "button:has-text('Log In')",
      "[data-testid='login-button']"
    ]
  }
}
```

**Acceptance Criteria:**

**Function-Level:**
- ✅ `AnalyzeFailureAsync` identifies root cause with 90%+ accuracy
- ✅ `RepairTestAsync` successfully repairs 80%+ of broken tests
- ✅ `ValidateRepairAsync` confirms repair within 5 minutes
- ✅ `GetRepairStrategiesAsync` returns applicable strategies based on failure type

**Class-Level:**
- ✅ Self-healing agent repairs 80%+ of selector-based failures
- ✅ Repair attempts complete within 5 minutes
- ✅ Repaired tests pass validation on first attempt 70%+ of the time
- ✅ Agent learns from successful repairs to improve future accuracy

---

### 5.4 Security Scanner (NEW)

**Responsibility:** Validate generated test code for security vulnerabilities and dangerous patterns.

**Interfaces:**

```csharp
public interface ISecurityScanner
{
    Task<SecurityScanResult> ScanTestCodeAsync(
        TestCodeArtifact artifact,
        CancellationToken cancellationToken = default);
    
    Task<List<string>> GetBannedPatternsAsync(
        CancellationToken cancellationToken = default);
    
    Task<bool> ValidateTestDataAsync(
        string testData,
        CancellationToken cancellationToken = default);
}
```

**Banned Patterns:**

```csharp
public class SecurityBannedPatterns
{
    public static readonly List<string> DangerousPatterns = new()
    {
        // Code Execution
        "eval(",
        "Function(",
        "setTimeout(.*string",
        "setInterval(.*string",
        
        // File System Access
        "System.IO.File.Write",
        "System.IO.File.Delete",
        "fs.writeFile",
        "fs.unlink",
        
        // Network Access
        "HttpClient.*http://",  // Only allow HTTPS
        "fetch(.*http://",
        "XMLHttpRequest",
        
        // Sensitive Data
        "password.*=.*\"",      // Hardcoded passwords
        "apiKey.*=.*\"",        // Hardcoded API keys
        "connectionString",
        
        // Dangerous Commands
        "Process.Start",
        "exec(",
        "spawn(",
        
        // SQL Injection Risks
        "ExecuteRaw",
        "FromSqlRaw.*\\+",      // String concatenation in SQL
    };
    
    public static readonly List<string> RequireReview = new()
    {
        "Thread.Sleep",         // Performance anti-pattern
        "Task.Delay(.*[0-9]{4,}",  // Long delays
        "while(true)",          // Infinite loops
        "recursion",            // Potential stack overflow
    };
}
```

**Security Scan Implementation:**

```csharp
public class SecurityScanner : ISecurityScanner
{
    public async Task<SecurityScanResult> ScanTestCodeAsync(
        TestCodeArtifact artifact,
        CancellationToken cancellationToken)
    {
        var violations = new List<SecurityViolation>();
        
        // Scan for banned patterns
        foreach (var pattern in SecurityBannedPatterns.DangerousPatterns)
        {
            var matches = Regex.Matches(artifact.SourceCode, pattern);
            foreach (Match match in matches)
            {
                violations.Add(new SecurityViolation
                {
                    Pattern = pattern,
                    LineNumber = GetLineNumber(artifact.SourceCode, match.Index),
                    Severity = "Critical",
                    Description = $"Dangerous pattern detected: {pattern}",
                    Remediation = GetRemediationAdvice(pattern)
                });
            }
        }
        
        // Scan for patterns requiring review
        foreach (var pattern in SecurityBannedPatterns.RequireReview)
        {
            var matches = Regex.Matches(artifact.SourceCode, pattern);
            foreach (Match match in matches)
            {
                violations.Add(new SecurityViolation
                {
                    Pattern = pattern,
                    LineNumber = GetLineNumber(artifact.SourceCode, match.Index),
                    Severity = "Medium",
                    Description = $"Pattern requires manual review: {pattern}",
                    Remediation = "Review for potential issues"
                });
            }
        }
        
        return new SecurityScanResult
        {
            ScanId = Guid.NewGuid().ToString(),
            ArtifactId = artifact.ArtifactId,
            ScannedAt = DateTime.UtcNow,
            Passed = violations.Count(v => v.Severity == "Critical") == 0,
            Violations = violations
        };
    }
}
```

**Acceptance Criteria:**

**Function-Level:**
- ✅ `ScanTestCodeAsync` detects 100% of banned patterns
- ✅ `GetBannedPatternsAsync` returns current pattern list
- ✅ `ValidateTestDataAsync` detects sensitive data in test inputs

**Class-Level:**
- ✅ Scanner blocks 100% of critical security violations
- ✅ Scan completes within 5s per test artifact
- ✅ False positive rate <5%
- ✅ Pattern library updated monthly with new threats

---

### 5.5 CI/CD Integration Service (NEW)

**Responsibility:** Integrate test generation into CI/CD pipelines with automated validation gates.

**Interfaces:**

```csharp
public interface ICICDIntegrationService
{
    Task<bool> TriggerTestGenerationOnPRAsync(
        string pullRequestId,
        CancellationToken cancellationToken = default);
    
    Task<CICDValidationResult> ValidateGeneratedTestsAsync(
        List<TestCodeArtifact> artifacts,
        CancellationToken cancellationToken = default);
    
    Task<bool> UpdatePRStatusAsync(
        string pullRequestId,
        CICDValidationResult result,
        CancellationToken cancellationToken = default);
}
```

**Azure Pipelines Integration:**

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
      - main
      - feature/*

pr:
  branches:
    include:
      - main

stages:
  - stage: TestGeneration
    displayName: 'AI-Powered Test Generation'
    jobs:
      - job: GenerateTests
        displayName: 'Generate Tests from Requirements'
        pool:
          vmImage: 'ubuntu-latest'
        timeoutInMinutes: 10
        steps:
          - task: UseDotNet@2
            inputs:
              version: '8.0.x'
          
          - script: |
              dotnet run --project src/Phase4.TestGeneration/CLI/TestGenerationCLI.csproj \
                --pr-id $(System.PullRequest.PullRequestId) \
                --work-items-changed $(System.PullRequest.SourceBranch)
            displayName: 'Generate Tests for Changed Requirements'
          
          - task: PublishTestResults@2
            inputs:
              testResultsFormat: 'VSTest'
              testResultsFiles: '**/TestResults/*.trx'
              failTaskOnFailedTests: true
          
          - script: |
              dotnet test src/Phase4.TestGeneration.Tests/ \
                --filter "Category=Generated" \
                --logger "trx;LogFileName=generated-tests.trx"
            displayName: 'Validate Generated Tests'
          
          - task: PublishCodeCoverageResults@1
            inputs:
              codeCoverageTool: 'Cobertura'
              summaryFileLocation: '**/coverage.cobertura.xml'
              failIfCoverageEmpty: false

  - stage: QualityGates
    displayName: 'Quality Gate Validation'
    dependsOn: TestGeneration
    jobs:
      - job: ValidateQuality
        displayName: 'Validate Test Quality'
        steps:
          - script: |
              dotnet run --project src/Phase4.TestGeneration/QualityGates/QualityGateCLI.csproj \
                --pr-id $(System.PullRequest.PullRequestId) \
                --minimum-quality-score 0.85
            displayName: 'Run Quality Gates'
          
          - task: PublishBuildArtifacts@1
            inputs:
              pathToPublish: 'TestResults/quality-report.json'
              artifactName: 'QualityReport'
```

**PR Status Update:**

```csharp
public class PRStatusUpdater
{
    public async Task UpdatePRStatus(
        string prId,
        CICDValidationResult result)
    {
        var status = new GitPullRequestStatus
        {
            State = result.Passed ? GitStatusState.Succeeded : GitStatusState.Failed,
            Description = result.Passed 
                ? $"Generated {result.TestsGenerated} tests, all passed quality gates"
                : $"Quality gate failures: {string.Join(", ", result.Failures)}",
            Context = new GitStatusContext
            {
                Name = "AI Test Generation",
                Genre = "continuous-integration"
            },
            TargetUrl = result.ReportUrl
        };
        
        await _gitClient.CreatePullRequestStatusAsync(status, prId);
    }
}
```

**Acceptance Criteria:**

**Function-Level:**
- ✅ `TriggerTestGenerationOnPRAsync` initiates generation within 30s of PR creation
- ✅ `ValidateGeneratedTestsAsync` completes within 10-minute PR window
- ✅ `UpdatePRStatusAsync` updates PR status with detailed results

**Class-Level:**
- ✅ CI/CD integration handles 50+ concurrent PRs
- ✅ Test generation completes within 10-minute timeout
- ✅ Quality gates block PRs with <85% quality score
- ✅ PR status includes links to detailed quality reports

---

## 6. Enhanced Monitoring & Metrics

### Key Performance Indicators (KPIs)

```csharp
public class TestGenerationMetrics
{
    // Generation Metrics
    public double TestGenerationSuccessRate { get; set; }      // Target: 95%+
    public double TestCompilationSuccessRate { get; set; }     // Target: 90%+
    public double GeneratedTestPassRate { get; set; }          // Target: 85%+
    public TimeSpan AverageGenerationTime { get; set; }        // Target: <15s
    
    // Quality Metrics
    public double AverageQualityScore { get; set; }            // Target: 0.85+
    public int QualityGateFailures { get; set; }               // Target: <10%
    public double RequirementsCoverage { get; set; }           // Target: 95%+
    
    // Self-Healing Metrics
    public double SelfHealingSuccessRate { get; set; }         // Target: 80%+
    public TimeSpan AverageRepairTime { get; set; }            // Target: <5min
    public int TestsAutoRepaired { get; set; }
    
    // Security Metrics
    public int SecurityViolationsBlocked { get; set; }
    public int CriticalViolationsDetected { get; set; }        // Target: 0
    public double SecurityScanPassRate { get; set; }           // Target: 100%
    
    // Business Metrics
    public double TestCreationTimeReduction { get; set; }      // Target: 70%+
    public double TestMaintenanceSavings { get; set; }         // Target: 60%+
    public int BugDetectionEffectiveness { get; set; }
    public double ROI { get; set; }                            // Target: 3x+
}
```

### OpenTelemetry Integration

```csharp
public class TestGenerationTelemetry
{
    private readonly Meter _meter;
    private readonly Counter<long> _testsGenerated;
    private readonly Histogram<double> _generationTime;
    private readonly Counter<long> _qualityGateFailures;
    private readonly Counter<long> _selfHealingAttempts;
    
    public TestGenerationTelemetry()
    {
        _meter = new Meter("Phase4.TestGeneration", "1.0.0");
        
        _testsGenerated = _meter.CreateCounter<long>(
            "tests_generated_total",
            description: "Total number of tests generated");
        
        _generationTime = _meter.CreateHistogram<double>(
            "test_generation_duration_seconds",
            description: "Time taken to generate a test case");
        
        _qualityGateFailures = _meter.CreateCounter<long>(
            "quality_gate_failures_total",
            description: "Number of tests failing quality gates");
        
        _selfHealingAttempts = _meter.CreateCounter<long>(
            "self_healing_attempts_total",
            description: "Number of self-healing repair attempts");
    }
    
    public void RecordTestGeneration(TestCase testCase, TimeSpan duration)
    {
        _testsGenerated.Add(1, new KeyValuePair<string, object>("framework", testCase.Type));
        _generationTime.Record(duration.TotalSeconds);
    }
}
```

---

## 7. Phased Implementation Roadmap

### Phase 4.1: Foundation (Weeks 1-4)

**Goal:** Basic test generation with manual validation

**Deliverables:**
- Requirements parser with quality assessment
- Test case generator (manual tests only)
- Traceability matrix
- Basic quality gates
- PostgreSQL schema

**Acceptance Criteria:**
- ✅ Parse 100+ user stories with 90%+ accuracy
- ✅ Generate manual test cases with 80%+ quality score
- ✅ Maintain traceability for 1000+ requirements
- ✅ Quality gates block tests with <0.75 score

### Phase 4.2: Code Generation & Security (Weeks 5-8)

**Goal:** Automated test code generation with security validation

**Deliverables:**
- Test code generator (xUnit only)
- Security scanner with banned patterns
- Code compilation validation
- Human approval workflow
- Git integration

**Acceptance Criteria:**
- ✅ Generate compilable xUnit tests with 85%+ success rate
- ✅ Block 100% of critical security violations
- ✅ Approval workflow handles 50+ concurrent requests
- ✅ Tests committed to Git with proper structure

### Phase 4.3: Self-Healing & CI/CD (Weeks 9-12)

**Goal:** Self-healing capabilities and CI/CD integration

**Deliverables:**
- Self-healing test agent
- Playwright MCP integration
- CI/CD pipeline integration
- Enhanced monitoring and metrics
- Playwright test generation

**Acceptance Criteria:**
- ✅ Self-heal 80%+ of broken tests within 5 minutes
- ✅ CI/CD integration completes within 10-minute PR window
- ✅ Generate Playwright tests with 85%+ quality score
- ✅ Comprehensive telemetry and dashboards

### Phase 4.4: Advanced Features (Weeks 13-16)

**Goal:** Predictive analytics and optimization

**Deliverables:**
- Risk-based test prioritization
- Change-impact analysis
- Predictive test generation
- Performance optimization
- ML-based quality scoring

**Acceptance Criteria:**
- ✅ Prioritize tests based on risk with 90%+ accuracy
- ✅ Impact analysis identifies affected tests within 1s
- ✅ Predictive generation reduces redundant tests by 30%
- ✅ Process 20+ work items per minute

---

## 8. Risk Assessment & Mitigation

### Identified Risks

| Risk | Probability | Impact | Mitigation Strategy |
|------|-------------|--------|---------------------|
| **Over-reliance on AI** | Medium | High | Implement quality gates, human approval for critical tests |
| **Quality variability** | High | Medium | Multi-level validation, quality scoring, iterative improvement |
| **Security vulnerabilities** | Low | Critical | Comprehensive security scanning, banned pattern library |
| **Performance degradation** | Medium | Medium | Batch processing, caching, performance monitoring |
| **Integration complexity** | Medium | High | Phased rollout, comprehensive testing, rollback mechanisms |
| **False positives in self-healing** | Medium | Medium | Validation before deployment, human review for critical tests |

### Mitigation Actions

1. **Gradual Rollout**
   - Phase 4.1: Manual validation required (100%)
   - Phase 4.2: Auto-approve high-quality tests (>0.95)
   - Phase 4.3: Full automation with quality gates
   - Phase 4.4: Self-healing enabled

2. **Quality Checkpoints**
   - Requirement quality assessment before generation
   - Test case quality gates before code generation
   - Security scanning before deployment
   - Human approval for critical tests

3. **Monitoring & Alerting**
   - Real-time quality metrics dashboard
   - Alerts for quality gate failures
   - Security violation notifications
   - Performance degradation alerts

4. **Rollback Mechanisms**
   - Version control for all generated tests
   - Ability to disable self-healing per test
   - Manual override for quality gates
   - Rollback to previous test versions

---

## 9. Security & Governance Framework

### Security Controls

```csharp
public class SecurityGovernanceFramework
{
    // Code Security
    public bool SanitizeGeneratedCode = true;
    public List<string> BannedPatterns = SecurityBannedPatterns.DangerousPatterns;
    public bool RequireSecurityScan = true;
    
    // Data Security
    public bool SanitizeTestData = true;
    public bool EncryptSensitiveData = true;
    public bool MaskPII = true;
    
    // Access Control
    public bool RequireApprovalForCriticalTests = true;
    public List<string> ApproversForCriticalTests = new() { "QA Lead", "Security Team" };
    
    // Audit & Compliance
    public bool EnableAuditLogging = true;
    public TimeSpan AuditRetention = TimeSpan.FromDays(365);
    public bool GenerateComplianceReports = true;
}
```

### Governance Policies

1. **Code Review Requirements**
   - All AI-generated tests for critical paths require human review
   - Security team review for tests accessing sensitive data
   - QA lead approval for tests with quality score <0.85

2. **Content Filtering**
   - Remove PII from test data
   - Mask sensitive information (passwords, API keys)
   - Sanitize SQL queries and file paths

3. **Approval Workflows**
   - Auto-approve: Quality score ≥0.95, non-critical
   - Manual review: Quality score 0.85-0.94, medium priority
   - Mandatory approval: Quality score <0.85 or critical priority

4. **Audit Trails**
   - Log all test generation requests
   - Track approval decisions
   - Record self-healing actions
   - Monitor security scan results

---

## 10. Success Metrics & KPIs

### Primary Success Metrics

| Metric | Baseline | Target | Measurement Method |
|--------|----------|--------|-------------------|
| **Test Creation Time** | 2 hours/test | 36 minutes/test (70% reduction) | Time tracking |
| **Requirements Coverage** | 75% | 95%+ | Traceability matrix |
| **Test Quality Score** | N/A | 85%+ average | Automated quality assessment |
| **Test Maintenance Time** | 1 hour/test/month | 24 minutes/test/month (60% reduction) | Self-healing metrics |
| **Security Compliance** | 85% | 100% | Security scan pass rate |
| **ROI** | N/A | 3x within 12 months | Cost savings vs. investment |

### Operational Metrics

| Metric | Target | Alert Threshold |
|--------|--------|-----------------|
| Test Generation Success Rate | 95%+ | <90% |
| Test Compilation Success Rate | 90%+ | <85% |
| Self-Healing Success Rate | 80%+ | <75% |
| Quality Gate Pass Rate | 85%+ | <80% |
| Security Scan Pass Rate | 100% | <100% |
| Average Generation Time | <15s | >30s |
| CI/CD Completion Time | <10 minutes | >15 minutes |

---

## 11. Deployment Architecture

### On-Premise Deployment

```
┌─────────────────────────────────────────────────────────────┐
│                  Enterprise Network                          │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │  Test Generation │──────│  PostgreSQL      │            │
│  │  Service         │      │  - Requirements  │            │
│  │  - .NET 8.0      │      │  - Traceability  │            │
│  │  - vLLM/Ollama   │      │  - Metrics       │            │
│  └──────────────────┘      └──────────────────┘            │
│           │                                                  │
│           ▼                                                  │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │  Azure DevOps    │◀─────│  Git Repository  │            │
│  │  - Work Items    │      │  - Test Code     │            │
│  │  - Test Plans    │      │  - Version Ctrl  │            │
│  └──────────────────┘      └──────────────────┘            │
│           │                                                  │
│           ▼                                                  │
│  ┌──────────────────────────────────────────┐              │
│  │  Observability Stack (Podman)            │              │
│  │  - Jaeger (Tracing)                      │              │
│  │  - Prometheus (Metrics)                  │              │
│  │  - Grafana (Dashboards)                  │              │
│  └──────────────────────────────────────────┘              │
└─────────────────────────────────────────────────────────────┘
```

### Cloud Deployment (Azure)

```
┌─────────────────────────────────────────────────────────────┐
│                      Azure Cloud                             │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │  App Service     │──────│  PostgreSQL      │            │
│  │  - Test Gen API  │      │  Flexible Server │            │
│  │  - Auto-scaling  │      │  - HA enabled    │            │
│  └──────────────────┘      └──────────────────┘            │
│           │                                                  │
│           ▼                                                  │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │  Azure DevOps    │◀─────│  Azure Repos     │            │
│  │  Services        │      │  - Git           │            │
│  └──────────────────┘      └──────────────────┘            │
│           │                                                  │
│           ▼                                                  │
│  ┌──────────────────────────────────────────┐              │
│  │  Azure Monitor + Application Insights    │              │
│  │  - Distributed Tracing                   │              │
│  │  - Metrics & Alerts                      │              │
│  │  - Log Analytics                         │              │
│  └──────────────────────────────────────────┘              │
└─────────────────────────────────────────────────────────────┘
```

---

## 12. Conclusion & Next Steps

### Architecture Completeness

This enhanced Phase 4 architecture incorporates comprehensive feedback addressing:

✅ **AI Technology Stack Enhancement** - Playwright MCP, specialized models, structured prompting  
✅ **Test Quality Assurance** - Quality gates, validation, scoring, human-in-the-loop  
✅ **Self-Healing Framework** - Automated repair, failure analysis, multiple strategies  
✅ **Security & Governance** - Banned patterns, security scanning, approval workflows, audit trails  
✅ **CI/CD Integration** - PR-based generation, validation gates, status updates  
✅ **Enhanced Monitoring** - Comprehensive KPIs, OpenTelemetry, dashboards  
✅ **Performance & Scalability** - Realistic targets, batch processing, caching  
✅ **Data Quality Controls** - Ambiguity detection, testability scoring, validation  
✅ **Risk Mitigation** - Phased rollout, quality checkpoints, rollback mechanisms  
✅ **Implementation Roadmap** - 4 phases over 16 weeks with clear deliverables  

### Next Steps

1. **Review & Approval** - Stakeholder review of architecture design
2. **Phase 2: Specification Development** - Create detailed implementation specs with acceptance criteria at all levels
3. **Phase 2: API Specification** - Complete API documentation with examples
4. **Phase 3-6: Implementation** - Follow comprehensive-implementation methodology
5. **Pilot Deployment** - Phase 4.1 rollout with manual validation
6. **Production Rollout** - Gradual expansion through Phase 4.2-4.4

### Estimated Timeline

- **Architecture Review**: 1 week
- **Specification Development**: 3 weeks
- **Phase 4.1 Implementation**: 4 weeks
- **Phase 4.2 Implementation**: 4 weeks
- **Phase 4.3 Implementation**: 4 weeks
- **Phase 4.4 Implementation**: 4 weeks
- **Total**: 20 weeks (5 months)

### Estimated Cost

- **Development**: 800 hours × $150/hour = $120,000
- **Infrastructure**: $5,000/year (PostgreSQL, observability)
- **Total Investment**: $125,000
- **Expected ROI**: 3x within 12 months ($375,000 savings)

---

## Appendices

### Appendix A: AI Model Selection Guide

| Model | Parameters | Use Case | Performance |
|-------|------------|----------|-------------|
| **Granite 4** | 4B | Requirements parsing, test generation | 15 tokens/s on CPU |
| **Phi-3-mini** | 3.8B | Self-healing, code repair | 20 tokens/s on CPU |
| **CodeLlama** | 7B | Code generation, validation | 10 tokens/s on CPU |
| **Llama 3** | 8B | Quality assessment, analysis | 12 tokens/s on CPU |

### Appendix B: Quality Gate Configuration

```yaml
quality_gates:
  - name: Coverage
    type: Coverage
    threshold: 0.85
    blocking: true
    
  - name: Complexity
    type: Complexity
    threshold: 15
    blocking: true
    
  - name: Maintainability
    type: Maintainability
    threshold: 0.75
    blocking: false
    
  - name: Security
    type: Security
    threshold: 1.0
    blocking: true
    
  - name: Performance
    type: Performance
    threshold: 0.80
    blocking: false
```

### Appendix C: Self-Healing Repair Strategies

| Strategy | Success Rate | Use Case |
|----------|--------------|----------|
| **Selector Repair** | 85% | CSS/XPath selector changes |
| **Flow Optimization** | 75% | Timing and synchronization issues |
| **Assertion Refinement** | 70% | Assertion logic errors |
| **Data Refresh** | 90% | Test data staleness |
| **Wait Condition** | 80% | Race conditions, async issues |

### Appendix D: Security Scan Rules

See `SecurityBannedPatterns` class in Section 5.4 for complete list of:
- Dangerous patterns (blocking)
- Patterns requiring review (warning)
- Remediation advice for each pattern

---

**Document Status:** Architecture Design Complete (Phase 1)  
**Next Phase:** Specification Development (Phase 2)  
**Approval Required:** Yes  
**Estimated Review Time:** 1 week
