import Layout from "@/components/Layout";
import { Card } from "@/components/ui/card";
import { CheckCircle2, Terminal, Settings, Shield, Server, AlertCircle } from "lucide-react";
import { SEO } from '@/components/SEO';

export default function WindowsDeployment() {
  return (
    <Layout>
      <SEO 
        title="Windows Service Deployment - CPU Agents for SDLC"
        description="Complete guide to deploying CPU Agents as a Windows Service for production use with automatic startup, configuration, and security hardening."
        keywords="Windows Service, CPU Agents deployment, Windows Server, .NET service, production deployment, Azure DevOps agents"
      />
      
      <section className="bg-accent border-b-2 border-border">
        <div className="container py-16">
          <div className="label-swiss mb-4">PRODUCTION DEPLOYMENT</div>
          <h1 className="text-5xl md:text-6xl font-display font-semibold text-foreground mb-6">
            Windows Service Deployment
          </h1>
          <p className="text-lg text-muted-foreground max-w-2xl">
            Deploy CPU Agents as a Windows Service for production use with automatic startup, 
            enterprise authentication, and secure secrets management.
          </p>
        </div>
      </section>

      <section className="container py-16">
        <div className="max-w-4xl mx-auto space-y-8">
          
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
                  <div className="font-semibold mb-1">.NET 8.0 SDK or later</div>
                  <div className="text-sm text-muted-foreground">Download from <a href="https://dotnet.microsoft.com/download" className="text-primary hover:underline" target="_blank" rel="noopener noreferrer">dotnet.microsoft.com</a></div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Windows Server 2019+ or Windows 10+</div>
                  <div className="text-sm text-muted-foreground">Required for Windows Service and Credential Manager features</div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Azure DevOps Organization & Project</div>
                  <div className="text-sm text-muted-foreground">Active Azure DevOps project with API access enabled</div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <CheckCircle2 className="w-5 h-5 text-primary mt-1 flex-shrink-0" />
                <div>
                  <div className="font-semibold mb-1">Azure Key Vault (Optional)</div>
                  <div className="text-sm text-muted-foreground">Recommended for enterprise deployment with certificate-based authentication</div>
                </div>
              </div>
            </div>
          </Card>

          {/* Installation Steps */}
          <Card className="p-8">
            <h2 className="text-3xl font-display font-semibold text-foreground mb-6 flex items-center gap-3">
              <Terminal className="w-8 h-8 text-primary" />
              Installation Steps
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
                  <h3 className="text-xl font-semibold">Restore Dependencies</h3>
                </div>
                <div className="ml-11">
                  <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                    <code className="text-sm">dotnet restore</code>
                  </pre>
                </div>
              </div>

              {/* Step 3 */}
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold">3</div>
                  <h3 className="text-xl font-semibold">Configure Settings</h3>
                </div>
                <div className="ml-11 space-y-3">
                  <p className="text-muted-foreground">Create <code className="bg-muted px-2 py-1 rounded text-sm">appsettings.json</code> in the project root:</p>
                  <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                    <code className="text-sm">{`{
  "AzureDevOps": {
    "OrganizationUrl": "https://dev.azure.com/your-org",
    "ProjectName": "YourProject"
  },
  "Authentication": {
    "Method": "Certificate",
    "Certificate": {
      "TenantId": "your-tenant-id",
      "ClientId": "your-client-id",
      "Thumbprint": "certificate-thumbprint"
    }
  },
  "Secrets": {
    "Provider": "AzureKeyVault",
    "KeyVault": {
      "VaultUri": "https://your-vault.vault.azure.net/"
    }
  },
  "Concurrency": {
    "ClaimDurationMinutes": 15,
    "StaleClaimCheckIntervalMinutes": 5
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}`}</code>
                  </pre>
                </div>
              </div>

              {/* Step 4 */}
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold">4</div>
                  <h3 className="text-xl font-semibold">Build Application</h3>
                </div>
                <div className="ml-11">
                  <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                    <code className="text-sm">dotnet build --configuration Release</code>
                  </pre>
                </div>
              </div>

              {/* Step 5 */}
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold">5</div>
                  <h3 className="text-xl font-semibold">Run Tests</h3>
                </div>
                <div className="ml-11">
                  <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                    <code className="text-sm">dotnet test</code>
                  </pre>
                </div>
              </div>

              {/* Step 6 */}
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold">6</div>
                  <h3 className="text-xl font-semibold">Publish Application</h3>
                </div>
                <div className="ml-11">
                  <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                    <code className="text-sm">dotnet publish --configuration Release --output ./publish</code>
                  </pre>
                </div>
              </div>

              {/* Step 7 */}
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold">7</div>
                  <h3 className="text-xl font-semibold">Deploy as Windows Service</h3>
                </div>
                <div className="ml-11 space-y-3">
                  <p className="text-muted-foreground">Open Command Prompt or PowerShell as Administrator:</p>
                  <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                    <code className="text-sm">{`sc create "CPUAgentsPhase3" binPath="C:\\Path\\To\\publish\\Phase3.AzureDevOps.exe"
sc description "CPUAgentsPhase3" "Autonomous AI agents for Azure DevOps SDLC automation"
sc start "CPUAgentsPhase3"`}</code>
                  </pre>
                  <div className="flex items-start gap-2 p-3 bg-primary/10 border border-primary/20 rounded">
                    <AlertCircle className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <div className="text-sm">
                      <strong>Note:</strong> Replace <code className="bg-muted px-1 rounded">C:\\Path\\To\\publish</code> with your actual publish directory path.
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </Card>

          {/* Security Hardening */}
          <Card className="p-8 bg-primary/5 border-primary/20">
            <h2 className="text-3xl font-display font-semibold text-foreground mb-6 flex items-center gap-3">
              <Shield className="w-8 h-8 text-primary" />
              Security Hardening
            </h2>
            
            <div className="space-y-6">
              <div>
                <h3 className="text-xl font-semibold mb-3">1. Certificate-Based Authentication</h3>
                <p className="text-muted-foreground mb-3">Recommended for production environments:</p>
                <ul className="space-y-2 text-muted-foreground">
                  <li className="flex items-start gap-2">
                    <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <span>Use X.509 certificates instead of Personal Access Tokens (PATs)</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <span>Store certificates in Windows Certificate Store with private key protection</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <span>Use managed identity in Azure environments for automatic credential rotation</span>
                  </li>
                </ul>
              </div>

              <div>
                <h3 className="text-xl font-semibold mb-3">2. Secrets Management</h3>
                <p className="text-muted-foreground mb-3">Use Azure Key Vault for production:</p>
                <ul className="space-y-2 text-muted-foreground">
                  <li className="flex items-start gap-2">
                    <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <span>Never store secrets in configuration files or source control</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <span>Use managed identity for Key Vault access (no credentials needed)</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <CheckCircle2 className="w-5 h-5 text-primary mt-0.5 flex-shrink-0" />
                    <span>Enable Key Vault auditing and access policies for compliance</span>
                  </li>
                </ul>
              </div>

              <div>
                <h3 className="text-xl font-semibold mb-3">3. Network Security</h3>
                <p className="text-muted-foreground mb-3">Configure firewall rules and proxy settings:</p>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">{`{
  "Network": {
    "AllowedIpRanges": [
      "10.0.0.0/8",
      "172.16.0.0/12"
    ],
    "Proxy": {
      "Enabled": true,
      "Url": "http://proxy.company.com:8080",
      "BypassSSLInspection": true
    }
  }
}`}</code>
                </pre>
              </div>
            </div>
          </Card>

          {/* Service Management */}
          <Card className="p-8">
            <h2 className="text-3xl font-display font-semibold text-foreground mb-6 flex items-center gap-3">
              <Server className="w-8 h-8 text-primary" />
              Service Management
            </h2>
            
            <div className="space-y-4">
              <div>
                <h3 className="text-lg font-semibold mb-2">Check Service Status</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">sc query "CPUAgentsPhase3"</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Stop Service</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">sc stop "CPUAgentsPhase3"</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Start Service</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">sc start "CPUAgentsPhase3"</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Configure Auto-Start</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">sc config "CPUAgentsPhase3" start=auto</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">View Service Logs</h3>
                <p className="text-sm text-muted-foreground mb-2">Logs are written to:</p>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">%LOCALAPPDATA%\AutonomousAgent\Logs\</code>
                </pre>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-2">Uninstall Service</h3>
                <pre className="bg-muted p-4 rounded border border-border overflow-x-auto">
                  <code className="text-sm">{`sc stop "CPUAgentsPhase3"
sc delete "CPUAgentsPhase3"`}</code>
                </pre>
              </div>
            </div>
          </Card>

        </div>
      </section>
    </Layout>
  );
}
