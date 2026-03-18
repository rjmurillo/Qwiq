<#
.SYNOPSIS
    Verifies Source Link information in NuGet packages.

.DESCRIPTION
    This script validates that Source Link information is correctly embedded in PDB files.
    It uses the dotnet-sourcelink tool to verify that all source file URLs are accessible.

    This script is intentionally naive - it verifies whatever PDB files it finds.
    Use Validate-PackageOutput.ps1 for build-time package count validation.

.PARAMETER SearchPaths
    Array of root paths to search for PDB files. Defaults to 'artifacts'.

.PARAMETER Pattern
    Regex path pattern to match within the search. Defaults to '[\\/]release'
    to anchor on a path separator, avoiding false matches on unrelated paths.

.EXAMPLE
    .\Verify-SourceLink.ps1
    Verifies Source Link in all PDB or DLL files under artifacts/**/release directories.

.EXAMPLE
    .\Verify-SourceLink.ps1 -SearchPaths "src","test" -Pattern "bin\\Release"
    Verifies PDB files in the traditional src/test bin/Release layout.

.NOTES
    Requires the dotnet-sourcelink tool to be installed:
    dotnet tool install --global sourcelink
    Or restored via: dotnet tool restore
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string[]]$SearchPaths = @("artifacts"),

    [Parameter(Mandatory = $false)]
    [string]$Pattern = "[\\/]release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Collect PDB files from all search paths
# We test PDBs directly because with snupkg format, PDBs are not embedded in nupkg files
$pdbFiles = @(foreach ($searchPath in $SearchPaths) {
    if (Test-Path $searchPath) {
        Get-ChildItem -Path $searchPath -Recurse -Filter "*.pdb" -File |
            Where-Object { $_.FullName -match $Pattern }
    }
})

# Deduplicate by assembly name (prefer net8.0 target)
$uniquePdbs = @{}
foreach ($pdb in $pdbFiles) {
    $baseName = $pdb.BaseName
    if (-not $uniquePdbs.ContainsKey($baseName)) {
        $uniquePdbs[$baseName] = $pdb
    }
    elseif ($pdb.FullName -match 'net8\.0' -and $uniquePdbs[$baseName].FullName -notmatch 'net8\.0') {
        # Prefer net8.0 version
        $uniquePdbs[$baseName] = $pdb
    }
}

$pdbFiles = $uniquePdbs.Values | Sort-Object Name

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Source Link Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Search paths: $($SearchPaths -join ', ')" -ForegroundColor White
Write-Host "  Pattern:      $Pattern" -ForegroundColor White
Write-Host "  PDBs found:   $($pdbFiles.Count)" -ForegroundColor White

if ($pdbFiles.Count -eq 0) {
    Write-Host "  No standalone PDBs found; checking for DLLs with embedded symbols..." -ForegroundColor Yellow

    # With DebugType=embedded, PDB data is inside the DLL itself.
    # Fall back to testing Qwiq DLLs directly, excluding test/third-party assemblies.
    $dllFiles = @(foreach ($searchPath in $SearchPaths) {
        if (Test-Path $searchPath) {
            Get-ChildItem -Path $searchPath -Recurse -Filter "Qwiq.*.dll" -File |
                Where-Object {
                    $_.FullName -match $Pattern -and
                    $_.Name -notmatch '(Test|Mock|Benchmark)'
                }
        }
    })

    # Deduplicate by assembly name (prefer net8.0 target)
    $uniqueDlls = @{}
    foreach ($dll in $dllFiles) {
        $baseName = $dll.BaseName
        if (-not $uniqueDlls.ContainsKey($baseName)) {
            $uniqueDlls[$baseName] = $dll
        }
        elseif ($dll.FullName -match 'net8\.0' -and $uniqueDlls[$baseName].FullName -notmatch 'net8\.0') {
            $uniqueDlls[$baseName] = $dll
        }
    }

    $dllFiles = $uniqueDlls.Values | Sort-Object Name
    Write-Host "  DLLs found:   $($dllFiles.Count) (embedded PDB)" -ForegroundColor White

    if ($dllFiles.Count -eq 0) {
        Write-Warning "No .pdb or Qwiq .dll files found in search paths matching pattern '$Pattern'"
        exit 1
    }

    $testFiles = $dllFiles
    $testMode = "DLL (embedded PDB)"
}
else {
    $testFiles = $pdbFiles
    $testMode = "PDB"
}

$failed = 0
$passed = 0

foreach ($file in $testFiles) {
    Write-Host "`nTesting Source Link in: $($file.Name) [$testMode]" -ForegroundColor White

    $output = dotnet sourcelink test $file.FullName 2>&1

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
Write-Host "  Mode:    $testMode" -ForegroundColor White
Write-Host "  Tested:  $($testFiles.Count)" -ForegroundColor White
Write-Host "  Passed:  $passed" -ForegroundColor Green
if ($failed -gt 0) {
    Write-Host "  Failed:  $failed" -ForegroundColor Red
}
else {
    Write-Host "  Failed:  $failed" -ForegroundColor Green
}

if ($failed -gt 0) {
    Write-Error "$failed file(s) failed Source Link validation"
    exit 1
}

Write-Host "`nAll $($testFiles.Count) file(s) passed Source Link validation" -ForegroundColor Green
exit 0
