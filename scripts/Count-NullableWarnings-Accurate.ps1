#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Accurately counts CS8xxx nullable reference type warnings by temporarily enabling them.

.DESCRIPTION
    This script temporarily modifies .editorconfig to enable CS8xxx warnings, builds each project,
    counts warnings, then restores the original .editorconfig.

.EXAMPLE
    ./scripts/Count-NullableWarnings-Accurate.ps1

.NOTES
    Author: GitHub Copilot Agent
    Date: December 4, 2025
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Debug",
    
    [Parameter(Mandatory = $false)]
    [switch]$ExportCsv
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Get repository root
$repoRoot = Split-Path -Parent $PSScriptRoot
Write-Host "Repository root: $repoRoot" -ForegroundColor Cyan

# Backup .editorconfig
$editorConfigPath = Join-Path $repoRoot ".editorconfig"
$backupPath = "$editorConfigPath.backup-$(Get-Date -Format 'yyyyMMddHHmmss')"

Write-Host "`nBacking up .editorconfig to: $backupPath" -ForegroundColor Yellow
Copy-Item $editorConfigPath $backupPath

try {
    # Modify .editorconfig to enable CS8xxx warnings
    Write-Host "Enabling CS8xxx warnings in .editorconfig..." -ForegroundColor Yellow
    $content = Get-Content $editorConfigPath
    $content = $content -replace 'dotnet_diagnostic\.CS8(\d{3})\.severity = none', 'dotnet_diagnostic.CS8$1.severity = warning'
    $content | Set-Content $editorConfigPath
    
    # Projects to analyze (source projects only)
    $projects = @(
        "src/Qwiq.Core/Qwiq.Core.csproj",
        "src/Qwiq.Core.Rest/Qwiq.Client.Rest.csproj",
        "src/Qwiq.Core.Soap/Qwiq.Client.Soap.csproj",
        "src/Qwiq.Identity/Qwiq.Identity.csproj",
        "src/Qwiq.Identity.Soap/Qwiq.Identity.Soap.csproj",
        "src/Qwiq.Linq/Qwiq.Linq.csproj",
        "src/Qwiq.Linq.Identity/Qwiq.Linq.Identity.csproj",
        "src/Qwiq.Mapper/Qwiq.Mapper.csproj",
        "src/Qwiq.Mapper.Identity/Qwiq.Mapper.Identity.csproj",
        "test/Qwiq.Mocks/Qwiq.Mocks.csproj"
    )
    
    $results = @()
    $totalWarnings = 0
    
    Write-Host "`nAnalyzing projects for CS8xxx warnings..." -ForegroundColor Yellow
    Write-Host "=" * 80
    
    foreach ($projectPath in $projects) {
        $fullPath = Join-Path $repoRoot $projectPath
        
        if (-not (Test-Path $fullPath)) {
            Write-Warning "Project not found: $projectPath"
            continue
        }
        
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
        Write-Host "`nAnalyzing: $projectName" -ForegroundColor Cyan
        
        # Build project and capture warnings
        $buildOutput = dotnet build $fullPath `
            -c $Configuration `
            /m:1 `
            /nodeReuse:false `
            /p:TreatWarningsAsErrors=false `
            /p:EnforceCodeStyleInBuild=false `
            --no-restore `
            2>&1 | Out-String
        
        # Count CS8xxx warnings
        $warningMatches = [regex]::Matches($buildOutput, "warning CS8\d{3}:")
        $warningCount = $warningMatches.Count
        
        # Group by warning code
        $warningsByCode = @{}
        foreach ($match in $warningMatches) {
            $code = ($match.Value -split ':')[0] -replace 'warning ', ''
            if ($warningsByCode.ContainsKey($code)) {
                $warningsByCode[$code]++
            } else {
                $warningsByCode[$code] = 1
            }
        }
        
        $totalWarnings += $warningCount
        
        # Display results
        if ($warningCount -eq 0) {
            Write-Host "  ✅ No CS8xxx warnings" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  $warningCount CS8xxx warning(s) found:" -ForegroundColor Yellow
            foreach ($code in ($warningsByCode.Keys | Sort-Object)) {
                $count = $warningsByCode[$code]
                Write-Host "    - $code : $count occurrence(s)" -ForegroundColor Gray
            }
        }
        
        # Store result
        $results += [PSCustomObject]@{
            Project = $projectName
            Path = $projectPath
            TotalWarnings = $warningCount
            WarningsByCode = $warningsByCode
            Status = if ($warningCount -eq 0) { "✅ Complete" } else { "⚠️ Needs work" }
        }
    }
    
    # Summary
    Write-Host "`n" + ("=" * 80) -ForegroundColor Cyan
    Write-Host "SUMMARY" -ForegroundColor Cyan
    Write-Host ("=" * 80) -ForegroundColor Cyan
    Write-Host "`nTotal CS8xxx warnings across all projects: $totalWarnings" -ForegroundColor $(if ($totalWarnings -eq 0) { "Green" } else { "Yellow" })
    Write-Host "`nProject Status:" -ForegroundColor Cyan
    
    $results | Format-Table -Property Project, TotalWarnings, Status -AutoSize
    
    # Export to CSV if requested
    if ($ExportCsv) {
        $csvPath = Join-Path $repoRoot ".agents/CS8xxx-warning-count-accurate.csv"
        $results | Select-Object Project, Path, TotalWarnings, Status | Export-Csv -Path $csvPath -NoTypeInformation
        Write-Host "`n✅ Results exported to: $csvPath" -ForegroundColor Green
    }
    
    # Exit code
    if ($totalWarnings -gt 0) {
        Write-Host "`n⚠️  Action required: $totalWarnings warnings need to be addressed" -ForegroundColor Yellow
        $exitCode = 1
    } else {
        Write-Host "`n✅ All projects are clean! Ready to remove CS8xxx suppressions." -ForegroundColor Green
        $exitCode = 0
    }
}
finally {
    # Restore original .editorconfig
    Write-Host "`nRestoring original .editorconfig..." -ForegroundColor Yellow
    Copy-Item $backupPath $editorConfigPath -Force
    Remove-Item $backupPath
    Write-Host "✅ .editorconfig restored" -ForegroundColor Green
}

exit $exitCode
