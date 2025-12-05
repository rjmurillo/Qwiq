---
applyTo: "**/*.cs"
---

# C# File Instructions

> **MANDATORY**: You MUST follow these instructions when editing any C# file in this repository.

## Quick Reference

- Read [copilot-instructions.md](../copilot-instructions.md) for full repository guidelines
- Nullable reference types are ENABLED repository-wide
- Use runtime null checks, NOT JetBrains.Annotations
- Follow existing patterns in similar files

## Context Loading

When working on C# files, you MUST:

1. Read this entire instruction file before making changes
2. Cross-reference with [copilot-instructions.md](../copilot-instructions.md)
3. Check similar files for established patterns
4. Complete the Validation Checklist before submitting

## Nullable Reference Types

This repository uses C# nullable reference types (`<Nullable>enable</Nullable>`).

### Correct Patterns

```csharp
// Nullable return type
public string? GetValue() => _value;

// Non-null parameter with runtime validation
public void Method(SomeType parameter)
{
    if (parameter == null) throw new ArgumentNullException(nameof(parameter));
}

// Constructor with null check before base call
public MyClass(IService service)
    : base(service?.Property ?? throw new ArgumentNullException(nameof(service)))
{
}

// Nullable value type assertions in tests
value.HasValue.ShouldBeFalse(); // NOT: value.ShouldBeNull()
```

### Patterns to AVOID

```csharp
// ❌ JetBrains.Annotations (removed from codebase)
[NotNull] public string Value { get; }
[CanBeNull] public string? Name { get; }

// ❌ Duplicate validation (pick ONE approach)
Contract.Requires(param != null);
if (param == null) throw new ArgumentNullException(nameof(param)); // Don't use both!

// ❌ Silent exception swallowing
catch (Exception) { return null; } // Log the error at minimum
```

## Exception Handling

### Required Pattern

```csharp
catch (Exception ex)
{
    System.Diagnostics.Trace.TraceError($"Operation failed: {ex.Message}");
    throw; // or return appropriate value with clear documentation
}
```

### Rules

- **Never** swallow exceptions silently with empty catch blocks
- **Always** log exceptions before handling or rethrowing
- Use `ArgumentNullException` for null parameters
- Use `ArgumentException` for invalid (but non-null) parameters
- Choose ONE validation approach: `Contract.Requires` OR runtime null checks

## Known Class Patterns

### Revision Dual-Constructor Pattern

The `Revision` class has two constructors for different scenarios:

```csharp
// Constructor 1: Revision accessed via WorkItem.Revisions collection
public Revision(IWorkItem workItem, int index) { }

// Constructor 2: Standalone revision (field snapshot, no WorkItem reference)
public Revision(IFieldDefinitionCollection fieldDefinitions, int index) { }
```

When `WorkItem` is null (constructor 2), `Revision.Id` returns `null`.

### Null-Conditional Access for Link Types

```csharp
// LinkTypeEnd.ImmutableName may be null - use null-conditional
string.Equals(rl.LinkTypeEnd?.ImmutableName, linkTypeEndName, StringComparison.OrdinalIgnoreCase)
```

## Quality & Design Guidance

- **Optimize for testability**: Design with seam-friendly abstractions. New collaborators should be injected via interfaces so tests can substitute mocks from `Qwiq.Mocks` or Moq without reflection tricks.
- **Preserve cohesion and minimize coupling**: Keep each class focused. If a change touches multiple bounded contexts (Core, Mapper, Identity), validate that responsibilities are still well separated. Expose behavior via interfaces rather than concrete implementations.
- **Avoid duplicated identity and field constants**: Reuse existing sources such as `CoreFieldRefNames`, `TestData`, and `IdentityConstants`. Introducing new literals requires justification and documentation in shared locations.
- **Separate configuration from behavior**: Leave defaults in `Directory.Build.props`, `Directory.Packages.props`, or existing option classes. Application code should consume configuration values, not redefine them.
- **Encapsulate variability**: When adding behavior that differs by transport (REST vs SOAP) or target framework, isolate logic in strategy classes or provider pattern extensions instead of branching across call sites.
- **Program by intention**: Sketch the collaborators you need as if they already exist, then implement them behind focused interfaces. This enforces method-level cohesion and makes test seams explicit (Hunt & Thomas, 1999).
- **Pattern-oriented design**: Identify the dominant pattern (Strategy, Bridge, Adapter, Façade, etc.), note its intent in XML documentation or review notes, and ensure new classes reinforce rather than dilute that context (Alexander, 1979; Coplien, 1999). Apply Common Variability Analysis: describe the shared concept, list its variations, then map relationships (is-a, has-a, uses) before coding.
- **Separate use from creation**: Keep instantiation logic (factories, builders, `GetInstance` helpers) distinct from runtime behavior so that instantiation remains a late decision (Bloch, 2001). If a service both constructs and uses collaborators, refactor toward constructor injection or deferred factories.

## Test Patterns

### ContextSpecification Base Class

Unit tests follow the Given/When/Then pattern:

```csharp
[TestClass]
public class Given_some_context : ContextSpecification
{
    private MyClass _sut; // System Under Test

    public override void Given()
    {
        // Arrange - setup mocks and dependencies
        _sut = new MyClass();
    }

    public override void When()
    {
        // Act - perform the action being tested
        _sut.DoSomething();
    }

    [TestMethod]
    public void Then_expected_behavior()
    {
        // Assert using Shouldly
        _sut.Result.ShouldBe(expected);
    }
}
```

### Test Data Guidelines

- Cover positive, negative, and edge cases
- Use `MockWorkItem`, `MockRevision`, etc. from `Qwiq.Mocks`
- `IEnumerable` collections need `.First()` or `.ToList()` for indexing
- SOAP-specific classes are `internal` and require TFS infrastructure

## Validation Checklist

Before submitting changes, verify:

- [ ] Code compiles without errors
- [ ] No new warnings introduced
- [ ] Nullable annotations are correct
- [ ] Exception handling follows logging pattern
- [ ] Linting passes:
  - Run `dotnet format` to apply analyzer code fixes to C# files
  - Run `dotnet pprettier --write .` to auto-fix all formatting
- [ ] Tests pass: `dotnet test --filter "TestCategory!=localOnly&..."`
- [ ] Similar files checked for established patterns
- [ ] Architectural qualities reviewed (testability, cohesion, coupling) and no duplicated identity/configuration literals introduced

## Decision Trees

### When Adding Null Checks

1. Is the parameter used before validation? → Move check earlier
2. Does a base constructor need the value? → Use `?? throw` pattern
3. Is this a public API? → Document nullability in XML comments

### When to Stop and Ask

- Uncertain about nullable annotations
- Adding new exception types
- Changing public API signatures
- Modifying core interfaces (IWorkItem, IRevision, etc.)

## Related Instruction Files

- [project.instructions.md](project.instructions.md) - For .csproj files
- [msbuild.instructions.md](msbuild.instructions.md) - For Directory.Build.props/targets
- [generic.instructions.md](generic.instructions.md) - For multi-file changes
