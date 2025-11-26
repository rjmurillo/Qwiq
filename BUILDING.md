# Building Qwiq

This document describes how to build Qwiq from source.

## Prerequisites

### Required
- **.NET 8 SDK** or later
  - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
  - Verify installation: `dotnet --version` (should show 8.0.x or higher)

### Optional
- **Visual Studio 2022** (17.8 or later) for IDE development
- **Git** for version control

## Quick Start

```bash
# Clone the repository
git clone https://github.com/rjmurillo/Qwiq.git
cd Qwiq

# Restore .NET local tools (includes Nerdbank.GitVersioning)
dotnet tool restore

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Build Process

### 1. Restore .NET Tools

This project uses **Nerdbank.GitVersioning (NBGV)** for automatic semantic versioning.

```bash
dotnet tool restore
```

### 2. Restore Dependencies

```bash
dotnet restore Qwiq.sln
```

### 3. Build the Solution

```bash
# Debug build (default)
dotnet build

# Release build
dotnet build --configuration Release

# Build without restore
dotnet build --no-restore
```

**Multi-Targeting:** Projects target both .NET Standard 2.0 and .NET 8. The build system automatically builds both target frameworks.

### 4. Run Tests

```bash
# Run all tests
dotnet test

# Run tests with filter
dotnet test --filter "TestCategory!=localOnly&TestCategory!=Benchmark"

# Run tests for specific project
dotnet test test/Qwiq.Core.Tests/Qwiq.Core.UnitTests.csproj
```

**Test Categories:**
- `localOnly` - Requires local TFS/Azure DevOps instance
- `Benchmark` - Performance benchmark tests
- `SOAP` - SOAP client tests
- `REST` - REST client tests

## Project Structure

```
Qwiq/
??? src/
?   ??? Qwiq.Core/              # Core abstractions and interfaces
?   ??? Qwiq.Core.Rest/         # REST API client
?   ??? Qwiq.Core.Soap/         # SOAP API client (legacy)
?   ??? Qwiq.Identity/          # Identity management
?   ??? Qwiq.Linq/              # LINQ query provider
?   ??? Qwiq.Linq.Identity/     # LINQ + Identity
?   ??? Qwiq.Mapper/            # Work item mapping
?   ??? Qwiq.Mapper.Identity/   # Mapper + Identity
??? test/
    ??? Qwiq.Core.Tests/
    ??? Qwiq.Mocks/             # Test mocks and helpers
    ??? Qwiq.Tests.Common/      # Shared test utilities
```

## Configuration

### Directory.Build.props

Shared MSBuild properties:
- **Language Version:** Latest C# features
- **Nullable Reference Types:** Enabled
- **Warning Level:** 5 (maximum diagnostic coverage)
- **Analysis Level:** Latest Roslyn analyzers

### Directory.Packages.props

Central Package Management (CPM):
- All package versions defined in one place
- Projects reference packages without versions
- Ensures version consistency

### version.json

Nerdbank.GitVersioning configuration:
- Base version: `10.0-alpha`
- Assembly version precision: `major.minor`
- Public releases from `master`/`main` branches

## Versioning

Version format: `{major}.{minor}.{height}[-{prerelease}]+{commit-id}`

**Example:** `10.0.42-alpha+g1234567`

### Check Version

```bash
# Display current version
dotnet nbgv get-version

# Show all version properties
dotnet nbgv get-version -a

# Get specific property
dotnet nbgv get-version -v CloudBuildNumber
```

## CI/CD

### GitHub Actions

Workflow: `.github/workflows/main.yml`

**Triggers:**
- Push to `develop` or `master`
- Pull requests to `develop` or `master`
- Manual dispatch

**Build Steps:**
1. Setup .NET 8 SDK
2. Restore .NET tools (NBGV)
3. Display version
4. Restore packages
5. Build (Release)
6. Run tests
7. Upload artifacts

### Local CI Testing

Replicate the CI build locally:

```bash
dotnet tool restore
dotnet restore Qwiq.sln
dotnet build Qwiq.sln --configuration Release --no-restore
dotnet test Qwiq.sln --configuration Release --no-build --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST"
```

## Troubleshooting

### "dotnet tool restore" fails

**Solution:**
```bash
# Recreate tool manifest
dotnet new tool-manifest --force
dotnet tool install nbgv
```

### NBGV shows version 0.0.0

**Solution:** Ensure you're on a branch with Git history:
```bash
git fetch --unshallow
```

### Nullable reference type warnings

**Expected behavior.** The project enables maximum code quality checks. Address warnings in new code.

### Test failures

Use the CI test filter to skip infrastructure-dependent tests:
```bash
dotnet test --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST"
```

## IDE Setup

### Visual Studio 2022

**Required Workloads:**
- .NET desktop development
- ASP.NET and web development

### Visual Studio Code

**Required Extensions:**
- C# Dev Kit
- .NET Install Tool

### JetBrains Rider

**Configuration:**
- .NET SDK: 8.0.x or later
- Enable Roslyn analyzers

## Performance Tips

```bash
# Parallel builds (default)
dotnet build -m

# Skip restore when packages are current
dotnet build --no-restore

# Use build output for tests
dotnet test --no-build
```

## Getting Help

- **Issues:** https://github.com/rjmurillo/Qwiq/issues
- **Discussions:** https://github.com/rjmurillo/Qwiq/discussions

## License

MIT License - see [LICENSE](LICENSE) for details.
