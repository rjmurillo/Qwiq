<#
.SYNOPSIS
    Validates that all packable projects produced NuGet packages.

.DESCRIPTION
    This script scans all .csproj files to identify projects marked as packable
    (IsPackable=true and GeneratePackageOnBuild=true), then validates that each
    produced a .nupkg file in the centralized package output directory.

    Symbol package (.snupkg) validation is conditional based on IncludeSymbols setting:
    - IncludeSymbols=true: Expects .snupkg files (portable symbols)
    - IncludeSymbols=false: No .snupkg expected (embedded symbols via DebugType=embedded)

    The SDK places packages in artifacts/package/{Configuration} when ArtifactsPath is set.

    This is a CI build validation script - it should fail the build if any
    expected packages are missing.

.PARAMETER SolutionRoot
    The root directory containing the solution. Defaults to current directory.

.PARAMETER Configuration
    The build configuration to check. Defaults to 'Release'.

.PARAMETER SourcePaths
    Array of paths to search for .csproj files. Defaults to 'src' and 'test'.

.PARAMETER PackageOutputPath
    The directory where packages are output. Defaults to artifacts/package/{Configuration}.

.PARAMETER ExpectSymbolPackages
    Whether to expect .snupkg files. Defaults to $false (embedded symbols).
    Set to $true if using IncludeSymbols=true with DebugType=portable.

.EXAMPLE
    .\Validate-PackageOutput.ps1
    Validates packages in the default artifacts/package/release directory (embedded symbols).

.EXAMPLE
    .\Validate-PackageOutput.ps1 -Configuration Debug
    Validates packages built with Debug configuration.

.EXAMPLE
    .\Validate-PackageOutput.ps1 -PackageOutputPath "C:\custom\output"
    Validates packages in a custom output directory.

.EXAMPLE
    .\Validate-PackageOutput.ps1 -ExpectSymbolPackages $true
    Validates packages including .snupkg files (for portable symbols builds).

.NOTES
    This script should be run after 'dotnet build /t:Build,Pack' completes.
    It will exit with code 1 if any expected packages are missing.

    Symbol Package Configuration:
    - Qwiq uses embedded symbols (DebugType=embedded, IncludeSymbols=false) by default
    - No .snupkg files are generated with embedded symbols (symbols are in .dll/.nupkg)
    - See ADR-012 for rationale on embedded vs portable symbols
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$SolutionRoot = ".",

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [string[]]$SourcePaths = @("src", "test"),

    [Parameter(Mandatory = $false)]
    [string]$PackageOutputPath = "",

    [Parameter(Mandatory = $false)]
    [bool]$ExpectSymbolPackages = $false
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Package Output Validation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Solution root:    $SolutionRoot" -ForegroundColor White
Write-Host "  Configuration:    $Configuration" -ForegroundColor White
Write-Host "  Source paths:     $($SourcePaths -join ', ')" -ForegroundColor White
Write-Host "  Symbol packages:  $(if ($ExpectSymbolPackages) { 'Expected (.snupkg)' } else { 'Not expected (embedded)' })" -ForegroundColor White

# Determine the package output path
# The SDK places packages in artifacts/package/{Configuration} when ArtifactsPath is set
if (-not $PackageOutputPath) {
    $PackageOutputPath = Join-Path $SolutionRoot "artifacts" "package" $Configuration.ToLower()
}

Write-Host "  Package output:   $PackageOutputPath" -ForegroundColor White

# Find all packable projects by scanning csproj files
$packableProjects = @()

foreach ($sourcePath in $SourcePaths) {
    $fullPath = Join-Path $SolutionRoot $sourcePath
    if (-not (Test-Path $fullPath)) {
        Write-Warning "Source path not found: $fullPath"
        continue
    }

    $csprojFiles = Get-ChildItem -Path $fullPath -Recurse -Filter "*.csproj" -File

    foreach ($csproj in $csprojFiles) {
        [xml]$content = Get-Content $csproj.FullName

        # Check if project is packable
        $isPackable = $content.SelectSingleNode("//IsPackable")?.InnerText
        $generatePackageOnBuild = $content.SelectSingleNode("//GeneratePackageOnBuild")?.InnerText

        # A project is packable if both conditions are met (or if GeneratePackageOnBuild is true without IsPackable=false)
        $shouldPack = ($isPackable -eq "true" -and $generatePackageOnBuild -eq "true") -or
                      ($generatePackageOnBuild -eq "true" -and $isPackable -ne "false")

        if ($shouldPack) {
            # Determine package ID (PackageId or project name)
            $packageId = $content.SelectSingleNode("//PackageId")?.InnerText
            if (-not $packageId) {
                # Default to assembly name or project name
                $packageId = $content.SelectSingleNode("//AssemblyName")?.InnerText
                if (-not $packageId) {
                    $packageId = [System.IO.Path]::GetFileNameWithoutExtension($csproj.Name)
                }
            }

            $packableProjects += [PSCustomObject]@{
                Name = $packageId
                ProjectFile = $csproj.FullName
                ProjectDir = $csproj.DirectoryName
            }
        }
    }
}

Write-Host "`nFound $($packableProjects.Count) packable project(s):" -ForegroundColor Cyan
foreach ($proj in $packableProjects) {
    Write-Host "  - $($proj.Name)" -ForegroundColor White
}

if ($packableProjects.Count -eq 0) {
    Write-Error "No packable projects found! Check that projects have IsPackable=true and GeneratePackageOnBuild=true"
    exit 1
}

# Validate each packable project produced its packages
$missingNupkg = @()
$missingSnupkg = @()
$foundPackages = @()

Write-Host "`n----------------------------------------" -ForegroundColor Cyan
Write-Host "Validating package output..." -ForegroundColor Cyan
Write-Host "----------------------------------------" -ForegroundColor Cyan

# Verify the package output directory exists
if (-not (Test-Path $PackageOutputPath)) {
    Write-Error "Package output directory not found: $PackageOutputPath"
    Write-Host "Ensure 'dotnet build /t:Build,Pack' completed successfully." -ForegroundColor Yellow
    exit 1
}

foreach ($proj in $packableProjects) {
    # Find .nupkg file (pattern: PackageName.*.nupkg, excluding .snupkg)
    $nupkg = Get-ChildItem -Path $PackageOutputPath -Filter "$($proj.Name).*.nupkg" -File |
        Where-Object { $_.Name -notmatch "\.snupkg$" } |
        Select-Object -First 1

    # Find .snupkg file (only if expecting symbol packages)
    $snupkg = $null
    if ($ExpectSymbolPackages) {
        $snupkg = Get-ChildItem -Path $PackageOutputPath -Filter "$($proj.Name).*.snupkg" -File |
            Select-Object -First 1
    }

    $status = ""
    $color = "Green"

    if ($ExpectSymbolPackages) {
        # Portable symbols mode: expect both .nupkg and .snupkg
        if ($nupkg -and $snupkg) {
            $status = "OK (.nupkg + .snupkg)"
            $foundPackages += [PSCustomObject]@{
                Name = $proj.Name
                Nupkg = $nupkg.Name
                Snupkg = $snupkg.Name
            }
        }
        elseif ($nupkg -and -not $snupkg) {
            $status = "PARTIAL (missing .snupkg)"
            $color = "Yellow"
            $missingSnupkg += $proj.Name
        }
        elseif (-not $nupkg -and $snupkg) {
            $status = "PARTIAL (missing .nupkg)"
            $color = "Yellow"
            $missingNupkg += $proj.Name
        }
        else {
            $status = "MISSING (no packages found)"
            $color = "Red"
            $missingNupkg += $proj.Name
            $missingSnupkg += $proj.Name
        }
    }
    else {
        # Embedded symbols mode: only expect .nupkg
        if ($nupkg) {
            $status = "OK (.nupkg with embedded symbols)"
            $foundPackages += [PSCustomObject]@{
                Name = $proj.Name
                Nupkg = $nupkg.Name
                Snupkg = "N/A (embedded)"
            }
        }
        else {
            $status = "MISSING (.nupkg not found)"
            $color = "Red"
            $missingNupkg += $proj.Name
        }
    }

    Write-Host "  $($proj.Name): " -NoNewline -ForegroundColor White
    Write-Host $status -ForegroundColor $color
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Package Validation Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Expected packages: $($packableProjects.Count)" -ForegroundColor White
Write-Host "  Found complete:    $($foundPackages.Count)" -ForegroundColor $(if ($foundPackages.Count -eq $packableProjects.Count) { "Green" } else { "Yellow" })

if ($missingNupkg.Count -gt 0) {
    Write-Host "  Missing .nupkg:    $($missingNupkg.Count)" -ForegroundColor Red
    foreach ($missing in $missingNupkg) {
        Write-Host "    - $missing" -ForegroundColor Red
    }
}

if ($ExpectSymbolPackages -and $missingSnupkg.Count -gt 0) {
    Write-Host "  Missing .snupkg:   $($missingSnupkg.Count)" -ForegroundColor Red
    foreach ($missing in $missingSnupkg) {
        Write-Host "    - $missing" -ForegroundColor Red
    }
}

# Exit with error if any packages are missing
$hasErrors = $missingNupkg.Count -gt 0
if ($ExpectSymbolPackages) {
    $hasErrors = $hasErrors -or ($missingSnupkg.Count -gt 0)
}

if ($hasErrors) {
    Write-Host "`nERROR: Package validation failed!" -ForegroundColor Red
    Write-Host "Build produced $($foundPackages.Count) of $($packableProjects.Count) expected packages." -ForegroundColor Red
    Write-Host "Ensure 'dotnet build /t:Build,Pack' completed successfully." -ForegroundColor Yellow
    exit 1
}

Write-Host "`nAll $($packableProjects.Count) packable projects produced their packages." -ForegroundColor Green
exit 0
