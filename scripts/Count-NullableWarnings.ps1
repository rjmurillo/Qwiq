#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Counts CS8xxx nullable reference type warnings across all projects.

.DESCRIPTION
    Builds each project individually with warnings enabled and counts the number of CS8xxx warnings.
    This helps track progress on nullable reference type migration.

.EXAMPLE
    ./scripts/Count-NullableWarnings.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

# Find all source projects
$srcProjects = Get-ChildItem -Path "$PSScriptRoot/../src" -Filter "*.csproj" -Recurse | Sort-Object FullName

$results = @()

Write-Host "Counting CS8xxx nullable warnings per project..." -ForegroundColor Cyan
Write-Host ""

foreach ($project in $srcProjects) {
    Write-Host "Analyzing: $($project.Name)" -ForegroundColor Yellow
    
    # Build the project with warnings enabled (TreatWarningsAsErrors=false to see all warnings)
    $buildOutput = & dotnet build $project.FullName -c Debug /p:TreatWarningsAsErrors=false /p:EnforceCodeStyleInBuild=false --no-incremental -v:quiet 2>&1 | Out-String
    
    # Count CS8xxx warnings
    $cs8Warnings = ($buildOutput | Select-String -Pattern "warning CS8\d{3}:" -AllMatches).Matches
    $warningCount = if ($cs8Warnings) { $cs8Warnings.Count } else { 0 }
    
    # Get unique warning codes
    $uniqueWarnings = @()
    if ($cs8Warnings) {
        $uniqueWarnings = $cs8Warnings | ForEach-Object { 
            if ($_ -match "(CS8\d{3})") { $matches[1] }
        } | Select-Object -Unique | Sort-Object
    }
    
    $results += [PSCustomObject]@{
        Project = $project.BaseName
        WarningCount = $warningCount
        WarningCodes = ($uniqueWarnings -join ', ')
    }
    
    Write-Host "  Warnings: $warningCount" -ForegroundColor $(if ($warningCount -eq 0) { 'Green' } else { 'Red' })
    if ($uniqueWarnings) {
        Write-Host "  Codes: $($uniqueWarnings -join ', ')" -ForegroundColor Gray
    }
    Write-Host ""
}

# Display summary table
Write-Host "Summary:" -ForegroundColor Cyan
$results | Format-Table -AutoSize

# Calculate totals
$totalWarnings = ($results | Measure-Object -Property WarningCount -Sum).Sum
$projectsWithWarnings = ($results | Where-Object { $_.WarningCount -gt 0 }).Count
$projectsClean = ($results | Where-Object { $_.WarningCount -eq 0 }).Count

Write-Host ""
Write-Host "Total CS8xxx warnings: $totalWarnings" -ForegroundColor $(if ($totalWarnings -eq 0) { 'Green' } else { 'Red' })
Write-Host "Projects with warnings: $projectsWithWarnings" -ForegroundColor $(if ($projectsWithWarnings -eq 0) { 'Green' } else { 'Yellow' })
Write-Host "Projects clean: $projectsClean" -ForegroundColor Green
Write-Host ""

# Return results for further processing if needed
return $results
