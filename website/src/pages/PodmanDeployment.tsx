import Layout from "@/components/Layout";
import { Card } from "@/components/ui/card";
import { CheckCircle2, Terminal, Container, Settings, Shield, AlertCircle, Package } from "lucide-react";
import { SEO } from '@/components/SEO';

export default function PodmanDeployment() {
  return (
    <Layout>
      <SEO 
        title="Podman/Docker Container Deployment - CPU Agents for SDLC"
        description="Deploy CPU Agents using Podman or Docker containers for isolated, reproducible deployments with volume mounting, networking, and orchestration support."
        keywords="Podman deployment, Docker container, containerization, CPU Agents, Azure DevOps container, rootless containers"
      />
      
      <section className="bg-accent border-b-2 border-border">
        <div className="container py-16">
          <div className="label-swiss mb-4">CONTAINER DEPLOYMENT</div>
          <h1 className="text-5xl md:text-6xl font-display font-semibold text-foreground mb-6">
            Podman/Docker Container Deployment
          </h1>
          <p className="text-lg text-muted-foreground max-w-2xl">
            Run CPU Agents in isolated container environments using Podman or Docker for 
            reproducible deployments, easy scaling, and simplified dependency management.
          </p>
        </div>
      </section>

      <section className="container py-16">
        <div className="max-w-4xl mx-auto space-y-8">
          
          {/* Why Containers */}
          <Card className="p-8 bg-primary/5 border-primary/20">
            <h2 className="text-3xl font-display font-semibold text-foreground mb-6 flex items-center gap-3">
              <Container className="w-8 h-8 text-primary" />
              Why Use Containers?
            </h2>
            <div className="grid md:grid-cols-2 gap-4">
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Isolation</div>
                  <div className="text-sm text-muted-foreground">Run agents in isolated environments without affecting host system</div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Reproducibility</div>
                  <div className="text-sm text-muted-foreground">Consistent deployments across dev, staging, and production</div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Portability</div>
                  <div className="text-sm text-muted-foreground">Deploy anywhere: Windows, Linux, macOS, cloud, or on-premises</div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Easy Scaling</div>
                  <div className="text-sm text-muted-foreground">Spin up multiple agent instances with simple commands</div>
                </div>
              </div>
            </div>
          </Card>

          {/* Prerequisites */}
          <Card className="p-8">
            <h2 className="text-3xl font-display font-semibold text-foreground mb-6 flex items-center gap-3">
              <Settings className="w-8 h-8 text-primary" />
              Prerequisites
            </h2>
            <div className="space-y-4">
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Podman 4.0+ or Docker 20.10+</div>
                  <div className="text-sm text-muted-foreground">
                    Install Podman: <a href="https://podman.io/getting-started/installation" className="text-primary hover:underline" target="_blank" rel="noopener noreferrer">podman.io</a> | 
                    Install Docker: <a href="https://docs.docker.com/get-docker/" className="text-primary hover:underline ml-1" target="_blank" rel="noopener noreferrer">docker.com</a>
                  </div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Git (for cloning repository)</div>
                  <div className="text-sm text-muted-foreground">Version control to clone the CPU Agents repository</div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Azure DevOps Organization & Project</div>
                  <div className="text-sm text-muted-foreground">Active Azure DevOps project with API access enabled</div>
                </div>
              </div>
            </div>
          </Card>

          {/* Dockerfile */}
          <Card className="p-8">
            <h2 className="text-3xl font-display font-semibold text-foreground mb-6 flex items-center gap-3">
              <Package className="w-8 h-8 text-primary" />
              Dockerfile
            </h2>
            <p className="text-muted-foreground mb-4">
              Create a <code className="bg-muted px-2 py-1 rounded text-sm">Dockerfile</code> in the project root:
            </p>
            <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
              <code className="text-sm">{`FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Phase3.AzureDevOps.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Phase3.AzureDevOps.dll"]`}</code>
            </pre>
          </Card>

          {/* Build & Run with Podman */}
          <Card className="p-8">
            <h2 className="text-3xl font-display font-semibold text-foreground mb-6 flex items-center gap-3">
              <Terminal className="w-8 h-8 text-primary" />
              Build & Run with Podman
            </h2>
            
            <div className="space-y-6">
              {/* Step 1 */}
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold">1</div>
                  <h3 className="text-xl font-semibold">Clone Repository</h3>
                </div>
                <div className="ml-11">
                  <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                    <code className="text-sm">{`git clone https://github.com/Lev0n82/CPU-Agents-for-SDLC.git
cd CPU-Agents-for-SDLC/src/Phase3.AzureDevOps`}</code>
                  </pre>
                </div>
              </div>

              {/* Step 2 */}
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold">2</div>
                  <h3 className="text-xl font-semibold">Create Configuration File</h3>
                </div>
                <div className="ml-11 space-y-3">
                  <p className="text-muted-foreground">Create <code className="bg-muted px-2 py-1 rounded text-sm">appsettings.json</code>:</p>
                  <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                    <code className="text-sm">{`{
  "AzureDevOps": {
    "OrganizationUrl": "https://dev.azure.com/your-org",
    "ProjectName": "YourProject"
  },
  "Authentication": {
    "Method": "PAT",
    "PAT": "your-personal-access-token-here"
  },
  "Secrets": {
    "Provider": "DPAPI",
    "DPAPI": {
      "StorePath": "/app/data/secrets"
    }
  },
  "Concurrency": {
    "ClaimDurationMinutes": 15,
    "StaleClaimCheckIntervalMinutes": 5
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}`}</code>
                  </pre>
                </div>
              </div>

              {/* Step 3 */}
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold">3</div>
                  <h3 className="text-xl font-semibold">Build Container Image</h3>
                </div>
                <div className="ml-11">
                  <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                    <code className="text-sm">podman build -t cpu-agents-phase3:latest .</code>
                  </pre>
                  <p className="text-sm text-muted-foreground mt-2">
                    For Docker, replace <code className="bg-muted px-1 rounded">podman</code> with <code className="bg-muted px-1 rounded">docker</code>
                  </p>
                </div>
              </div>

              {/* Step 4 */}
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold">4</div>
                  <h3 className="text-xl font-semibold">Run Container</h3>
                </div>
                <div className="ml-11">
                  <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                    <code className="text-sm">{`podman run -d \\
  --name cpu-agents-phase3 \\
  -v ./appsettings.json:/app/appsettings.json:ro \\
  -v ./data:/app/data \\
  --restart unless-stopped \\
  cpu-agents-phase3:latest`}</code>
                  </pre>
                  <div className="flex items-start gap-2 p-3 bg-primary/10 border border-primary/20 rounded mt-3">
                    <AlertCircle className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <div className="text-sm">
                      <strong>Volume Mounts:</strong>
                      <ul className="list-disc ml-4 mt-1">
                        <li><code className="bg-muted px-1 rounded">appsettings.json</code> - Configuration (read-only)</li>
                        <li><code className="bg-muted px-1 rounded">./data</code> - Persistent storage for secrets and state</li>
                      </ul>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </Card>

          {/* Build & Run with Docker */}
          <Card className="p-8 bg-accent">
            <h2 className="text-3xl font-display font-semibold text-foreground mb-6 flex items-center gap-3">
              <Terminal className="w-8 h-8 text-primary" />
              Build & Run with Docker
            </h2>
            
            <div className="space-y-4">
              <div>
                <h3 className="text-lg font-semibold mb-2">Build Image</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">docker build -t cpu-agents-phase3:latest .</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Run Container</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">{`docker run -d \\
  --name cpu-agents-phase3 \\
  -v $(pwd)/appsettings.json:/app/appsettings.json:ro \\
  -v $(pwd)/data:/app/data \\
  --restart unless-stopped \\
  cpu-agents-phase3:latest`}</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Using Docker Compose</h3>
                <p className="text-muted-foreground mb-2">Create <code className="bg-muted px-2 py-1 rounded text-sm">docker-compose.yml</code>:</p>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">{`version: '3.8'

services:
  cpu-agents:
    build: .
    container_name: cpu-agents-phase3
    volumes:
      - ./appsettings.json:/app/appsettings.json:ro
      - ./data:/app/data
    restart: unless-stopped
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"`}</code>
                </pre>
                <p className="text-sm text-muted-foreground mt-2">Run with:</p>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto mt-2">
                  <code className="text-sm">docker-compose up -d</code>
                </pre>
              </div>
            </div>
          </Card>

          {/* Container Management */}
          <Card className="p-8">
            <h2 className="text-3xl font-display font-semibold text-foreground mb-6 flex items-center gap-3">
              <Settings className="w-8 h-8 text-primary" />
              Container Management
            </h2>
            
            <div className="space-y-4">
              <div>
                <h3 className="text-lg font-semibold mb-2">View Container Logs</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">podman logs -f cpu-agents-phase3</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Check Container Status</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">podman ps -a</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Stop Container</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">podman stop cpu-agents-phase3</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Start Container</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">podman start cpu-agents-phase3</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Restart Container</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">podman restart cpu-agents-phase3</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Execute Commands in Container</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">podman exec -it cpu-agents-phase3 /bin/bash</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Remove Container</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">{`podman stop cpu-agents-phase3
podman rm cpu-agents-phase3`}</code>
                </pre>
              </div>
            </div>
          </Card>

          {/* Security Best Practices */}
          <Card className="p-8 bg-primary/5 border-primary/20">
            <h2 className="text-3xl font-display font-semibold text-foreground mb-6 flex items-center gap-3">
              <Shield className="w-8 h-8 text-primary" />
              Security Best Practices
            </h2>
            
            <div className="space-y-4">
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Use Rootless Containers</div>
                  <div className="text-sm text-muted-foreground">
                    Podman supports rootless containers by default. For Docker, enable rootless mode for enhanced security.
                  </div>
                </div>
              </div>

              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Mount Configuration as Read-Only</div>
                  <div className="text-sm text-muted-foreground">
                    Use <code className="bg-muted px-1 rounded">:ro</code> flag when mounting appsettings.json to prevent accidental modifications.
                  </div>
                </div>
              </div>

              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Use Secrets Management</div>
                  <div className="text-sm text-muted-foreground">
                    For production, use Docker/Podman secrets or external secrets managers instead of mounting configuration files.
                  </div>
                </div>
              </div>

              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Limit Container Resources</div>
                  <div className="text-sm text-muted-foreground">
                    Use <code className="bg-muted px-1 rounded">--memory</code> and <code className="bg-muted px-1 rounded">--cpus</code> flags to prevent resource exhaustion.
                  </div>
                </div>
              </div>

              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Regular Image Updates</div>
                  <div className="text-sm text-muted-foreground">
                    Rebuild images regularly to include latest security patches from base images.
                  </div>
                </div>
              </div>
            </div>
          </Card>

        </div>
      </section>
    </Layout>
  );
}
