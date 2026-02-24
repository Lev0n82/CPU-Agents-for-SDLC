---
name: documentation-firstworkflow
description: Input: Initial architecture design, stakeholder feedback
Process:
Read architecture review feedback thoroughly
Create comprehensive response document addressing each point
Update architecture design to +version with all refinements
Document technical decisions and trade-offs
Output: Architecture review response (15,000+ words), Architecture design v+1
Key Sections if relevant:
Concurrency control mechanisms
Secrets management architecture
Offline synchronization with conflict resolution
Operational resilience (checkpointing, recovery)
Observability integration (OpenTelemetry)
---

# Documentation Firstworkflow

## Instructions

YAML
namespace {Namespace};

/// <summary>
/// {XML documentation summary}
/// </summary>
public interface {InterfaceName}
{
    /// <summary>
    /// {Method documentation}
    /// </summary>
    /// <param name="{paramName}">{Parameter description}</param>
    /// <returns>{Return value description}</returns>
    /// <exception cref="{ExceptionType}">{Exception condition}</exception>
    Task<{ReturnType}> {MethodName}({ParamType} {paramName}, CancellationToken cancellationToken = default);
}
Documentation-First Implementation
Create comprehensive implementation documentation before writing code, ensuring production-ready specifications with complete acceptance criteria, test coverage, and operational procedures.
When to Use This Skill
Use this skill when:
Implementing complex multi-module systems (10+ modules)
Creating enterprise-grade software with strict quality requirements
Defining 100+ acceptance criteria across multiple levels (function, class, module, system)
Planning 200+ automated tests (unit, integration, system)
Requiring complete operational documentation (deployment, troubleshooting, monitoring)
Documentation-First Workflow
Phase 1: Architecture Review & Refinement
Input: Initial architecture design, stakeholder feedback
Process:
Read architecture review feedback thoroughly
Create comprehensive response document addressing each point
Update architecture design to v3.0+ with all refinements
Document technical decisions and trade-offs
Output: Architecture review response (15,000+ words), Architecture design v3.0 (4,000+ lines)
Key Sections:
Concurrency control mechanisms
Secrets management architecture
Offline synchronization with conflict resolution
Operational resilience (checkpointing, recovery)
Observability integration (OpenTelemetry)
Phase 2: Acceptance Criteria Definition
Input: Architecture design v3.0, module specifications
Process:
Identify all modules (typically 10-15 for complex systems)
For each module, define acceptance criteria at 4 levels:
Function level: Individual method behavior
Class level: Class responsibilities and contracts
Module level: Module integration and dependencies
System level: End-to-end workflows
Output: Acceptance criteria document (1,900+ lines, 350+ criteria)
Template: Use templates/acceptance_criteria_template.md
Criteria Format:
Plain Text
namespace {Namespace};

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// {XML documentation summary}
/// </summary>
public class {ClassName} : {InterfaceName}
{
    private readonly {DependencyType} _{dependencyName};
    private readonly ILogger<{ClassName}> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="{ClassName}"/> class.
    /// </summary>
    /// <param name="{dependencyName}">{Dependency description}</param>
    /// <param name="logger">Logger instance.</param>
    public {ClassName}(
        {DependencyType} {dependencyName},
        ILogger<{ClassName}> logger)
    {
        _{dependencyName} = {dependencyName} ?? throw new ArgumentNullException(nameof({dependencyName}));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// {Method documentation}
    /// </summary>
    public async Task<{ReturnType}> {MethodName}(
        {ParamType} {paramName},
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{MethodName} called with {ParamName}: {ParamValue}",
            nameof({MethodName}), nameof({paramName}), {paramName});

        try
        {
            // Implementation
            {MethodBody}

            _logger.LogInformation("{MethodName} completed successfully");
            return {result};
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{MethodName} failed");
            throw;
        }
    }
}
Phase 3: Code Specifications
Input: Acceptance criteria, architecture design
Process:
For each module, document all classes with:
Complete interface definitions
Full class implementations with XML documentation
Implementation details table (performance, thread safety, etc.)
Configuration examples
Usage examples
Acceptance criteria mapping
Output: Implementation guide Part 1 (2,000+ lines covering all modules)
Class Specification Template:
Markdown
{
  "{SectionName}": {
    "{SettingName}": "{DefaultValue}",
    "{SettingName}": {DefaultValue}
  }
}
Implementation Details:
Aspect
Specification
Performance
<500ms per operation
Thread Safety
Thread-safe with SemaphoreSlim
Caching
In-memory with 1-hour TTL
Configuration:
JSON
// Configure services
var services = new ServiceCollection();
services.Add{ServiceName}(config =>
{
    config.{SettingName} = "{Value}";
});

var serviceProvider = services.BuildServiceProvider();

// Use service
var {serviceName} = serviceProvider.GetRequiredService<{InterfaceName}>();
var result = await {serviceName}.{MethodName}({arguments});
Acceptance Criteria:
✅ AC-X.Y.1: {Criterion 1}
✅ AC-X.Y.2: {Criterion 2}
Plain Text
{
  "method": "{AuthMethod}",
  "credentials": {
    "{credentialField}": "{value}"
  }
}
Phase 5: Deployment & Configuration
Input: Code specifications, API documentation
Process:
Write step-by-step deployment guide:
Prerequisites
Installation steps
Configuration
Deployment options (Windows Service, Docker, Kubernetes)
Document all configuration options in reference table
Create security hardening guide
Output: Implementation guide Part 3 (800+ lines)
Configuration Table Format:
Setting
Type
Default
Description
Auth.Method
enum
PAT
Authentication method
Phase 6: Testing Strategy
Input: Acceptance criteria, code specifications
Process:
Plan unit tests (typically 200+ tests for 95% coverage)
Plan integration tests (typically 80+ tests)
Plan system tests (typically 10+ tests)
Define performance testing scenarios
Map tests to acceptance criteria
Output: Implementation guide Part 4 (600+ lines)
Test Plan Structure:
Unit tests by module
Integration test scenarios
System test workflows
Performance benchmarks
Test coverage targets
Phase 7: Operational Procedures
Input: All previous documentation
Process:
Create troubleshooting guide:
Common issues with symptoms and solutions
Diagnostic procedures
Write operational runbook:
Daily operations (health checks, log review)
Weekly operations (performance analysis, updates)
Monthly operations (capacity planning, audits)
Define monitoring and alerting:
Key metrics with thresholds
Alert configuration
Dashboard specifications
Output: Implementation guide Part 5 (500+ lines)
Document Structure
Complete implementation guide structure:
Markdown
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-02-21T00:00:00Z"
}
Quality Standards
Documentation Metrics
Target metrics for production-ready documentation:
Metric
Target
Example (Phase 3)
Total lines
10,000+
11,713
Total words
40,000+
47,759
Modules documented
10+
13
Classes specified
40+
45
Acceptance criteria
300+
355
Automated tests
300+
318
API endpoints
15+
20
Configuration options
25+
30
Documentation Completeness Checklist
✅ Architecture design with all refinements
✅ Acceptance criteria at 4 levels (function, class, module, system)
✅ Complete source code for all classes
✅ Implementation details tables (performance, thread safety, caching)
✅ Configuration examples for all components
✅ Usage examples for all APIs
✅ REST API documentation with request/response examples
✅ SDK examples in multiple languages
✅ Step-by-step deployment guide
✅ Complete configuration reference
✅ Security hardening procedures
✅ Test plan with coverage mapping
✅ Troubleshooting guide with solutions
✅ Operational runbook (daily/weekly/monthly)
✅ Monitoring and alerting specifications
Implementation Roadmap
After documentation is complete, follow this implementation sequence:
Week 1-2: Critical Foundations
Implement core modules (authentication, concurrency, secrets)
Write unit tests (target: 60 tests)
Verify acceptance criteria for critical modules
Week 3-4: Advanced Features
Implement feature modules (work items, test plans, Git)
Write integration tests (target: 30 tests)
Verify module integration
Week 5-6: Operational Excellence
Implement resilience and observability
Write system tests (target: 5 tests)
Performance testing and optimization
Week 7-8: Final Integration
Complete remaining modules
Full test suite execution (target: 95% coverage)
Deployment to staging environment
Production deployment
Tips for Success
Writing Efficient Documentation
Start with architecture review responses - Address all feedback comprehensively before updating architecture
Define acceptance criteria early - Prevents scope creep and ensures testability
Write complete source code - Not pseudocode; actual compilable code with XML documentation
Include real examples - Configuration and usage examples should be copy-paste ready
Map tests to criteria - Every acceptance criterion should have corresponding tests
Common Pitfalls to Avoid
Incomplete specifications - Missing implementation details, configuration, or examples
Vague acceptance criteria - Criteria must be testable and measurable
Insufficient test coverage - Target 95%+ for production systems
Missing operational procedures - Troubleshooting and monitoring are critical
Skipping security hardening - Document security best practices upfront
Collaboration with Stakeholders
Review architecture responses - Get approval before updating architecture
Validate acceptance criteria - Ensure criteria match stakeholder expectations
Share API documentation early - Get feedback on API design before implementation
Test deployment procedures - Verify deployment guide works in target environment
Iterate on operational procedures - Refine based on real-world usage
Reference Documents
For detailed examples and templates, see:
references/phase3_example.md - Complete Phase 3 example with all 7 phases
templates/acceptance_criteria_template.md - Acceptance criteria template
templates/implementation_guide_template.md - Implementation guide template
Summary
The documentation-first approach ensures:
✅ Complete specifications before code implementation
✅ Testable acceptance criteria at all levels
✅ Production-ready documentation for deployment and operations
✅ 95%+ test coverage with comprehensive test plans
✅ Reduced implementation time (clear specifications prevent rework)
✅ Better stakeholder alignment (review documentation before coding)
Estimated time savings: 30-40% reduction in total project time by preventing rework and clarifying requirements upfront.
