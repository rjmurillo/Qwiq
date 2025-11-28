# Copilot Coding Agent Instructions for QWIQ

## Repository Overview

QWIQ (**Q**uick **W**ork **I**tem **Q**uery) is a .NET library providing a simplified API for querying Azure DevOps / Team Foundation Server work items. It wraps the TFS Client OM with cleaner interfaces, factory patterns, and mock support.

**Key Characteristics:**
- Multi-target .NET Framework (net472) and .NET 8.0 (net8.0) library
- **Windows-only build requirement** for full solution (.NET Framework targets)
- SDK-style projects with centralized configuration
- Uses Nerdbank.GitVersioning for version management

## Build & Test Commands

### Prerequisites
- **Windows machine required** for full build (net472 projects need .NET Framework SDK)
- .NET SDK 8.0+ (for net8.0 targets)
- MSBuild and NuGet for legacy project support

### Build Commands
```bash
# Full solution restore and build (Windows only)
dotnet restore Qwiq.sln
dotnet build Qwiq.sln --configuration Release

# net8.0 only build (cross-platform)
dotnet build Qwiq.sln --configuration Release --framework net8.0
```

### Test Commands
```bash
# Run tests (exclude integration/local-only tests)
dotnet test Qwiq.sln --configuration Release --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST&TestCategory!=IntegrationTests"
```

**Test Categories to Exclude:**
- `localOnly` - Requires local TFS instance
- `Benchmark` - Performance tests
- `SOAP` / `REST` - Integration tests requiring server
- `IntegrationTests` - Full integration tests

## Project Layout

### Source Projects (`src/`)
| Project | Description | Target |
|---------|-------------|--------|
| `Qwiq.Core` | Core interfaces and abstractions | net472;net8.0 |
| `Qwiq.Core.Rest` | REST API client implementation | net472;net8.0 |
| `Qwiq.Core.Soap` | SOAP client implementation | net472 (Windows) |
| `Qwiq.Linq` | LINQ query provider | net472;net8.0 |
| `Qwiq.Mapper` | Object mapping layer | net472;net8.0 |
| `Qwiq.Identity` / `.Soap` | Identity management | net472 |

### Test Projects (`test/`)
- Unit tests: `*.Tests` projects
- Integration tests: `Qwiq.IntegrationTests`
- Mocks: `Qwiq.Mocks`
- Benchmarks: `*.Benchmark*` projects

### Key Configuration Files
- `Directory.Build.props` - Central project properties (warnings, analyzers)
- `Directory.Build.targets` - Build overrides (disables legacy FxCop, GitVersionTask)
- `nuget.config` - NuGet package sources
- `version.json` - Nerdbank.GitVersioning configuration
- `.editorconfig` - Code style (4-space indent, CRLF line endings)

## Critical Build Notes

### 1. Target Framework Requirements
- **net472 projects require Windows + .NET Framework SDK**
- On Linux/macOS, only net8.0 targets will build successfully
- Use `--framework net8.0` flag when building on non-Windows

### 2. Warning Suppressions
The repo suppresses many warnings in `Directory.Build.targets`:
- All CA* (Code Analysis) warnings are suppressed as technical debt
- CS1591 (missing XML docs) is suppressed
- SYSLIB* warnings for obsolete APIs are suppressed

### 3. Analyzer Configuration
- `EnableNETAnalyzers` is enabled but `AnalysisMode` is set to `None`
- Microsoft.CodeAnalysis.NetAnalyzers is included for future enablement

## Common Issues & Workarounds

### GitHub Workflow Preferences (from PR #30 feedback)
1. Use `global-json-file: ./global.json` instead of `dotnet-version` if global.json exists
2. Always run `dotnet tool restore` before build steps
3. Use environment variables for test filters (reuse across jobs)
4. Use deterministic build flags: `/p:Deterministic=true /p:UseSharedCompilation=false`
5. Upload binlogs as artifacts for debugging

### Security Considerations (from PR feedback)
1. Do NOT target .NET Framework < 4.7.2 (security vulnerability)
2. Avoid suppressing NU1701/NU1702 warnings - indicates potential compatibility issues
3. Replace JetBrains.Annotations with built-in nullable annotations

### Code Style
1. Remove JetBrains.Annotations attributes - use C# nullable annotations instead
2. Don't use nuspec files - use SDK-style `<Package*>` properties in csproj
3. Remove `AssemblyInfo.Common.cs` linked files - use SDK auto-generation
4. Follow existing patterns for exception handling and null checks

## CI/CD Pipeline

The main workflow (`.github/workflows/main.yml`) runs on:
- Push to `develop` or `master`
- Pull requests to `develop` or `master`

**Expected Workflow Steps:**
1. Checkout with `fetch-depth: 0` (for versioning)
2. Setup .NET SDK
3. Restore NuGet packages
4. Build solution
5. Run tests with category filters
6. Upload test results and binaries

## When Making Changes

1. **Always restore before building:** `dotnet restore Qwiq.sln`
2. **Test on Windows for full coverage** - net472 tests won't run elsewhere
3. **Run linting implicitly** - analyzers run during build
4. **Check both TFMs** - changes may behave differently on net472 vs net8.0
5. **Update Directory.Build.props** for solution-wide package/property changes
6. **Reference existing patterns** in similar files when adding new code

## Trust These Instructions

These instructions are accurate for the current state of the repository. If something doesn't work as documented, first verify the instructions before exploring alternatives. Report any discrepancies found during your work.
