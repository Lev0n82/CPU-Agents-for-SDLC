import Layout from "@/components/Layout";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Code2, FileSearch, GitMerge, Bug, Cpu, CheckCircle2, AlertTriangle } from "lucide-react";
import { SEO } from '@/components/SEO';
import { useState, useRef, useEffect } from "react";

export default function AIDemo() {
  const [activeDemo, setActiveDemo] = useState<string | null>(null);
  const demoSectionRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (activeDemo && demoSectionRef.current) {
      // Smooth scroll to the demo section with offset for header
      const yOffset = -100; // Offset for fixed header
      const element = demoSectionRef.current;
      const y = element.getBoundingClientRect().top + window.pageYOffset + yOffset;
      window.scrollTo({ top: y, behavior: 'smooth' });
    }
  }, [activeDemo]);

  const aiCapabilities = [
    {
      id: "code-review",
      icon: Code2,
      title: "AI Code Review",
      description: "Local AI analyzes code quality, detects patterns, and provides improvement suggestions",
      model: "Granite 4 (8B parameters)",
      exampleInput: `public class UserService {
    public User GetUser(int id) {
        var user = db.Users.Find(id);
        return user;
    }
}`,
      exampleOutput: {
        score: 6.5,
        issues: [
          { severity: "warning", message: "Missing null check - Find() can return null", line: 3 },
          { severity: "info", message: "Consider async/await for database operations", line: 2 },
          { severity: "info", message: "Add XML documentation for public methods", line: 2 }
        ],
        suggestions: [
          "Add null validation before returning user",
          "Convert to async Task<User> GetUserAsync(int id)",
          "Add logging for user retrieval operations"
        ]
      }
    },
    {
      id: "test-obsolescence",
      icon: FileSearch,
      title: "Test Obsolescence Detection",
      description: "Identifies outdated tests that no longer match current requirements or code structure",
      model: "Phi-3 (3.8B parameters)",
      exampleInput: `Test: "Should validate user email format"
Code: Email validation removed, now using OAuth only
Last Modified: 180 days ago
Pass Rate: 100% (but testing obsolete code path)`,
      exampleOutput: {
        obsolete: true,
        confidence: 0.92,
        reasons: [
          "Code path no longer exists in current implementation",
          "Requirements changed to OAuth-only authentication",
          "Test hasn't been updated in 6 months despite code changes"
        ],
        recommendation: "Archive or rewrite test to validate OAuth flow instead"
      }
    },
    {
      id: "conflict-resolution",
      icon: GitMerge,
      title: "Merge Conflict Resolution",
      description: "AI-powered intelligent merge conflict resolution with context awareness",
      model: "Llama 3 (8B parameters)",
      exampleInput: `<<<<<<< HEAD
public void ProcessOrder(Order order) {
    ValidateOrder(order);
    CalculateTotal(order);
    SaveOrder(order);
}
=======
public async Task ProcessOrderAsync(Order order) {
    await ValidateOrderAsync(order);
    await CalculateTotalAsync(order);
    await SaveOrderAsync(order);
}
>>>>>>> feature/async-refactor`,
      exampleOutput: {
        resolution: "accept-incoming",
        confidence: 0.95,
        reasoning: [
          "Incoming branch follows async/await best practices",
          "Async pattern is consistent with rest of codebase (87% async methods)",
          "No functional logic changes, only async conversion"
        ],
        suggestedCode: `public async Task ProcessOrderAsync(Order order) {
    await ValidateOrderAsync(order);
    await CalculateTotalAsync(order);
    await SaveOrderAsync(order);
}`
      }
    },
    {
      id: "root-cause",
      icon: Bug,
      title: "Root Cause Analysis",
      description: "Analyzes test failures and bug reports to identify underlying root causes",
      model: "Granite 4 (8B parameters)",
      exampleInput: `Bug: "Application crashes when processing large orders"
Stack Trace: NullReferenceException at OrderProcessor.CalculateTotal()
Recent Changes: Added discount calculation feature
Test Failures: 3/45 integration tests failing
Error Pattern: Only occurs with orders >$10,000`,
      exampleOutput: {
        rootCause: "Null reference in discount calculation for high-value orders",
        confidence: 0.88,
        analysis: [
          "Discount calculation assumes DiscountTier is always set",
          "High-value orders (>$10k) use different pricing tier logic",
          "New discount feature didn't account for premium tier edge case"
        ],
        suggestedFix: `// Add null check before discount calculation
if (order.DiscountTier != null) {
    discount = CalculateDiscount(order);
} else {
    discount = 0; // Premium tier uses different pricing
}`,
        relatedIssues: [
          "Similar pattern in ShippingCalculator.cs (line 45)",
          "Consider adding tier validation in Order constructor"
        ]
      }
    }
  ];

  return (
    <Layout>
      <SEO 
        title="AI Capabilities Demo - CPU Agents"
        description="Interactive demonstration of 4 local AI capabilities: code review with Granite 4, test obsolescence detection with Phi-3, merge conflict resolution with Llama 3, and root cause analysis. All running locally via Ollama with zero cloud dependencies."
        keywords="AI code review, test obsolescence, merge conflict resolution, root cause analysis, Ollama, Granite 4, Phi-3, Llama 3, local AI"
      />
      <div className="container py-16">
        {/* Header */}
        <div className="mb-12">
          <Badge className="mb-4 bg-primary text-primary-foreground">
            Interactive Demo
          </Badge>
          <h1 className="text-4xl md:text-5xl font-display font-bold text-foreground mb-4">
            AI Capabilities Demo
          </h1>
          <p className="text-lg text-muted-foreground max-w-3xl">
            Experience 4 local AI-powered capabilities running via vLLM or Ollama with Granite 4, Phi-3, and Llama 3 models.
            100% local execution with zero cloud dependencies. Use vLLM for production (GPU acceleration, batching) or Ollama for development.
          </p>
        </div>

        {/* AI Model Info */}
        <Card className="p-6 border-2 border-primary/30 bg-primary/5 mb-12">
          <div className="flex items-start gap-4">
            <Cpu className="w-8 h-8 text-primary flex-shrink-0 mt-1" />
            <div>
              <h3 className="text-lg font-display font-semibold text-foreground mb-2">
                Local AI via vLLM or Ollama
              </h3>
              <p className="text-sm text-muted-foreground mb-3">
                All AI capabilities run locally on your workstation. Use <strong>vLLM</strong> for production (GPU acceleration, batching, higher throughput) 
                or <strong>Ollama</strong> for development (easier setup, CPU-optimized). No data leaves your network, ensuring complete privacy and security.
              </p>
              <div className="flex flex-wrap gap-2">
                <Badge variant="outline" className="border-primary text-primary">Granite 4 (8B)</Badge>
                <Badge variant="outline" className="border-primary text-primary">Phi-3 (3.8B)</Badge>
                <Badge variant="outline" className="border-primary text-primary">Llama 3 (8B)</Badge>
                <Badge variant="outline" className="border-primary text-primary">vLLM (Production)</Badge>
                <Badge variant="outline" className="border-primary text-primary">Ollama (Dev)</Badge>
                <Badge variant="outline" className="border-primary text-primary">100% Local</Badge>
              </div>
            </div>
          </div>
        </Card>

        {/* AI Capabilities Grid */}
        <div className="grid md:grid-cols-2 gap-6 mb-12">
          {aiCapabilities.map((capability) => {
            const Icon = capability.icon;
            const isActive = activeDemo === capability.id;
            
            return (
              <Card 
                key={capability.id} 
                className={`p-6 border-2 transition-all cursor-pointer ${
                  isActive ? 'border-primary bg-primary/5' : 'border-border hover:border-primary/50'
                }`}
                onClick={() => setActiveDemo(isActive ? null : capability.id)}
              >
                <div className="flex items-start gap-4 mb-4">
                  <div className="w-12 h-12 bg-primary/10 flex items-center justify-center flex-shrink-0">
                    <Icon className="w-6 h-6 text-primary" />
                  </div>
                  <div className="flex-1">
                    <h3 className="text-lg font-display font-semibold text-foreground mb-1">
                      {capability.title}
                    </h3>
                    <p className="text-sm text-muted-foreground mb-2">
                      {capability.description}
                    </p>
                    <Badge variant="outline" className="text-xs">
                      {capability.model}
                    </Badge>
                  </div>
                </div>
                
                <Button 
                  variant={isActive ? "default" : "outline"}
                  className="w-full"
                  onClick={(e) => {
                    e.stopPropagation();
                    setActiveDemo(isActive ? null : capability.id);
                  }}
                >
                  {isActive ? "Hide Example" : "View Example"}
                </Button>
              </Card>
            );
          })}
        </div>

        {/* Active Demo Details */}
        {activeDemo && (
          <Card ref={demoSectionRef} className="p-8 border-2 border-primary/30 bg-card">
            {aiCapabilities.filter(c => c.id === activeDemo).map((capability) => {
              const Icon = capability.icon;
              return (
                <div key={capability.id}>
                  <div className="flex items-center gap-3 mb-6">
                    <Icon className="w-8 h-8 text-primary" />
                    <div>
                      <h2 className="text-2xl font-display font-bold text-foreground">
                        {capability.title}
                      </h2>
                      <p className="text-sm text-muted-foreground">
                        Powered by {capability.model}
                      </p>
                    </div>
                  </div>

                  <div className="grid md:grid-cols-2 gap-6">
                    {/* Input */}
                    <div>
                      <h3 className="text-lg font-display font-semibold text-foreground mb-3">
                        Input
                      </h3>
                      <Card className="p-4 bg-muted border-2 border-border">
                        <pre className="text-xs text-foreground overflow-x-auto whitespace-pre-wrap">
                          {capability.exampleInput}
                        </pre>
                      </Card>
                    </div>

                    {/* Output */}
                    <div>
                      <h3 className="text-lg font-display font-semibold text-foreground mb-3">
                        AI Analysis Output
                      </h3>
                      <Card className="p-4 bg-muted border-2 border-border">
                        <pre className="text-xs text-foreground overflow-x-auto whitespace-pre-wrap">
                          {JSON.stringify(capability.exampleOutput, null, 2)}
                        </pre>
                      </Card>
                    </div>
                  </div>

                  {/* Key Insights */}
                  {capability.id === "code-review" && (
                    <div className="mt-6">
                      <h3 className="text-lg font-display font-semibold text-foreground mb-3">
                        Key Insights
                      </h3>
                      <div className="space-y-2">
                        <div className="flex items-start gap-2">
                          <AlertTriangle className="w-4 h-4 text-yellow-600 flex-shrink-0 mt-0.5" />
                          <span className="text-sm text-muted-foreground">
                            Quality score: <strong className="text-foreground">6.5/10</strong> - Room for improvement
                          </span>
                        </div>
                        <div className="flex items-start gap-2">
                          <CheckCircle2 className="w-4 h-4 text-primary flex-shrink-0 mt-0.5" />
                          <span className="text-sm text-muted-foreground">
                            Detected <strong className="text-foreground">3 issues</strong> and provided <strong className="text-foreground">3 suggestions</strong>
                          </span>
                        </div>
                      </div>
                    </div>
                  )}

                  {capability.id === "test-obsolescence" && (
                    <div className="mt-6">
                      <h3 className="text-lg font-display font-semibold text-foreground mb-3">
                        Key Insights
                      </h3>
                      <div className="space-y-2">
                        <div className="flex items-start gap-2">
                          <AlertTriangle className="w-4 h-4 text-red-600 flex-shrink-0 mt-0.5" />
                          <span className="text-sm text-muted-foreground">
                            Test marked as <strong className="text-foreground">obsolete</strong> with <strong className="text-foreground">92% confidence</strong>
                          </span>
                        </div>
                        <div className="flex items-start gap-2">
                          <CheckCircle2 className="w-4 h-4 text-primary flex-shrink-0 mt-0.5" />
                          <span className="text-sm text-muted-foreground">
                            Recommendation: Archive or rewrite to test current OAuth flow
                          </span>
                        </div>
                      </div>
                    </div>
                  )}

                  {capability.id === "conflict-resolution" && (
                    <div className="mt-6">
                      <h3 className="text-lg font-display font-semibold text-foreground mb-3">
                        Key Insights
                      </h3>
                      <div className="space-y-2">
                        <div className="flex items-start gap-2">
                          <CheckCircle2 className="w-4 h-4 text-green-600 flex-shrink-0 mt-0.5" />
                          <span className="text-sm text-muted-foreground">
                            Resolution: <strong className="text-foreground">Accept incoming branch</strong> with <strong className="text-foreground">95% confidence</strong>
                          </span>
                        </div>
                        <div className="flex items-start gap-2">
                          <CheckCircle2 className="w-4 h-4 text-primary flex-shrink-0 mt-0.5" />
                          <span className="text-sm text-muted-foreground">
                            Async pattern consistent with 87% of codebase methods
                          </span>
                        </div>
                      </div>
                    </div>
                  )}

                  {capability.id === "root-cause" && (
                    <div className="mt-6">
                      <h3 className="text-lg font-display font-semibold text-foreground mb-3">
                        Key Insights
                      </h3>
                      <div className="space-y-2">
                        <div className="flex items-start gap-2">
                          <Bug className="w-4 h-4 text-red-600 flex-shrink-0 mt-0.5" />
                          <span className="text-sm text-muted-foreground">
                            Root cause: <strong className="text-foreground">Null reference in discount calculation</strong> (88% confidence)
                          </span>
                        </div>
                        <div className="flex items-start gap-2">
                          <CheckCircle2 className="w-4 h-4 text-primary flex-shrink-0 mt-0.5" />
                          <span className="text-sm text-muted-foreground">
                            Identified <strong className="text-foreground">2 related issues</strong> in codebase
                          </span>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              );
            })}
          </Card>
        )}

        {/* CTA */}
        <Card className="p-8 border-2 border-primary/30 bg-primary/5 mt-12">
          <h2 className="text-2xl font-display font-bold text-foreground mb-4">
            Ready to Deploy Local AI?
          </h2>
          <p className="text-muted-foreground mb-6">
            All these capabilities run locally on your workstations with vLLM (production) or Ollama (development). 
            No cloud dependencies, complete data privacy, and zero latency for AI-powered SDLC automation.
          </p>
          <div className="flex gap-4">
            <Button className="bg-primary text-primary-foreground hover:bg-primary/90">
              View Quick Start Guide
            </Button>
            <Button variant="outline" className="border-2 border-primary text-primary hover:bg-primary hover:text-primary-foreground">
              Setup vLLM/Ollama
            </Button>
          </div>
        </Card>
      </div>
    </Layout>
  );
}
