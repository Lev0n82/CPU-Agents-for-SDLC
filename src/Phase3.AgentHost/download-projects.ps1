#!/usr/bin/env pwsh
# Download EDCS and Picasso project data from Azure DevOps
# This script connects to Azure DevOps and downloads all project artifacts

param(
    [string]$OrganizationUrl = "https://dev.azure.com/csc-ddsb",
    [string[]]$Projects = @("EDCS", "Picasso"),
    [string]$OutputDir = "./project-data"
)

# Ensure output directory exists
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

Write-Host "=== Azure DevOps Project Data Downloader ===" -ForegroundColor Cyan
Write-Host "Organization: $OrganizationUrl" -ForegroundColor Yellow
Write-Host "Projects: $($Projects -join ', ')" -ForegroundColor Yellow
Write-Host "Output Directory: $OutputDir" -ForegroundColor Yellow
Write-Host ""

# Get PAT from Azure Key Vault
Write-Host "Retrieving PAT from Azure Key Vault..." -ForegroundColor Green
$pat = az keyvault secret show --name AzureAgentPat --vault-name qa-dev-app --query "value" --output tsv 2>$null

if ([string]::IsNullOrEmpty($pat)) {
    Write-Error "Failed to retrieve PAT from Azure Key Vault. Please ensure you're logged in with 'az login'"
    exit 1
}

Write-Host "PAT retrieved successfully!" -ForegroundColor Green

# Create base64 encoded auth header
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
$headers = @{
    Authorization = "Basic $base64AuthInfo"
    "Content-Type" = "application/json"
}

foreach ($project in $Projects) {
    Write-Host "`n=== Processing Project: $project ===" -ForegroundColor Cyan
    
    $projectDir = Join-Path $OutputDir $project
    if (-not (Test-Path $projectDir)) {
        New-Item -ItemType Directory -Path $projectDir -Force | Out-Null
    }

    # 1. Get Project Details
    Write-Host "  Downloading project details..." -ForegroundColor Yellow
    try {
        $projectUrl = "$OrganizationUrl/_apis/projects/$($project)?api-version=7.1"
        $projectDetails = Invoke-RestMethod -Uri $projectUrl -Method Get -Headers $headers
        $projectDetails | ConvertTo-Json -Depth 10 | Out-File (Join-Path $projectDir "project-details.json") -Encoding UTF8
        Write-Host "    ✓ Project details saved" -ForegroundColor Green
    } catch {
        Write-Warning "    ✗ Failed to get project details: $_"
    }

    # 2. Get Work Items
    Write-Host "  Downloading work items..." -ForegroundColor Yellow
    try {
        # Query for all work items in the project
        $wiqlUrl = "$OrganizationUrl/$project/_apis/wit/wiql?api-version=7.1"
        $wiqlBody = @{
            query = "SELECT [System.Id], [System.Title], [System.State], [System.WorkItemType], [System.CreatedDate], [System.ChangedDate] FROM WorkItems WHERE [System.TeamProject] = '$project' ORDER BY [System.CreatedDate] DESC"
        } | ConvertTo-Json
        
        $wiqlResult = Invoke-RestMethod -Uri $wiqlUrl -Method Post -Headers $headers -Body $wiqlBody
        
        if ($wiqlResult.workItems -and $wiqlResult.workItems.Count -gt 0) {
            $workItemIds = $wiqlResult.workItems | Select-Object -ExpandProperty id
            
            # Get work items in batches of 200
            $batchSize = 200
            $allWorkItems = @()
            
            for ($i = 0; $i -lt $workItemIds.Count; $i += $batchSize) {
                $batch = $workItemIds[$i..([Math]::Min($i + $batchSize - 1, $workItemIds.Count - 1))]
                $idsParam = $batch -join ","
                
                $batchUrl = "$OrganizationUrl/_apis/wit/workitems?ids=$idsParam&`$expand=all&api-version=7.1"
                $batchResult = Invoke-RestMethod -Uri $batchUrl -Method Get -Headers $headers
                $allWorkItems += $batchResult.value
            }
            
            $allWorkItems | ConvertTo-Json -Depth 10 | Out-File (Join-Path $projectDir "work-items.json") -Encoding UTF8
            Write-Host "    ✓ Downloaded $($allWorkItems.Count) work items" -ForegroundColor Green
        } else {
            Write-Host "    ℹ No work items found" -ForegroundColor Gray
        }
    } catch {
        Write-Warning "    ✗ Failed to get work items: $_"
    }

    # 3. Get Test Plans
    Write-Host "  Downloading test plans..." -ForegroundColor Yellow
    try {
        $testPlansUrl = "$OrganizationUrl/$project/_apis/test/plans?api-version=7.1"
        $testPlans = Invoke-RestMethod -Uri $testPlansUrl -Method Get -Headers $headers
        
        if ($testPlans.value -and $testPlans.value.Count -gt 0) {
            $testPlans.value | ConvertTo-Json -Depth 10 | Out-File (Join-Path $projectDir "test-plans.json") -Encoding UTF8
            Write-Host "    ✓ Downloaded $($testPlans.value.Count) test plans" -ForegroundColor Green
            
            # Get test suites and cases for each plan
            $allTestCases = @()
            foreach ($plan in $testPlans.value) {
                try {
                    $suitesUrl = "$OrganizationUrl/$project/_apis/test/plans/$($plan.id)/suites?api-version=7.1"
                    $suites = Invoke-RestMethod -Uri $suitesUrl -Method Get -Headers $headers
                    
                    foreach ($suite in $suites.value) {
                        $casesUrl = "$OrganizationUrl/$project/_apis/test/plans/$($plan.id)/suites/$($suite.id)/testcases?api-version=7.1"
                        $cases = Invoke-RestMethod -Uri $casesUrl -Method Get -Headers $headers
                        $allTestCases += $cases.value
                    }
                } catch {
                    Write-Warning "      Failed to get test cases for plan $($plan.id): $_"
                }
            }
            
            if ($allTestCases.Count -gt 0) {
                $allTestCases | ConvertTo-Json -Depth 10 | Out-File (Join-Path $projectDir "test-cases.json") -Encoding UTF8
                Write-Host "    ✓ Downloaded $($allTestCases.Count) test cases" -ForegroundColor Green
            }
        } else {
            Write-Host "    ℹ No test plans found" -ForegroundColor Gray
        }
    } catch {
        Write-Warning "    ✗ Failed to get test plans: $_"
    }

    # 4. Get Repositories
    Write-Host "  Downloading repository list..." -ForegroundColor Yellow
    try {
        $reposUrl = "$OrganizationUrl/$project/_apis/git/repositories?api-version=7.1"
        $repos = Invoke-RestMethod -Uri $reposUrl -Method Get -Headers $headers
        
        if ($repos.value -and $repos.value.Count -gt 0) {
            $repos.value | ConvertTo-Json -Depth 10 | Out-File (Join-Path $projectDir "repositories.json") -Encoding UTF8
            Write-Host "    ✓ Downloaded $($repos.value.Count) repositories" -ForegroundColor Green
        } else {
            Write-Host "    ℹ No repositories found" -ForegroundColor Gray
        }
    } catch {
        Write-Warning "    ✗ Failed to get repositories: $_"
    }

    # 5. Get Pipelines
    Write-Host "  Downloading pipelines..." -ForegroundColor Yellow
    try {
        $pipelinesUrl = "$OrganizationUrl/$project/_apis/pipelines?api-version=7.1"
        $pipelines = Invoke-RestMethod -Uri $pipelinesUrl -Method Get -Headers $headers
        
        if ($pipelines.value -and $pipelines.value.Count -gt 0) {
            $pipelines.value | ConvertTo-Json -Depth 10 | Out-File (Join-Path $projectDir "pipelines.json") -Encoding UTF8
            Write-Host "    ✓ Downloaded $($pipelines.value.Count) pipelines" -ForegroundColor Green
        } else {
            Write-Host "    ℹ No pipelines found" -ForegroundColor Gray
        }
    } catch {
        Write-Warning "    ✗ Failed to get pipelines: $_"
    }

    # 6. Get Builds
    Write-Host "  Downloading recent builds..." -ForegroundColor Yellow
    try {
        $buildsUrl = "$OrganizationUrl/$project/_apis/build/builds?`$top=100&api-version=7.1"
        $builds = Invoke-RestMethod -Uri $buildsUrl -Method Get -Headers $headers
        
        if ($builds.value -and $builds.value.Count -gt 0) {
            $builds.value | ConvertTo-Json -Depth 10 | Out-File (Join-Path $projectDir "builds.json") -Encoding UTF8
            Write-Host "    ✓ Downloaded $($builds.value.Count) recent builds" -ForegroundColor Green
        } else {
            Write-Host "    ℹ No builds found" -ForegroundColor Gray
        }
    } catch {
        Write-Warning "    ✗ Failed to get builds: $_"
    }

    # 7. Get Releases
    Write-Host "  Downloading releases..." -ForegroundColor Yellow
    try {
        $releasesUrl = "$OrganizationUrl/$project/_apis/release/releases?api-version=7.1"
        $releases = Invoke-RestMethod -Uri $releasesUrl -Method Get -Headers $headers
        
        if ($releases.value -and $releases.value.Count -gt 0) {
            $releases.value | ConvertTo-Json -Depth 10 | Out-File (Join-Path $projectDir "releases.json") -Encoding UTF8
            Write-Host "    ✓ Downloaded $($releases.value.Count) releases" -ForegroundColor Green
        } else {
            Write-Host "    ℹ No releases found" -ForegroundColor Gray
        }
    } catch {
        Write-Warning "    ✗ Failed to get releases: $_"
    }
}

Write-Host "`n=== Download Complete ===" -ForegroundColor Cyan
Write-Host "Data saved to: $OutputDir" -ForegroundColor Green
Write-Host ""
Write-Host "Summary of downloaded data:" -ForegroundColor Yellow
Get-ChildItem -Path $OutputDir -Recurse -Filter "*.json" | ForEach-Object {
    $size = (Get-Item $_.FullName).Length / 1KB
    Write-Host "  $($_.Name) - $([Math]::Round($size, 2)) KB" -ForegroundColor Gray
}