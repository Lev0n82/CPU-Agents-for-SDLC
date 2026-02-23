import Layout from "@/components/Layout";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Link } from "wouter";
import {
  CheckCircle2,
  ArrowRight,
  TestTube2,
  GitBranch,
  Database,
  FolderGit2,
  Code2,
  Zap,
} from "lucide-react";
import { SEO } from '@/components/SEO';

export default function Phase32() {
  const modules = [
    {
      icon: TestTube2,
      title: "Test Plan Service",
      description: "Complete test case lifecycle management with Azure DevOps Test Plans integration",
      classes: 4,
      acceptanceCriteria: 78,
      features: [
        "Create and configure test cases with steps, expected results, and attachments",
        "Execute test cases and record outcomes (Passed, Failed, Blocked, Not Applicable)",
        "Link test cases to requirements for traceability",
        "Query test cases by suite, plan, or custom filters",
        "Update test case properties and configurations",
      ],
      keyCapabilities: [
        "Full CRUD operations for test cases",
        "Test execution result recording",
        "Requirement linkage for traceability",
        "Test suite and plan management",
        "Attachment handling for evidence",
      ],
    },
    {
      icon: GitBranch,
      title: "Git Service",
      description: "LibGit2Sharp integration for local Git repository operations",
      classes: 3,
      acceptanceCriteria: 65,
      features: [
        "Clone repositories with authentication (PAT, SSH, Certificate)",
        "Commit changes with author information and messages",
        "Push commits to remote repositories",
        "Pull latest changes from remote",
        "Branch management (create, switch, merge)",
      ],
      keyCapabilities: [
        "Multi-authentication support",
        "Conflict detection and reporting",
        "Credential management integration",
        "Progress reporting for long operations",
        "Error handling with retry logic",
      ],
    },
    {
      icon: Database,
      title: "Offline Synchronization",
      description: "SQLite-based caching for reliable operation during network outages",
      classes: 5,
      acceptanceCriteria: 92,
      features: [
        "Automatic caching of work items, test cases, and metadata",
        "Queue updates for offline modifications",
        "Synchronize queued changes when connectivity restored",
        "4 conflict resolution policies (Server Wins, Client Wins, Manual, Timestamp)",
        "Configurable sync intervals and retry policies",
      ],
      keyCapabilities: [
        "SQLite database with EF Core",
        "Automatic cache invalidation",
        "Conflict detection and resolution",
        "Batch synchronization for efficiency",
        "Telemetry for sync operations",
      ],
    },
    {
      icon: FolderGit2,
      title: "Git Workspace Management",
      description: "Persistent workspace management with dependency caching",
      classes: 3,
      acceptanceCriteria: 47,
      features: [
        "Create and manage persistent Git workspaces",
        "Cache NuGet packages and build artifacts",
        "Workspace cleanup and maintenance",
        "Disk space monitoring and alerts",
        "Workspace isolation for parallel operations",
      ],
      keyCapabilities: [
        "Configurable workspace root directory",
        "Automatic cleanup of stale workspaces",
        "Dependency caching for faster builds",
        "Workspace locking for concurrency",
        "Telemetry for workspace operations",
      ],
    },
  ];

  const integrationPoints = [
    {
      title: "Azure DevOps Test Plans",
      description: "Native integration with Azure Test Plans API for test case management",
      endpoints: [
        "Create test case",
        "Execute test case",
        "Record test result",
        "Link to requirement",
        "Query test suites",
      ],
    },
    {
      title: "LibGit2Sharp",
      description: "Native Git operations without external dependencies",
      operations: [
        "Clone repository",
        "Commit changes",
        "Push to remote",
        "Pull from remote",
        "Branch management",
      ],
    },
    {
      title: "SQLite Database",
      description: "Local database for offline caching and synchronization",
      tables: [
        "WorkItems",
        "TestCases",
        "SyncQueue",
        "ConflictLog",
        "Metadata",
      ],
    },
  ];

  const performanceMetrics = [
    { metric: "Test Case Creation", value: "<2s", description: "Average time to create test case" },
    { metric: "Git Clone", value: "<30s", description: "Clone 100MB repository" },
    { metric: "Offline Sync", value: "<5s", description: "Sync 100 queued updates" },
    { metric: "Workspace Setup", value: "<10s", description: "Create new workspace with cache" },
  ];

  return (
    <Layout>
      <SEO
        title="Phase 3.2 - Core Services"
        description="Phase 3.2 implementation: Test Plan Service, Git Service, Offline Synchronization, and Git Workspace Management with 15 classes and 282 acceptance criteria for local CPU-based autonomous agents"
        keywords="Phase 3.2, test plans, Git integration, offline sync, workspace management, LibGit2Sharp, SQLite, Azure Test Plans, local AI"
      />

      {/* Hero Section */}
      <section className="relative overflow-hidden bg-background py-20">
        <div className="container">
          <div className="max-w-4xl mx-auto text-center">
            <div className="label-swiss mb-6">PHASE 3.2 // CORE SERVICES</div>
            <h1 className="text-5xl md:text-6xl font-display font-semibold text-foreground mb-6">
              Test Plans, Git, and Offline Sync
            </h1>
            <p className="text-xl text-muted-foreground mb-8 max-w-2xl mx-auto">
              Complete test lifecycle management, Git operations, and offline synchronization
              for reliable autonomous operation in any network condition.
            </p>
            <div className="flex flex-wrap gap-6 justify-center">
              <div className="data-card">
                <div className="metric">15</div>
                <div className="label-swiss mt-2">Classes</div>
              </div>
              <div className="data-card">
                <div className="metric">282</div>
                <div className="label-swiss mt-2">Acceptance Criteria</div>
              </div>
              <div className="data-card">
                <div className="metric">100%</div>
                <div className="label-swiss mt-2">Test Coverage</div>
              </div>
              <div className="data-card">
                <div className="metric">0</div>
                <div className="label-swiss mt-2">Build Errors</div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Modules Section */}
      <section className="py-20 bg-muted/30">
        <div className="container">
          <div className="max-w-6xl mx-auto">
            <h2 className="text-4xl font-display font-semibold text-center mb-4">
              Four Core Modules
            </h2>
            <p className="text-center text-muted-foreground mb-12 max-w-2xl mx-auto">
              Phase 3.2 delivers essential services for test management, Git operations,
              offline resilience, and workspace management.
            </p>

            <div className="grid md:grid-cols-2 gap-8">
              {modules.map((module, index) => (
                <Card key={index} className="p-8 hover:shadow-lg transition-shadow">
                  <div className="flex items-start gap-4 mb-6">
                    <div className="w-12 h-12 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <module.icon className="w-6 h-6 text-primary" />
                    </div>
                    <div className="flex-1">
                      <h3 className="text-2xl font-semibold mb-2">{module.title}</h3>
                      <p className="text-muted-foreground">{module.description}</p>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4 mb-6">
                    <div className="bg-muted/50 rounded-lg p-4">
                      <div className="text-2xl font-bold text-primary">{module.classes}</div>
                      <div className="text-sm text-muted-foreground">Classes</div>
                    </div>
                    <div className="bg-muted/50 rounded-lg p-4">
                      <div className="text-2xl font-bold text-primary">{module.acceptanceCriteria}</div>
                      <div className="text-sm text-muted-foreground">Acceptance Criteria</div>
                    </div>
                  </div>

                  <div className="space-y-3 mb-6">
                    <div className="font-semibold text-sm uppercase tracking-wide text-muted-foreground">
                      Features
                    </div>
                    {module.features.map((feature, idx) => (
                      <div key={idx} className="flex items-start gap-2">
                        <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                        <span className="text-sm text-muted-foreground">{feature}</span>
                      </div>
                    ))}
                  </div>

                  <div className="border-t pt-6">
                    <div className="font-semibold text-sm uppercase tracking-wide text-muted-foreground mb-3">
                      Key Capabilities
                    </div>
                    <div className="flex flex-wrap gap-2">
                      {module.keyCapabilities.map((capability, idx) => (
                        <span
                          key={idx}
                          className="px-3 py-1 bg-primary/10 text-primary text-xs rounded-full"
                        >
                          {capability}
                        </span>
                      ))}
                    </div>
                  </div>
                </Card>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Integration Points */}
      <section className="py-20 bg-background">
        <div className="container">
          <div className="max-w-6xl mx-auto">
            <h2 className="text-4xl font-display font-semibold text-center mb-4">
              Integration Points
            </h2>
            <p className="text-center text-muted-foreground mb-12 max-w-2xl mx-auto">
              Phase 3.2 integrates with Azure DevOps, LibGit2Sharp, and SQLite for
              comprehensive SDLC automation.
            </p>

            <div className="grid md:grid-cols-3 gap-8">
              {integrationPoints.map((point, index) => (
                <Card key={index} className="p-6">
                  <h3 className="text-xl font-semibold mb-3">{point.title}</h3>
                  <p className="text-sm text-muted-foreground mb-4">{point.description}</p>
                  <div className="space-y-2">
                    {(point.endpoints || point.operations || point.tables)?.map((item, idx) => (
                      <div key={idx} className="flex items-center gap-2">
                        <Code2 className="w-4 h-4 text-primary flex-shrink-0" />
                        <span className="text-sm">{item}</span>
                      </div>
                    ))}
                  </div>
                </Card>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Performance Metrics */}
      <section className="py-20 bg-muted/30">
        <div className="container">
          <div className="max-w-4xl mx-auto">
            <h2 className="text-4xl font-display font-semibold text-center mb-4">
              Performance Metrics
            </h2>
            <p className="text-center text-muted-foreground mb-12">
              Phase 3.2 delivers fast, reliable operations for all core services.
            </p>

            <div className="grid md:grid-cols-2 gap-6">
              {performanceMetrics.map((metric, index) => (
                <Card key={index} className="p-6 flex items-center gap-4">
                  <Zap className="w-8 h-8 text-primary flex-shrink-0" />
                  <div className="flex-1">
                    <div className="font-semibold mb-1">{metric.metric}</div>
                    <div className="text-sm text-muted-foreground">{metric.description}</div>
                  </div>
                  <div className="text-2xl font-bold text-primary">{metric.value}</div>
                </Card>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 bg-background">
        <div className="container">
          <div className="max-w-4xl mx-auto text-center">
            <h2 className="text-4xl font-display font-semibold mb-6">
              Ready to Explore Phase 3.3?
            </h2>
            <p className="text-xl text-muted-foreground mb-8">
              Continue to Phase 3.3 for operational resilience, observability, and performance optimization.
            </p>
            <div className="flex flex-wrap gap-4 justify-center">
              <Link href="/phase-3-3">
                <Button size="lg" className="bg-primary text-primary-foreground hover:bg-primary/90 group">
                  View Phase 3.3
                  <ArrowRight className="w-5 h-5 ml-2 group-hover:translate-x-1 transition-transform" />
                </Button>
              </Link>
              <Link href="/documentation">
                <Button size="lg" variant="outline" className="border-2">
                  View Documentation
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </section>
    </Layout>
  );
}
