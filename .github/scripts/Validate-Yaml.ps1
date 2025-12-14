#!/usr/bin/env pwsh
<#
.SYNOPSIS
YAML Syntax Validation (For CI/CD Only - NOT for agents)

.DESCRIPTION
This script validates YAML syntax using Python's yaml module.
Cross-platform compatible (Windows, Linux, macOS).

AGENTS: DO NOT USE THIS SCRIPT. The pre-commit hook validates YAML automatically.
Running this manually wastes tokens in an OODA loop.

.PARAMETER FilePath
Path to the YAML file to validate

.EXAMPLE
pwsh .github/scripts/Validate-Yaml.ps1 .github/workflows/main.yml

.NOTES
For formatting/style, use: dotnet pprettier --write file.yml
This script is for CI/CD pipelines only.
#>

param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$FilePath
)

# Check if file exists
if (-not (Test-Path $FilePath)) {
    Write-Error "File not found: $FilePath"
    exit 1
}

# Validate syntax using Python
try {
    $pythonCmd = if ($IsWindows -or $null -eq $IsWindows) { 'python' } else { 'python3' }
    $output = & $pythonCmd -c "import yaml; yaml.safe_load(open('$FilePath'))" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Valid YAML: $FilePath" -ForegroundColor Green
        exit 0
    }
    else {
        Write-Host "✗ Invalid YAML: $FilePath" -ForegroundColor Red
        Write-Host $output -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Error "Failed to validate YAML: $_"
    exit 1
}
