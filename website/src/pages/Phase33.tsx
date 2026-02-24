import Layout from "@/components/Layout";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Link } from "wouter";
import {
  CheckCircle2,
  ArrowRight,
  Shield,
  Activity,
  Gauge,
  RefreshCw,
  Zap,
  Eye,
  TrendingUp,
} from "lucide-react";
import { SEO } from '@/components/SEO';

export default function Phase33() {
  const modules = [
    {
      icon: Shield,
      title: "Operational Resilience",
      description: "Polly 8.x patterns for production-grade fault tolerance and reliability",
      classes: 3,
      acceptanceCriteria: 68,
      patterns: [
        {
          name: "Retry Policy",
          description: "Exponential backoff with jitter for transient failures",
          config: "3 retries, 2s base delay, 30s max delay",
        },
        {
          name: "Circuit Breaker",
          description: "Prevent cascading failures with automatic recovery",
          config: "50% failure threshold, 30s break duration, 10 sample size",
        },
        {
          name: "Timeout Policy",
          description: "Prevent indefinite waits with configurable timeouts",
          config: "30s default, 120s for Git operations",
        },
        {
          name: "Bulkhead Isolation",
          description: "Limit concurrent operations to prevent resource exhaustion",
          config: "10 max parallel, 20 max queued",
        },
      ],
      features: [
        "Automatic retry with exponential backoff",
        "Circuit breaker with half-open state",
        "Configurable timeout policies",
        "Bulkhead isolation for resource protection",
        "Telemetry for all resilience events",
      ],
    },
    {
      icon: Activity,
      title: "Observability",
      description: "OpenTelemetry integration with distributed tracing and metrics",
      classes: 3,
      acceptanceCriteria: 54,
      capabilities: [
        {
          name: "Distributed Tracing",
          description: "W3C Trace Context with correlation IDs",
          features: ["Span creation", "Context propagation", "Parent-child relationships", "Baggage support"],
        },
        {
          name: "Metrics Collection",
          description: "Custom metrics for performance monitoring",
          features: ["Counters", "Histograms", "Gauges", "OTLP export"],
        },
        {
          name: "Structured Logging",
          description: "Serilog integration with correlation IDs",
          features: ["JSON formatting", "File rotation", "Console output", "OpenTelemetry export"],
        },
      ],
      features: [
        "OpenTelemetry SDK integration",
        "OTLP export to Prometheus, Jaeger, Grafana",
        "Custom metrics for work items, API calls, cache hits",
        "Distributed tracing with W3C Trace Context",
        "Structured logging with correlation IDs",
      ],
    },
    {
      icon: Gauge,
      title: "Performance Optimization",
      description: "In-memory caching and rate limiting for optimal performance",
      classes: 2,
      acceptanceCriteria: 42,
      optimizations: [
        {
          name: "In-Memory Cache",
          description: "Microsoft.Extensions.Caching.Memory for fast lookups",
          config: "5-minute TTL, 1000 max entries, LRU eviction",
        },
        {
          name: "Token Bucket Rate Limiter",
          description: "Prevent API throttling with intelligent rate limiting",
          config: "100 tokens/minute, 10 token burst, auto-replenishment",
        },
      ],
      features: [
        "In-memory caching with configurable TTL",
        "Cache invalidation on updates",
        "Token bucket rate limiting",
        "Automatic rate limit recovery",
        "Telemetry for cache and rate limiter",
      ],
    },
  ];

  const observabilityStack = [
    {
      component: "OpenTelemetry Collector",
      description: "OTLP receiver and exporter",
      ports: "4317 (gRPC), 4318 (HTTP)",
    },
    {
      component: "Prometheus",
      description: "Metrics storage and querying",
      ports: "9092 (HTTP)",
    },
    {
      component: "Jaeger",
      description: "Distributed tracing UI",
      ports: "16686 (HTTP)",
    },
    {
      component: "Grafana",
      description: "Visualization and dashboards",
      ports: "3001 (HTTP)",
    },
  ];

  const dashboards = [
    {
      name: "CPU Agents - Overview",
      metrics: [
        "Work items processed (counter)",
        "API call latency (histogram)",
        "Cache hit rate (gauge)",
        "Circuit breaker state (gauge)",
      ],
    },
    {
      name: "CPU Agents - Performance",
      metrics: [
        "Cache statistics (hits, misses, evictions)",
        "Rate limiter tokens (available, consumed)",
        "Sync operation duration (histogram)",
        "Memory usage (gauge)",
      ],
    },
  ];

  const performanceTargets = [
    { metric: "Work Item Processing", target: "10+ items/min", status: "✅ Achieved" },
    { metric: "API Latency (P95)", target: "<500ms", status: "✅ Achieved" },
    { metric: "Cache Hit Rate", target: ">80%", status: "✅ Achieved" },
    { metric: "Memory Usage", target: "<512MB", status: "✅ Achieved" },
  ];

  return (
    <Layout>
      <SEO
        title="Phase 3.3 - Operational Resilience"
        description="Phase 3.3 implementation: Operational Resilience with Polly 8.x, OpenTelemetry Observability, and Performance Optimization with 8 classes and 164 acceptance criteria for local CPU-based AI agents"
        keywords="Phase 3.3, resilience, observability, performance, Polly, OpenTelemetry, circuit breaker, retry, rate limiting, caching, Grafana, Prometheus, Jaeger, local AI, CPU optimization"
      />

      {/* Hero Section */}
      <section className="relative overflow-hidden bg-background py-20">
        <div className="container">
          <div className="max-w-4xl mx-auto text-center">
            <div className="label-swiss mb-6">PHASE 3.3 // OPERATIONAL RESILIENCE</div>
            <h1 className="text-5xl md:text-6xl font-display font-semibold text-foreground mb-6">
              Resilience, Observability, Performance
            </h1>
            <p className="text-xl text-muted-foreground mb-8 max-w-2xl mx-auto">
              Production-grade fault tolerance with Polly 8.x, complete observability with OpenTelemetry,
              and performance optimization for enterprise workloads.
            </p>
            <div className="flex flex-wrap gap-6 justify-center">
              <div className="data-card">
                <div className="metric">8</div>
                <div className="label-swiss mt-2">Classes</div>
              </div>
              <div className="data-card">
                <div className="metric">164</div>
                <div className="label-swiss mt-2">Acceptance Criteria</div>
              </div>
              <div className="data-card">
                <div className="metric">5</div>
                <div className="label-swiss mt-2">Resilience Patterns</div>
              </div>
              <div className="data-card">
                <div className="metric">2</div>
                <div className="label-swiss mt-2">Grafana Dashboards</div>
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
              Three Core Modules
            </h2>
            <p className="text-center text-muted-foreground mb-12 max-w-2xl mx-auto">
              Phase 3.3 delivers production-grade resilience, complete observability, and
              performance optimization for enterprise deployments.
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

                  {module.patterns && (
                    <div className="grid md:grid-cols-2 gap-4 mb-6">
                      {module.patterns.map((pattern, idx) => (
                        <div key={idx} className="bg-muted/30 rounded-lg p-4">
                          <div className="font-semibold mb-2 flex items-center gap-2">
                            <Shield className="w-4 h-4 text-primary" />
                            {pattern.name}
                          </div>
                          <p className="text-sm text-muted-foreground mb-2">{pattern.description}</p>
                          <div className="text-xs text-primary font-mono">{pattern.config}</div>
                        </div>
                      ))}
                    </div>
                  )}

                  {module.capabilities && (
                    <div className="grid md:grid-cols-3 gap-4 mb-6">
                      {module.capabilities.map((capability, idx) => (
                        <div key={idx} className="bg-muted/30 rounded-lg p-4">
                          <div className="font-semibold mb-2 flex items-center gap-2">
                            <Eye className="w-4 h-4 text-primary" />
                            {capability.name}
                          </div>
                          <p className="text-sm text-muted-foreground mb-3">{capability.description}</p>
                          <div className="space-y-1">
                            {capability.features.map((feature, fidx) => (
                              <div key={fidx} className="text-xs text-muted-foreground flex items-center gap-1">
                                <CheckCircle2 className="w-3 h-3 text-primary" />
                                {feature}
                              </div>
                            ))}
                          </div>
                        </div>
                      ))}
                    </div>
                  )}

                  {module.optimizations && (
                    <div className="grid md:grid-cols-2 gap-4 mb-6">
                      {module.optimizations.map((optimization, idx) => (
                        <div key={idx} className="bg-muted/30 rounded-lg p-4">
                          <div className="font-semibold mb-2 flex items-center gap-2">
                            <Zap className="w-4 h-4 text-primary" />
                            {optimization.name}
                          </div>
                          <p className="text-sm text-muted-foreground mb-2">{optimization.description}</p>
                          <div className="text-xs text-primary font-mono">{optimization.config}</div>
                        </div>
                      ))}
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

      {/* Observability Stack */}
      <section className="py-20 bg-background">
        <div className="container">
          <div className="max-w-6xl mx-auto">
            <h2 className="text-4xl font-display font-semibold text-center mb-4">
              Complete Observability Stack
            </h2>
            <p className="text-center text-muted-foreground mb-12 max-w-2xl mx-auto">
              Full OpenTelemetry stack with Grafana dashboards, Prometheus metrics, and Jaeger tracing.
            </p>

            <div className="grid md:grid-cols-2 gap-8 mb-12">
              {observabilityStack.map((component, index) => (
                <Card key={index} className="p-6">
                  <div className="flex items-start gap-4">
                    <Activity className="w-8 h-8 text-primary flex-shrink-0" />
                    <div className="flex-1">
                      <h3 className="text-xl font-semibold mb-2">{component.component}</h3>
                      <p className="text-sm text-muted-foreground mb-2">{component.description}</p>
                      <div className="text-xs text-primary font-mono">{component.ports}</div>
                    </div>
                  </div>
                </Card>
              ))}
            </div>

            <div className="grid md:grid-cols-2 gap-8">
              {dashboards.map((dashboard, index) => (
                <Card key={index} className="p-6 bg-primary/5 border-primary/20">
                  <h3 className="text-xl font-semibold mb-4 flex items-center gap-2">
                    <TrendingUp className="w-5 h-5 text-primary" />
                    {dashboard.name}
                  </h3>
                  <div className="space-y-2">
                    {dashboard.metrics.map((metric, idx) => (
                      <div key={idx} className="flex items-center gap-2">
                        <CheckCircle2 className="w-4 h-4 text-primary flex-shrink-0" />
                        <span className="text-sm">{metric}</span>
                      </div>
                    ))}
                  </div>
                </Card>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Performance Targets */}
      <section className="py-20 bg-muted/30">
        <div className="container">
          <div className="max-w-4xl mx-auto">
            <h2 className="text-4xl font-display font-semibold text-center mb-4">
              Performance Targets
            </h2>
            <p className="text-center text-muted-foreground mb-12">
              Phase 3.3 achieves all performance targets for enterprise workloads.
            </p>

            <div className="grid md:grid-cols-2 gap-6">
              {performanceTargets.map((target, index) => (
                <Card key={index} className="p-6 flex items-center gap-4">
                  <Gauge className="w-8 h-8 text-primary flex-shrink-0" />
                  <div className="flex-1">
                    <div className="font-semibold mb-1">{target.metric}</div>
                    <div className="text-sm text-muted-foreground">Target: {target.target}</div>
                  </div>
                  <div className="text-lg font-bold text-green-600">{target.status}</div>
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
              Ready to Explore Phase 3.4?
            </h2>
            <p className="text-xl text-muted-foreground mb-8">
              Continue to Phase 3.4 for test lifecycle management and migration tooling.
            </p>
            <div className="flex flex-wrap gap-4 justify-center">
              <Link href="/phase-3-4">
                <Button size="lg" className="bg-primary text-primary-foreground hover:bg-primary/90 group">
                  View Phase 3.4
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
