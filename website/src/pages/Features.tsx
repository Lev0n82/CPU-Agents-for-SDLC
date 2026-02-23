import Layout from "@/components/Layout";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { 
  Lock, Zap, Shield, Database, GitBranch, TestTube2, 
  HardDrive, Activity, BarChart3, RefreshCw, Cpu,
  CheckCircle2, AlertTriangle, TrendingUp, FileSearch,
  Code2, Target, Eye, Gauge, Users
} from "lucide-react";
import { Link } from "wouter";
import { SEO } from '@/components/SEO';

export default function Features() {
  const phaseModules = [
    {
      phase: "Phase 3.1",
      title: "Critical Foundations",
      badge: "Foundation",
      description: "Core infrastructure for authentication, concurrency control, secrets management, and work item operations",
      stats: { classes: 13, criteria: 302, tests: 23 },
      modules: [
        {
          icon: Lock,
          name: "Authentication & Authorization",
          description: "Multi-provider authentication with PAT, Certificate, and MSAL Device Code Flow",
          features: [
            "Personal Access Token (PAT) with validation",
            "X.509 Certificate-based authentication",
            "MSAL Device Code Flow for interactive auth",
            "Thread-safe token caching (85% API call reduction)",
          ]
        },
        {
          icon: Zap,
          name: "Concurrency Control",
          description: "Work item claim mechanism with ETag-based optimistic concurrency",
          features: [
            "Atomic claim/release/renew operations",
            "WIQL-based filtering for available work items",
            "ETag-based optimistic concurrency control",
            "Stale claim recovery background service",
          ]
        },
        {
          icon: Shield,
          name: "Secrets Management",
          description: "Pluggable secrets providers with Azure Key Vault, Credential Manager, and DPAPI",
          features: [
            "Azure Key Vault integration (production)",
            "Windows Credential Manager (development)",
            "DPAPI encryption (local storage)",
            "Automatic PAT rotation framework",
          ]
        },
        {
          icon: Database,
          name: "Work Item Service",
          description: "Full CRUD operations with WIQL validation and attachment handling",
          features: [
            "Complete work item CRUD operations",
            "WIQL injection prevention",
            "90%+ attachment compression",
            "Batch operations for performance",
          ]
        },
      ]
    },
    {
      phase: "Phase 3.2",
      title: "Core Services",
      badge: "Services",
      description: "Test plan management, Git integration, offline synchronization, and workspace management",
      stats: { classes: 15, criteria: 282, tests: 18 },
      modules: [
        {
          icon: TestTube2,
          name: "Test Plan Service",
          description: "Azure Test Plans integration for test case and suite management",
          features: [
            "Test case CRUD operations",
            "Test suite hierarchy management",
            "Test result tracking and reporting",
            "Automated test execution integration",
          ]
        },
        {
          icon: GitBranch,
          name: "Git Service",
          description: "LibGit2Sharp-based Git operations for repository management",
          features: [
            "Clone, pull, push, branch operations",
            "Commit and merge functionality",
            "Conflict detection and resolution",
            "Repository status and diff tracking",
          ]
        },
        {
          icon: HardDrive,
          name: "Offline Synchronization",
          description: "SQLite-based caching with conflict resolution policies",
          features: [
            "Local SQLite cache for work items",
            "4 conflict resolution policies",
            "Automatic sync on reconnection",
            "Network outage resilience",
          ]
        },
        {
          icon: Database,
          name: "Git Workspace Management",
          description: "Workspace isolation and cleanup for multi-agent scenarios",
          features: [
            "Isolated workspace per agent",
            "Automatic cleanup on completion",
            "Disk space monitoring",
            "Workspace state persistence",
          ]
        },
      ]
    },
    {
      phase: "Phase 3.3",
      title: "Operational Resilience",
      badge: "Resilience",
      description: "Production-grade resilience patterns, observability stack, and performance optimization",
      stats: { classes: 8, criteria: 164, tests: 12 },
      modules: [
        {
          icon: Activity,
          name: "Resilience Patterns",
          description: "Polly 8.x patterns for fault tolerance and reliability",
          features: [
            "Retry with exponential backoff",
            "Circuit breaker pattern",
            "Timeout and bulkhead isolation",
            "Rate limiting and throttling",
          ]
        },
        {
          icon: BarChart3,
          name: "Observability Stack",
          description: "OpenTelemetry with Grafana, Prometheus, and Jaeger",
          features: [
            "Distributed tracing with Jaeger",
            "Metrics collection with Prometheus",
            "Grafana dashboards for visualization",
            "Correlation IDs for request tracking",
          ]
        },
        {
          icon: TrendingUp,
          name: "Performance Optimization",
          description: "Caching, batching, and profiling for high throughput",
          features: [
            "In-memory and distributed caching",
            "Batch API operations",
            "Query optimization",
            "Target: 10 work items/min, <500ms latency",
          ]
        },
      ]
    },
    {
      phase: "Phase 3.4",
      title: "Migration & Deployment",
      badge: "Migration",
      description: "Test lifecycle management, obsolescence detection, and Phase 2 to Phase 3 migration tooling",
      stats: { classes: 10, criteria: 163, tests: 15 },
      modules: [
        {
          icon: RefreshCw,
          name: "Test Lifecycle Management",
          description: "Automated test obsolescence detection with local AI",
          features: [
            "AI-powered obsolescence detection (vLLM/Ollama)",
            "Test coverage analysis",
            "Automated test archival",
            "Test maintenance recommendations",
          ]
        },
        {
          icon: Cpu,
          name: "Migration Tooling",
          description: "Phase 2 to Phase 3 migration with rollback support",
          features: [
            "Automated schema migration",
            "Data transformation and validation",
            "Rollback support for failed migrations",
            "Migration progress tracking",
          ]
        },
      ]
    },
  ];

  const testingLevels = [
    {
      icon: FileSearch,
      title: "Requirements Analysis & Code Review",
      description: "AI-powered requirements traceability and code review with local models",
      details: [
        "Requirements-to-test traceability matrix generation",
        "Code review with vLLM/Ollama (Granite 4, Phi-3) for quality scoring",
        "Security vulnerability detection (OWASP Top 10)",
        "Test coverage gap analysis from requirements",
      ]
    },
    {
      icon: Code2,
      title: "Unit Testing",
      description: "Function-level and class-level automated tests with xUnit",
      details: [
        "Function-level acceptance criteria validation",
        "Class-level behavior verification",
        "Mock/stub isolation for dependencies",
        "Code coverage target: 95%+ per module",
      ]
    },
    {
      icon: Target,
      title: "Integration & System Testing",
      description: "Module integration and end-to-end system validation",
      details: [
        "Module-level integration tests (API contracts)",
        "System-level acceptance criteria validation",
        "End-to-end workflow testing (bug investigation, test execution)",
        "Azure DevOps integration testing",
      ]
    },
    {
      icon: TestTube2,
      title: "Functional Testing",
      description: "Business requirement validation and user scenario testing",
      details: [
        "User story acceptance testing",
        "Business rule validation",
        "Workflow scenario testing (3 pre-built workflows)",
        "Data validation and boundary testing",
      ]
    },
    {
      icon: Gauge,
      title: "Non-Functional Testing",
      description: "Performance, scalability, and reliability validation",
      details: [
        "Performance testing: 10+ work items/min, <500ms latency",
        "Load testing: Multi-agent concurrent execution",
        "Resilience testing: Circuit breaker, retry, timeout patterns",
        "Offline sync testing: Network outage scenarios",
      ]
    },
    {
      icon: Shield,
      title: "Security Testing",
      description: "OWASP Top 10 compliance and penetration testing",
      details: [
        "OWASP Top 10 vulnerability scanning",
        "Authentication and authorization testing",
        "Secrets management validation (Azure Key Vault, DPAPI)",
        "WIQL injection prevention testing",
      ]
    },
    {
      icon: Users,
      title: "Accessibility Testing (WCAG 2.2 AAA)",
      description: "Comprehensive accessibility compliance with automated and manual testing",
      details: [
        "WCAG 2.2 AAA certification (highest compliance level)",
        "Automated accessibility testing with axe-core",
        "Manual testing with screen readers (NVDA, JAWS)",
        "Keyboard navigation and focus management validation",
        "Color contrast and text scaling compliance",
        "Multi-resolution testing (PC and mobile)",
      ]
    },
    {
      icon: Eye,
      title: "End-to-End Testing (Playwright)",
      description: "Cross-browser automated testing with multi-resolution coverage",
      details: [
        "Playwright framework for E2E automation",
        "4 most common PC resolutions + 4 mobile resolutions",
        "Cross-browser testing (Chrome, Firefox, Edge, Safari)",
        "Visual regression testing with screenshots",
      ]
    },
  ];

  const aiCapabilities = [
    {
      icon: CheckCircle2,
      title: "Code Review Analysis",
      description: "Local AI via vLLM or Ollama (Granite 4, Phi-3) analyzes code quality, security vulnerabilities, and best practices",
    },
    {
      icon: AlertTriangle,
      title: "Test Obsolescence Detection",
      description: "Identifies outdated test cases based on code changes and coverage analysis",
    },
    {
      icon: GitBranch,
      title: "Conflict Resolution Intelligence",
      description: "Suggests merge conflict resolutions based on code context and history",
    },
    {
      icon: Activity,
      title: "Root Cause Analysis",
      description: "Diagnoses test failures and bug root causes using historical data and code analysis",
    },
  ];

  return (
    <Layout>
      <SEO
        title="Features"
        description="Complete feature overview of CPU Agents Phase 3.1-3.4: Authentication, Concurrency, Secrets, Work Items, Test Plans, Git, Offline Sync, Resilience, Observability, Performance, and Migration with local AI via vLLM or Ollama"
        keywords="features, CPU agents, authentication, concurrency, secrets management, work items, test plans, Git integration, offline sync, resilience, observability, performance, migration, local AI, vLLM, Ollama, Granite 4"
      />

      {/* Hero Section */}
      <section className="relative overflow-hidden bg-background py-20">
        <div className="container">
          <div className="max-w-4xl">
            <Badge className="mb-4 bg-primary text-primary-foreground">
              Phase 3.1-3.4 // Complete Feature Set
            </Badge>
            <h1 className="text-4xl md:text-5xl font-display font-bold text-foreground mb-6">
              Complete Feature Overview
            </h1>
            <p className="text-lg text-muted-foreground mb-8 leading-relaxed">
              45 classes across 4 phases delivering authentication, concurrency control, secrets management, 
              work item operations, test plans, Git integration, offline synchronization, production resilience, 
              complete observability, performance optimization, and migration tooling with local AI decision-making.
            </p>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <Card className="p-4 border-2 border-border text-center">
                <div className="text-3xl font-display font-bold text-primary mb-1">45</div>
                <div className="text-sm text-muted-foreground">Classes</div>
              </Card>
              <Card className="p-4 border-2 border-border text-center">
                <div className="text-3xl font-display font-bold text-primary mb-1">302</div>
                <div className="text-sm text-muted-foreground">Acceptance Criteria</div>
              </Card>
              <Card className="p-4 border-2 border-border text-center">
                <div className="text-3xl font-display font-bold text-primary mb-1">68</div>
                <div className="text-sm text-muted-foreground">Tests</div>
              </Card>
              <Card className="p-4 border-2 border-border text-center">
                <div className="text-3xl font-display font-bold text-primary mb-1">4</div>
                <div className="text-sm text-muted-foreground">Phases</div>
              </Card>
            </div>
          </div>
        </div>
      </section>

      {/* AI Capabilities Section */}
      <section className="py-16 bg-card">
        <div className="container">
          <div className="mb-12">
            <Badge className="mb-4 bg-primary text-primary-foreground">
              Local AI Integration
            </Badge>
            <h2 className="text-3xl md:text-4xl font-display font-bold text-foreground mb-4">
              AI Decision-Making Capabilities
            </h2>
            <p className="text-lg text-muted-foreground max-w-3xl">
              Local CPU-based AI via vLLM (production) or Ollama (development) with Granite 4, Phi-3, Llama 3 for intelligent code analysis 
              and decision-making. Zero cloud dependencies, complete data privacy.
            </p>
          </div>
          <div className="grid md:grid-cols-2 gap-6">
            {aiCapabilities.map((capability, index) => {
              const Icon = capability.icon;
              return (
                <Card key={index} className="p-6 border-2 border-primary/20 hover:border-primary/50 transition-colors">
                  <div className="flex items-start gap-4">
                    <div className="w-12 h-12 bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <Icon className="w-6 h-6 text-primary" />
                    </div>
                    <div>
                      <h3 className="text-lg font-display font-semibold text-foreground mb-2">
                        {capability.title}
                      </h3>
                      <p className="text-sm text-muted-foreground">
                        {capability.description}
                      </p>
                    </div>
                  </div>
                </Card>
              );
            })}
          </div>
        </div>
      </section>

      {/* Phase Modules */}
      {phaseModules.map((phase, phaseIndex) => (
        <section key={phaseIndex} className={phaseIndex % 2 === 0 ? "py-16 bg-background" : "py-16 bg-card"}>
          <div className="container">
            <div className="mb-12">
              <Badge className="mb-4 bg-primary text-primary-foreground">
                {phase.phase} // {phase.badge}
              </Badge>
              <h2 className="text-3xl md:text-4xl font-display font-bold text-foreground mb-4">
                {phase.title}
              </h2>
              <p className="text-lg text-muted-foreground max-w-3xl mb-6">
                {phase.description}
              </p>
              <div className="flex items-center gap-6 text-sm text-muted-foreground">
                <span><strong className="text-foreground">{phase.stats.classes}</strong> classes</span>
                <span>•</span>
                <span><strong className="text-foreground">{phase.stats.criteria}</strong> acceptance criteria</span>
                <span>•</span>
                <span><strong className="text-foreground">{phase.stats.tests}</strong> tests</span>
              </div>
            </div>
            <div className="grid md:grid-cols-2 gap-6">
              {phase.modules.map((module, moduleIndex) => {
                const Icon = module.icon;
                return (
                  <Card key={moduleIndex} className="p-6 border-2 border-border hover:border-primary/50 transition-colors">
                    <div className="flex items-start gap-4 mb-4">
                      <div className="w-12 h-12 bg-primary/10 flex items-center justify-center flex-shrink-0">
                        <Icon className="w-6 h-6 text-primary" />
                      </div>
                      <div className="flex-1">
                        <h3 className="text-xl font-display font-semibold text-foreground mb-2">
                          {module.name}
                        </h3>
                        <p className="text-sm text-muted-foreground mb-4">
                          {module.description}
                        </p>
                      </div>
                    </div>
                    <ul className="space-y-2">
                      {module.features.map((feature, featureIndex) => (
                        <li key={featureIndex} className="flex items-start gap-2 text-sm text-muted-foreground">
                          <CheckCircle2 className="w-4 h-4 text-primary flex-shrink-0 mt-0.5" />
                          <span>{feature}</span>
                        </li>
                      ))}
                    </ul>
                  </Card>
                );
              })}
            </div>
          </div>
        </section>
      ))}

      {/* Testing & Quality Assurance Section */}
      <section className="py-16 bg-card">
        <div className="container">
          <div className="mb-12">
            <Badge className="mb-4 bg-primary text-primary-foreground">
              Testing & Quality Assurance
            </Badge>
            <h2 className="text-3xl md:text-4xl font-display font-bold text-foreground mb-4">
              Comprehensive Test Coverage Development
            </h2>
            <p className="text-lg text-muted-foreground max-w-3xl mb-6">
              Multi-level automated testing from requirements analysis through WCAG 2.2 AAA certification. 
              Built-in self-testing functionality with acceptance criteria validation at function, class, module, and system levels.
            </p>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <Card className="p-4 border-2 border-primary/20 text-center">
                <div className="text-2xl font-display font-bold text-primary mb-1">8</div>
                <div className="text-xs text-muted-foreground">Test Types</div>
              </Card>
              <Card className="p-4 border-2 border-primary/20 text-center">
                <div className="text-2xl font-display font-bold text-primary mb-1">95%+</div>
                <div className="text-xs text-muted-foreground">Code Coverage</div>
              </Card>
              <Card className="p-4 border-2 border-primary/20 text-center">
                <div className="text-2xl font-display font-bold text-primary mb-1">AAA</div>
                <div className="text-xs text-muted-foreground">WCAG 2.2</div>
              </Card>
              <Card className="p-4 border-2 border-primary/20 text-center">
                <div className="text-2xl font-display font-bold text-primary mb-1">4</div>
                <div className="text-xs text-muted-foreground">Test Levels</div>
              </Card>
            </div>
          </div>
          <div className="grid md:grid-cols-2 gap-6">
            {testingLevels.map((level, index) => {
              const Icon = level.icon;
              return (
                <Card key={index} className="p-6 border-2 border-border hover:border-primary/50 transition-colors">
                  <div className="flex items-start gap-4 mb-4">
                    <div className="w-12 h-12 bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <Icon className="w-6 h-6 text-primary" />
                    </div>
                    <div className="flex-1">
                      <h3 className="text-lg font-display font-semibold text-foreground mb-2">
                        {level.title}
                      </h3>
                      <p className="text-sm text-muted-foreground mb-4">
                        {level.description}
                      </p>
                    </div>
                  </div>
                  <ul className="space-y-2">
                    {level.details.map((detail, detailIndex) => (
                      <li key={detailIndex} className="flex items-start gap-2 text-sm text-muted-foreground">
                        <CheckCircle2 className="w-4 h-4 text-primary flex-shrink-0 mt-0.5" />
                        <span>{detail}</span>
                      </li>
                    ))}
                  </ul>
                </Card>
              );
            })}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-16 bg-primary/5">
        <div className="container">
          <div className="max-w-3xl mx-auto text-center">
            <h2 className="text-3xl md:text-4xl font-display font-bold text-foreground mb-4">
              Ready to Deploy?
            </h2>
            <p className="text-lg text-muted-foreground mb-8">
              Explore the complete architecture or jump straight into deployment with our quick start guide.
            </p>
            <div className="flex flex-wrap justify-center gap-4">
              <Link href="/architecture">
                <Button size="lg" className="bg-primary text-primary-foreground hover:bg-primary/90">
                  View Architecture
                </Button>
              </Link>
              <Link href="/quick-start">
                <Button size="lg" variant="outline" className="border-2 border-primary text-primary hover:bg-primary hover:text-primary-foreground">
                  Quick Start Guide
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </section>
    </Layout>
  );
}
