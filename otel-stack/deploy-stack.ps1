# OpenTelemetry Stack Deployment Script for Windows
# This script starts the OpenTelemetry observability stack

Write-Host "=======================================" -ForegroundColor Green
Write-Host "OpenTelemetry Stack Deployment" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Check if Docker is running
$dockerRunning = docker info 2>$null
if (-not $dockerRunning) {
    Write-Host "ERROR: Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Set environment variables
$env:GRAFANA_PASSWORD = "${env:GRAFANA_PASSWORD:-admin123}"

Write-Host "Deploying OpenTelemetry stack..." -ForegroundColor Yellow

# Stop and remove existing containers
try {
    Write-Host "Stopping existing containers..." -ForegroundColor Yellow
    docker-compose down
}
catch {
    Write-Host "Note: No containers to stop" -ForegroundColor Blue
}

# Build and start the stack
Write-Host "Starting OpenTelemetry stack..." -ForegroundColor Yellow
docker-compose up -d

# Wait for services to start
Write-Host "Waiting for services to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Check service status
Write-Host "Checking service status..." -ForegroundColor Yellow

$services = @(
    @{Name="OpenTelemetry Collector"; Port=19133},
    @{Name="Jaeger Tracing"; Port=17686},
    @{Name="Prometheus"; Port=19090},
    @{Name="Grafana"; Port=13000}
)

foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$($service.Port)" -TimeoutSec 5 -UseBasicParsing
        Write-Host "✓ $($service.Name) is running on port $($service.Port)" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠ $($service.Name) may not be ready yet (port $($service.Port))" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "OpenTelemetry Stack is now running!" -ForegroundColor Green
Write-Host ""
Write-Host "Access URLs:" -ForegroundColor Cyan
Write-Host "- OpenTelemetry Collector: http://localhost:18418 (HTTP), localhost:18417 (gRPC)" -ForegroundColor White
Write-Host "- Jaeger UI: http://localhost:17686" -ForegroundColor White
Write-Host "- Prometheus: http://localhost:19090" -ForegroundColor White
Write-Host "- Grafana: http://localhost:13000" -ForegroundColor White
Write-Host "    Username: admin" -ForegroundColor White
Write-Host "    Password: $env:GRAFANA_PASSWORD" -ForegroundColor White
Write-Host ""
Write-Host "To stop the stack, run: docker-compose down" -ForegroundColor Yellow
Write-Host "To view logs, run: docker-compose logs -f" -ForegroundColor Yellow