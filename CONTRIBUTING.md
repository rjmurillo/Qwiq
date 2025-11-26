# Contributing to Qwiq

Thank you for your interest in contributing to Qwiq!

## Table of Contents

- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Commit Guidelines](#commit-guidelines)
- [Pull Requests](#pull-requests)

## Getting Started

### Prerequisites

1. **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Git** - [Download](https://git-scm.com/downloads)
3. **IDE** (optional)
   - Visual Studio 2022 (17.8+)
   - Visual Studio Code with C# Dev Kit
   - JetBrains Rider

### Setup

```bash
# Fork and clone
git clone https://github.com/YOUR-USERNAME/Qwiq.git
cd Qwiq

# Add upstream remote
git remote add upstream https://github.com/rjmurillo/Qwiq.git

# Build
dotnet tool restore
dotnet restore
dotnet build
dotnet test
```

See [BUILDING.md](BUILDING.md) for detailed instructions.

## Development Workflow

### 1. Create a Branch

```bash
# Update develop
git checkout develop
git pull upstream develop

# Create feature branch
git checkout -b feature/your-feature-name
```

**Branch naming:**
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation
- `refactor/` - Code refactoring
- `test/` - Test changes
- `chore/` - Build/tooling

### 2. Make Changes

- Write clean, readable code
- Follow existing style
- Add/update tests
- Update docs if needed

### 3. Test

```bash
# Run all tests
dotnet test

# Run with filter (excludes integration tests)
dotnet test --filter "TestCategory!=localOnly&TestCategory!=Benchmark"
```

### 4. Commit

Use [conventional commits](https://www.conventionalcommits.org/):

```bash
git commit -m "feat: add support for Azure DevOps OAuth"
git commit -m "fix: resolve null reference in WorkItemStore"
git commit -m "docs: update API documentation"
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
- **Warnings:** Level 5 (treat as guidance)

### Style

```csharp
// ? Good
public class WorkItemStore : IWorkItemStore
{
    private readonly IWorkItemStoreProxy _proxy;
    
    public WorkItemStore(IWorkItemStoreProxy proxy)
    {
        _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
    }
    
    public IWorkItem GetWorkItem(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive.", nameof(id));
        
        return _proxy.GetWorkItem(id);
    }
}
```

### Naming

- **Classes/Interfaces:** `PascalCase` (`WorkItemStore`, `IWorkItem`)
- **Methods:** `PascalCase` (`GetWorkItem`, `QueryByWiql`)
- **Properties:** `PascalCase` (`WorkItemType`, `AssignedTo`)
- **Private fields:** `_camelCase` (`_workItemStore`, `_proxy`)
- **Parameters/locals:** `camelCase` (`workItemId`, `result`)

### Null Safety

Always be explicit about nullability:

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
- `Save_ValidWorkItem_UpdatesStore`

### Categories

Apply categories for filtering:

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
// ? Good
result.Should().NotBeNull();
result.Items.Should().HaveCount(5);
result.Items.Should().Contain(x => x.Id == 123);

// ? Avoid
Assert.IsNotNull(result);
Assert.AreEqual(5, result.Items.Count);
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
- **perf:** Performance improvement
- **test:** Test changes
- **build:** Build system changes
- **ci:** CI/CD changes
- **chore:** Other changes

### Examples

```bash
# Simple feature
git commit -m "feat: add OAuth support"

# Bug fix with details
git commit -m "fix: resolve memory leak in connection pool

The pool was not disposing connections on exceptions.
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
- Explain *why* in the body, not *what*

## Pull Requests

### Before Submitting

- [ ] Code compiles: `dotnet build`
- [ ] Tests pass: `dotnet test`
- [ ] No warnings introduced
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
- [ ] Documentation update

## Testing
Describe how you tested the changes

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-reviewed my code
- [ ] Commented complex code
- [ ] Updated documentation
- [ ] No new warnings
- [ ] Added tests
- [ ] All tests pass
```

### Review Process

1. **Automated checks** - CI must pass
2. **Code review** - Maintainer approval required
3. **Testing** - Verify test coverage
4. **Merge** - Squash and merge to `develop`

## Release Process

We use **Nerdbank.GitVersioning (NBGV)** for versioning.

### Version Format

`{major}.{minor}.{height}[-{prerelease}]+{commit-id}`

Example: `10.0.42-alpha+g1234567`

### Releases

- **Development:** PRs to `develop` create pre-releases
- **Production:** Merges to `master`/`main` create public releases
- **Versioning:** Automatic via Git history

## Getting Help

- ?? [Build Documentation](BUILDING.md)
- ?? [Issue Tracker](https://github.com/rjmurillo/Qwiq/issues)
- ?? [Discussions](https://github.com/rjmurillo/Qwiq/discussions)

## Recognition

Contributors are recognized in release notes and the repository contributors page.

Thank you for contributing! ??
