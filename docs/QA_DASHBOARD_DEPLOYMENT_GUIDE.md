# QA Dashboard Deployment Guide

## Overview
The QA Dashboard implementation provides comprehensive QA metrics tracking for Azure DevOps projects with secure credential management through Azure Key Vault.

## Prerequisites

### 1. Azure Key Vault Setup
- Create Azure Key Vault: `qa-dev-app.vault.azure.net`
- Create secret named `AzureAgentPat` containing the Azure DevOps PAT
- Grant access to the application identity to read secrets

### 2. Azure DevOps PAT
- Generate Personal Access Token with these scopes:
  - Work Items: Read
  - Test Management: Read
  - Build: Read (if tracking build metrics)
- Save token to Azure Key Vault as `AzureAgentPat` secret

### 3. Local Development Setup

#### Option A: Azure CLI Authentication
```bash
# Login to Azure CLI
az login

# Set default subscription (if needed)
az account set --subscription "Your-Subscription-Name"
```

#### Option B: Environment Variables for Service Principal
```bash
# Set these environment variables
export AZURE_TENANT_ID="your-tenant-id"
export AZURE_CLIENT_ID="your-service-principal-id" 
export AZURE_CLIENT_SECRET="your-service-principal-secret"
```

## Deployment Steps

### Step 1: Configure Observability Stack

```bash
# Navigate to otel-stack directory
cd otel-stack

# Start OpenTelemetry observability stack
docker-compose up -d

# Verify services are running
docker-compose ps
```

Expected running services:
- otel-collector:4317
- jaeger:16686 (UI: http://localhost:17686)
- prometheus:9090 (UI: http://localhost:19090)
- grafana:3000 (UI: http://localhost:13000)

### Step 2: Configure Grafana

1. Access Grafana at http://localhost:13000
2. Login with credentials: admin/admin123
3. Import QA Dashboard:
   - Navigate to Dashboards → Import
   - Upload file: `otel-stack/config/grafana/dashboards/qa-dashboard.json`
   - Select Prometheus as data source

### Step 3: Configure Application Settings

Update `src/Phase3.AgentHost/appsettings.json`:

```json
{
  "AzureDevOps": {
    "OrganizationUrl": "https://dev.azure.com/your-organization",
    "ProjectName": "your-project-name",
    "AuthenticationMethod": "PAT"
  },
  "Secrets": {
    "Provider": "AzureKeyVault",
    "AzureKeyVault": {
      "VaultUri": "https://qa-dev-app.vault.azure.net"
    }
  }
}
```

### Step 4: Build and Run QA Dashboard

```bash
# Build the application
cd src/Phase3.AgentHost
dotnet build

# Run the agent host service
dotnet run
```

### Step 5: Verify Data Synchronization

1. Check application logs for QA data sync progress
2. Access Grafana dashboard to view metrics
3. Monitor OpenTelemetry metrics through Prometheus

## Configuration Details

### Azure Key Vault Authentication Methods

The application supports these Azure authentication methods:

#### 1. DefaultAzureCredential (Recommended)
- Automatically tries multiple authentication methods
- Order of authentication attempts:
  1. Environment variables
  2. Managed Identity
  3. Azure CLI
  4. Azure PowerShell
  5. Interactive browser

#### 2. Service Principal
```json
{
  "AzureKeyVault": {
    "VaultUri": "https://qa-dev-app.vault.azure.net",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id", 
    "ClientSecret": "your-client-secret"
  }
}
```

### QA Dashboard Metrics

The dashboard tracks these industry-standard QA metrics:

#### Primary Metrics
- **Escaped Bugs**: Bugs found in production
- **Test Coverage**: Percentage of requirements with test cases
- **Defect Density**: Bugs per requirement
- **Test Case Effectiveness**: Defects found per test case

#### Timeline Metrics
- **Test Case Development Delay**: Days from requirements publication to test case creation
- **Test Execution Delay**: Days from test case creation to first execution

#### Productivity Metrics
- **Test Case Productivity**: Test cases created per day
- **Defect Leakage Rate**: Bugs found post-initial testing

## Security Best Practices

### Credential Management
- Never store secrets in code or configuration files
- Use Azure Key Vault for all sensitive data
- Rotate PAT tokens regularly
- Use Managed Identity where possible

### Network Security
- Use HTTPS for all external connections
- Implement firewall rules for database access
- Use Azure Private Endpoints for Key Vault

### Monitoring & Alerting
- Monitor failed authentication attempts
- Set up alerts for credential expiration
- Track access patterns to detect anomalies

## Troubleshooting

### Common Issues

#### Authentication Failures
```bash
# Check Azure CLI login status
az account show

# Verify Key Vault access
az keyvault secret list --vault-name qa-dev-app
```

#### Data Synchronization Issues
- Check Azure DevOps project permissions
- Verify PAT has correct scopes
- Monitor application logs for API errors

#### Metrics Not Showing
- Verify OpenTelemetry collector is running
- Check Prometheus scrape configuration
- Validate Grafana data source connection

### Debug Mode

Enable detailed logging by setting environment variable:
```bash
export ASPNETCORE_ENVIRONMENT=Development
```

Or modify appsettings.Development.json:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

## Scaling Considerations

### Database Performance
- Monitor SQLite database size
- Implement data retention policies
- Consider migration to SQL Server for large datasets

### Azure DevOps API Limits
- Implement rate limiting
- Use bulk APIs where available
- Monitor API usage quotas

### Memory & Performance
- Monitor memory usage during large sync operations
- Implement pagination for large APIs
- Consider background processing for heavy operations

## Backup & Recovery

### Database Backup
```bash
# Backup SQLite database
cp qa-cache.db qa-cache-backup-$(date +%Y%m%d).db

# Restore from backup
cp qa-cache-backup-*.db qa-cache.db
```

### Key Vault Backup
```bash
# Backup secrets
az keyvault secret backup --file backup-secret.blob --id https://qa-dev-app.vault.azure.net/secrets/AzureAgentPat

# Restore secret
az keyvault secret restore --file backup-secret.blob
```

## Compliance & Auditing

### Access Logging
- Enable Key Vault diagnostic logs
- Monitor Azure DevOps access logs
- Track dashboard usage patterns

### Data Retention
- Define data retention policies
- Implement automated cleanup
- Archive historical data for compliance

This deployment guide provides a complete setup for the QA Dashboard with secure Azure Key Vault integration and industry-standard DevOps practices.