<#
.SYNOPSIS
    Verifies Source Link information and package counts in NuGet packages.

.DESCRIPTION
    This script validates that:
    1. The expected number of .nupkg packages are produced
    2. Each .nupkg has a corresponding .snupkg symbol package (1:1 ratio)
    3. Source Link information is correctly embedded in all NuGet packages

    It uses the dotnet-sourcelink tool to verify that all source file URLs are accessible.

.PARAMETER SearchPaths
    Array of root paths to search for .nupkg files. Defaults to 'src' and 'test'.

.PARAMETER Pattern
    The path pattern to match within the search. Defaults to 'bin\Release'.

.PARAMETER ExpectedPackageCount
    The expected number of .nupkg packages. If not specified, count validation is skipped.
    When specified, the script will fail if the actual count doesn't match.

.PARAMETER SkipSymbolValidation
    If specified, skips validation that .snupkg count matches .nupkg count.

.EXAMPLE
    .\Verify-SourceLink.ps1
    Verifies all .nupkg files in src/**/bin/Release and test/**/bin/Release directories.

.EXAMPLE
    .\Verify-SourceLink.ps1 -ExpectedPackageCount 10
    Verifies exactly 10 .nupkg files exist with matching .snupkg files.

.EXAMPLE
    .\Verify-SourceLink.ps1 -SearchPaths "artifacts" -Pattern "packages"
    Verifies all .nupkg files in artifacts/**/packages directories.

.NOTES
    Requires the dotnet-sourcelink tool to be installed:
    dotnet tool install --global sourcelink
    Or restored via: dotnet tool restore
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string[]]$SearchPaths = @("src", "test"),

    [Parameter(Mandatory = $false)]
    [string]$Pattern = "bin\\Release",

    [Parameter(Mandatory = $false)]
    [int]$ExpectedPackageCount = 0,

    [Parameter(Mandatory = $false)]
    [switch]$SkipSymbolValidation
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Collect all packages from all search paths
$packages = @()
$symbolPackages = @()

foreach ($searchPath in $SearchPaths) {
    if (Test-Path $searchPath) {
        $found = Get-ChildItem -Path $searchPath -Recurse -Filter "*.nupkg" -File |
            Where-Object { $_.FullName -match $Pattern -and $_.Name -notmatch "\.snupkg$" }
        if ($found) {
            $packages += $found
        }

        $foundSymbols = Get-ChildItem -Path $searchPath -Recurse -Filter "*.snupkg" -File |
            Where-Object { $_.FullName -match $Pattern }
        if ($foundSymbols) {
            $symbolPackages += $foundSymbols
        }
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Package Count Validation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Search paths:   $($SearchPaths -join ', ')" -ForegroundColor White
Write-Host "  Pattern:        $Pattern" -ForegroundColor White
Write-Host "  .nupkg count:   $($packages.Count)" -ForegroundColor White
Write-Host "  .snupkg count:  $($symbolPackages.Count)" -ForegroundColor White

if ($ExpectedPackageCount -gt 0) {
    Write-Host "  Expected:       $ExpectedPackageCount" -ForegroundColor White

    if ($packages.Count -ne $ExpectedPackageCount) {
        Write-Host "`nERROR: Package count mismatch!" -ForegroundColor Red
        Write-Host "  Expected $ExpectedPackageCount .nupkg files but found $($packages.Count)" -ForegroundColor Red
        Write-Host "`nPackages found:" -ForegroundColor Yellow
        foreach ($pkg in $packages) {
            Write-Host "  - $($pkg.Name)" -ForegroundColor White
        }
        exit 1
    }
    Write-Host "`n  Package count validation: PASSED" -ForegroundColor Green
}

if (-not $SkipSymbolValidation) {
    if ($packages.Count -ne $symbolPackages.Count) {
        Write-Host "`nERROR: Symbol package count mismatch!" -ForegroundColor Red
        Write-Host "  Found $($packages.Count) .nupkg but $($symbolPackages.Count) .snupkg files" -ForegroundColor Red
        Write-Host "  Each .nupkg should have a corresponding .snupkg" -ForegroundColor Red

        # Show which packages are missing symbol packages
        $packageNames = $packages | ForEach-Object { $_.Name -replace '\.nupkg$', '' }
        $symbolNames = $symbolPackages | ForEach-Object { $_.Name -replace '\.snupkg$', '' }

        $missingSymbols = $packageNames | Where-Object { $_ -notin $symbolNames }
        if ($missingSymbols) {
            Write-Host "`nPackages missing .snupkg:" -ForegroundColor Yellow
            foreach ($missing in $missingSymbols) {
                Write-Host "  - $missing" -ForegroundColor Red
            }
        }
        exit 1
    }
    Write-Host "  Symbol package validation: PASSED" -ForegroundColor Green
}

if ($packages.Count -eq 0) {
    Write-Warning "No .nupkg files found in search paths matching pattern '$Pattern'"
    exit 0
}

# Collect PDB files for Source Link testing - one per package
# We test PDBs directly because with snupkg format, PDBs are not embedded in nupkg files
# For each package, find the corresponding PDB in the same project directory
$pdbFiles = @()
foreach ($package in $packages) {
    # Extract package name without version (e.g., "Qwiq.Core" from "Qwiq.Core.10.0.60-xxx.nupkg")
    $packageBaseName = $package.Name -replace '\.\d+\.\d+\.\d+.*\.nupkg$', ''

    # Find PDB in the same directory tree (prefer net8.0, then any)
    $pkgDir = $package.Directory.Parent  # Go up from bin/Release to project folder
    $matchingPdb = Get-ChildItem -Path $pkgDir.FullName -Recurse -Filter "$packageBaseName.pdb" -File |
        Where-Object { $_.FullName -match $Pattern } |
        Sort-Object { if ($_.FullName -match 'net8\.0') { 0 } else { 1 } } |
        Select-Object -First 1

    if ($matchingPdb) {
        $pdbFiles += $matchingPdb
    }
    else {
        Write-Warning "Could not find PDB for package: $($package.Name)"
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Source Link Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Found $($pdbFiles.Count) PDB file(s) to verify" -ForegroundColor Cyan

if ($pdbFiles.Count -eq 0) {
    Write-Warning "No .pdb files found in search paths matching pattern '$Pattern'"
    exit 0
}

$failed = 0
$passed = 0

foreach ($pdb in $pdbFiles) {
    Write-Host "`nTesting Source Link in: $($pdb.Name)" -ForegroundColor White

    $output = dotnet sourcelink test $pdb.FullName 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  FAILED" -ForegroundColor Red
        # Show a sample of the errors (first 3 lines)
        $errorLines = $output | Where-Object { $_ -match "error:" } | Select-Object -First 3
        foreach ($line in $errorLines) {
            Write-Host "    $line" -ForegroundColor Yellow
        }
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
Write-Host "  Packages: $($packages.Count)" -ForegroundColor White
Write-Host "  PDBs tested: $($pdbFiles.Count)" -ForegroundColor White
Write-Host "  Passed: $passed" -ForegroundColor Green
if ($failed -gt 0) {
    Write-Host "  Failed: $failed" -ForegroundColor Red
}
else {
    Write-Host "  Failed: $failed" -ForegroundColor Green
}

if ($failed -gt 0) {
    Write-Error "$failed PDB file(s) failed Source Link validation"
    exit 1
}

Write-Host "`nAll $($pdbFiles.Count) PDB file(s) passed Source Link validation" -ForegroundColor Green
exit 0
