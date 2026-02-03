# Autonomous Agent - Validation Test Script
# This script validates the agent's functionality

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("SelfTest", "Scheduling", "All")]
    [string]$TestType = "All"
)

Write-Host "=== Autonomous Agent Validation Tests ===" -ForegroundColor Cyan
Write-Host ""

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Join-Path $scriptDir "src\AutonomousAgent.Core"

function Test-SelfTestFramework {
    Write-Host "TEST 1: Self-Test Framework" -ForegroundColor Yellow
    Write-Host "----------------------------" -ForegroundColor Yellow
    Write-Host "Running the agent to verify self-tests execute..." -ForegroundColor White
    Write-Host ""
    
    Push-Location $projectDir
    
    # Run the agent and capture output
    $output = dotnet run 2>&1 | Out-String
    
    Pop-Location
    
    # Check for self-test markers in output
    $hasStarted = $output -match "Self-Test Sequence"
    $hasTests = $output -match "\[PASS\]"
    $hasCompleted = $output -match "Self-test sequence completed successfully"
    
    Write-Host "Results:" -ForegroundColor White
    Write-Host "  - Self-test sequence initiated: " -NoNewline
    if ($hasStarted) {
        Write-Host "PASS" -ForegroundColor Green
    } else {
        Write-Host "FAIL" -ForegroundColor Red
    }
    
    Write-Host "  - Tests executed: " -NoNewline
    if ($hasTests) {
        Write-Host "PASS" -ForegroundColor Green
    } else {
        Write-Host "FAIL" -ForegroundColor Red
    }
    
    Write-Host "  - All tests passed: " -NoNewline
    if ($hasCompleted) {
        Write-Host "PASS" -ForegroundColor Green
    } else {
        Write-Host "FAIL" -ForegroundColor Red
    }
    
    Write-Host ""
    
    if ($hasStarted -and $hasTests -and $hasCompleted) {
        Write-Host "✓ Self-Test Framework: PASSED" -ForegroundColor Green
        return $true
    } else {
        Write-Host "✗ Self-Test Framework: FAILED" -ForegroundColor Red
        Write-Host ""
        Write-Host "Output:" -ForegroundColor Yellow
        Write-Host $output
        return $false
    }
}

function Test-SchedulingConfiguration {
    Write-Host "TEST 2: Scheduling Configuration" -ForegroundColor Yellow
    Write-Host "--------------------------------" -ForegroundColor Yellow
    Write-Host "Verifying appsettings.json contains scheduling config..." -ForegroundColor White
    Write-Host ""
    
    $configPath = Join-Path $projectDir "appsettings.json"
    
    if (-not (Test-Path $configPath)) {
        Write-Host "✗ Configuration file not found: $configPath" -ForegroundColor Red
        return $false
    }
    
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    
    $hasScheduler = $null -ne $config.Scheduler
    $hasRebootConfig = $null -ne $config.Scheduler.NightlyReboot
    $hasEnabled = $null -ne $config.Scheduler.NightlyReboot.Enabled
    $hasHour = $null -ne $config.Scheduler.NightlyReboot.Hour
    $hasMinute = $null -ne $config.Scheduler.NightlyReboot.Minute
    
    Write-Host "Results:" -ForegroundColor White
    Write-Host "  - Scheduler section exists: " -NoNewline
    if ($hasScheduler) {
        Write-Host "PASS" -ForegroundColor Green
    } else {
        Write-Host "FAIL" -ForegroundColor Red
    }
    
    Write-Host "  - NightlyReboot config exists: " -NoNewline
    if ($hasRebootConfig) {
        Write-Host "PASS" -ForegroundColor Green
    } else {
        Write-Host "FAIL" -ForegroundColor Red
    }
    
    Write-Host "  - All required fields present: " -NoNewline
    if ($hasEnabled -and $hasHour -and $hasMinute) {
        Write-Host "PASS" -ForegroundColor Green
    } else {
        Write-Host "FAIL" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "Current Configuration:" -ForegroundColor White
    Write-Host "  Enabled: $($config.Scheduler.NightlyReboot.Enabled)" -ForegroundColor Cyan
    Write-Host "  Hour: $($config.Scheduler.NightlyReboot.Hour)" -ForegroundColor Cyan
    Write-Host "  Minute: $($config.Scheduler.NightlyReboot.Minute)" -ForegroundColor Cyan
    Write-Host ""
    
    if ($hasScheduler -and $hasRebootConfig -and $hasEnabled -and $hasHour -and $hasMinute) {
        Write-Host "✓ Scheduling Configuration: PASSED" -ForegroundColor Green
        return $true
    } else {
        Write-Host "✗ Scheduling Configuration: FAILED" -ForegroundColor Red
        return $false
    }
}

function Test-BuildSuccess {
    Write-Host "TEST 3: Build Verification" -ForegroundColor Yellow
    Write-Host "--------------------------" -ForegroundColor Yellow
    Write-Host "Building the project..." -ForegroundColor White
    Write-Host ""
    
    Push-Location $scriptDir
    
    $buildOutput = dotnet build 2>&1 | Out-String
    $buildSuccess = $LASTEXITCODE -eq 0
    
    Pop-Location
    
    if ($buildSuccess) {
        Write-Host "✓ Build: PASSED" -ForegroundColor Green
        Write-Host "  No warnings or errors detected." -ForegroundColor White
    } else {
        Write-Host "✗ Build: FAILED" -ForegroundColor Red
        Write-Host ""
        Write-Host "Build Output:" -ForegroundColor Yellow
        Write-Host $buildOutput
    }
    
    Write-Host ""
    return $buildSuccess
}

# Run tests based on parameter
$allPassed = $true

if ($TestType -eq "All" -or $TestType -eq "SelfTest") {
    $buildResult = Test-BuildSuccess
    $allPassed = $allPassed -and $buildResult
    
    if ($buildResult) {
        $selfTestResult = Test-SelfTestFramework
        $allPassed = $allPassed -and $selfTestResult
    }
}

if ($TestType -eq "All" -or $TestType -eq "Scheduling") {
    $schedulingResult = Test-SchedulingConfiguration
    $allPassed = $allPassed -and $schedulingResult
}

# Summary
Write-Host ""
Write-Host "=== Test Summary ===" -ForegroundColor Cyan
if ($allPassed) {
    Write-Host "All tests PASSED ✓" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Some tests FAILED ✗" -ForegroundColor Red
    exit 1
}
