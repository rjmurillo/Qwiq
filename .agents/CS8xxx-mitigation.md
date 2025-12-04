# Product Requirements Document: CS8xxx Nullable Reference Type Warning Mitigation

## Document Information

- **Document ID**: CS8xxx-MITIGATION-PRD
- **Version**: 1.0
- **Date**: December 4, 2025
- **Status**: Draft
- **Repository**: Qwiq - Quick Work Item Queries for Azure DevOps/TFS
- **Branch**: `develop`

---

## Executive Summary

This PRD outlines the strategy for systematically eliminating all suppressed CS8xxx nullable reference type warnings across the Qwiq codebase. These warnings are currently suppressed in `.editorconfig` as technical debt from the SDK-style migration. The goal is to properly annotate all code with nullable reference types, improving type safety, reducing null reference exceptions, and aligning with modern C# best practices.

**Current State**: 16 CS8xxx warnings suppressed; ~198 estimated warnings across 5 projects
**Target State**: 0 CS8xxx warnings; all code properly annotated with nullable reference types
**Timeline**: Phased approach over multiple PRs (estimated 4-6 weeks)

---

## 1. Problem Statement

### 1.1 Background

Qwiq has nullable reference types enabled repository-wide (`<Nullable>enable</Nullable>` in `Directory.Build.props`), but during the SDK-style migration (PRs #31, #32, #43-#47), all CS8xxx compiler warnings were suppressed in `.editorconfig` to prevent build breaks. This created technical debt that needs systematic resolution.

### 1.2 Current Issues

1. **Hidden Null Safety Issues**: Suppressed warnings mask potential `NullReferenceException` risks
2. **Incomplete Type Safety**: Code cannot benefit from nullable reference type protection
3. **Developer Experience**: IDE warnings don't guide developers away from null-related bugs
4. **Maintenance Burden**: Future refactoring may reintroduce null-safety issues without detection
5. **Inconsistent Codebase**: Qwiq.Core is fully annotated (0 warnings), other projects are not

### 1.3 Impact

**Without Resolution**:
- Increased runtime null reference exceptions
- Poor code maintainability
- Inconsistent patterns across the codebase
- Inability to detect null-safety regressions

**With Resolution**:
- Compile-time null safety enforcement
- Better developer experience with accurate IDE warnings
- Reduced null reference exceptions in production
- Consistent, modern C# patterns throughout

---

## 2. Goals

### 2.1 Primary Goals

1. **Enable All CS8xxx Warnings**: Remove all CS8xxx suppressions from `.editorconfig`
2. **Zero Warning Build**: Achieve clean builds with nullable reference types fully enabled
3. **Proper Null Annotations**: All interfaces and implementations correctly annotated
4. **No Behavior Changes**: Code changes are annotation-only; no functional modifications
5. **Maintain Test Coverage**: All existing tests continue to pass

### 2.2 Success Metrics

| Metric | Current | Target | Measurement |
|--------|---------|--------|-------------|
| CS8xxx suppressions in .editorconfig | 16 | 0 | Count of `dotnet_diagnostic.CS8xxx.severity = none` lines |
| Build warnings (nullable) | ~198 estimated | 0 | `dotnet build` output with CS8xxx enabled |
| Test pass rate | 100% | 100% | Test suite results |
| Projects fully annotated | 1/7 (14%) | 7/7 (100%) | Manual verification |
| Production NullReferenceExceptions | Baseline | -20% (6 months post-migration) | Telemetry (if available) |

### 2.3 Non-Goals

1. **Functional Changes**: This is NOT a feature or bug-fix initiative
2. **API Changes**: No breaking changes to public APIs
3. **Performance Optimization**: Not addressing performance issues
4. **New Nullable Features**: Not adding new C# 10+ nullable features beyond basic annotation
5. **Legacy Framework Changes**: Not changing .NET 4.7.2 compatibility requirements

---

## 3. Scope

### 3.1 In-Scope Projects

Prioritized by warning count and dependency order:

| Priority | Project | Target Frameworks | Est. Warnings | Dependencies | Status |
|----------|---------|-------------------|---------------|--------------|--------|
| ✅ Complete | `Qwiq.Core` | net472;netstandard2.0;net8.0 | 0 | None | Fully annotated |
| 🥇 P0 | `Qwiq.Core.Rest` | net472;netstandard2.0;net8.0 | ~42 | Qwiq.Core | Partially complete |
| 🥈 P1 | `Qwiq.Identity` | net472;net8.0 | ~28 | Qwiq.Core | Not started |
| 🥈 P1 | `Qwiq.Core.Soap` | net472 | Unknown | Qwiq.Core | Not started |
| 🥉 P2 | `Qwiq.Linq` | net472;net8.0 | ~128 | Qwiq.Core | Not started |
| 🥉 P2 | `Qwiq.Mapper` | net472;net8.0 | Unknown | Qwiq.Core, Qwiq.Linq | Not started |
| 🥉 P2 | `Qwiq.Mapper.Identity` | net472;net8.0 | Unknown | Qwiq.Mapper, Qwiq.Identity | Not started |

**Test Projects** (in parallel with source projects):
- `Qwiq.Core.Tests`
- `Qwiq.Identity.Tests`
- `Qwiq.Linq.Tests`
- `Qwiq.Mapper.Tests`
- `Qwiq.Mocks` (critical for test infrastructure)

### 3.2 Out-of-Scope

1. **External Dependencies**: Not fixing nullable warnings in NuGet packages
2. **Generated Code**: Not annotating auto-generated files (e.g., AssemblyInfo)
3. **Obsolete APIs**: Not addressing obsolete API warnings (SYSLIB0021, SYSLIB0050, etc.)
4. **CLS Compliance**: Not addressing CS3xxx warnings
5. **Documentation**: Not fixing CS1591 (missing XML comments)

---

## 4. CS8xxx Warning Catalog

### 4.1 Suppressed Warnings

| Code | Description | Severity | Frequency (Estimated) | Fix Complexity |
|------|-------------|----------|----------------------|----------------|
| CS8600 | Converting null literal or possible null value to non-nullable type | High | Common | Medium |
| CS8601 | Possible null reference assignment | High | Common | Medium |
| CS8602 | Dereference of a possibly null reference | Critical | Very Common | Medium-High |
| CS8603 | Possible null reference return | High | Common | Low-Medium |
| CS8604 | Possible null argument for parameter | High | Very Common | Low-Medium |
| CS8605 | Unboxing a possibly null value | Medium | Rare | Low |
| CS8618 | Non-nullable property must contain a non-null value when exiting constructor | High | Common | Medium |
| CS8619 | Nullability of reference types in value doesn't match target type | Medium | Rare | Medium |
| CS8620 | Argument cannot be used for parameter due to nullability differences | High | Common | Medium |
| CS8625 | Cannot convert null literal to non-nullable reference type | High | Common | Low |
| CS8629 | Nullable value type may be null | Medium | Uncommon | Low |
| CS8764 | Nullability of return type doesn't match overridden member | High | Uncommon | Medium |
| CS8765 | Nullability of type of parameter doesn't match overridden member | High | Uncommon | Medium |
| CS8766 | Nullability of reference types in return type doesn't match implicitly implemented member | High | Uncommon | Medium |
| CS8767 | Nullability of reference types in type of parameter doesn't match implicitly implemented member | High | Uncommon | Medium |
| CS8769 | Nullability of reference types in type of parameter doesn't match implemented member | High | Uncommon | Medium |

### 4.2 Warning Distribution (Estimated)

```
Qwiq.Core.Rest:       ~42 warnings  (21%)
Qwiq.Linq:           ~128 warnings  (65%)
Qwiq.Identity:        ~28 warnings  (14%)
Qwiq.Core.Soap:      TBD warnings
Qwiq.Mapper:         TBD warnings
Qwiq.Mapper.Identity: TBD warnings
--------------------------------------------
TOTAL:               ~198+ warnings
```

---

## 5. Technical Approach

### 5.1 Annotation Strategy

#### 5.1.1 Established Patterns (from Qwiq.Core)

The following patterns are proven from the completed Qwiq.Core migration:

**Pattern 1: Nullable Reference Types**
```csharp
// Use ? for properties/fields that can legitimately be null
public string? Name { get; }
public object? Value { get; set; }
```

**Pattern 2: Non-Null Guarantee with `null!`**
```csharp
// For lazy-initialized fields guaranteed to be set before use
private Func<IFieldCollection> _fieldFactory = null!;

public MyClass()
{
    // Later initialization guaranteed by calling pattern
    Initialize();
}
```

**Pattern 3: Runtime Null Checks**
```csharp
// Constructor parameter validation
public Field(IRevisionInternal revision, IFieldDefinition fieldDefinition)
{
    _revision = revision ?? throw new ArgumentNullException(nameof(revision));
    FieldDefinition = fieldDefinition ?? throw new ArgumentNullException(nameof(fieldDefinition));
}
```

**Pattern 4: Null-Conditional Access**
```csharp
// Use ?. for potentially null references
var immutableName = linkTypeEnd?.ImmutableName;
```

**Pattern 5: Nullability Attributes**
```csharp
// For Try* pattern methods
public bool TryGetValue([MaybeNullWhen(false)] out string value)
{
    if (HasValue)
    {
        value = _value;
        return true;
    }
    value = null;
    return false;
}

// For nullable inputs that won't be null on success
public void Process([AllowNull] string input)
{
    // Implementation
}

// For non-null returns from nullable parameters
[return: NotNullIfNotNull(nameof(input))]
public string? Transform(string? input) => input?.ToUpper();
```

**Pattern 6: Nullable Value Types**
```csharp
// For value types that can be null
public int? Id => _workItem?.Id;

// Assertions
value.HasValue.ShouldBeFalse(); // Not ShouldBeNull<int>()
```

#### 5.1.2 Interface and Implementation Consistency

**Critical Rule**: When annotating, BOTH interfaces AND implementations must match:

```csharp
// Interface
public interface IWorkItem
{
    string? Title { get; }  // Nullable
    IWorkItemType Type { get; }  // Non-nullable
}

// Implementation
public class WorkItem : IWorkItem
{
    public string? Title => Fields["System.Title"]?.ToString();  // Must match
    public IWorkItemType Type => _type;  // Must match
}
```

### 5.2 Fix Workflow

#### Phase 1: Project-Level Analysis
1. **Isolate Project**: Work on one project at a time
2. **Enable Warnings**: Comment out suppressions for target project
3. **Build and Catalog**: Run `dotnet build` and catalog all warnings by type
4. **Count Warnings**: Verify estimated warning count

#### Phase 2: Systematic Annotation
1. **Interfaces First**: Annotate all interfaces in the project
2. **Base Classes**: Annotate abstract classes and base implementations
3. **Concrete Implementations**: Annotate remaining concrete classes
4. **Internal Types**: Annotate internal types last

#### Phase 3: Verification
1. **Build Clean**: Ensure zero warnings for the project
2. **Run Tests**: Execute full test suite for affected projects
3. **Integration Tests**: Run integration tests if applicable (exclude `localOnly`, `SOAP`, `REST`, `IntegrationTests` categories in CI)
4. **Manual Testing**: Spot-check critical paths if needed

#### Phase 4: Commit and PR
1. **Small Commits**: Commit after each interface/class completion
2. **Conventional Commits**: Use `refactor(project): annotate Type with nullable reference types`
3. **PR per Project**: One PR per project (or logical group)
4. **Documentation**: Update copilot-instructions.md status table

### 5.3 Common Fix Patterns

#### Fix CS8600: Converting null literal to non-nullable
```csharp
// Before
string name = GetName(); // May return null

// After - Option 1: Make variable nullable
string? name = GetName();

// After - Option 2: Provide default
string name = GetName() ?? "Unknown";
```

#### Fix CS8601: Possible null reference assignment
```csharp
// Before
public string Name { get; set; } // Assigned from nullable source

// After - Option 1: Make property nullable
public string? Name { get; set; }

// After - Option 2: Add validation
public string Name
{
    get => _name;
    set => _name = value ?? throw new ArgumentNullException(nameof(value));
}
```

#### Fix CS8602: Dereference of possibly null reference
```csharp
// Before
string upper = name.ToUpper(); // name might be null

// After - Option 1: Null-conditional
string? upper = name?.ToUpper();

// After - Option 2: Null check
if (name != null)
{
    string upper = name.ToUpper();
}

// After - Option 3: Null-forgiving operator (use sparingly)
string upper = name!.ToUpper(); // Only if you KNOW name is not null
```

#### Fix CS8603: Possible null reference return
```csharp
// Before
public string GetName() => _name; // _name is nullable

// After - Option 1: Make return type nullable
public string? GetName() => _name;

// After - Option 2: Guarantee non-null
public string GetName() => _name ?? "Unknown";
```

#### Fix CS8604: Possible null argument
```csharp
// Before
Process(nullableValue); // Process expects non-null

// After - Option 1: Null check
if (nullableValue != null)
{
    Process(nullableValue);
}

// After - Option 2: Provide default
Process(nullableValue ?? defaultValue);

// After - Option 3: Change method signature
void Process(string? value) // Accept nullable
```

#### Fix CS8618: Non-nullable property must contain non-null value
```csharp
// Before
public string Name { get; set; } // Never assigned in constructor

// After - Option 1: Initialize in constructor
public MyClass()
{
    Name = "Default";
}

// After - Option 2: Make nullable
public string? Name { get; set; }

// After - Option 3: Use null! for deferred initialization
public string Name { get; set; } = null!; // Guaranteed by Initialize()
```

#### Fix CS8625: Cannot convert null literal to non-nullable
```csharp
// Before
string name = null; // Assigning null to non-nullable

// After
string? name = null;
```

#### Fix CS8764-CS8769: Override/Implementation Mismatches
```csharp
// Before
public interface IBase
{
    string? GetValue(); // Nullable return
}

public class Derived : IBase
{
    public string GetValue() => "value"; // Mismatch! CS8766
}

// After
public class Derived : IBase
{
    public string? GetValue() => "value"; // Matches interface
}
```

### 5.4 Tools and Commands

#### Enable Warnings for Analysis
Edit `.editorconfig` to change severity from `none` to `warning`:
```ini
# Before
dotnet_diagnostic.CS8602.severity = none

# After (for testing)
dotnet_diagnostic.CS8602.severity = warning
```

#### Build and View Warnings
```powershell
# Build specific project with warnings
dotnet build src/Qwiq.Core.Rest/Qwiq.Core.Rest.csproj -c Debug -v:minimal

# Build and save warnings to file
dotnet build src/Qwiq.Core.Rest/Qwiq.Core.Rest.csproj > warnings.txt 2>&1

# Count warnings
dotnet build src/Qwiq.Core.Rest/Qwiq.Core.Rest.csproj 2>&1 | Select-String "warning CS8"
```

#### Test After Changes
```powershell
# Run tests for specific project
dotnet test test/Qwiq.Core.Tests/Qwiq.Core.Tests.csproj --no-build

# Run all tests with CI filters
dotnet test Qwiq.sln --configuration Debug --no-build `
  --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST&TestCategory!=IntegrationTests"
```

---

## 6. Migration Phases

### 6.1 Phase 0: Preparation (Week 0)

**Goals**:
- Establish baseline metrics
- Set up tracking infrastructure
- Create PRD (this document)

**Tasks**:
1. ✅ Document current state in copilot-instructions.md
2. ✅ Create CS8xxx-mitigation.md PRD
3. Enable warnings per-project and count actual warnings
4. Create GitHub issue for tracking
5. Set up branch naming convention: `refactor/nullable-{project-name}`

**Deliverables**:
- CS8xxx-mitigation.md (this document)
- Baseline warning counts per project
- GitHub tracking issue

### 6.2 Phase 1: P0 Projects (Weeks 1-2)

**Goals**: Complete Qwiq.Core.Rest (~42 warnings)

**Why P0**:
- Partially complete (~42 warnings remaining)
- High usage (REST client is primary API)
- Only depends on completed Qwiq.Core

**Tasks**:
1. Create branch `refactor/nullable-core-rest`
2. Enable CS8xxx warnings for Qwiq.Core.Rest
3. Catalog and fix all warnings
4. Verify Qwiq.Core.Tests passes
5. Create PR with title: `refactor(core.rest): complete nullable reference type annotations`

**Success Criteria**:
- ✅ Zero CS8xxx warnings in Qwiq.Core.Rest
- ✅ All tests pass
- ✅ No behavior changes
- ✅ PR approved and merged

### 6.3 Phase 2: P1 Projects (Weeks 2-3)

**Goals**: Complete Qwiq.Identity and Qwiq.Core.Soap

**Why P1**:
- Qwiq.Identity (~28 warnings) - Moderate complexity, isolated domain
- Qwiq.Core.Soap (unknown warnings) - Windows-only, legacy, isolated

**Tasks**:
1. **Qwiq.Identity**:
   - Create branch `refactor/nullable-identity`
   - Enable CS8xxx warnings
   - Fix all warnings
   - Verify Qwiq.Identity.Tests passes
   - PR: `refactor(identity): complete nullable reference type annotations`

2. **Qwiq.Core.Soap**:
   - Create branch `refactor/nullable-core-soap`
   - Count warnings (baseline unknown)
   - Enable CS8xxx warnings
   - Fix all warnings
   - Verify integration tests (if applicable)
   - PR: `refactor(core.soap): complete nullable reference type annotations`

**Success Criteria** (per project):
- ✅ Zero CS8xxx warnings
- ✅ All unit tests pass
- ✅ Integration tests pass (manual, not CI)

### 6.4 Phase 3: P2 Projects (Weeks 4-6)

**Goals**: Complete Qwiq.Linq, Qwiq.Mapper, Qwiq.Mapper.Identity

**Why P2**:
- Higher warning counts (~128 in Qwiq.Linq)
- More complex interdependencies
- Expression tree and reflection code (higher complexity)

**Sub-Phase 3a: Qwiq.Linq (Week 4)**
- Largest warning count (~128)
- Complex expression visitor pattern
- Create branch `refactor/nullable-linq`
- PR: `refactor(linq): complete nullable reference type annotations`

**Sub-Phase 3b: Qwiq.Mapper (Week 5)**
- Depends on Qwiq.Core and Qwiq.Linq
- Reflection-heavy code
- Create branch `refactor/nullable-mapper`
- PR: `refactor(mapper): complete nullable reference type annotations`

**Sub-Phase 3c: Qwiq.Mapper.Identity (Week 6)**
- Depends on Qwiq.Mapper and Qwiq.Identity
- Smallest scope in P2
- Create branch `refactor/nullable-mapper-identity`
- PR: `refactor(mapper.identity): complete nullable reference type annotations`

**Success Criteria** (per project):
- ✅ Zero CS8xxx warnings
- ✅ All unit tests pass
- ✅ Integration tests verified (manual)

### 6.5 Phase 4: Test Infrastructure (Parallel with Phases 1-3)

**Goals**: Annotate test projects and mocks

**Projects**:
- `Qwiq.Mocks` (CRITICAL - used by all tests)
- `Qwiq.Core.Tests`
- `Qwiq.Identity.Tests`
- `Qwiq.Linq.Tests`
- `Qwiq.Mapper.Tests`

**Approach**:
- Annotate Qwiq.Mocks early (enables better test development)
- Annotate test projects in parallel with source projects
- Tests often have more relaxed nullable requirements (mocks, test data)

**Success Criteria**:
- ✅ Zero CS8xxx warnings in all test projects
- ✅ Test suite still passes

### 6.6 Phase 5: Final Cleanup and Removal (Week 7)

**Goals**: Remove all CS8xxx suppressions from .editorconfig

**Tasks**:
1. Verify ALL projects have zero warnings
2. Run full solution build with CS8xxx enabled
3. Run entire test suite (including manual integration tests)
4. Remove all 16 lines from `.editorconfig`:
   ```ini
   # DELETE THESE LINES
   dotnet_diagnostic.CS8600.severity = none
   dotnet_diagnostic.CS8601.severity = none
   # ... (all 16 suppressions)
   ```
5. Update copilot-instructions.md:
   - Change all "⚠️ Needs annotation" to "✅ Fully annotated (0 nullable warnings)"
   - Remove CS86xx warnings section (no longer suppressed)
6. Create PR: `refactor: remove all CS8xxx nullable warning suppressions`

**Success Criteria**:
- ✅ Zero CS8xxx suppressions in .editorconfig
- ✅ Clean solution build (zero warnings)
- ✅ All tests pass
- ✅ Documentation updated

---

## 7. Risk Assessment and Mitigation

### 7.1 Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **Breaking API Changes** | Low | Critical | - Work annotation-only (no signature changes)<br>- Review all public API changes carefully<br>- Add breaking change CI check |
| **Behavior Changes from Fixes** | Medium | High | - No logic changes during annotation<br>- Comprehensive test coverage required<br>- Manual testing of critical paths |
| **Test Failures** | Medium | Medium | - Run tests after each file/class<br>- Use `git bisect` to isolate failures<br>- Maintain test-first discipline |
| **Merge Conflicts** | High | Low | - Small, focused PRs<br>- Merge frequently from develop<br>- Prioritize based on dependencies |
| **Performance Regression** | Low | Low | - Nullable checks are compile-time only<br>- Run benchmarks if changes touch hot paths |
| **Over-use of `null!`** | Medium | Medium | - Code review guidelines<br>- Prefer proper initialization<br>- Document all `null!` uses |

### 7.2 Process Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **Incomplete Migration** | Low | High | - Track progress in copilot-instructions.md<br>- GitHub issue with checklist<br>- Automated warning count in CI |
| **Scope Creep** | Medium | Medium | - Strict "annotation-only" rule<br>- Defer refactoring to separate PRs<br>- PR reviews enforce scope |
| **Developer Fatigue** | Low | Medium | - Break into small PRs<br>- Celebrate milestones<br>- Rotate responsibility if team-based |
| **Long-Running PRs** | Medium | Low | - Time-box each project (1 week max)<br>- Request reviews promptly<br>- Merge quickly after approval |

### 7.3 Rollback Strategy

If critical issues arise after merging:

**Immediate Rollback** (Production Issue):
1. Revert the merged PR(s)
2. Re-suppress affected CS8xxx warnings in `.editorconfig`
3. Release hotfix build
4. Post-mortem to identify root cause

**Partial Rollback** (Specific Project):
1. Re-suppress warnings for affected project only:
   ```ini
   # Temporary re-suppression for Qwiq.Linq
   [src/Qwiq.Linq/**.cs]
   dotnet_diagnostic.CS8602.severity = none
   ```
2. File GitHub issue for proper fix
3. Address in next sprint

**No Rollback** (False Positive):
1. Add targeted suppression with comment:
   ```csharp
   #pragma warning disable CS8602 // Dereference of possibly null reference
   var value = item.Value; // Known to be non-null due to validation
   #pragma warning restore CS8602
   ```
2. Document reasoning in code comments
3. Consider refactoring in future PR

---

## 8. Dependencies and Prerequisites

### 8.1 Technical Prerequisites

- ✅ Windows development environment (for SOAP projects)
- ✅ .NET 8.0 SDK (pinned in `global.json`)
- ✅ Visual Studio 2022+ or VS Code with C# extension
- ✅ Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- ✅ Qwiq.Core fully annotated (baseline complete)

### 8.2 Process Prerequisites

- ✅ copilot-instructions.md established (PR #31 feedback)
- ✅ Conventional Commits standard adopted
- ✅ Test suite running reliably
- GitHub issue tracking template
- PR review process established

### 8.3 Blocking Dependencies

**Before Starting**:
1. ✅ No blocking PRs in review queue (minimize merge conflicts)
2. Verify test suite health (all tests passing on `develop`)
3. Ensure CI pipeline is stable

**During Migration**:
- Complete projects in dependency order:
  - Qwiq.Core (complete) → Qwiq.Core.Rest → Qwiq.Linq → Qwiq.Mapper
  - Qwiq.Core (complete) → Qwiq.Identity → Qwiq.Mapper.Identity
  - Qwiq.Core (complete) → Qwiq.Core.Soap

---

## 9. Testing Requirements

### 9.1 Unit Testing

**Requirements**:
- ✅ 100% of existing unit tests must pass
- ✅ No new test failures introduced
- ✅ Mock objects properly annotated (Qwiq.Mocks project)

**Test Execution**:
```powershell
# Run unit tests with CI filters
dotnet test Qwiq.sln --configuration Debug --no-build `
  --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST&TestCategory!=IntegrationTests"
```

### 9.2 Integration Testing

**Requirements**:
- ⚠️ Manual execution required (requires TFS/Azure DevOps credentials)
- ⚠️ SOAP tests require Windows and local TFS instance
- ⚠️ REST tests require Azure DevOps access

**Test Execution** (Manual):
```powershell
# Run integration tests (manual, not CI)
dotnet test test/Qwiq.Integration.Tests/Qwiq.Integration.Tests.csproj --no-build

# Run SOAP-specific tests (Windows only)
dotnet test --filter "TestCategory=SOAP"
```

**When Required**:
- After Qwiq.Core.Rest changes (REST integration tests)
- After Qwiq.Core.Soap changes (SOAP integration tests)
- Before final Phase 5 cleanup

### 9.3 Regression Testing

**Smoke Test Scenarios**:
1. **Query Execution**: Execute basic WIQL query via REST client
2. **LINQ Translation**: Translate simple LINQ expression to WIQL
3. **Mapper**: Map IWorkItem to strongly-typed POCO
4. **Identity Resolution**: Resolve identity fields in bulk

**Validation**:
- No null reference exceptions
- Correct query results
- Proper error handling for invalid inputs

### 9.4 Test Automation

**CI Build Requirements**:
- ✅ Build succeeds with zero warnings
- ✅ All unit tests pass
- ✅ No CS8xxx warnings in build output

**Optional Enhancements**:
- Add warning count check to CI pipeline
- Fail build if CS8xxx warnings detected (after Phase 5)
- Automated null-safety regression testing

---

## 10. Documentation Requirements

### 10.1 Code Documentation

**Update Per File**:
- XML documentation comments remain unchanged (CS1591 still suppressed)
- Add inline comments for non-obvious null handling:
  ```csharp
  // Revision.Id returns null when WorkItem reference is unavailable (dual-constructor pattern)
  public int? Id => WorkItem?.Id;
  ```

### 10.2 Repository Documentation

**Update `.github/copilot-instructions.md`**:

Current section to update:
```markdown
### Nullable Reference Types Status

- **Qwiq.Core**: ✅ Fully annotated (0 nullable warnings)
- **Qwiq.Core.Rest**: ⚠️ Partially annotated (~42 warnings remaining)
- **Qwiq.Core.Soap**: ⚠️ Needs annotation
- **Qwiq.Linq**: ⚠️ Needs annotation (~128 warnings)
- **Qwiq.Identity**: ⚠️ Needs annotation (~28 warnings)
- **Qwiq.Mapper**: ⚠️ Needs annotation
```

**After Phase 5**:
```markdown
### Nullable Reference Types

All projects fully annotated with nullable reference types (0 warnings repository-wide).

Common patterns:
- Use `T?` for properties that can legitimately return null
- Use `null!` for lazy-initialized fields guaranteed to be set before use
- Update both interfaces AND implementations when changing nullability
- Use `[MaybeNullWhen(false)]` for Try* out parameters
```

**Add Section on Nullable Warnings**:
```markdown
### Handling CS8xxx Warnings

Nullable reference type warnings (CS8xxx) are ENABLED and enforced. Do not suppress these warnings without justification:

✅ **Acceptable**:
- Targeted suppressions with explanatory comments
- Known false positives (document in code)

❌ **Not Acceptable**:
- Blanket `#pragma warning disable CS8xxx` without comments
- Using `null!` operator to silence warnings
- Re-enabling suppressions in `.editorconfig`
```

### 10.3 PR Documentation

**PR Template Requirements**:
```markdown
## CS8xxx Nullable Reference Type Migration

**Project**: [Project Name]
**Warnings Fixed**: [Count]
**Breaking Changes**: None (annotation-only)

### Changes
- [ ] All interfaces annotated
- [ ] All implementations annotated
- [ ] Tests updated (if needed)
- [ ] Zero CS8xxx warnings in build output
- [ ] All tests passing

### Verification
- [ ] `dotnet build src/[Project]/[Project].csproj` - Zero warnings
- [ ] `dotnet test test/[Project].Tests/[Project].Tests.csproj` - All tests pass
```

---

## 11. Success Criteria

### 11.1 Definition of Done (Per Project)

A project is considered "complete" when:

1. ✅ **Zero CS8xxx Warnings**: Build produces no CS8xxx warnings
2. ✅ **All Tests Pass**: 100% test pass rate maintained
3. ✅ **Interface/Implementation Consistency**: All nullability annotations match between interfaces and implementations
4. ✅ **No Breaking Changes**: Public API signatures unchanged (nullability additions are compatible)
5. ✅ **Code Review Approved**: At least one approving review
6. ✅ **Documentation Updated**: copilot-instructions.md status updated
7. ✅ **Merged to develop**: PR merged without conflicts

### 11.2 Definition of Done (Overall Migration)

The entire migration is complete when:

1. ✅ **All 7 Projects Complete**: All source projects fully annotated
2. ✅ **All Test Projects Complete**: All test projects fully annotated
3. ✅ **Zero Suppressions**: All 16 CS8xxx suppressions removed from `.editorconfig`
4. ✅ **Clean Solution Build**: `dotnet build Qwiq.sln` produces zero warnings
5. ✅ **All Tests Pass**: Full test suite passes (unit + manual integration)
6. ✅ **Documentation Complete**: copilot-instructions.md fully updated
7. ✅ **GitHub Issue Closed**: Tracking issue marked as complete

### 11.3 Acceptance Criteria

**Functional**:
- ✅ No runtime null reference exceptions introduced
- ✅ All LINQ queries translate correctly
- ✅ All mappers function as before
- ✅ All REST/SOAP clients work correctly

**Non-Functional**:
- ✅ Build time unchanged (±10%)
- ✅ No performance regression in benchmarks
- ✅ Code coverage maintained or improved

**Process**:
- ✅ All commits follow Conventional Commits
- ✅ All PRs small and focused (<500 lines changed per PR)
- ✅ Average PR review time <2 days

---

## 12. Timeline and Milestones

### 12.1 Gantt Chart (7 Weeks)

```
Week | Phase               | Projects                        | Status
-----|---------------------|---------------------------------|--------
  0  | Preparation         | PRD, Baseline, GitHub Issue     | 🔵 Current
  1  | P0 - Core.Rest      | Qwiq.Core.Rest (~42 warnings)   | ⬜️
  2  | P0 - Core.Rest      | Qwiq.Core.Rest (complete)       | ⬜️
  3  | P1 - Identity       | Qwiq.Identity (~28 warnings)    | ⬜️
     | P1 - Core.Soap      | Qwiq.Core.Soap (TBD warnings)   | ⬜️
  4  | P2 - Linq           | Qwiq.Linq (~128 warnings)       | ⬜️
  5  | P2 - Mapper         | Qwiq.Mapper (TBD warnings)      | ⬜️
  6  | P2 - Mapper.Id      | Qwiq.Mapper.Identity (TBD)      | ⬜️
  7  | Final Cleanup       | Remove suppressions, docs       | ⬜️
```

### 12.2 Milestones

| Milestone | Target Date | Criteria | Status |
|-----------|-------------|----------|--------|
| **M0: Baseline Established** | Week 0 | PRD complete, warning counts known | 🔵 In Progress |
| **M1: P0 Complete** | End of Week 2 | Qwiq.Core.Rest: 0 warnings | ⬜️ Planned |
| **M2: P1 Complete** | End of Week 3 | Qwiq.Identity + Qwiq.Core.Soap: 0 warnings | ⬜️ Planned |
| **M3: P2 Complete** | End of Week 6 | Qwiq.Linq + Qwiq.Mapper + Qwiq.Mapper.Identity: 0 warnings | ⬜️ Planned |
| **M4: Migration Complete** | End of Week 7 | All suppressions removed, docs updated | ⬜️ Planned |

### 12.3 Buffer Time

- **Week 8 (Buffer)**: Reserved for unexpected issues, rollbacks, or additional testing
- **Risk Contingency**: +2 weeks if critical blocker discovered

---

## 13. Resource Requirements

### 13.1 Development Resources

**Single Developer** (Recommended):
- **Time Commitment**: 25-30 hours over 7 weeks (~4 hours/week)
- **Skills Required**:
  - C# nullable reference types expertise
  - LINQ expression tree knowledge (for Qwiq.Linq)
  - Familiarity with TFS/Azure DevOps APIs
  - Git workflow proficiency

**Multiple Developers** (Optional):
- Can parallelize P1 and P2 work
- Requires clear ownership per project
- Higher merge conflict risk

### 13.2 Review Resources

**Code Reviewers**:
- 1-2 reviewers per PR
- ~30 minutes per review
- Nullability expertise helpful but not required

### 13.3 Testing Resources

**Manual Testing**:
- Access to Azure DevOps organization (for REST integration tests)
- Access to TFS server (for SOAP integration tests) - optional
- Windows machine for SOAP client verification

---

## 14. Communication Plan

### 14.1 Stakeholder Updates

**Weekly Status Updates** (if team-based):
- Progress against timeline
- Warnings fixed this week
- Blockers or risks identified
- Next week's plan

**Format**:
```markdown
## CS8xxx Migration - Week N Status

**Completed**: [Project] - X warnings fixed
**In Progress**: [Project] - Y warnings remaining
**Blockers**: [None/List blockers]
**Next Week**: [Project] - Z warnings estimated
```

### 14.2 Documentation Updates

**After Each Project**:
- Update copilot-instructions.md status table
- Update GitHub tracking issue checklist
- Comment on PR with verification results

**After Phase 5**:
- GitHub release notes (if applicable)
- CHANGELOG.md entry
- README.md update (if applicable)

---

## 15. Open Questions and Assumptions

### 15.1 Open Questions

1. **Qwiq.Core.Soap Warning Count**: Need to enable warnings and count (estimated TBD)
2. **Qwiq.Mapper Warning Count**: Need baseline measurement
3. **Performance Impact**: Should benchmarks be run for LINQ and Mapper projects?
4. **Breaking Change Policy**: Are nullability changes considered breaking for this library?
5. **CI Enforcement**: Should CI fail on CS8xxx warnings after Phase 5?

### 15.2 Assumptions

1. ✅ No major feature work will conflict during 7-week migration
2. ✅ Test suite remains stable and reliable
3. ✅ Qwiq.Core patterns are proven and should be replicated
4. ✅ `TreatWarningsAsErrors` is enabled (already configured in Directory.Build.props)
5. ✅ One developer can complete migration in 7 weeks
6. ✅ Integration tests can be run manually (not blocking CI)

### 15.3 Decisions Needed

1. **Approval**: Does this PRD require formal approval before starting?
2. **Branch Strategy**: Use single long-running branch or per-project branches? ✅ **Recommended**: Per-project branches
3. **Review Process**: Self-merge allowed or required reviewer? ✅ **Recommended**: Required reviewer
4. **Hotfix Policy**: Can hotfixes bypass nullable annotation? ✅ **Recommended**: Yes, but create follow-up issue

---

## 16. Appendices

### 16.1 Appendix A: Nullable Attributes Reference

| Attribute | Usage | Example |
|-----------|-------|---------|
| `[MaybeNullWhen(false)]` | Try* pattern out parameters | `bool TryGet([MaybeNullWhen(false)] out T value)` |
| `[NotNullWhen(true)]` | Null-check helpers | `bool IsNotNull([NotNullWhen(true)] object? obj)` |
| `[NotNullIfNotNull(nameof(param))]` | Return nullability matches parameter | `[return: NotNullIfNotNull(nameof(input))] string? Transform(string? input)` |
| `[AllowNull]` | Accept null for non-nullable property | `[AllowNull] string Value { get; set; }` |
| `[DisallowNull]` | Disallow null for nullable property | `[DisallowNull] string? Value { get; set; }` |
| `[MaybeNull]` | Return may be null despite non-nullable type | `[return: MaybeNull] T GetValue()` |
| `[NotNull]` | Return never null despite nullable type | `[return: NotNull] string? GetValue()` |

### 16.2 Appendix B: Compatibility Shims

**NullableAttributes.cs** (`src/Qwiq.Core/Compatibility/`):
- Provides nullable attribute definitions for `net472` and `netstandard2.0`
- Conditionally compiled: `#if NETFRAMEWORK || NETSTANDARD2_0`
- **Do NOT use the `Polyfill` NuGet package** - conflicts with Microsoft.VisualStudio.Services.Client

### 16.3 Appendix C: Git Workflow

**Branch Naming**:
```
refactor/nullable-core-rest
refactor/nullable-identity
refactor/nullable-core-soap
refactor/nullable-linq
refactor/nullable-mapper
refactor/nullable-mapper-identity
refactor/nullable-test-infrastructure
refactor/nullable-cleanup
```

**Commit Message Format**:
```
refactor(core.rest): annotate WorkItemStore with nullable reference types

- Mark IWorkItemStore.Query return type as nullable
- Add null checks to constructor parameters
- Update implementations to match interface nullability

Fixes CS8602, CS8604, CS8618 (15 warnings)
```

**PR Title Format**:
```
refactor(core.rest): complete nullable reference type annotations
```

### 16.4 Appendix D: Related PRs

**Previous Migration Work**:
- PR #31: SDK-style project conversion
- PR #32: Central Package Management
- PR #43-47: Cleanup and modernization

**This Initiative**:
- PRs TBD (one per project in Phases 1-4)
- Final cleanup PR (Phase 5)

---

## 17. Glossary

| Term | Definition |
|------|------------|
| **CS8xxx** | C# compiler warning codes for nullable reference type violations |
| **NRT** | Nullable Reference Types - C# 8.0+ feature for compile-time null safety |
| **Annotation** | Adding `?` suffix to reference types that can be null |
| **Nullability Context** | Scope where nullable reference types are enabled (`<Nullable>enable</Nullable>`) |
| **Null-forgiving Operator** | `!` operator (e.g., `value!`) that suppresses nullable warnings |
| **Null-conditional Operator** | `?.` operator for safe member access on nullable references |
| **Null-coalescing Operator** | `??` operator for providing default values for null |
| **WIQL** | Work Item Query Language (Azure DevOps/TFS query syntax) |
| **SOAP** | Simple Object Access Protocol (legacy TFS client API) |
| **REST** | Representational State Transfer (modern Azure DevOps API) |

---

## Document Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-04 | AI Assistant | Initial PRD creation |

---

## Approval Signatures

| Role | Name | Signature | Date |
|------|------|-----------|------|
| **Repository Owner** | rjmurillo | _Pending_ | _Pending_ |
| **Technical Lead** | rjmurillo | _Pending_ | _Pending_ |

---

**End of Document**
