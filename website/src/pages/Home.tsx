import Layout from "@/components/Layout";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Link } from "wouter";
import {
  Cpu,
  Lock,
  Zap,
  Database,
  Server,
  CheckCircle2,
  ArrowRight,
  Code2,
  Smartphone,
  GitBranch,
  Target,
  TrendingUp,
} from "lucide-react";
import { SEO } from '@/components/SEO';

export default function Home() {
  const metrics = [
    { value: "95%", unit: "complete", label: "System Ready" },
    { value: "45", unit: "classes", label: "Phase 3.1-3.4" },
    { value: "4", unit: "AI features", label: "Local CPU Models" },
    { value: "50K+", unit: "words", label: "Documentation" },
  ];

  const features = [
    {
      icon: Cpu,
      title: "Local AI Decision-Making",
      description:
        "Ollama integration with CPU-optimized models (Granite 4, Phi-3, Llama 3) for code review, test obsolescence detection, conflict resolution, and root cause analysis.",
    },
    {
      icon: Lock,
      title: "Production Resilience",
      description:
        "Polly 8.x patterns: retry with exponential backoff, circuit breaker, timeout, bulkhead isolation, and rate limiting.",
    },
    {
      icon: Zap,
      title: "Complete Observability",
      description:
        "OpenTelemetry with Grafana dashboards, Prometheus metrics, Jaeger tracing, and distributed correlation IDs.",
    },
    {
      icon: Database,
      title: "Azure DevOps",
      description:
        "Native integration with Azure Boards, Test Plans, and Repos for seamless workflow automation.",
    },
    {
      icon: Server,
      title: "Autonomous Workflows",
      description:
        "Pre-built workflows for bug investigation, test execution, and code review with extensible action registry.",
    },
    {
      icon: CheckCircle2,
      title: "Offline Sync",
      description:
        "SQLite caching with 4 conflict resolution policies for reliable operation during network outages.",
    },
  ];

  const components = [
    {
      icon: Code2,
      title: "Agent Host",
      description:
        "Autonomous polling loop with workflow engine built on .NET 8.0 for Windows 11.",
      features: [
        "11 Built-in Actions",
        "3 Pre-built Workflows",
        "OpenTelemetry Instrumentation",
        "JSON Workflow Definitions",
      ],
    },
    {
      icon: Smartphone,
      title: "AI Decision Module",
      description:
        "Local CPU-based AI via Ollama for intelligent code analysis and decision-making with quantized models.",
      features: [
        "Granite 4 / Phi-3 / Llama 3",
        "Code Quality Scoring",
        "Security Vulnerability Detection",
        "Test Obsolescence Analysis",
      ],
    },
    {
      icon: Server,
      title: "Phase 3 Services",
      description:
        "45 production-ready classes across authentication, Git, test plans, and resilience.",
      features: [
        "Multi-Auth (PAT, Cert, MSAL)",
        "LibGit2Sharp Integration",
        "Offline Synchronization",
        "Performance Optimization",
      ],
    },
  ];

  return (
    <Layout>
      <SEO
        title="Home"
        description="Self-aware autonomous AI agents with local CPU-based models (Granite 4, Phi-3) via vLLM or Ollama. Phase 3.1-3.4 complete with 45 classes, 100% local execution, zero cloud dependencies."
        keywords="CPU agents, autonomous AI, SDLC automation, Azure DevOps integration, enterprise AI, software development lifecycle, vLLM, Ollama, Granite 4, Phi-3, local AI, CPU models, llama.cpp"
      />
      {/* Hero Section */}
      <section className="relative overflow-hidden bg-background">
        <div className="container py-20 md:py-32">
          <div className="grid-asymmetric-1">
            {/* Left Column - Main Content */}
            <div className="animate-slide-in-left">
              <div className="label-swiss mb-6">
                AUTONOMOUS AI // 95% PRODUCTION-READY
              </div>
              <h1 className="text-5xl md:text-7xl font-display font-semibold text-foreground mb-6 leading-tight">
                CPU Agents
                <br />
                <span className="text-primary">for SDLC</span>
              </h1>
              <p className="text-lg text-muted-foreground mb-8 max-w-xl leading-relaxed">
                Autonomous AI agents that <strong className="text-foreground">automate software development workflows</strong> by integrating with Azure DevOps and local CPU-based AI models. 
                Automatically manage work items, execute test cases, review code with AI, and update documentation—reducing manual SDLC overhead by up to 70%.
              </p>
              <p className="text-base text-muted-foreground mb-8 max-w-xl">
                <strong className="text-primary">Phase 3.1-3.4 architecture complete</strong> with 45 classes, local AI decision-making via Ollama (Granite 4, Phi-3), 
                production-grade resilience, and complete OpenTelemetry observability.
              </p>
              <div className="flex flex-wrap gap-4">
                <Link href="/features">
                  <Button
                    size="lg"
                    className="bg-primary text-primary-foreground hover:bg-primary/90 font-medium"
                  >
                    View Features
                    <ArrowRight className="w-5 h-5 ml-2" />
                  </Button>
                </Link>
                <Link href="/quick-start">
                  <Button
                    size="lg"
                    variant="outline"
                    className="border-2 border-primary text-primary hover:bg-primary hover:text-primary-foreground font-medium"
                  >
                    Quick Start
                  </Button>
                </Link>
                <a
                  href="https://github.com/Lev0n82/CPU-Agents-for-SDLC"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <Button
                    size="lg"
                    variant="outline"
                    className="border-2 border-primary text-primary hover:bg-primary hover:text-primary-foreground font-medium"
                  >
                    <GitBranch className="w-5 h-5 mr-2" />
                    GitHub
                  </Button>
                </a>
              </div>
            </div>

            {/* Right Column - Metrics */}
            <div className="grid grid-cols-2 gap-4 animate-slide-in-right">
              {metrics.map((metric, index) => (
                <div key={index} className="data-card">
                  <div className="metric">{metric.value}</div>
                  <div className="text-sm text-muted-foreground mt-1">
                    {metric.unit}
                  </div>
                  <div className="label-swiss mt-4">{metric.label}</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      {/* Value Proposition Section */}
      <section className="py-24 bg-background">
        <div className="container">
          <div className="max-w-4xl mx-auto">
            <div className="label-swiss mb-6 text-center">WHY CPU AGENTS</div>
            <h2 className="text-4xl md:text-5xl font-display font-semibold text-center text-foreground mb-8">
              Automate Your SDLC with AI
            </h2>
            <div className="w-16 h-1 bg-primary mx-auto mb-12" />
            
            <div className="grid md:grid-cols-3 gap-8 mb-12">
              <Card className="p-6 border-2 border-primary/20">
                <div className="text-4xl font-bold text-primary mb-2">70%</div>
                <div className="text-sm text-muted-foreground">Reduction in manual SDLC tasks</div>
              </Card>
              <Card className="p-6 border-2 border-primary/20">
                <div className="text-4xl font-bold text-primary mb-2">3x</div>
                <div className="text-sm text-muted-foreground">Faster work item processing</div>
              </Card>
              <Card className="p-6 border-2 border-primary/20">
                <div className="text-4xl font-bold text-primary mb-2">24/7</div>
                <div className="text-sm text-muted-foreground">Continuous autonomous operation</div>
              </Card>
            </div>

            <div className="space-y-6">
              <Card className="p-8">
                <h3 className="text-2xl font-semibold mb-4 flex items-center gap-3">
                  <Zap className="w-6 h-6 text-primary" />
                  What CPU Agents Do
                </h3>
                <p className="text-muted-foreground leading-relaxed mb-4">
                  CPU Agents are autonomous AI agents that execute software development tasks on your local machine. 
                  They integrate directly with Azure DevOps to automate repetitive SDLC workflows without requiring cloud infrastructure.
                </p>
                <ul className="space-y-2 text-muted-foreground">
                  <li className="flex items-start gap-2">
                    <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <span>Automatically claim, update, and close work items based on project rules</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <span>Execute test cases, validate results, and update test plans</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <span>Review code changes and provide automated feedback</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <span>Synchronize documentation and maintain traceability</span>
                  </li>
                </ul>
              </Card>

              <Card className="p-8 bg-primary/5 border-primary/20">
                <h3 className="text-2xl font-semibold mb-4 flex items-center gap-3">
                  <Target className="w-6 h-6 text-primary" />
                  Why Azure DevOps Integration
                </h3>
                <p className="text-muted-foreground leading-relaxed mb-4">
                  Azure DevOps is the central hub for enterprise software development. By integrating CPU Agents with Azure DevOps, 
                  you unlock automation across your entire SDLC—from planning to deployment.
                </p>
                <div className="grid md:grid-cols-2 gap-4">
                  <div className="flex items-start gap-3">
                    <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <span className="text-primary font-bold">1</span>
                    </div>
                    <div>
                      <div className="font-semibold mb-1">Work Item Automation</div>
                      <div className="text-sm text-muted-foreground">Agents claim, process, and update work items automatically based on priority and capacity</div>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <span className="text-primary font-bold">2</span>
                    </div>
                    <div>
                      <div className="font-semibold mb-1">Test Execution</div>
                      <div className="text-sm text-muted-foreground">Automated test case execution with real-time results synced to test plans</div>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <span className="text-primary font-bold">3</span>
                    </div>
                    <div>
                      <div className="font-semibold mb-1">Code Review</div>
                      <div className="text-sm text-muted-foreground">AI-powered code analysis with automated pull request comments and suggestions</div>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <span className="text-primary font-bold">4</span>
                    </div>
                    <div>
                      <div className="font-semibold mb-1">Documentation Sync</div>
                      <div className="text-sm text-muted-foreground">Keep wikis, README files, and work item descriptions up-to-date automatically</div>
                    </div>
                  </div>
                </div>
              </Card>

              <Card className="p-8">
                <h3 className="text-2xl font-semibold mb-4 flex items-center gap-3">
                  <TrendingUp className="w-6 h-6 text-primary" />
                  Business Value
                </h3>
                <div className="grid md:grid-cols-3 gap-6">
                  <div>
                    <div className="text-3xl font-bold text-primary mb-2">$50K+</div>
                    <div className="text-sm font-semibold mb-1">Annual Savings per Team</div>
                    <div className="text-xs text-muted-foreground">Reduced manual effort and faster delivery cycles</div>
                  </div>
                  <div>
                    <div className="text-3xl font-bold text-primary mb-2">40%</div>
                    <div className="text-sm font-semibold mb-1">Faster Time-to-Market</div>
                    <div className="text-xs text-muted-foreground">Automated workflows eliminate bottlenecks</div>
                  </div>
                  <div>
                    <div className="text-3xl font-bold text-primary mb-2">99.9%</div>
                    <div className="text-sm font-semibold mb-1">Process Consistency</div>
                    <div className="text-xs text-muted-foreground">Eliminate human error in repetitive tasks</div>
                  </div>
                </div>
              </Card>
            </div>
          </div>
        </div>
      </section>

      <section className="diagonal-section bg-accent">
        <div className="container py-20">
          <div className="mb-16 animate-fade-in-up">
            <div className="label-swiss mb-4">CORE CAPABILITIES</div>
            <h2 className="text-4xl md:text-5xl font-display font-semibold text-foreground mb-4">
              Key Features
            </h2>
            <div className="w-16 h-1 bg-primary" />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {features.map((feature, index) => (
              <div
                key={index}
                className="data-card animate-fade-in-up"
                style={{ animationDelay: `${index * 0.1}s` }}
              >
                <feature.icon className="w-12 h-12 text-primary mb-6" />
                <h3 className="text-xl font-display font-semibold text-foreground mb-3">
                  {feature.title}
                </h3>
                <p className="text-sm text-muted-foreground leading-relaxed">
                  {feature.description}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Architecture Section */}
      <section className="diagonal-section-reverse bg-background">
        <div className="container py-20">
          <div className="mb-16">
            <div className="label-swiss mb-4">SYSTEM DESIGN</div>
            <h2 className="text-4xl md:text-5xl font-display font-semibold text-foreground mb-4">
              Architecture
            </h2>
            <div className="w-16 h-1 bg-primary" />
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {components.map((component, index) => (
              <div key={index} className="data-card">
                <component.icon className="w-16 h-16 text-primary mb-6" />
                <h3 className="text-2xl font-display font-semibold text-foreground mb-3">
                  {component.title}
                </h3>
                <p className="text-sm text-muted-foreground mb-6 leading-relaxed">
                  {component.description}
                </p>
                <div className="space-y-3">
                  {component.features.map((feature, i) => (
                    <div key={i} className="flex items-start gap-3">
                      <div className="w-1 h-1 bg-primary mt-2 flex-shrink-0" />
                      <span className="text-sm text-foreground">{feature}</span>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>

          <div className="mt-12 text-center">
            <Link href="/architecture">
              <Button
                variant="outline"
                className="border-2 border-primary text-primary hover:bg-primary hover:text-primary-foreground font-medium"
              >
                View Full Architecture
                <ArrowRight className="w-4 h-4 ml-2" />
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="bg-primary text-primary-foreground">
        <div className="container py-20">
          <div className="max-w-3xl mx-auto text-center">
            <h2 className="text-4xl md:text-5xl font-display font-semibold mb-6">
              Ready to Deploy?
            </h2>
            <p className="text-lg opacity-90 mb-8">
              Get your autonomous agent running on Windows 11 in under 5
              minutes.
            </p>

            {/* Quick Command Preview */}
            <div className="bg-primary-foreground/10 border-2 border-primary-foreground/20 p-6 mb-8 text-left">
              <div className="label-swiss mb-3 text-primary-foreground/70">
                QUICK START COMMAND
              </div>
              <pre className="text-primary-foreground font-mono text-sm overflow-x-auto">
                <code>
                  {`git clone https://github.com/Lev0n82/CPU-Agents-for-SDLC.git
cd CPU-Agents-for-SDLC\\desktop-agent\\src\\AutonomousAgent.Core
dotnet run`}
                </code>
              </pre>
            </div>

            <Link href="/quick-start">
              <Button
                size="lg"
                variant="outline"
                className="border-2 border-primary-foreground text-primary-foreground hover:bg-primary-foreground hover:text-primary font-medium text-lg px-8"
              >
                Start Deployment
                <ArrowRight className="w-6 h-6 ml-2" />
              </Button>
            </Link>
          </div>
        </div>
      </section>
    </Layout>
  );
}
