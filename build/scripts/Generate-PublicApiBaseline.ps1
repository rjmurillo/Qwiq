#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates PublicAPI.Shipped.txt and PublicAPI.Unshipped.txt baseline files for all packable projects.

.DESCRIPTION
    This script builds the solution and extracts RS0016 errors (public API not declared) to generate
    the initial PublicAPI baseline files for the Microsoft.CodeAnalysis.PublicApiAnalyzers analyzer.

.EXAMPLE
    ./Generate-PublicApiBaseline.ps1
#>

param(
    [string]$Configuration = "Release",
    [string]$SolutionPath = "Qwiq.sln"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Write-Host "Generating PublicAPI baseline files..." -ForegroundColor Cyan

# Find all packable projects
$packableProjects = Get-ChildItem -Path "src" -Filter "*.csproj" -Recurse | Where-Object {
    $content = Get-Content $_.FullName -Raw
    $content -match '<IsPackable>true</IsPackable>' -or $content -match '<GeneratePackageOnBuild>true</GeneratePackageOnBuild>'
}

Write-Host "Found $($packableProjects.Count) packable projects" -ForegroundColor Green

foreach ($project in $packableProjects) {
    Write-Host "`nProcessing: $($project.Name)" -ForegroundColor Yellow
    
    # Build the project and capture RS0016 errors
    $buildOutput = dotnet build $project.FullName -c $Configuration --no-incremental 2>&1 | Out-String
    
    # Extract RS0016 errors (public API not declared)
    $apiMembers = @($buildOutput | Select-String -Pattern "error RS0016: Symbol '([^']+)' is not part" | ForEach-Object {
        $_.Matches.Groups[1].Value
    } | Sort-Object -Unique)
    
    if ($apiMembers.Count -eq 0 -or $null -eq $apiMembers[0]) {
        Write-Host "  No public API members found (or already has baseline)" -ForegroundColor Gray
        continue
    }
    
    Write-Host "  Found $($apiMembers.Count) public API members" -ForegroundColor Green
    
    # Create PublicAPI.Shipped.txt (empty for new baseline)
    $shippedPath = Join-Path $project.DirectoryName "PublicAPI.Shipped.txt"
    $unshippedPath = Join-Path $project.DirectoryName "PublicAPI.Unshipped.txt"
    
    # Create PublicAPI.Shipped.txt with nullable enable directive
    @"
#nullable enable
"@ | Set-Content $shippedPath -NoNewline
    
    Write-Host "  Created: $($shippedPath | Resolve-Path -Relative)" -ForegroundColor Green
    
    # Create PublicAPI.Unshipped.txt with all current API members
    $unshippedContent = @"
#nullable enable
$($apiMembers -join "`n")
"@
    
    $unshippedContent | Set-Content $unshippedPath
    Write-Host "  Created: $($unshippedPath | Resolve-Path -Relative)" -ForegroundColor Green
}

Write-Host "`nBaseline generation complete!" -ForegroundColor Cyan
Write-Host "Run 'dotnet build' to verify the baselines are correct." -ForegroundColor Cyan
