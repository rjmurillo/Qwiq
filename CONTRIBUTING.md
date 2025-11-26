# Contributing to Qwiq

Thank you for your interest in contributing to Qwiq! This document provides guidelines and instructions for contributing.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Commit Message Conventions](#commit-message-conventions)
- [Pull Request Process](#pull-request-process)
- [Release Process](#release-process)

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](https://www.contributor-covenant.org/). By participating, you are expected to uphold this code.

## Getting Started

### Prerequisites

1. **Install .NET 8 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify: `dotnet --version`

2. **Install Git**
   - Download from: https://git-scm.com/downloads

3. **Fork the Repository**
   - Visit: https://github.com/rjmurillo/Qwiq
   - Click "Fork" button

4. **Clone Your Fork**
   ```bash
   git clone https://github.com/YOUR-USERNAME/Qwiq.git
   cd Qwiq
   ```

5. **Add Upstream Remote**
   ```bash
   git remote add upstream https://github.com/rjmurillo/Qwiq.git
   ```

### Build the Project

```bash
# Restore tools (including NBGV)
dotnet tool restore

# Restore packages
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

See [BUILDING.md](BUILDING.md) for detailed build instructions.

## Development Workflow

### 1. Create a Feature Branch

```bash
# Update your local develop branch
git checkout develop
git pull upstream develop

# Create a feature branch
git checkout -b feature/your-feature-name
```

Branch naming conventions:
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation changes
- `refactor/` - Code refactoring
- `test/` - Test additions or modifications
- `chore/` - Build/tooling changes

### 2. Make Your Changes

- Write clean, readable code
- Follow existing code style
- Add/update tests as needed
- Update documentation if required

### 3. Test Your Changes

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test test/Qwiq.Core.Tests/

# Run with filter
dotnet test --filter "TestCategory!=localOnly"
```

### 4. Commit Your Changes

Follow [conventional commit](#commit-message-conventions) format:

```bash
git add .
git commit -m "feat: add new feature description"
```

### 5. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub.

## Coding Standards

### General Guidelines

- **C# Version:** Use latest language features (C# 12+)
- **Target Frameworks:** 
  - .NET Standard 2.0 (for compatibility)
  - .NET 8 (for modern features)
- **Nullable Reference Types:** Enabled project-wide
- **Warning Level:** 5 (maximum)

### Code Style

This project follows standard C# coding conventions:

```csharp
// ? Good: Clear, descriptive names
public class WorkItemStore : IWorkItemStore
{
    private readonly IWorkItemStoreProxy _proxy;
    
    public WorkItemStore(IWorkItemStoreProxy proxy)
    {
        _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
    }
    
    public IWorkItem GetWorkItem(int id)
    {
        // Implementation
    }
}

// ? Bad: Unclear names, missing null checks
public class WIS
{
    private IWorkItemStoreProxy p;
    
    public WIS(IWorkItemStoreProxy proxy)
    {
        p = proxy;
    }
    
    public IWorkItem Get(int i)
    {
        // Implementation
    }
}
```

### Naming Conventions

- **Classes/Interfaces:** PascalCase (`WorkItemStore`, `IWorkItem`)
- **Methods:** PascalCase (`GetWorkItem`, `QueryByWiql`)
- **Properties:** PascalCase (`WorkItemType`, `AssignedTo`)
- **Fields (private):** _camelCase with underscore (`_workItemStore`)
- **Parameters:** camelCase (`workItemId`, `queryText`)
- **Local variables:** camelCase (`workItem`, `result`)

### Null Safety

Always use nullable reference types annotations:

```csharp
// ? Good: Explicit nullability
public string? GetOptionalValue(int id)
{
    return _cache.TryGetValue(id, out var value) ? value : null;
}

public string GetRequiredValue(int id)
{
    return _cache[id] ?? throw new KeyNotFoundException($"Value not found for id: {id}");
}

// ? Bad: Unclear nullability
public string GetValue(int id)
{
    return _cache[id]; // Can this return null?
}
```

### Comments

- Use XML documentation comments for public APIs
- Add inline comments only when code is not self-explanatory
- Prefer self-documenting code over comments

```csharp
/// <summary>
/// Retrieves a work item by its unique identifier.
/// </summary>
/// <param name="id">The work item ID.</param>
/// <returns>The work item, or null if not found.</returns>
/// <exception cref="ArgumentException">Thrown when id is less than or equal to zero.</exception>
public IWorkItem? GetWorkItem(int id)
{
    if (id <= 0)
        throw new ArgumentException("Work item ID must be positive.", nameof(id));
    
    return _store.Query(id);
}
```

## Testing Guidelines

### Test Structure

Follow the **Arrange-Act-Assert** pattern:

```csharp
[TestClass]
public class WorkItemStoreTests
{
    [TestMethod]
    public void GetWorkItem_ValidId_ReturnsWorkItem()
    {
        // Arrange
        var mockProxy = new MockWorkItemStoreProxy();
        var store = new WorkItemStore(mockProxy);
        var expectedId = 123;
        
        // Act
        var result = store.GetWorkItem(expectedId);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedId);
    }
}
```

### Test Naming

Use descriptive test names:
- Pattern: `MethodName_Scenario_ExpectedBehavior`
- Example: `Query_EmptyWiql_ThrowsArgumentException`

### Test Categories

Apply test categories for filtering:

```csharp
[TestMethod]
[TestCategory("Unit")]
public void UnitTest() { }

[TestMethod]
[TestCategory("Integration")]
[TestCategory("localOnly")]
public void IntegrationTest() { }

[TestMethod]
[TestCategory("Benchmark")]
public void BenchmarkTest() { }
```

### Assertions

Use **FluentAssertions** for readable assertions:

```csharp
// ? Good: Fluent and readable
result.Should().NotBeNull();
result.Items.Should().HaveCount(5);
result.Items.Should().Contain(x => x.Id == 123);

// ? Avoid: Old-style assertions
Assert.IsNotNull(result);
Assert.AreEqual(5, result.Items.Count);
```

### Test Coverage

- Aim for >80% code coverage for new code
- All public APIs should have tests
- Critical paths require comprehensive tests
- Edge cases and error conditions must be tested

## Commit Message Conventions

This project follows [Conventional Commits](https://www.conventionalcommits.org/).

### Format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Types

- **feat:** New feature
- **fix:** Bug fix
- **docs:** Documentation changes
- **style:** Code style changes (formatting, semicolons, etc.)
- **refactor:** Code refactoring
- **perf:** Performance improvements
- **test:** Adding or updating tests
- **build:** Build system or dependency changes
- **ci:** CI/CD configuration changes
- **chore:** Other changes (tooling, etc.)
- **revert:** Revert a previous commit

### Examples

```bash
# Feature
git commit -m "feat: add support for Azure DevOps OAuth"

# Fix
git commit -m "fix: correct null reference in WorkItemStore.Query"

# Breaking change
git commit -m "feat!: remove deprecated WorkItemStore.GetAll method

BREAKING CHANGE: GetAll() has been removed. Use Query() instead."

# With scope
git commit -m "feat(linq): add support for WIQL projections"

# Multiple paragraphs
git commit -m "fix: resolve memory leak in connection pooling

The connection pool was not properly disposing connections when
exceptions occurred during query execution.

Closes #123"
```

### Best Practices

- Use imperative mood ("add" not "added" or "adds")
- Keep first line under 72 characters
- Reference issue numbers when applicable
- Provide context in the body for complex changes

## Pull Request Process

### Before Submitting

1. ? **Code compiles without errors**
   ```bash
   dotnet build
   ```

2. ? **All tests pass**
   ```bash
   dotnet test
   ```

3. ? **Code follows style guidelines**
   - No unnecessary warnings
   - Nullable reference types properly annotated
   - XML documentation for public APIs

4. ? **Changes are committed with conventional commit messages**

5. ? **Branch is up to date with upstream/develop**
   ```bash
   git fetch upstream
   git rebase upstream/develop
   ```

### Pull Request Template

When creating a PR, include:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to break)
- [ ] Documentation update

## How Has This Been Tested?
Describe the tests you ran and their results

## Checklist
- [ ] My code follows the project's style guidelines
- [ ] I have performed a self-review of my code
- [ ] I have commented my code where necessary
- [ ] I have updated the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix/feature works
- [ ] New and existing unit tests pass locally
- [ ] Any dependent changes have been merged

## Related Issues
Closes #issue_number
```

### Review Process

1. **Automated Checks:** CI pipeline must pass
2. **Code Review:** At least one maintainer approval required
3. **Testing:** Verify tests cover the changes
4. **Documentation:** Ensure docs are updated if needed

### After Approval

Maintainers will:
1. Merge your PR to `develop` branch
2. Include it in the next release
3. Update changelog

## Release Process

This project uses **Nerdbank.GitVersioning (NBGV)** for automatic versioning.

### Version Scheme

```
{major}.{minor}.{patch}[-{prerelease}]+{git-commit-id}
```

Example: `10.0.42-alpha+g1234567`

### Creating a Release

1. **Update version.json** (if needed)
   ```json
   {
     "version": "10.0-alpha",
     "versionIncrement": "minor"
   }
   ```

2. **Merge to master/main**
   - PRs to `master` create public releases
   - PRs to `develop` create pre-release versions

3. **Tag the release** (optional)
   ```bash
   git tag v10.0.42
   git push origin v10.0.42
   ```

4. **GitHub Actions** automatically:
   - Builds the release
   - Runs all tests
   - Creates release artifacts
   - Publishes to NuGet (when configured)

### Pre-release Versions

Development builds from `develop` branch:
- Version: `10.0.{height}-alpha`
- Published to MyGet feed
- Not production-ready

## Getting Help

- ?? **Documentation:** [README.md](README.md), [BUILDING.md](BUILDING.md)
- ?? **Issues:** https://github.com/rjmurillo/Qwiq/issues
- ?? **Discussions:** https://github.com/rjmurillo/Qwiq/discussions
- ?? **Maintainers:** See [CODEOWNERS](.github/CODEOWNERS) file

## Recognition

Contributors are recognized in:
- Release notes
- Repository contributors page
- Special thanks in documentation

Thank you for contributing to Qwiq! ??
