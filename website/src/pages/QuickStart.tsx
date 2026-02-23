import Layout from "@/components/Layout";
import { Button } from "@/components/ui/button";
import { CheckCircle2, Download, Play, Settings, Copy } from "lucide-react";

export default function QuickStart() {
  const steps = [
    {
      number: "01",
      title: "Prerequisites",
      icon: Settings,
      content: [
        "Windows 11 (Pro/Enterprise) or Linux (Ubuntu 22.04+)",
        ".NET 8.0 SDK (Windows) or Rust 1.70+ (Linux)",
        "Administrator privileges",
        "Git for Windows/Linux",
        "vLLM (production) or Ollama (development) for local AI",
        "PostgreSQL 15+ and Oracle 19c+ for data storage",
      ],
      command: null,
    },
    {
      number: "02",
      title: "Install AI Backend (vLLM or Ollama)",
      icon: Download,
      content: [
        "Option A: vLLM for production (GPU acceleration, batching)",
        "Option B: Ollama for development (easier setup, CPU-optimized)",
        "Pull Granite 4, Phi-3, and Llama 3 models",
        "Verify models are ready for local inference"
      ],
      command: "# Option A: Install vLLM (Production)\npip install vllm\nvllm serve granite-code:8b --port 8000\n\n# Option B: Install Ollama (Development)\ncurl -fsSL https://ollama.com/install.sh | sh\n\n# Pull AI models (both options)\nollama pull granite-code:8b\nollama pull phi3:3.8b\nollama pull llama3:8b\n\n# Verify installation\nollama list",
    },
    {
      number: "03",
      title: "Clone Repository",
      icon: Download,
      content: ["Clone the CPU Agents repository from GitHub"],
      command:
        "git clone https://github.com/Lev0n82/CPU-Agents-for-SDLC.git\ncd CPU-Agents-for-SDLC",
    },
    {
      number: "04",
      title: "Configure Agent",
      icon: Settings,
      content: [
        "Copy appsettings.example.json to appsettings.json",
        "Configure Azure DevOps connection string",
        "Set Ollama API endpoint (default: http://localhost:11434)",
        "Configure PostgreSQL and Oracle connection strings",
        "Set authentication method (PAT, Certificate, or MSAL)"
      ],
      command: `# Copy configuration template\ncp appsettings.example.json appsettings.json\n\n# Edit configuration\n{\n  \"AzureDevOps\": {\n    \"OrganizationUrl\": \"https://dev.azure.com/your-org\",\n    \"Project\": \"your-project\",\n    \"AuthMethod\": \"PAT\"\n  },\n  \"Ollama\": {\n    \"ApiUrl\": \"http://localhost:11434\",\n    \"CodeReviewModel\": \"granite-code:8b\",\n    \"TestAnalysisModel\": \"phi3:3.8b\"\n  },\n  \"Database\": {\n    \"PostgreSQL\": \"Host=localhost;Database=cpuagents\",\n    \"Oracle\": \"Data Source=localhost:1521/ORCL\"\n  }\n}`,
    },
    {
      number: "05",
      title: "Navigate to Agent",
      icon: Play,
      content: ["Navigate to the desktop agent directory based on your OS"],
      command:
        "# Windows (.NET)\ncd desktop-agent\\src\\AutonomousAgent.Core\n\n# Linux (Rust)\ncd desktop-agent-rust\\src",
    },
    {
      number: "06",
      title: "Run Agent",
      icon: Play,
      content: [
        "Execute the agent in development mode",
        "Self-tests will run automatically on startup",
        "Watch the console for test results",
        "Agent will start polling for work items"
      ],
      command: "# Windows (.NET)\ndotnet run\n\n# Linux (Rust)\ncargo run --release",
    },
  ];

  return (
    <Layout>
      {/* Header */}
      <section className="bg-accent border-b-2 border-border">
        <div className="container py-16">
          <div className="label-swiss mb-4">DEPLOYMENT GUIDE</div>
          <h1 className="text-5xl md:text-6xl font-display font-semibold text-foreground mb-6">
            Quick Start
          </h1>
          <p className="text-lg text-muted-foreground max-w-2xl">
            Get your autonomous agent running on Windows 11 in under{" "}
            <span className="text-primary font-medium">5 minutes</span>. Follow
            these steps to deploy the desktop agent in development mode.
          </p>
        </div>
      </section>

      {/* Steps */}
      <section className="container py-16">
        <div className="max-w-4xl mx-auto space-y-12">
          {steps.map((step, index) => (
            <div key={index} className="data-card group">
              {/* Step Header */}
              <div className="flex items-start gap-6 mb-6">
                <div className="flex-shrink-0">
                  <div className="w-16 h-16 bg-primary flex items-center justify-center">
                    <step.icon className="w-8 h-8 text-primary-foreground" />
                  </div>
                </div>
                <div className="flex-1">
                  <div className="label-swiss mb-2">STEP {step.number}</div>
                  <h2 className="text-3xl font-display font-semibold text-foreground mb-4">
                    {step.title}
                  </h2>
                  <ul className="space-y-3">
                    {step.content.map((item, i) => (
                      <li
                        key={i}
                        className="flex items-start gap-3 text-muted-foreground"
                      >
                        <CheckCircle2 className="w-5 h-5 text-primary flex-shrink-0 mt-0.5" />
                        <span>{item}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>

              {/* Command */}
              {step.command && (
                <div className="bg-accent border-2 border-border p-6">
                  <div className="flex items-center justify-between mb-3">
                    <span className="label-swiss text-muted-foreground">
                      COMMAND
                    </span>
                    <Button
                      size="sm"
                      variant="outline"
                      className="border-2 border-primary text-primary hover:bg-primary hover:text-primary-foreground"
                      onClick={() => {
                        navigator.clipboard.writeText(step.command || "");
                      }}
                    >
                      <Copy className="w-4 h-4 mr-2" />
                      Copy
                    </Button>
                  </div>
                  <pre className="text-foreground font-mono text-sm overflow-x-auto">
                    <code>{step.command}</code>
                  </pre>
                </div>
              )}
            </div>
          ))}
        </div>

        {/* Success Message */}
        <div className="max-w-4xl mx-auto mt-16">
          <div className="bg-primary/10 border-2 border-primary p-8">
            <div className="flex items-start gap-6">
              <CheckCircle2 className="w-12 h-12 text-primary flex-shrink-0" />
              <div>
                <h3 className="text-2xl font-display font-semibold text-foreground mb-3">
                  Success! Agent is Running
                </h3>
                <p className="text-muted-foreground mb-6 leading-relaxed">
                  Your autonomous agent is now operational. You should see
                  self-test results in the console window. The agent will
                  perform system-level validation before becoming fully
                  operational.
                </p>
                <div className="space-y-2 text-sm text-muted-foreground">
                  <div className="flex items-center gap-3">
                    <div className="w-1 h-1 bg-primary" />
                    <span>Expected output: "Self-tests passed: 6/6"</span>
                  </div>
                  <div className="flex items-center gap-3">
                    <div className="w-1 h-1 bg-primary" />
                    <span>Agent status: OPERATIONAL</span>
                  </div>
                  <div className="flex items-center gap-3">
                    <div className="w-1 h-1 bg-primary" />
                    <span>Next reboot scheduled: 00:00 (configurable)</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Troubleshooting */}
        <div className="max-w-4xl mx-auto mt-16">
          <div className="label-swiss mb-6">TROUBLESHOOTING</div>
          <div className="space-y-6">
            <div className="data-card">
              <h3 className="text-xl font-display font-semibold text-foreground mb-3">
                Ollama Connection Failed
              </h3>
              <p className="text-sm text-muted-foreground mb-4">
                If the agent cannot connect to Ollama, verify that Ollama is running and accessible at the configured endpoint.
              </p>
              <div className="bg-accent border-2 border-border p-4">
                <pre className="text-foreground font-mono text-sm">
                  <code># Check if Ollama is running{"\n"}curl http://localhost:11434/api/tags{"\n\n"}# Restart Ollama service{"\n"}systemctl restart ollama  # Linux{"\n"}# or check Task Manager for Ollama process (Windows)</code>
                </pre>
              </div>
            </div>

            <div className="data-card">
              <h3 className="text-xl font-display font-semibold text-foreground mb-3">
                Azure DevOps Authentication Error
              </h3>
              <p className="text-sm text-muted-foreground mb-4">
                If authentication fails, verify your PAT token has the correct scopes: Work Items (Read, Write), Code (Read), Test Management (Read).
              </p>
              <div className="bg-accent border-2 border-border p-4">
                <pre className="text-foreground font-mono text-sm">
                  <code># Test authentication manually{"\n"}curl -u :YOUR_PAT_TOKEN https://dev.azure.com/your-org/_apis/projects{"\n\n"}# Required scopes:{"\n"}# - vso.work (Read & Write){"\n"}# - vso.code (Read){"\n"}# - vso.test (Read)</code>
                </pre>
              </div>
            </div>

            <div className="data-card">
              <h3 className="text-xl font-display font-semibold text-foreground mb-3">
                Database Connection Issues
              </h3>
              <p className="text-sm text-muted-foreground mb-4">
                Ensure PostgreSQL and Oracle databases are running and accessible. Check connection strings in appsettings.json.
              </p>
              <div className="bg-accent border-2 border-border p-4">
                <pre className="text-foreground font-mono text-sm">
                  <code># Test PostgreSQL connection{"\n"}psql -h localhost -U postgres -d cpuagents{"\n\n"}# Test Oracle connection{"\n"}sqlplus username/password@localhost:1521/ORCL{"\n\n"}# Create databases if missing{"\n"}createdb cpuagents  # PostgreSQL</code>
                </pre>
              </div>
            </div>

            <div className="data-card">
              <h3 className="text-xl font-display font-semibold text-foreground mb-3">
                Self-Tests Failing
              </h3>
              <p className="text-sm text-muted-foreground mb-4">
                If self-tests fail on startup, check the console output for specific error messages. Common issues include missing dependencies or incorrect configuration.
              </p>
              <div className="bg-accent border-2 border-border p-4">
                <pre className="text-foreground font-mono text-sm">
                  <code># Run with verbose logging{"\n"}dotnet run --configuration Debug{"\n\n"}# Check logs directory{"\n"}tail -f logs/agent-*.log{"\n\n"}# Verify all dependencies{"\n"}dotnet restore  # Windows{"\n"}cargo check    # Linux</code>
                </pre>
              </div>
            </div>
          </div>
        </div>

        {/* Next Steps */}
        <div className="max-w-4xl mx-auto mt-16">
          <div className="label-swiss mb-6">NEXT STEPS</div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <a href="/windows-deployment">
              <div className="data-card group cursor-pointer">
                <h3 className="text-xl font-display font-semibold text-foreground mb-3 group-hover:text-primary transition-colors">
                  Windows Service
                </h3>
                <p className="text-sm text-muted-foreground leading-relaxed">
                  Deploy as a Windows Service for production use with automatic
                  startup.
                </p>
              </div>
            </a>
            <a href="/podman-deployment">
              <div className="data-card group cursor-pointer">
                <h3 className="text-xl font-display font-semibold text-foreground mb-3 group-hover:text-primary transition-colors">
                  Podman Container
                </h3>
                <p className="text-sm text-muted-foreground leading-relaxed">
                  Run in an isolated container environment using Podman.
                </p>
              </div>
            </a>
          </div>
        </div>
      </section>
    </Layout>
  );
}
