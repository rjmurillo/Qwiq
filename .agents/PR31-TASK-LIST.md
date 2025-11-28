# PR #31 Review Feedback Task List

This document tracks the implementation of review feedback from PR #31.

## Status Legend
- ✅ Complete
- ⏳ In Progress  
- ❌ Not Started
- 🔄 Skipped/Deferred

---

## Phase 1: Critical Bug Fixes (Priority 1)
All items address potential NullReferenceException or race condition issues.

| # | Status | File | Issue | Fix Applied |
|---|--------|------|-------|-------------|
| 1.1 | ✅ | `IdentityDescriptor.cs` (REST) | Null descriptor access | Add null guard before accessing properties |
| 1.2 | ✅ | `FieldDefinition.cs` (SOAP) | Null check in base call | Add null guard in constructor base call |
| 1.3 | ✅ | `QueryDefinition.cs` (REST) | Null constructor parameter | Add null guard to constructor |
| 1.4 | ✅ | `QueryDefinition.cs` (SOAP) | Null constructor parameter | Add null guard to constructor |
| 1.5 | ✅ | `WorkItemLinkTypeEnd.cs` | ImmutableName not set | Fix constructor chaining to pass immutableName |
| 1.6 | ✅ | `WorkItem.cs` | Null Lazy type | Add null check for `_type?.Value` |
| 1.7 | ✅ | `IdentityTypeMapper.cs` | Race condition | Add locking mechanism for thread safety |
| 1.8 | ✅ | `GenericComparer.cs` | Hash code inconsistency | Implement content-based hash for IEnumerable |
| 1.9 | ✅ | `IWorkItem.Extensions.cs` | Wrong exception type | Use ArgumentException for empty collection |

**Commit:** `fix(core): add null guards and fix race condition in various components`

---

## Phase 2: AssemblyInfo Migration (Priority 1)
Migrate from legacy AssemblyInfo.cs to SDK-generated attributes.

| # | Status | Task | Details |
|---|--------|------|---------|
| 2.1 | ✅ | Delete `AssemblyInfo.Common.cs` | Removed shared assembly info |
| 2.2 | ✅ | Delete project `Properties/AssemblyInfo.cs` files | 18 files deleted |
| 2.3 | ✅ | Update `Directory.Build.props` | Enable SDK-generated attributes |
| 2.4 | ✅ | Add InternalsVisibleTo to csproj files | Core, REST, SOAP projects |
| 2.5 | ✅ | Remove csproj references to AssemblyInfo.Common.cs | All projects updated |

**Commit:** `refactor: migrate from legacy AssemblyInfo to SDK-generated attributes`

---

## Phase 3: Major Priority Fixes (Priority 2)
Code quality improvements flagged by reviewers.

| # | Status | File | Issue | Suggested Fix |
|---|--------|------|-------|---------------|
| 3.1 | ❌ | Multiple REST files | TargetTypeMapper → TypeMapper | Rename for clarity |
| 3.2 | ❌ | `WorkItemStore.cs` | Simplify proxy creation | Use pattern match in LINQ |
| 3.3 | ❌ | `IdentityTypeMapper.cs` | Verify visibility | Ensure internal visibility is correct |

---

## Phase 4: Minor Priority Fixes (Priority 3)
Style and minor improvements.

| # | Status | File | Issue | Suggested Fix |
|---|--------|------|-------|---------------|
| 4.1 | ❌ | `WorkItemStore.cs` (REST) | Extract config method | Create helper for WorkItemLinkType config |
| 4.2 | ❌ | `.github/copilot-instructions.md` | Review and update | Ensure instructions reflect modernized repo |

---

## Phase 5: Trivial Improvements (Priority 4)
Nice-to-have improvements.

| # | Status | File | Issue | Suggested Fix |
|---|--------|------|-------|---------------|
| 5.1 | ❌ | `WorkItemLinkInfo.cs` | Use named tuple | Add descriptive names |
| 5.2 | ❌ | Test timeouts | Consider adjusting | Review 1000ms timeouts |

---

## Phase 6: Build Verification
Ensure all changes compile correctly.

| # | Status | Task | Notes |
|---|--------|------|-------|
| 6.1 | ✅ | Build Qwiq.Core | Passes |
| 6.2 | ✅ | Build Qwiq.Client.Rest | Passes |
| 6.3 | ✅ | Build Qwiq.Client.Soap | Passes |
| 6.4 | ✅ | Build Qwiq.Mocks | Passes |
| 6.5 | ✅ | Build Qwiq.Core.UnitTests | Passes |
| 6.6 | ⚠️ | Full solution build | Pre-existing test errors in Identity tests |

**Note:** There are pre-existing compile errors in:
- `test/Qwiq.Integration.Tests/Identity/IdentityMapperTests.cs`
- `test/Qwiq.Identity.Tests/BulkIdentityAwareAttributeMapperStrategyTests.cs`

These errors existed before the PR #31 changes and are not caused by this work.

---

## Summary

**Completed:** 
- 9 critical bug fixes (Phase 1)
- AssemblyInfo migration (Phase 2)
- Build verification for core projects (Phase 6)

**Remaining:**
- Phase 3-5 improvements (lower priority)
- Fix pre-existing test errors (separate issue)

**Commits Made:**
1. `fix(core): add null guards and fix race condition in various components`
2. `refactor: migrate from legacy AssemblyInfo to SDK-generated attributes`
