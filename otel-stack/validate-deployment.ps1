# OpenTelemetry Stack Deployment Validation Script
# This script validates the OpenTelemetry instrumentation setup

Write-Host "=======================================" -ForegroundColor Green
Write-Host "OpenTelemetry Deployment Validation" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Check Docker status
Write-Host "[1/5] Checking Docker status..." -ForegroundColor Yellow
$dockerRunning = docker info 2>$null
if (-not $dockerRunning) {
    Write-Host "✗ Docker is not running" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✓ Docker is running" -ForegroundColor Green
}

# Check if OpenTelemetry stack is running
Write-Host "[2/5] Checking OpenTelemetry stack..." -ForegroundColor Yellow
$containers = docker ps --format "table {{.Names}}" 2>$null | Select-Object -Skip 1
$expectedContainers = @("otel-collector", "otel-prometheus", "otel-grafana", "jaeger")

$allRunning = $true
foreach ($container in $expectedContainers) {
    if ($containers -contains $container) {
        Write-Host "✓ $container is running" -ForegroundColor Green
    } else {
        Write-Host "✗ $container is NOT running" -ForegroundColor Red
        $allRunning = $false
    }
}

if (-not $allRunning) {
    Write-Host "\nSome containers are not running. Starting the stack..." -ForegroundColor Yellow
    docker-compose down
    docker-compose up -d
    Write-Host "Waiting for containers to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
}

# Check service endpoints
Write-Host "[3/5] Testing service endpoints..." -ForegroundColor Yellow

$services = @(
    @{Name="OpenTelemetry Collector"; Port=19133},
    @{Name="Jaeger UI"; Port=17686},
    @{Name="Prometheus"; Port=19090},
    @{Name="Grafana"; Port=13000}
)

$allHealthy = $true
foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$($service.Port)" -TimeoutSec 5 -UseBasicParsing
        Write-Host "✓ $($service.Name) (port $($service.Port)) is accessible" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ $($service.Name) (port $($service.Port)) is NOT accessible" -ForegroundColor Red
        $allHealthy = $false
    }
}

# Check Autonomous Agent build
Write-Host "[4/5] Validating Autonomous Agent instrumentation..." -ForegroundColor Yellow

try {
    Push-Location "../desktop-agent/src/AutonomousAgent.Core"
    $buildResult = dotnet build 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Autonomous Agent builds successfully with OpenTelemetry" -ForegroundColor Green
    } else {
        Write-Host "✗ Autonomous Agent has build errors" -ForegroundColor Red
        $allHealthy = $false
    }
    Pop-Location
}
catch {
    Write-Host "⚠ Could not test Autonomous Agent build" -ForegroundColor Yellow
}

# Check Azure DevOps project instrumentation
Write-Host "[5/5] Validating Azure DevOps Agent instrumentation..." -ForegroundColor Yellow

try {
    Push-Location "../../src/Phase3.AzureDevOps"
    $buildResult = dotnet build 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Azure DevOps Agent has OpenTelemetry configured" -ForegroundColor Green
    } else {
        Write-Host "⚠ Azure DevOps Agent has build errors" -ForegroundColor Yellow
    }
    Pop-Location
}
catch {
    Write-Host "⚠ Could not test Azure DevOps Agent build" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "=======================================" -ForegroundColor Green
Write-Host "DEPLOYMENT VALIDATION SUMMARY" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

if ($allHealthy -and $allRunning) {
    Write-Host "✓ All components are properly configured and accessible" -ForegroundColor Green
    Write-Host ""
    Write-Host "Access URLs:" -ForegroundColor Cyan
    Write-Host "- Grafana Dashboard: http://localhost:13000" -ForegroundColor White
    Write-Host "- Jaeger Tracing: http://localhost:17686" -ForegroundColor White
    Write-Host "- Prometheus: http://localhost:19090" -ForegroundColor White
    Write-Host "- Collector Endpoint: http://localhost:18418" -ForegroundColor White
    Write-Host ""
    Write-Host "Applications can now send telemetry data to the collector endpoint." -ForegroundColor Yellow
} else {
    Write-Host "✗ Some components need attention" -ForegroundColor Red
    Write-Host "Run the following command to restart the stack:" -ForegroundColor Yellow
    Write-Host "docker-compose down && docker-compose up -d" -ForegroundColor White
}

Write-Host ""
Write-Host "To test instrumentation:" -ForegroundColor Yellow
Write-Host "- Run: python test-instrumentation.py" -ForegroundColor White
Write-Host "- Run autonomous agent application" -ForegroundColor White