---
name: qwiq-testing
description: Testing patterns for QWIQ including TDD for refactoring, ContextSpecification base class, test categories, and mock system usage. Use when writing tests, fixing test failures, or refactoring code.
---

# QWIQ Testing Patterns

## Purpose

This skill provides guidance for writing and maintaining tests in the QWIQ repository. It covers the ContextSpecification pattern, test categories, mock system usage, and mandatory TDD practices for refactoring.

## When To Use

- Writing new unit tests
- Fixing failing tests
- Refactoring code (TDD is MANDATORY)
- Understanding test categories and filters
- Using the mock system (`Qwiq.Mocks`)

## Instructions

### 1. Test Commands

```powershell
# Run unit tests (excludes integration tests)
dotnet test Qwiq.sln -c Release --no-build --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST&TestCategory!=IntegrationTests"

# Run specific test project
dotnet test test/Qwiq.Core.Tests/Qwiq.Core.Tests.csproj

# Run tests matching a pattern
dotnet test --filter "FullyQualifiedName~WorkItemCore"
```

### 2. Test Categories

| Category | Description | When to Run |
|----------|-------------|-------------|
| (default) | Unit tests | Always (CI) |
| `localOnly` | Requires local TFS | Manual only |
| `Benchmark` | Performance tests | Manual only |
| `SOAP` | SOAP integration | Manual (interactive auth) |
| `REST` | REST integration | Manual (interactive auth) |
| `IntegrationTests` | Full integration | Manual (interactive auth) |

### 3. ContextSpecification Pattern

All unit tests follow Given/When/Then:

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

### 4. Mock System Usage

Use mocks from `Qwiq.Mocks`:

```csharp
// Create isolated mock store per test
using var store = new MockWorkItemStore();

// Add test data
store.Add(new MockWorkItem("Bug") { Title = "Test Bug" });

// Execute code under test
var results = myService.QueryBugs(store);

// Assert results
results.ShouldHaveSingleItem();
```

**Available Mocks:**
- `MockWorkItemStore` - In-memory work item storage
- `MockWorkItem` - Work item with field storage
- `MockRevision` - Revision data
- `MockIdentityManagementService` - Identity resolution
- `MockFieldDefinitionCollection` - Lazy field definitions

### 5. TDD for Refactoring (MANDATORY)

When refactoring, especially for nullable fixes:

**Step 1: Write Tests FIRST**
```csharp
[TestMethod]
public void Parameterless_constructor_should_allow_field_operations()
{
    var item = new TestableWorkItemCore();
    item.SetValue("test", "value");  // Should not throw
    var result = item.GetValue("test");
    result.ShouldEqual("value");
}
```

**Step 2: Verify Tests Pass** with current implementation

**Step 3: Make Changes** (minimal, behavior-preserving)

**Step 4: Verify Tests Still Pass**

### 6. Assertion Patterns

```csharp
// Use Shouldly
result.ShouldBe(expected);
collection.ShouldHaveSingleItem();
action.ShouldThrow<ArgumentNullException>();

// For nullable value types
value.HasValue.ShouldBeFalse();  // NOT: value.ShouldBeNull()

// For collections
items.First().ShouldBe(expected);  // IEnumerable needs .First()
```

### 7. Test Data Constants

Use `TestData.cs` for integration test constants:

```csharp
public static class TestData
{
    public const int BasicWorkItemId = 1;
    public const int HierarchyParentId = 3;
    public const string TestUserUpn = "rjmurillo@msn.com";
    public const string ProjectName = "WIT";
}
```

### 8. Code Coverage

Run tests with coverage collection:

```powershell
# Run tests with XPlat Code Coverage (Coverlet)
dotnet test Qwiq.sln -c Release --collect:"XPlat Code Coverage" --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST&TestCategory!=IntegrationTests"

# Generate HTML report
reportgenerator "-reports:artifacts/TestResults/**/coverage.cobertura.xml" "-targetdir:./artifacts/coverage" "-reporttypes:Html;HtmlSummary;Badges"

# Open report
start ./artifacts/coverage/index.html
```

**Coverage Configuration:**
- Settings file: `coverage.runsettings` (repository root)
- Output format: Cobertura XML
- Report location: `artifacts/coverage/`
- Included: Only Qwiq.* production assemblies
- Excluded: Test projects, mocks, benchmarks, generated code

**Coverage Guidelines:**
| Metric | Minimum | Target |
|--------|---------|--------|
| Line Coverage (new code) | 70% | 80% |
| Branch Coverage (new code) | 60% | 70% |

## Examples

### Example 1: Writing a Unit Test

**User goal:** Test a new method on WorkItemStore

**Process:**
1. Create test class inheriting `ContextSpecification`
2. In `Given()`, set up `MockWorkItemStore` with test data
3. In `When()`, call the method under test
4. Add `[TestMethod]` methods for each assertion
5. Use Shouldly for assertions

**Expected output:** Test class following Given/When/Then pattern

### Example 2: TDD for Nullable Fix

**User goal:** Fix CS8618 on a field without breaking behavior

**Process:**
1. Write test documenting current behavior
2. Run test - must pass
3. Fix the field initialization
4. Run test - must still pass
5. If test fails, fix code (not test) unless test was wrong

**Expected output:** Field properly initialized, tests prove no behavior change

## Related Resources

- See [REFERENCE.md](./REFERENCE.md) for test project details and sandbox environment
- See [../qwiq-csharp/SKILL.md](../qwiq-csharp/SKILL.md) for code patterns being tested
