import Layout from "@/components/Layout";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Link } from "wouter";
import {
  CheckCircle2,
  ArrowRight,
  TestTube2,
  RefreshCw,
  FileCode2,
  Brain,
} from "lucide-react";
import { SEO } from '@/components/SEO';

export default function Phase34() {
  const modules = [
    {
      icon: TestTube2,
      title: "Test Case Lifecycle Management",
      description: "Automatic detection and management of obsolete test cases",
      classes: 5,
      acceptanceCriteria: 87,
      features: [
        "Detect obsolete test cases based on requirement changes",
        "Pattern-based analysis (removed requirements, deprecated features)",
        "Confidence scoring for obsolescence detection",
        "Automatic test case closure with audit trail",
        "Notification to stakeholders for manual review",
      ],
      detectionStrategies: [
        {
          name: "Requirement Removal",
          description: "Test cases linked to removed requirements",
          confidence: "High (95%)",
        },
        {
          name: "Feature Deprecation",
          description: "Test cases for deprecated features",
          confidence: "High (90%)",
        },
        {
          name: "Long-term Failure",
          description: "Test cases failing for >30 days",
          confidence: "Medium (70%)",
        },
        {
          name: "No Execution",
          description: "Test cases not executed in >90 days",
          confidence: "Low (50%)",
        },
      ],
    },
    {
      icon: RefreshCw,
      title: "Migration Tooling",
      description: "Phase 2 to Phase 3 migration with analysis and rollback",
      classes: 5,
      acceptanceCriteria: 76,
      features: [
        "Analyze Phase 2 configuration and identify breaking changes",
        "Generate migration report with required actions",
        "Automated migration with data preservation",
        "Rollback capability for failed migrations",
        "Post-migration validation and testing",
      ],
      migrationSteps: [
        {
          step: "Pre-Migration Analysis",
          description: "Analyze Phase 2 configuration and dependencies",
          duration: "5-10 minutes",
        },
        {
          step: "Backup Creation",
          description: "Create full backup of Phase 2 configuration and data",
          duration: "2-5 minutes",
        },
        {
          step: "Configuration Migration",
          description: "Migrate settings to Phase 3 format",
          duration: "1-2 minutes",
        },
        {
          step: "Data Migration",
          description: "Migrate work items, test cases, and metadata",
          duration: "10-30 minutes",
        },
        {
          step: "Post-Migration Validation",
          description: "Verify all services and run smoke tests",
          duration: "5-10 minutes",
        },
      ],
    },
  ];

  const migrationChecklist = [
    {
      category: "Pre-Migration",
      items: [
        "Backup Phase 2 configuration and data",
        "Review migration report and breaking changes",
        "Schedule maintenance window",
        "Notify stakeholders",
      ],
    },
    {
      category: "Migration",
      items: [
        "Run migration tool with --dry-run flag",
        "Review migration plan",
        "Execute migration",
        "Monitor migration progress",
      ],
    },
    {
      category: "Post-Migration",
      items: [
        "Verify all services are running",
        "Run integration tests",
        "Check Grafana dashboards",
        "Validate work item processing",
      ],
    },
    {
      category: "Rollback (if needed)",
      items: [
        "Stop Phase 3 services",
        "Restore Phase 2 backup",
        "Verify Phase 2 functionality",
        "Analyze migration failure logs",
      ],
    },
  ];

  const testLifecycleFlow = [
    { stage: "Creation", description: "Test case created and linked to requirement" },
    { stage: "Active", description: "Test case executed regularly with pass/fail results" },
    { stage: "Obsolescence Detection", description: "Automatic analysis detects potential obsolescence" },
    { stage: "Review", description: "Stakeholder review and approval for closure" },
    { stage: "Closure", description: "Test case closed with audit trail and reason" },
  ];

  return (
    <Layout>
      <SEO
        title="Phase 3.4 - Migration & Deployment"
        description="Phase 3.4 implementation: Test Case Lifecycle Management and Migration Tooling with 10 classes and 163 acceptance criteria for Phase 2 to Phase 3 migration with local AI-powered obsolescence detection"
        keywords="Phase 3.4, test lifecycle, migration, obsolete tests, Phase 2 to Phase 3, rollback, deployment, automation, local AI, CPU models"
      />

      {/* Hero Section */}
      <section className="relative overflow-hidden bg-background py-20">
        <div className="container">
          <div className="max-w-4xl mx-auto text-center">
            <div className="label-swiss mb-6">PHASE 3.4 // MIGRATION & DEPLOYMENT</div>
            <h1 className="text-5xl md:text-6xl font-display font-semibold text-foreground mb-6">
              Test Lifecycle & Migration
            </h1>
            <p className="text-xl text-muted-foreground mb-8 max-w-2xl mx-auto">
              Automatic test case lifecycle management and comprehensive migration tooling
              for seamless Phase 2 to Phase 3 upgrades.
            </p>
            <div className="flex flex-wrap gap-6 justify-center">
              <div className="data-card">
                <div className="metric">10</div>
                <div className="label-swiss mt-2">Classes</div>
              </div>
              <div className="data-card">
                <div className="metric">163</div>
                <div className="label-swiss mt-2">Acceptance Criteria</div>
              </div>
              <div className="data-card">
                <div className="metric">4</div>
                <div className="label-swiss mt-2">Detection Strategies</div>
              </div>
              <div className="data-card">
                <div className="metric">5</div>
                <div className="label-swiss mt-2">Migration Steps</div>
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
              Two Core Modules
            </h2>
            <p className="text-center text-muted-foreground mb-12 max-w-2xl mx-auto">
              Phase 3.4 delivers intelligent test lifecycle management and comprehensive
              migration tooling for production deployments.
            </p>

            <div className="space-y-8">
              {modules.map((module, index) => (
                <Card key={index} className="p-8">
                  <div className="flex items-start gap-4 mb-6">
                    <div className="w-12 h-12 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <module.icon className="w-6 h-6 text-primary" />
                    </div>
                    <div className="flex-1">
                      <h3 className="text-2xl font-semibold mb-2">{module.title}</h3>
                      <p className="text-muted-foreground">{module.description}</p>
                    </div>
                    <div className="flex gap-4">
                      <div className="bg-muted/50 rounded-lg p-4 text-center">
                        <div className="text-2xl font-bold text-primary">{module.classes}</div>
                        <div className="text-xs text-muted-foreground">Classes</div>
                      </div>
                      <div className="bg-muted/50 rounded-lg p-4 text-center">
                        <div className="text-2xl font-bold text-primary">{module.acceptanceCriteria}</div>
                        <div className="text-xs text-muted-foreground">Criteria</div>
                      </div>
                    </div>
                  </div>

                  {module.detectionStrategies && (
                    <div className="mb-6">
                      <div className="font-semibold text-sm uppercase tracking-wide text-muted-foreground mb-4">
                        Detection Strategies
                      </div>
                      <div className="grid md:grid-cols-2 gap-4">
                        {module.detectionStrategies.map((strategy, idx) => (
                          <div key={idx} className="bg-muted/30 rounded-lg p-4">
                            <div className="font-semibold mb-2 flex items-center gap-2">
                              <Brain className="w-4 h-4 text-primary" />
                              {strategy.name}
                            </div>
                            <p className="text-sm text-muted-foreground mb-2">{strategy.description}</p>
                            <div className="text-xs text-primary font-mono">{strategy.confidence}</div>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  {module.migrationSteps && (
                    <div className="mb-6">
                      <div className="font-semibold text-sm uppercase tracking-wide text-muted-foreground mb-4">
                        Migration Steps
                      </div>
                      <div className="space-y-3">
                        {module.migrationSteps.map((step, idx) => (
                          <div key={idx} className="flex items-start gap-4 bg-muted/30 rounded-lg p-4">
                            <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                              <span className="text-primary font-bold text-sm">{idx + 1}</span>
                            </div>
                            <div className="flex-1">
                              <div className="font-semibold mb-1">{step.step}</div>
                              <p className="text-sm text-muted-foreground">{step.description}</p>
                            </div>
                            <div className="text-xs text-primary font-mono whitespace-nowrap">
                              {step.duration}
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  <div className="border-t pt-6">
                    <div className="font-semibold text-sm uppercase tracking-wide text-muted-foreground mb-3">
                      Key Features
                    </div>
                    <div className="grid md:grid-cols-2 gap-3">
                      {module.features.map((feature, idx) => (
                        <div key={idx} className="flex items-start gap-2">
                          <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                          <span className="text-sm text-muted-foreground">{feature}</span>
                        </div>
                      ))}
                    </div>
                  </div>
                </Card>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Test Lifecycle Flow */}
      <section className="py-20 bg-background">
        <div className="container">
          <div className="max-w-4xl mx-auto">
            <h2 className="text-4xl font-display font-semibold text-center mb-4">
              Test Case Lifecycle
            </h2>
            <p className="text-center text-muted-foreground mb-12">
              Automatic lifecycle management from creation to closure with intelligent obsolescence detection.
            </p>

            <div className="relative">
              {testLifecycleFlow.map((stage, index) => (
                <div key={index} className="flex items-start gap-4 mb-6 last:mb-0">
                  <div className="relative">
                    <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0 border-2 border-primary">
                      <span className="text-primary font-bold">{index + 1}</span>
                    </div>
                    {index < testLifecycleFlow.length - 1 && (
                      <div className="absolute left-6 top-12 w-0.5 h-6 bg-primary/30" />
                    )}
                  </div>
                  <Card className="flex-1 p-6">
                    <h3 className="text-xl font-semibold mb-2">{stage.stage}</h3>
                    <p className="text-muted-foreground">{stage.description}</p>
                  </Card>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Migration Checklist */}
      <section className="py-20 bg-muted/30">
        <div className="container">
          <div className="max-w-6xl mx-auto">
            <h2 className="text-4xl font-display font-semibold text-center mb-4">
              Migration Checklist
            </h2>
            <p className="text-center text-muted-foreground mb-12">
              Complete checklist for Phase 2 to Phase 3 migration with rollback procedures.
            </p>

            <div className="grid md:grid-cols-2 gap-8">
              {migrationChecklist.map((section, index) => (
                <Card key={index} className="p-6">
                  <h3 className="text-xl font-semibold mb-4 flex items-center gap-2">
                    <FileCode2 className="w-5 h-5 text-primary" />
                    {section.category}
                  </h3>
                  <div className="space-y-3">
                    {section.items.map((item, idx) => (
                      <div key={idx} className="flex items-start gap-2">
                        <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                        <span className="text-sm text-muted-foreground">{item}</span>
                      </div>
                    ))}
                  </div>
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
              Ready to Deploy?
            </h2>
            <p className="text-xl text-muted-foreground mb-8">
              Explore deployment options for Windows Service, Podman, or Docker containers.
            </p>
            <div className="flex flex-wrap gap-4 justify-center">
              <Link href="/windows-deployment">
                <Button size="lg" className="bg-primary text-primary-foreground hover:bg-primary/90 group">
                  Windows Deployment
                  <ArrowRight className="w-5 h-5 ml-2 group-hover:translate-x-1 transition-transform" />
                </Button>
              </Link>
              <Link href="/podman-deployment">
                <Button size="lg" variant="outline" className="border-2">
                  Podman/Docker Deployment
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
