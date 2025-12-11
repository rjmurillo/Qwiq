---
name: qwiq-build
description: Build system guidance for QWIQ including MSBuild configuration, Central Package Management, multi-targeting, and project file conventions. Use when encountering build errors, modifying project files, or managing dependencies.
---

# QWIQ Build System

## Purpose

This skill provides guidance for the QWIQ build system, including SDK-style projects, Central Package Management, multi-targeting strategy, and MSBuild configuration. It helps resolve build errors and maintain consistency across project files.

## When To Use

- Build errors or warnings
- Modifying `.csproj`, `.props`, or `.targets` files
- Adding or updating NuGet package dependencies
- Understanding multi-targeting behavior
- Troubleshooting CI/CD build failures

## Instructions

### 1. Build Commands

```powershell
# Standard build
dotnet build Qwiq.sln -c Release

# Strict build (warnings as errors) - CI default
dotnet build Qwiq.sln -c Release /p:PedanticMode=true

# Flexible build (for diagnosing analyzers)
dotnet build Qwiq.sln -c Release /p:PedanticMode=false

# Single-threaded build (fixes Windows file locking)
dotnet build Qwiq.sln /m:1 /nodeReuse:false -c Release

# Restore tools first (for nbgv versioning)
dotnet tool restore
```

### 2. Multi-Targeting Strategy

| Project Type | Target Frameworks | Notes |
|--------------|-------------------|-------|
| Core libraries | `net472;netstandard2.0;net8.0` | Full multi-targeting |
| SOAP projects | `net472` only | Windows-only, TFS Client OM |
| REST projects | `net472;netstandard2.0;net8.0` | Cross-platform |
| Test projects | `net472;net8.0` | Skip netstandard |

**Windows Required:** SOAP projects require Windows for `net472` targets.

### 3. Central Package Management

All package versions are defined in `Directory.Packages.props`:

```xml
<!-- In Directory.Packages.props -->
<PackageVersion Include="Shouldly" Version="4.2.0" />

<!-- In project .csproj - NO version attribute -->
<PackageReference Include="Shouldly" />
```

**To add a new package:**
1. Add version to `Directory.Packages.props`
2. Reference without version in consuming `.csproj`

**Never add `Version` attribute to `PackageReference`** - this breaks Central Package Management.

### 4. Key Configuration Files

| File | Purpose | Modify? |
|------|---------|---------|
| `Directory.Build.props` | Shared MSBuild properties | ⚠️ Rarely |
| `Directory.Build.targets` | Shared build targets | ⚠️ Rarely |
| `Directory.Packages.props` | Package versions | ✅ For dependencies |
| `global.json` | SDK version pin | ⚠️ SDK upgrades only |
| `.editorconfig` | Analyzer severities | ✅ For rule changes |

### 5. Analyzer Configuration

Analyzer severities are in `.editorconfig`, NOT MSBuild files:

```editorconfig
# Suppress a warning
dotnet_diagnostic.CA1234.severity = none

# Enable as warning
dotnet_diagnostic.CA1234.severity = warning
```

### 6. InternalsVisibleTo

Defined in `.csproj` files (not AssemblyInfo.cs):

```xml
<ItemGroup>
  <InternalsVisibleTo Include="Qwiq.Core.UnitTests" />
</ItemGroup>
```

### 7. Common Build Issues

**Windows File Locking:**
```powershell
# Use single-threaded build
dotnet build /m:1 /nodeReuse:false -v:minimal
```

**Package Conflicts:**
| Package | Problem | Solution |
|---------|---------|----------|
| `Polyfill` | Conflicts with VSS Client | Use custom `NullableAttributes.cs` |
| `Should` | Conflicts with modern frameworks | Use `Shouldly` |

**Multi-TFM Race Conditions:**
```powershell
# Clean and rebuild twice
dotnet clean; dotnet build; dotnet build
# OR disable reference assemblies
dotnet build /p:ProduceReferenceAssembly=false
```

## Examples

### Example 1: Adding a New Package

**User goal:** Add `Moq` package to test project

**Process:**
1. Check if `Moq` exists in `Directory.Packages.props`
2. If not, add: `<PackageVersion Include="Moq" Version="4.20.70" />`
3. In test `.csproj`, add: `<PackageReference Include="Moq" />`
4. Run `dotnet restore`

**Expected output:** Package available without version in csproj

### Example 2: Fixing Build Error

**User goal:** Resolve "file in use" error on Windows

**Process:**
1. Stop any running `dotnet` processes
2. Run `dotnet build /m:1 /nodeReuse:false`
3. If persists, run `dotnet clean` first

**Expected output:** Build succeeds without file locking errors

## Related Resources

- See [REFERENCE.md](./REFERENCE.md) for project layout and target frameworks
- See [../qwiq-cicd/SKILL.md](../qwiq-cicd/SKILL.md) for CI build configuration
