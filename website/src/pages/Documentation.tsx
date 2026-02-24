import Layout from "@/components/Layout";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Download, GitBranch, FileText, Presentation, CheckCircle2 } from "lucide-react";
import { SEO } from '@/components/SEO';

export default function Documentation() {
  const aiDocuments = [
    {
      title: "AI Decision Module Guide",
      description: "Local CPU-based AI via Ollama (Granite 4, Phi-3, Llama 3) for code review, test obsolescence detection, conflict resolution, and root cause analysis with structured prompts and confidence scoring.",
      size: "New",
      lines: "Complete",
      pdfPath: "/docs/ai-decision-module-guide.pdf",
      githubPath: "https://github.com/Lev0n82/CPU-Agents-for-SDLC/blob/main/docs/ai_decision_module.md",
      icon: FileText,
      badge: "Local AI Integration"
    },
    {
      title: "OpenTelemetry Deployment Guide",
      description: "Complete observability stack deployment with Grafana dashboards, Prometheus metrics, Jaeger tracing, and OpenTelemetry Collector configuration for local Podman deployment.",
      size: "New",
      lines: "Complete",
      pdfPath: "/docs/opentelemetry-deployment.pdf",
      githubPath: "https://github.com/Lev0n82/CPU-Agents-for-SDLC/blob/main/docs/opentelemetry_deployment.md",
      icon: FileText,
      badge: "Observability"
    },
    {
      title: "Agent Host Application Guide",
      description: "Autonomous polling loop, workflow engine, 11 built-in actions, and 3 pre-built workflows (bug investigation, test execution, code review) with JSON workflow definitions.",
      size: "New",
      lines: "Complete",
      pdfPath: "/docs/agent-host-guide.pdf",
      githubPath: "https://github.com/Lev0n82/CPU-Agents-for-SDLC/blob/main/docs/agent_host_application.md",
      icon: FileText,
      badge: "Agent Host"
    },
  ];

  const documents = [
    {
      title: "Phase 3.1 Implementation Guide",
      description: "Complete code specifications for all 13 modules with detailed class-by-class implementation guides, configuration examples, and usage patterns.",
      size: "788KB",
      lines: "3,869 lines",
      pdfPath: "/docs/phase3-implementation-guide.pdf",
      githubPath: "https://github.com/Lev0n82/CPU-Agents-for-SDLC/blob/main/docs/phase3_implementation_guide.md",
      icon: FileText,
      badge: "Implementation"
    },
    {
      title: "Phase 3.1 Acceptance Criteria",
      description: "355 acceptance criteria defined at function, class, module, and system levels. Complete validation framework for quality assurance.",
      size: "632KB",
      lines: "1,935 lines",
      pdfPath: "/docs/phase3-acceptance-criteria.pdf",
      githubPath: "https://github.com/Lev0n82/CPU-Agents-for-SDLC/blob/main/docs/phase3_acceptance_criteria.md",
      icon: CheckCircle2,
      badge: "Quality"
    },
    {
      title: "Phase 3.1 Architecture Design v3.0",
      description: "Enterprise architecture specification with 13 modules, concurrency control, secrets management, offline synchronization, and operational resilience.",
      size: "915KB",
      lines: "4,028 lines",
      pdfPath: "/docs/phase3-architecture-design-v3.pdf",
      githubPath: "https://github.com/Lev0n82/CPU-Agents-for-SDLC/blob/main/docs/phase3_architecture_design_v3.md",
      icon: FileText,
      badge: "Architecture"
    },
    {
      title: "Phase 3.1 Architecture Review Response",
      description: "Comprehensive response to architecture review feedback addressing concurrency control, secrets management, conflict resolution, and operational resilience.",
      size: "593KB",
      lines: "1,881 lines",
      pdfPath: "/docs/phase3-architecture-review-response.pdf",
      githubPath: "https://github.com/Lev0n82/CPU-Agents-for-SDLC/blob/main/docs/phase3_architecture_review_response.md",
      icon: FileText,
      badge: "Review"
    },
    {
      title: "Phase 3.1 Complete Report",
      description: "Final implementation report with all achievements, metrics, test results, and lessons learned. Complete project summary.",
      size: "Added",
      lines: "Complete",
      pdfPath: "/docs/phase3-complete-report.pdf",
      githubPath: "https://github.com/Lev0n82/CPU-Agents-for-SDLC",
      icon: FileText,
      badge: "Report"
    }
  ];

  return (
    <Layout>
      <SEO
        title="Documentation"
        description="Comprehensive Phase 3.1-3.4 documentation: Implementation Guide, Acceptance Criteria, Architecture Design, Ollama AI Integration, and OpenTelemetry Observability. Download branded PDFs."
        keywords="documentation, implementation guide, acceptance criteria, architecture design, technical documentation, PDF downloads, Phase 3.1 docs, Ollama, local AI, CPU models"
      />
      <div className="container py-16">
        {/* Header */}
        <div className="mb-12">
          <Badge className="mb-4 bg-primary text-primary-foreground">
            Phase 3.1 // Documentation
          </Badge>
          <h1 className="text-4xl md:text-5xl font-display font-bold text-foreground mb-4">
            Documentation
          </h1>
          <p className="text-lg text-muted-foreground max-w-3xl">
            Comprehensive documentation for Phase 3.1-3.4 (Complete System). All documents are available as 
            branded PDFs for download and as markdown files on GitHub.
          </p>
        </div>

        {/* Presentation */}
        <div className="mb-12">
          <Card className="p-8 border-2 border-primary/30 bg-gradient-to-br from-primary/5 to-transparent">
            <div className="flex items-start gap-6">
              <div className="w-16 h-16 bg-primary flex items-center justify-center flex-shrink-0">
                <Presentation className="w-8 h-8 text-primary-foreground" />
              </div>
              <div className="flex-1">
                <Badge className="mb-3 bg-primary text-primary-foreground">
                  Featured
                </Badge>
                <h2 className="text-2xl font-display font-bold text-foreground mb-3">
                  Phase 3.1 Implementation Presentation
                </h2>
                <p className="text-muted-foreground mb-6">
                  Professional 18-slide presentation summarizing all Phase 3.1 achievements, architecture, 
                  metrics, and roadmap. Includes comprehensive speaker notes for stakeholder presentations.
                </p>
                <div className="flex gap-3">
                  <a href="manus-slides://HNldHRsW9pNKGiwJHOSffi" target="_blank" rel="noopener noreferrer">
                    <Button className="bg-primary text-primary-foreground hover:bg-primary/90">
                      <Presentation className="w-4 h-4 mr-2" />
                      View Presentation
                    </Button>
                  </a>
                </div>
              </div>
            </div>
          </Card>
        </div>

        {/* AI & Observability Documentation */}
        <div className="mb-12">
          <h2 className="text-3xl font-display font-bold text-foreground mb-6">
            AI & Observability Documentation
          </h2>
          <p className="text-muted-foreground mb-6">
            Latest documentation for AI Decision Module, OpenTelemetry observability stack, and Agent Host Application.
          </p>
          <div className="grid gap-6">
            {aiDocuments.map((doc, index) => {
              const Icon = doc.icon;
              return (
                <Card key={index} className="p-6 border-2 border-primary/30 hover:border-primary transition-colors bg-primary/5">
                  <div className="flex items-start gap-6">
                    <div className="w-14 h-14 bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <Icon className="w-7 h-7 text-primary" />
                    </div>
                    <div className="flex-1">
                      <div className="flex items-start justify-between gap-4 mb-3">
                        <div>
                          <Badge variant="outline" className="mb-2 text-xs border-primary text-primary">
                            {doc.badge}
                          </Badge>
                          <h3 className="text-xl font-display font-semibold text-foreground mb-2">
                            {doc.title}
                          </h3>
                          <p className="text-sm text-muted-foreground mb-3">
                            {doc.description}
                          </p>
                          <div className="flex items-center gap-4 text-xs text-muted-foreground">
                            <span><strong className="text-foreground">Status:</strong> {doc.size}</span>
                            <span>•</span>
                            <span><strong className="text-foreground">Implementation:</strong> {doc.lines}</span>
                          </div>
                        </div>
                      </div>
                      <div className="flex gap-3 mt-4">
                        <a href={doc.githubPath} target="_blank" rel="noopener noreferrer">
                          <Button variant="default" size="sm" className="bg-primary text-primary-foreground hover:bg-primary/90">
                            <GitBranch className="w-4 h-4 mr-2" />
                            View on GitHub
                          </Button>
                        </a>
                      </div>
                    </div>
                  </div>
                </Card>
              );
            })}
          </div>
        </div>

        {/* Documentation Grid */}
        <div className="mb-12">
          <h2 className="text-3xl font-display font-bold text-foreground mb-6">
            Phase 3.1 Documentation
          </h2>
          <div className="grid gap-6">
            {documents.map((doc, index) => {
              const Icon = doc.icon;
              return (
                <Card key={index} className="p-6 border-2 border-border hover:border-primary/50 transition-colors">
                  <div className="flex items-start gap-6">
                    <div className="w-14 h-14 bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <Icon className="w-7 h-7 text-primary" />
                    </div>
                    <div className="flex-1">
                      <div className="flex items-start justify-between gap-4 mb-3">
                        <div>
                          <Badge variant="outline" className="mb-2 text-xs">
                            {doc.badge}
                          </Badge>
                          <h3 className="text-xl font-display font-semibold text-foreground mb-2">
                            {doc.title}
                          </h3>
                          <p className="text-sm text-muted-foreground mb-3">
                            {doc.description}
                          </p>
                          <div className="flex items-center gap-4 text-xs text-muted-foreground">
                            <span><strong className="text-foreground">Size:</strong> {doc.size}</span>
                            <span>•</span>
                            <span><strong className="text-foreground">Length:</strong> {doc.lines}</span>
                          </div>
                        </div>
                      </div>
                      <div className="flex gap-3 mt-4">
                        <a href={doc.pdfPath} download>
                          <Button variant="default" size="sm" className="bg-primary text-primary-foreground hover:bg-primary/90">
                            <Download className="w-4 h-4 mr-2" />
                            Download PDF
                          </Button>
                        </a>
                        <a href={doc.githubPath} target="_blank" rel="noopener noreferrer">
                          <Button variant="outline" size="sm" className="border-2 border-primary text-primary hover:bg-primary hover:text-primary-foreground">
                            <GitBranch className="w-4 h-4 mr-2" />
                            View on GitHub
                          </Button>
                        </a>
                      </div>
                    </div>
                  </div>
                </Card>
              );
            })}
          </div>
        </div>

        {/* Summary Stats */}
        <div>
          <h2 className="text-3xl font-display font-bold text-foreground mb-6">
            Documentation Summary
          </h2>
          <div className="grid md:grid-cols-4 gap-6">
            <Card className="p-6 border-2 border-border text-center">
              <div className="text-4xl font-display font-bold text-primary mb-2">11,713</div>
              <h3 className="text-sm font-display font-semibold text-foreground mb-1">
                Total Lines
              </h3>
              <p className="text-xs text-muted-foreground">
                Comprehensive documentation
              </p>
            </Card>
            <Card className="p-6 border-2 border-border text-center">
              <div className="text-4xl font-display font-bold text-primary mb-2">5</div>
              <h3 className="text-sm font-display font-semibold text-foreground mb-1">
                PDF Documents
              </h3>
              <p className="text-xs text-muted-foreground">
                Branded and downloadable
              </p>
            </Card>
            <Card className="p-6 border-2 border-border text-center">
              <div className="text-4xl font-display font-bold text-primary mb-2">355</div>
              <h3 className="text-sm font-display font-semibold text-foreground mb-1">
                Acceptance Criteria
              </h3>
              <p className="text-xs text-muted-foreground">
                4 levels of validation
              </p>
            </Card>
            <Card className="p-6 border-2 border-border text-center">
              <div className="text-4xl font-display font-bold text-primary mb-2">18</div>
              <h3 className="text-sm font-display font-semibold text-foreground mb-1">
                Presentation Slides
              </h3>
              <p className="text-xs text-muted-foreground">
                With speaker notes
              </p>
            </Card>
          </div>
        </div>
      </div>
    </Layout>
  );
}
