#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Counts CS8xxx nullable reference type warnings per project.

.DESCRIPTION
    Builds each project in the solution with CS8xxx warnings enabled and counts warnings by type.
    Outputs a summary table showing warning counts per project.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default is Debug.

.EXAMPLE
    ./scripts/Count-NullableWarnings.ps1
    
.EXAMPLE
    ./scripts/Count-NullableWarnings.ps1 -Configuration Release
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$InformationPreference = 'Continue'

# Get solution directory
$scriptDir = Split-Path -Parent $PSCommandPath
$solutionDir = Split-Path -Parent $scriptDir
$solutionFile = Join-Path $solutionDir 'Qwiq.sln'

Write-Information "Solution: $solutionFile"
Write-Information "Configuration: $Configuration"
Write-Information ""

# Find all project files in src and test directories
$projectDirs = @(
    (Join-Path $solutionDir 'src'),
    (Join-Path $solutionDir 'test')
)

$projects = @()
foreach ($dir in $projectDirs) {
    if (Test-Path $dir) {
        $projects += Get-ChildItem -Path $dir -Filter '*.csproj' -Recurse
    }
}

Write-Information "Found $($projects.Count) projects to analyze"
Write-Information ""

# Results collection
$results = @()

foreach ($project in $projects) {
    $projectName = $project.BaseName
    $projectPath = $project.FullName
    
    Write-Information "Analyzing: $projectName"
    
    # Build project and capture output
    $buildOutput = dotnet build $projectPath `
        -c $Configuration `
        /p:TreatWarningsAsErrors=false `
        /p:EnforceCodeStyleInBuild=false `
        --no-incremental `
        2>&1 | Out-String
    
    # Count CS8xxx warnings
    $cs8Warnings = [regex]::Matches($buildOutput, 'warning CS8\d{3}')
    $warningCount = $cs8Warnings.Count
    
    # Count by specific warning code
    $warningsByCode = $cs8Warnings | 
        ForEach-Object { $_.Value -replace 'warning ', '' } |
        Group-Object |
        Sort-Object Name
    
    $warningCodes = if ($warningsByCode) {
        ($warningsByCode | ForEach-Object { "$($_.Name)($($_.Count))" }) -join ', '
    } else {
        'None'
    }
    
    $results += [PSCustomObject]@{
        Project = $projectName
        TotalWarnings = $warningCount
        WarningCodes = $warningCodes
    }
    
    Write-Verbose "  Warnings: $warningCount"
}

Write-Information ""
Write-Information "=== CS8xxx Warning Summary ==="
Write-Information ""

# Display results table
$results | Format-Table -AutoSize | Out-String | Write-Information

# Summary statistics
$totalWarnings = ($results | Measure-Object -Property TotalWarnings -Sum).Sum
$projectsWithWarnings = ($results | Where-Object { $_.TotalWarnings -gt 0 }).Count
$projectsClean = ($results | Where-Object { $_.TotalWarnings -eq 0 }).Count

Write-Information ""
Write-Information "=== Summary ==="
Write-Information "Total Projects: $($results.Count)"
Write-Information "Projects with 0 warnings: $projectsClean"
Write-Information "Projects with warnings: $projectsWithWarnings"
Write-Information "Total CS8xxx warnings: $totalWarnings"
Write-Information ""

if ($totalWarnings -eq 0) {
    Write-Information "✅ SUCCESS: All projects have zero CS8xxx warnings!"
    exit 0
} else {
    Write-Warning "⚠️ Found $totalWarnings CS8xxx warnings across $projectsWithWarnings projects"
    exit 1
}
