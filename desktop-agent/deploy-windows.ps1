# Autonomous Agent - Windows Deployment Script
# This script automates the deployment of the agent as a Windows Service

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Install", "Uninstall", "Restart", "Status")]
    [string]$Action = "Install",
    
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "AutonomousAgent",
    
    [Parameter(Mandatory=$false)]
    [string]$DisplayName = "Autonomous AI Agent",
    
    [Parameter(Mandatory=$false)]
    [string]$Description = "Self-aware autonomous AI agent for enterprise testing and requirements management"
)

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Get the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishDir = Join-Path $scriptDir "publish"
$exePath = Join-Path $publishDir "AutonomousAgent.Core.exe"

function Install-Service {
    Write-Host "=== Installing Autonomous Agent as Windows Service ===" -ForegroundColor Cyan
    
    # Check if service already exists
    $existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($existingService) {
        Write-Host "Service '$ServiceName' already exists. Uninstalling first..." -ForegroundColor Yellow
        Uninstall-Service
    }
    
    # Build and publish the project
    Write-Host "Step 1: Building and publishing the project..." -ForegroundColor Green
    Push-Location $scriptDir
    
    dotnet publish -c Release -r win-x64 --self-contained true -o $publishDir
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    
    Pop-Location
    Write-Host "Build completed successfully." -ForegroundColor Green
    
    # Create the service
    Write-Host "Step 2: Creating Windows Service..." -ForegroundColor Green
    
    sc.exe create $ServiceName binPath=$exePath start=auto
    sc.exe description $ServiceName $Description
    sc.exe config $ServiceName DisplayName=$DisplayName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to create service!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Service created successfully." -ForegroundColor Green
    
    # Start the service
    Write-Host "Step 3: Starting the service..." -ForegroundColor Green
    sc.exe start $ServiceName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "WARNING: Service created but failed to start. Check Event Viewer for details." -ForegroundColor Yellow
    } else {
        Write-Host "Service started successfully." -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "=== Installation Complete ===" -ForegroundColor Cyan
    Write-Host "Service Name: $ServiceName" -ForegroundColor White
    Write-Host "Status: " -NoNewline -ForegroundColor White
    Get-Service -Name $ServiceName | Select-Object Status | Format-Table -HideTableHeaders
    Write-Host ""
    Write-Host "To view logs, open Event Viewer (eventvwr.msc) and check Windows Logs > Application" -ForegroundColor Yellow
}

function Uninstall-Service {
    Write-Host "=== Uninstalling Autonomous Agent Service ===" -ForegroundColor Cyan
    
    # Check if service exists
    $existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $existingService) {
        Write-Host "Service '$ServiceName' does not exist. Nothing to uninstall." -ForegroundColor Yellow
        return
    }
    
    # Stop the service if running
    if ($existingService.Status -eq 'Running') {
        Write-Host "Stopping service..." -ForegroundColor Green
        sc.exe stop $ServiceName
        Start-Sleep -Seconds 2
    }
    
    # Delete the service
    Write-Host "Deleting service..." -ForegroundColor Green
    sc.exe delete $ServiceName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to delete service!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Service uninstalled successfully." -ForegroundColor Green
}

function Restart-Service {
    Write-Host "=== Restarting Autonomous Agent Service ===" -ForegroundColor Cyan
    
    # Check if service exists
    $existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $existingService) {
        Write-Host "ERROR: Service '$ServiceName' does not exist!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Stopping service..." -ForegroundColor Green
    sc.exe stop $ServiceName
    Start-Sleep -Seconds 2
    
    Write-Host "Starting service..." -ForegroundColor Green
    sc.exe start $ServiceName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to start service!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Service restarted successfully." -ForegroundColor Green
}

function Show-Status {
    Write-Host "=== Autonomous Agent Service Status ===" -ForegroundColor Cyan
    
    $existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $existingService) {
        Write-Host "Service '$ServiceName' is not installed." -ForegroundColor Yellow
        return
    }
    
    Write-Host ""
    Get-Service -Name $ServiceName | Format-List Name, DisplayName, Status, StartType
    Write-Host ""
    
    if ($existingService.Status -eq 'Running') {
        Write-Host "The service is currently running." -ForegroundColor Green
    } else {
        Write-Host "The service is NOT running." -ForegroundColor Red
    }
}

# Execute the requested action
switch ($Action) {
    "Install" { Install-Service }
    "Uninstall" { Uninstall-Service }
    "Restart" { Restart-Service }
    "Status" { Show-Status }
}
