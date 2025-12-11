# QWIQ Testing Reference Documentation

## Integration Test Sandbox Environment

| Setting | Value |
|---------|-------|
| Organization URL | `https://qwiq-sandbox.visualstudio.com/` |
| Project Name | `WIT` |
| Project ID | `0a4c0240-1a67-45de-93db-fc1de9f54ffb` |
| Process Template | `WIT_TEST` |
| Test User | Richard Murillo (`rjmurillo@msn.com`) |

## Test Work Items

| ID | Type | Title | Purpose |
|----|------|-------|---------|
| 1 | Bug | Integration Test | Basic work item tests |
| 2 | Task | Child Task for Integration Tests | Child of ID 3 (hierarchy) |
| 3 | User Story | Parent Story for Integration Tests | Parent for hierarchy tests |
| 4 | Bug | Bug for Mapper Integration Tests | Mapper tests |
| 5 | Bug | Work Item with Links for Integration Tests | Work item with links |
| 6 | Task | Child Task 2 for Hierarchy | Second child of ID 3 |

**Hierarchy Structure:**
```
User Story (ID: 3) - "Parent Story for Integration Tests"
├── Task (ID: 2) - "Child Task for Integration Tests"
└── Task (ID: 6) - "Child Task 2 for Hierarchy"
```

## Environment Variables for CI/CD

| Variable | Purpose | Default |
|----------|---------|---------|
| `QWIQ_TEST_URL` | Override sandbox URL | `https://qwiq-sandbox.visualstudio.com/` |
| `QWIQ_PROJECT_GUID` | Override project GUID | `0a4c0240-1a67-45de-93db-fc1de9f54ffb` |
| `AZURE_DEVOPS_EXT_PAT` | PAT for authentication | (Windows auth) |

**PAT Scopes Required:**
- Work Items (Read & Write)
- Project and Team (Read)
- Identity (Read)

## Package Tests

The `Qwiq.Package.Tests` project validates NuGet package contents:

```powershell
# Run package tests (requires dotnet pack first)
dotnet pack Qwiq.sln -c Release
dotnet test test/Qwiq.Package.Tests/Qwiq.Package.Tests.csproj -c Release

# Update baselines after package changes
dotnet verify accept -w test/Qwiq.Package.Tests
```

## Known Limitations

1. **Interactive Authentication**: Integration tests prompt for credentials
2. **Single Test User**: Only one user in sandbox
3. **REST/SOAP Differences**: REST returns `System.AreaLevel1-7` fields, SOAP does not
4. **Windows Required**: Full test suite needs Windows for `net472`

## Test Assertion Cheat Sheet

```csharp
// Equality
result.ShouldBe(expected);
result.ShouldNotBe(unexpected);

// Collections
items.ShouldContain(item);
items.ShouldHaveSingleItem();
items.ShouldBeEmpty();
items.Count().ShouldBe(5);

// Exceptions
Should.Throw<ArgumentNullException>(() => method(null));
action.ShouldThrow<InvalidOperationException>();

// Nullability
result.ShouldBeNull();
result.ShouldNotBeNull();
nullableInt.HasValue.ShouldBeFalse();  // For int?

// Strings
text.ShouldStartWith("prefix");
text.ShouldContain("substring");
text.ShouldBeNullOrEmpty();
```
