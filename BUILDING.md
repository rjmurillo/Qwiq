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

# Verify NBGV is working
dotnet nbgv get-version

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Detailed Build Instructions

### 1. Restore .NET Tools

This project uses **Nerdbank.GitVersioning (NBGV)** for automatic semantic versioning based on Git history.

```bash
dotnet tool restore
```

This reads `.config/dotnet-tools.json` and installs:
- `nbgv` (version 3.9.50) - Nerdbank.GitVersioning tool

### 2. Check Version Information

```bash
# Display the current version calculated from Git
dotnet nbgv get-version

# Get version in JSON format
dotnet nbgv get-version -f json

# Get specific version property
dotnet nbgv get-version -v Version
```

### 3. Restore Dependencies

```bash
dotnet restore Qwiq.sln
```

This restores all NuGet packages defined in:
- `Directory.Packages.props` (central package version management)
- Individual project files

### 4. Build the Solution

```bash
# Debug build (default)
dotnet build

# Release build
dotnet build --configuration Release

# Build without restore
dotnet build --no-restore
```

#### Multi-Targeting

Most projects target both:
- **.NET Standard 2.0** (for maximum compatibility)
- **.NET 8** (for modern features)

The build system automatically builds both target frameworks.

### 5. Run Tests

```bash
# Run all tests
dotnet test

# Run tests without building
dotnet test --no-build

# Run tests with filter
dotnet test --filter "TestCategory!=localOnly&TestCategory!=Benchmark"

# Run tests with detailed output
dotnet test --verbosity detailed

# Run tests for specific project
dotnet test test/Qwiq.Core.Tests/Qwiq.Core.UnitTests.csproj
```

#### Test Categories

Tests are categorized:
- `localOnly` - Tests requiring local TFS/Azure DevOps instance
- `Benchmark` - Performance benchmark tests
- `SOAP` - Tests for SOAP client
- `REST` - Tests for REST client

### 6. Clean Build

```bash
# Clean build outputs
dotnet clean

# Clean and rebuild
dotnet clean && dotnet build
```

## Build Configuration

### Directory.Build.props

Shared MSBuild properties for all projects:

```xml
<PropertyGroup>
  <LangVersion>latest</LangVersion>
  <Nullable>enable</Nullable>
  <WarningLevel>5</WarningLevel>
  <AnalysisLevel>latest</AnalysisLevel>
</PropertyGroup>
```

### Directory.Packages.props

Central package version management (CPM):
- All package versions are defined in one place
- Projects reference packages without versions
- Ensures consistency across the solution

### version.json

NBGV configuration:
- Base version: `10.0-alpha`
- Version increments on minor version
- Assembly version precision: major.minor
- Public releases only from master/main branches

## Project Structure

```
Qwiq/
??? src/
?   ??? Qwiq.Core/              # Core abstractions and interfaces
?   ??? Qwiq.Core.Rest/         # REST API client implementation
?   ??? Qwiq.Core.Soap/         # SOAP API client implementation
?   ??? Qwiq.Identity/          # Identity management
?   ??? Qwiq.Linq/              # LINQ query provider
?   ??? Qwiq.Linq.Identity/     # LINQ + Identity integration
?   ??? Qwiq.Mapper/            # Work item mapping
?   ??? Qwiq.Mapper.Identity/   # Mapper + Identity integration
??? test/
?   ??? Qwiq.Core.Tests/        # Core unit tests
?   ??? Qwiq.Identity.Tests/    # Identity unit tests
?   ??? Qwiq.Linq.Tests/        # LINQ unit tests
?   ??? Qwiq.Mapper.Tests/      # Mapper unit tests
?   ??? Qwiq.Integration.Tests/ # Integration tests
?   ??? Qwiq.Mocks/             # Test mocks and helpers
?   ??? Qwiq.Tests.Common/      # Shared test utilities
??? .github/
    ??? workflows/
        ??? main.yml            # CI/CD pipeline
```

## CI/CD Pipeline

### GitHub Actions

The project uses GitHub Actions for continuous integration:

**Workflow:** `.github/workflows/main.yml`

**Triggers:**
- Push to `develop` or `master` branches
- Pull requests to `develop` or `master` branches
- Manual workflow dispatch

**Build Steps:**
1. Checkout code (full Git history for NBGV)
2. Setup .NET 8 SDK
3. Restore .NET tools (NBGV)
4. Display version information
5. Restore NuGet packages
6. Build solution (Release configuration)
7. Run tests (with filters)
8. Upload test results as artifacts
9. Upload binaries as artifacts

**Environment Variables:**
```yaml
TEST_FILTER: "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST"
DOTNET_CLI_TELEMETRY_OPTOUT: 1
DOTNET_SKIP_FIRST_TIME_EXPERIENCE: 1
```

### Local Testing with CI Conditions

To replicate the CI build locally:

```bash
# Set environment variables
$env:TEST_FILTER = "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1

# Run the same commands as CI
dotnet tool restore
dotnet nbgv get-version
dotnet restore Qwiq.sln
dotnet build Qwiq.sln --configuration Release --no-restore
dotnet test Qwiq.sln --configuration Release --no-build --filter $env:TEST_FILTER
```

## Versioning

This project uses **Nerdbank.GitVersioning** for automatic semantic versioning.

### Version Format

- **Development builds:** `10.0.{height}-alpha+{git-commit-id}`
- **Public releases:** `10.0.{height}`

Where `{height}` is the number of commits since the version base.

### Version Sources

1. **AssemblyVersion:** Major.Minor (e.g., `10.0`)
2. **AssemblyFileVersion:** Major.Minor.Patch.0 (e.g., `10.0.42.0`)
3. **AssemblyInformationalVersion:** Full semantic version with metadata

### Checking Your Version

```bash
# Simple version display
dotnet nbgv get-version

# Show all version properties
dotnet nbgv get-version -a

# Get cloud build number
dotnet nbgv get-version -v CloudBuildNumber
```

### Creating a Release

To create a public release:

1. Merge changes to `master` or `main` branch
2. Tag the release: `git tag v10.0.42`
3. Push the tag: `git push origin v10.0.42`

NBGV will recognize the tag and generate a public release version.

## Troubleshooting

### "dotnet tool restore" fails

**Problem:** Tool manifest not found or corrupted.

**Solution:**
```bash
# Recreate tool manifest
dotnet new tool-manifest --force
dotnet tool install nbgv
```

### NBGV shows version 0.0.0

**Problem:** Not on a tracked branch or Git history is shallow.

**Solution:**
```bash
# Ensure full Git history
git fetch --unshallow

# Check current branch matches publicReleaseRefSpec
git branch --show-current
```

### Build warnings about nullable reference types

**Problem:** Nullable reference types are enabled with `WarningLevel=5`.

**Expected:** This is intentional. The project targets maximum code quality.

**Action:** Address warnings in new code. Legacy warnings are being addressed gradually.

### Tests fail with "localOnly" or "Benchmark" categories

**Problem:** Running tests that require local infrastructure.

**Solution:** Use the CI test filter:
```bash
dotnet test --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST"
```

### Multi-targeting build issues

**Problem:** Build fails for only one target framework.

**Solution:**
```bash
# Build specific target framework
dotnet build --framework netstandard2.0
dotnet build --framework net8.0

# Check for target-specific errors
dotnet build --verbosity detailed
```

## IDE Configuration

### Visual Studio 2022

**Recommended Settings:**
- Enable nullable reference types warnings
- EditorConfig support enabled
- Code cleanup on save (optional)

**Required Workloads:**
- .NET desktop development
- ASP.NET and web development (for REST client)

**Extensions:**
- Nerdbank.GitVersioning (optional, for version info in IDE)

### Visual Studio Code

**Required Extensions:**
- C# Dev Kit
- .NET Install Tool

**Recommended Extensions:**
- GitLens
- EditorConfig for VS Code

### JetBrains Rider

**Configuration:**
- .NET Core SDK: 8.0.x or later
- Enable Roslyn analyzers
- Enable nullable reference type inspection

## Performance Tips

### Faster Builds

```bash
# Parallel builds (default, but can be explicit)
dotnet build -m

# Skip building tests unless needed
dotnet build src/

# Use build output for tests
dotnet test --no-build
```

### Incremental Builds

```bash
# Use --no-restore when packages are already restored
dotnet build --no-restore

# Use --no-dependencies to skip building referenced projects
dotnet build MyProject --no-dependencies
```

## Getting Help

- **Documentation:** [README.md](README.md)
- **Issues:** https://github.com/rjmurillo/Qwiq/issues
- **Discussions:** https://github.com/rjmurillo/Qwiq/discussions
- **Migration Summary:** [MIGRATION_SUMMARY.md](MIGRATION_SUMMARY.md)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on:
- Code style
- Testing requirements
- Pull request process
- Commit message conventions

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) for details.
