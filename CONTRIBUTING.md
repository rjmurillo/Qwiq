# Contributing to Qwiq

Thank you for your interest in contributing to Qwiq!

## Table of Contents

- [Building from Source](#building-from-source)
- [Development Setup](#development-setup)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Commit Guidelines](#commit-guidelines)
- [Pull Requests](#pull-requests)

## Building from Source

### Prerequisites

**Required:**
- **.NET 8 SDK** or later - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** - [Download](https://git-scm.com/downloads)

**Optional:**
- Visual Studio 2022 (17.8+)
- Visual Studio Code with C# Dev Kit
- JetBrains Rider

Verify installation:
```bash
dotnet --version  # Should show 8.0.x or higher
git --version
```

### Quick Start

```bash
# Clone the repository
git clone https://github.com/rjmurillo/Qwiq.git
cd Qwiq

# Restore .NET tools (includes Nerdbank.GitVersioning)
dotnet tool restore

# Restore packages and build
dotnet restore
dotnet build

# Run tests
dotnet test
```

### Build Commands

```bash
# Debug build (default)
dotnet build

# Release build
dotnet build --configuration Release

# Build without restore
dotnet build --no-restore

# Clean and rebuild
dotnet clean && dotnet build
```

**Note:** Projects target both .NET Standard 2.0 and .NET 8. The build system handles both automatically.

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests excluding integration tests (like CI does)
dotnet test --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST"

# Run specific test project
dotnet test test/Qwiq.Core.Tests/

# Run with detailed output
dotnet test --verbosity detailed
```

**Test Categories:**
- `localOnly` - Requires local TFS/Azure DevOps instance
- `Benchmark` - Performance tests
- `SOAP` - SOAP client tests
- `REST` - REST client tests

### Project Structure

```
Qwiq/
??? src/
?   ??? Qwiq.Core/              # Core abstractions
?   ??? Qwiq.Core.Rest/         # REST API client
?   ??? Qwiq.Core.Soap/         # SOAP client (legacy)
?   ??? Qwiq.Identity/          # Identity helpers
?   ??? Qwiq.Linq/              # LINQ provider
?   ??? Qwiq.Mapper/            # Object mapping
?   ??? ...
??? test/
?   ??? Qwiq.Core.Tests/
?   ??? Qwiq.Mocks/             # Test helpers
?   ??? ...
??? .github/workflows/          # CI/CD
```

### Versioning

This project uses **Nerdbank.GitVersioning (NBGV)** for automatic semantic versioning.

**Version format:** `{major}.{minor}.{height}[-{prerelease}]+{commit-id}`

**Example:** `10.0.42-alpha+g1234567`

```bash
# Check current version
dotnet nbgv get-version

# Get specific version property
dotnet nbgv get-version -v SimpleVersion
```

### CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/main.yml`) runs on:
- Push to `develop` or `master`
- Pull requests to `develop` or `master`

**Build steps:**
1. Setup .NET 8 SDK
2. Restore .NET tools (NBGV)
3. Display version
4. Restore packages
5. Build (Release)
6. Run tests
7. Upload artifacts

**Replicate CI locally:**
```bash
dotnet tool restore
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST"
```

### Troubleshooting

**Problem:** "dotnet tool restore" fails

**Solution:**
```bash
dotnet new tool-manifest --force
dotnet tool install nbgv
```

**Problem:** NBGV shows version 0.0.0

**Solution:**
```bash
git fetch --unshallow  # Ensure full Git history
```

**Problem:** Many nullable reference type warnings

**Expected.** The project enables maximum code quality checks (Warning Level 5). Address warnings in new code; legacy warnings are being addressed gradually.

**Problem:** Tests fail

**Solution:** Use the CI test filter to skip infrastructure-dependent tests (see [Running Tests](#running-tests)).

## Development Setup

### Fork and Clone

```bash
# Fork the repository on GitHub, then:
git clone https://github.com/YOUR-USERNAME/Qwiq.git
cd Qwiq

# Add upstream remote
git remote add upstream https://github.com/rjmurillo/Qwiq.git

# Build and verify
dotnet tool restore
dotnet build
dotnet test
```

### IDE Setup

**Visual Studio 2022:**
- Required workloads: .NET desktop development, ASP.NET and web development
- Enable nullable reference type warnings
- EditorConfig support enabled

**Visual Studio Code:**
- Install: C# Dev Kit, .NET Install Tool

**JetBrains Rider:**
- .NET SDK: 8.0.x or later
- Enable Roslyn analyzers

## Development Workflow

### 1. Create a Branch

```bash
# Update your local develop branch
git checkout develop
git pull upstream develop

# Create feature branch
git checkout -b feature/your-feature-name
```

**Branch naming:**
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation
- `refactor/` - Code restructuring
- `test/` - Test changes
- `chore/` - Build/tooling

### 2. Make Changes

- Write clean, readable code
- Follow existing style
- Add/update tests
- Update docs if needed

### 3. Test

```bash
dotnet build
dotnet test
```

### 4. Commit

Use [conventional commits](https://www.conventionalcommits.org/):

```bash
git commit -m "feat: add OAuth support"
git commit -m "fix: resolve memory leak in connection pool"
git commit -m "docs: update API examples"
```

### 5. Push and Create PR

```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub.

## Coding Standards

### General

- **C# Version:** Latest features (C# 12+)
- **Targets:** .NET Standard 2.0 and .NET 8
- **Nullable:** Enabled (all reference types)
- **Warnings:** Level 5

### Style

```csharp
// ? Good: Clear naming, null checks
public class WorkItemStore : IWorkItemStore
{
    private readonly IWorkItemStoreProxy _proxy;
    
    public WorkItemStore(IWorkItemStoreProxy proxy)
    {
        _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
    }
    
    public IWorkItem? GetWorkItem(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive.", nameof(id));
        
        return _proxy.GetWorkItem(id);
    }
}
```

### Naming

- **Classes/Interfaces:** `PascalCase` (`WorkItemStore`, `IWorkItem`)
- **Methods/Properties:** `PascalCase` (`GetWorkItem`, `AssignedTo`)
- **Private fields:** `_camelCase` (`_proxy`, `_store`)
- **Parameters/locals:** `camelCase` (`workItemId`, `result`)

### Null Safety

Be explicit about nullability:

```csharp
// ? Good: Clear intent
public string? GetOptionalValue(int id)
{
    return _cache.TryGetValue(id, out var value) ? value : null;
}

public string GetRequiredValue(int id)
{
    return _cache[id] ?? throw new KeyNotFoundException($"Not found: {id}");
}

// ? Bad: Unclear
public string GetValue(int id)
{
    return _cache[id]; // Can this be null?
}
```

### Documentation

Use XML comments for public APIs:

```csharp
/// <summary>
/// Retrieves a work item by ID.
/// </summary>
/// <param name="id">The work item ID (must be positive).</param>
/// <returns>The work item, or null if not found.</returns>
/// <exception cref="ArgumentException">Thrown when id is zero or negative.</exception>
public IWorkItem? GetWorkItem(int id)
```

## Testing

### Structure

Use **Arrange-Act-Assert**:

```csharp
[TestMethod]
public void GetWorkItem_ValidId_ReturnsWorkItem()
{
    // Arrange
    var store = new MockWorkItemStore();
    var expectedId = 123;
    
    // Act
    var result = store.GetWorkItem(expectedId);
    
    // Assert
    result.Should().NotBeNull();
    result!.Id.Should().Be(expectedId);
}
```

### Naming

Pattern: `MethodName_Scenario_ExpectedBehavior`

Examples:
- `Query_EmptyWiql_ThrowsArgumentException`
- `GetWorkItem_NonExistentId_ReturnsNull`

### Categories

```csharp
[TestMethod]
[TestCategory("Unit")]
public void UnitTest() { }

[TestMethod]
[TestCategory("Integration")]
[TestCategory("localOnly")]
public void IntegrationTest() { }
```

### Assertions

Use **FluentAssertions**:

```csharp
// ? Good: Readable and fluent
result.Should().NotBeNull();
result.Items.Should().HaveCount(5);
result.Items.Should().Contain(x => x.Id == 123);
```

### Coverage

- Aim for >80% code coverage
- All public APIs should have tests
- Test both success and failure paths

## Commit Guidelines

Follow [Conventional Commits](https://www.conventionalcommits.org/):

### Format

```
<type>[optional scope]: <description>

[optional body]

[optional footer]
```

### Types

- **feat:** New feature
- **fix:** Bug fix
- **docs:** Documentation
- **style:** Formatting
- **refactor:** Code restructuring
- **perf:** Performance
- **test:** Tests
- **build:** Build system
- **ci:** CI/CD
- **chore:** Other

### Examples

```bash
# Simple
git commit -m "feat: add OAuth support"

# With details
git commit -m "fix: resolve memory leak in connection pool

Pool was not disposing connections on exceptions.
Added proper using blocks and exception handling.

Closes #123"

# Breaking change
git commit -m "feat!: remove deprecated GetAll method

BREAKING CHANGE: GetAll() removed. Use Query() instead."

# With scope
git commit -m "feat(linq): add projection support"
```

### Best Practices

- Use imperative mood ("add" not "added")
- Keep first line under 72 characters
- Reference issues when applicable
- Explain *why* in body, not *what*

## Pull Requests

### Before Submitting

- [ ] Code compiles: `dotnet build`
- [ ] Tests pass: `dotnet test`
- [ ] No new warnings
- [ ] Nullable annotations correct
- [ ] XML docs for public APIs
- [ ] Conventional commit messages
- [ ] Branch up to date with `upstream/develop`

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation

## Testing
How you tested the changes

## Checklist
- [ ] Follows style guidelines
- [ ] Self-reviewed code
- [ ] Commented complex code
- [ ] Updated documentation
- [ ] No new warnings
- [ ] Added tests
- [ ] Tests pass
```

### Review Process

1. **Automated checks** - CI must pass
2. **Code review** - Maintainer approval required
3. **Merge** - Squash and merge to `develop`

## Release Process

Releases use **NBGV** for automatic versioning.

**Version format:** `{major}.{minor}.{height}[-{prerelease}]+{commit}`

- **Development:** PRs to `develop` ? pre-release versions
- **Production:** Merges to `master`/`main` ? public releases
- **Versioning:** Automatic from Git history

## Getting Help

- ?? [Issue Tracker](https://github.com/rjmurillo/Qwiq/issues)
- ?? [Discussions](https://github.com/rjmurillo/Qwiq/discussions)

Thank you for contributing! ??
