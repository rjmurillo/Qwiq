# Qwiq Repository Instructions for Coding Agents

## Overview

Qwiq is a .NET Framework library that provides a simplified abstraction layer for Team Foundation Server (TFS) and Azure DevOps work item tracking. It offers core abstractions (`IWorkItem`, `IWorkItemStore`, `IQuery`), SOAP and REST client implementations, a LINQ-to-WIQL query provider, attribute-based object mapping, identity resolution, and comprehensive mocks for testing.

**Tech Stack:** C# only, .NET Framework 4.6, legacy non-SDK csproj format with `packages.config`, MSTest V2 for testing, Nerdbank.GitVersioning for versioning.

**Platform Constraint:** Builds require Windows with MSBuild (Visual Studio 2017+ or MSBuild 15+/17.x) and `nuget.exe`. Linux/Mono builds are not supported.

## Build Commands (Windows Only)

Always run these commands in order. The build will fail if packages are not restored first.

### 1. Restore NuGet Packages (Required First)

```powershell
nuget restore Qwiq.sln -NonInteractive -PackagesDirectory packages -ConfigFile nuget.config
```

The `-PackagesDirectory packages` flag is critical. Projects use `packages.config` and expect assemblies in the local `packages/` directory. Using only the global cache causes missing reference errors.

### 2. Build Solution

```powershell
msbuild Qwiq.sln /p:Configuration=Release /p:Platform="Any CPU" /v:minimal /m
```

Preconditions: Windows, MSBuild on PATH, NuGet restore completed. FxCop is disabled via `Directory.Build.targets`.

### 3. Run Tests

Tests are MSTest V2 assemblies. CI uses VSTest with this filter to exclude environment-dependent tests:

```powershell
vstest.console.exe test\**\*Tests.dll /Logger:trx /TestCaseFilter:"TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST"
```

**Test Categories Explained:**
- `localOnly`: Requires a real TFS/Azure DevOps instance
- `Benchmark`: Performance benchmarks (use BenchmarkDotNet runner)
- `SOAP`/`REST`: Protocol-specific integration tests
- `IntegrationTests` assembly: Excluded entirely from CI; designed for manual SOAP/REST comparison testing

Never run `localOnly`, `SOAP`, `REST`, `Benchmark`, or `IntegrationTests` in automated CI.

## CI Pipeline

The authoritative validation is `.github/workflows/main.yml` running on `windows-latest`. It performs NuGet restore, MSBuild, and VSTest with the filter above, then uploads `.trx` results and binaries as artifacts.

**When editing CI workflows, follow these guidelines:**
- Use `global-json-file: ./global.json` instead of `dotnet-version` in `actions/setup-dotnet`
- Add `dotnet tool restore` after setting up .NET for Nerdbank.GitVersioning
- Include deterministic build flags: `/p:Deterministic=true /p:UseSharedCompilation=false /nodeReuse:false`
- Upload binlogs as artifacts for failure diagnosis: `/bl:./artifacts/logs/build.binlog`
- Extract test filters to environment variables or a `.runsettings` file instead of long CLI strings
- Use `windows-latest` only; `windows-2019` is retired
- Prefer simple, obvious solutions over complex workarounds

## Project Layout

### Root Files
- `Qwiq.sln` - Solution file
- `Directory.Build.props` - Injects Nerdbank.GitVersioning and Microsoft.CodeAnalysis.NetAnalyzers
- `Directory.Build.targets` - Disables legacy FxCop, neutralizes GitVersionTask, suppresses CA/IDE warnings
- `nuget.config` - Package source configuration with local `packages/` directory
- `version.json` - Nerdbank.GitVersioning configuration (base version: 10.0)
- `coverage.runsettings` - Code coverage configuration for VSTest

### Source Code (`src/`)
- `Qwiq.Core/` - Core abstractions: `IWorkItem`, `IWorkItemStore`, `IQuery`, `IProject`, exceptions, type conversion
- `Qwiq.Core.Soap/` - SOAP/TFS Client OM implementation (Windows/.NET Framework only)
- `Qwiq.Core.Rest/` - REST client implementation
- `Qwiq.Linq/` - LINQ-to-WIQL provider (`WiqlTranslator`, `QueryRewriter`)
- `Qwiq.Mapper/` - Attribute-based mapping (`[FieldDefinition]`, `[WorkItemType]`)
- `Qwiq.Identity/` - Identity resolution contracts
- `Qwiq.Identity.Soap/`, `Qwiq.Linq.Identity/`, `Qwiq.Mapper.Identity/` - Identity-aware extensions

### Test Code (`test/`)
- `Qwiq.Mocks/` - In-memory store and mocks for unit testing
- `Qwiq.Tests.Common/` - Shared Given/When/Then test base (`ContextSpecification`)
- `Qwiq.Core.UnitTests/`, `Qwiq.Linq.UnitTests/`, `Qwiq.Mapper.UnitTests/`, `Qwiq.Identity.UnitTests/` - Unit tests
- `Qwiq.IntegrationTests/` - Environment-dependent integration tests (excluded from CI)
- `Qwiq.Benchmark/`, `Qwiq.*BenchmarkTests/` - Performance benchmarks

### Key Entry Points
- `WorkItemStoreFactory` in `Qwiq.Core` - Creates work item stores
- `WiqlTranslator` in `Qwiq.Linq` - Converts LINQ expressions to WIQL
- `AttributeMapperStrategy` in `Qwiq.Mapper` - Maps work items to POCOs

## Guidelines for Code Changes

**Do not:**
- Modify `Directory.Build.props`, `Directory.Build.targets`, or `nuget.config` as part of feature PRs
- Upgrade NuGet dependencies (especially Microsoft.TeamFoundation*/VisualStudio.Services* or Newtonsoft.Json) unless explicitly requested
- Attempt large-scale migrations (SDK-style projects, target framework changes) in feature/bugfix PRs
- Re-enable CA/IDE warnings piecemeal; they are suppressed centrally as technical debt

**Do:**
- Follow existing code patterns and conventions
- Use `Qwiq.Mocks` for unit tests instead of mocking interfaces directly
- Inherit from `ContextSpecification` in `Qwiq.Tests.Common` for Given/When/Then test structure
- Run `nuget restore` before any build or test command

**For workarounds:** If a non-standard solution is needed, check helper files into the repository and document clearly why the workaround is necessary and when it can be removed.

## Trust These Instructions

These instructions are authoritative for build, test, and layout. Trust them and do not re-derive commands by guesswork. Only search the repository when information here is missing, commands fail, or you are working in an area not covered above.
