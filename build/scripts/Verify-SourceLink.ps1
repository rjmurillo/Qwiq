<#
.SYNOPSIS
    Verifies Source Link information in NuGet packages.

.DESCRIPTION
    This script validates that Source Link information is correctly embedded in NuGet packages.
    It uses the dotnet-sourcelink tool to verify that all source file URLs are accessible.

.PARAMETER Path
    The root path to search for .nupkg files. Defaults to 'src'.

.PARAMETER Pattern
    The path pattern to match within the search. Defaults to 'bin\Release'.

.EXAMPLE
    .\Verify-SourceLink.ps1
    Verifies all .nupkg files in src/**/bin/Release directories.

.EXAMPLE
    .\Verify-SourceLink.ps1 -Path "artifacts" -Pattern "packages"
    Verifies all .nupkg files in artifacts/**/packages directories.

.NOTES
    Requires the dotnet-sourcelink tool to be installed:
    dotnet tool install --global sourcelink
    Or restored via: dotnet tool restore
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Path = "src",

    [Parameter(Mandatory = $false)]
    [string]$Pattern = "bin\\Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Find all .nupkg files (exclude .snupkg symbol packages)
$packages = Get-ChildItem -Path $Path -Recurse -Filter "*.nupkg" -File |
    Where-Object { $_.FullName -match $Pattern -and $_.Name -notmatch "\.snupkg$" }

if ($packages.Count -eq 0) {
    Write-Warning "No .nupkg files found in '$Path' matching pattern '$Pattern'"
    exit 0
}

Write-Host "Found $($packages.Count) NuGet package(s) to verify" -ForegroundColor Cyan

$failed = 0
$passed = 0

foreach ($package in $packages) {
    Write-Host "`nTesting Source Link in: $($package.Name)" -ForegroundColor White

    dotnet sourcelink test $package.FullName

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Source Link validation failed for $($package.Name)"
        $failed++
    }
    else {
        Write-Host "  PASSED" -ForegroundColor Green
        $passed++
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Source Link Verification Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Passed: $passed" -ForegroundColor Green
if ($failed -gt 0) {
    Write-Host "  Failed: $failed" -ForegroundColor Red
}
else {
    Write-Host "  Failed: $failed" -ForegroundColor Green
}
Write-Host "  Total:  $($packages.Count)" -ForegroundColor White

if ($failed -gt 0) {
    Write-Error "$failed NuGet package(s) failed Source Link validation"
    exit 1
}

Write-Host "`nAll $($packages.Count) NuGet package(s) passed Source Link validation" -ForegroundColor Green
exit 0
