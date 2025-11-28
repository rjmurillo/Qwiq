# Copilot Coding Agent Instructions for QWIQ

## Repository Overview

QWIQ (**Q**uick **W**ork **I**tem **Q**uery) is a .NET library providing a simplified API for querying Azure DevOps / Team Foundation Server work items. It wraps the TFS Client OM with cleaner interfaces, factory patterns, and mock support.

**Key Characteristics:**
- Modern SDK-style projects with multi-targeting
- Target frameworks: `net472`, `netstandard2.0`, `net8.0` (varies by project)
- **Windows-only build requirement** for full framework coverage
- Central Package Management via `Directory.Packages.props`
- Nerdbank.GitVersioning for version management (via dotnet tool manifest)

## Build & Test Commands

### Prerequisites
- **Windows machine** required for `net472` targets (SOAP client)
- .NET 8.0 SDK (pinned in `global.json`)
- Visual Studio 2022+ or VS Code with C# extension

### Build Commands
```powershell
# Restore tools (nbgv for versioning)
dotnet tool restore

# Restore packages and build
dotnet restore Qwiq.sln
dotnet build Qwiq.sln --configuration Release

# Or single command (restore is implicit)
dotnet build Qwiq.sln -c Release
```

### Test Commands
```powershell
# Run tests with category exclusions
dotnet test Qwiq.sln --configuration Release --no-build --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST&TestCategory!=IntegrationTests"
```

**Test Categories to Exclude:**
- `localOnly` - Requires local TFS instance
- `Benchmark` - Performance tests
- `SOAP` / `REST` - Integration tests requiring server
- `IntegrationTests` - Full integration tests

## Project Layout

### Source Projects (`src/`)
| Project | Target Frameworks | Description |
|---------|-------------------|-------------|
| `Qwiq.Core` | net472;netstandard2.0;net8.0 | Core interfaces and abstractions |
| `Qwiq.Core.Rest` | net472;netstandard2.0;net8.0 | REST API client implementation |
| `Qwiq.Core.Soap` | net472 | SOAP client (Windows only) |
| `Qwiq.Linq` | net472;net8.0 | LINQ query provider |
| `Qwiq.Mapper` | net472;net8.0 | Object mapping layer |
| `Qwiq.Identity` | net472;net8.0 | Identity management |
| `Qwiq.Identity.Soap` | net472 | Identity SOAP client (Windows only) |

### Test Projects (`test/`)
| Project | Target Frameworks | Description |
|---------|-------------------|-------------|
| `Qwiq.Core.Tests` | net472;net8.0 | Core unit tests |
| `Qwiq.Linq.Tests` | net472;net8.0 | LINQ provider tests |
| `Qwiq.Mapper.Tests` | net472;net8.0 | Mapper tests |
| `Qwiq.Identity.Tests` | net472;net8.0 | Identity tests |
| `Qwiq.IntegrationTests` | net472 | Full integration tests |
| `Qwiq.Mocks` | net472;net8.0 | Mock implementations |

### Key Configuration Files
| File | Purpose |
|------|---------|
| `global.json` | Pins .NET SDK version (8.0.100) |
| `Directory.Build.props` | Shared MSBuild properties, package metadata |
| `Directory.Build.targets` | Shared build targets |
| `Directory.Packages.props` | Central Package Management |
| `.config/dotnet-tools.json` | Dotnet tool manifest (nbgv) |
| `version.json` | Nerdbank.GitVersioning configuration |
| `.editorconfig` | Code style (4-space indent, CRLF line endings) |
| `nuget.config` | NuGet package sources |

## Critical Build Notes

### 1. Windows-Only Build for SOAP
- SOAP projects (`Qwiq.Core.Soap`, `Qwiq.Identity.Soap`) require Windows
- They depend on `Microsoft.TeamFoundationServer.ExtendedClient` which only supports `net472`
- GitHub Actions workflow uses `windows-latest` runner

### 2. Multi-Targeting Strategy
- Core libraries: `net472;netstandard2.0;net8.0`
- SOAP projects: `net472` only (Windows dependency)
- REST projects: `net472;netstandard2.0;net8.0`
- Test projects: `net472;net8.0`

### 3. Central Package Management
- All package versions are defined in `Directory.Packages.props`
- Individual csproj files use `<PackageReference Include="..." />` without versions
- To add a new package: add version to `Directory.Packages.props`, then reference in csproj

### 4. SDK-Style Packaging
- NuGet packages are built using SDK pack (no .nuspec files)
- Package metadata is in `Directory.Build.props` (Authors, Copyright, License, etc.)
- Project-specific metadata in individual csproj files (Description, PackageId)

## Code Style & Patterns

### ⚠️ IMPORTANT: JetBrains.Annotations REMOVED
- **DO NOT USE** `[NotNull]`, `[CanBeNull]`, `[Pure]`, `[ItemNotNull]`, or any JetBrains annotations
- These were removed during the .NET 8 modernization
- Use runtime null checks with `ArgumentNullException` instead
- Future: Enable `<Nullable>enable</Nullable>` for compile-time null safety

### Null Validation Pattern
```csharp
// CORRECT: Runtime null check with clear exception
public void Method(SomeType parameter)
{
    if (parameter == null) throw new ArgumentNullException(nameof(parameter));
    // or for constructor base calls:
    // : base(parameter?.Property ?? throw new ArgumentNullException(nameof(parameter)))
}

// WRONG: JetBrains annotation (removed from codebase)
public void Method([NotNull] SomeType parameter) // DO NOT USE
```

### Exception Handling
- Use `ArgumentNullException` for null parameters
- Use `ArgumentException` for invalid (but non-null) parameters
- Use `Contract.Requires` for design-by-contract assertions (optional)

### Known Patterns
1. Factory pattern used extensively (e.g., `WorkItemStoreFactory`)
2. Interfaces for all public types to support mocking
3. Internal types marked with `internal` visibility
4. Lazy initialization for expensive operations

## CI/CD Pipeline

The main workflow (`.github/workflows/main.yml`) runs on:
- Push to `develop` or `master`
- Pull requests to `develop` or `master`

**Expected Workflow Steps:**
1. Checkout with `fetch-depth: 0` (for versioning)
2. Setup .NET SDK using `global.json`
3. Restore dotnet tools (`dotnet tool restore`)
4. Restore packages (`dotnet restore`)
5. Build solution (`dotnet build`)
6. Run tests with category filters
7. Upload test results and binaries

## When Making Changes

1. **Build with dotnet CLI:** `dotnet build Qwiq.sln -c Release`
2. **Test with filters:** `dotnet test --filter "TestCategory!=localOnly&..."`
3. **Follow existing code patterns** - check similar files for conventions
4. **Use Central Package Management** - add versions to `Directory.Packages.props`
5. **No JetBrains annotations** - use runtime null checks instead
6. **Update task list** - if working from `.agents/PR31-TASK-LIST.md`

## Compatibility Shims

### IdentityTypeMapper
- Location: `src/Qwiq.Core/Compatibility/IdentityTypeMapper.cs`
- Purpose: Compatibility shim for class removed in Microsoft.VisualStudio.Services.Client v19+
- Can be removed when: Qwiq drops support for identity type mapping OR Microsoft restores this class

## Trust These Instructions

These instructions reflect the modernized state of the repository (PR #31). The repository has been migrated from:
- ❌ Legacy .csproj format → ✅ SDK-style projects
- ❌ packages.config → ✅ Central Package Management
- ❌ .nuspec files → ✅ SDK-style packaging
- ❌ JetBrains.Annotations → ✅ Runtime null checks
- ❌ .NET Framework 4.6 → ✅ Multi-targeting (net472/netstandard2.0/net8.0)
