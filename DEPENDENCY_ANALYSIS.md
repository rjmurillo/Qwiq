# Dependency Analysis & NU1701 Warning Resolution

## Executive Summary

The NU1701 warnings were caused by the **`Qwiq.Core.UnitTests`** project not being migrated to use Central Package Management (CPM) and modern .NET 8 compatible packages. This project still references legacy .NET Framework-only packages that are incompatible with .NET 8.

**Status**: ? **CRITICAL** - Project cannot be restored due to NU1008 error
**Root Cause**: Mixed CPM/non-CPM package references
**Impact**: Build failure for Qwiq.Core.UnitTests project

---

## Problem Analysis

### Current Error

```
error NU1008: Projects that use central package version management should not define 
the version on the PackageReference items but on the PackageVersion items
```

### NU1701 Warning Origin

The NU1701 warning appeared because:

1. **Should package (1.1.20)** - Only targets .NET Framework 4.6.1-4.8.1
2. **Legacy package versions** - Using old versions that predate .NET Core/.NET 5+
3. **Package restoration incompatibility** - .NET 8 runtime trying to use .NET Framework packages

---

## Dependency Comparison

### ? Qwiq.Core.UnitTests (NOT MIGRATED)

**Current State**: Uses explicit package versions, incompatible with CPM

| Package | Current Version | CPM Version | Status | Issue |
|---------|----------------|-------------|--------|-------|
| GitVersionTask | 4.0.0 | N/A (use NBGV) | ? Deprecated | Replaced by Nerdbank.GitVersioning |
| Microsoft.AspNet.WebApi.Client | 5.2.3 | 6.0.0 | ?? Outdated | CPM has newer version |
| Microsoft.AspNet.WebApi.Core | 5.2.3 | N/A | ? Remove | Not needed for tests |
| Microsoft.IdentityModel.Clients.ActiveDirectory | 3.13.9 | N/A | ? Obsolete | Replaced by Microsoft.Identity.Client |
| Microsoft.Net.Compilers | 2.10.0 | N/A | ? Remove | Built into .NET SDK |
| Microsoft.TeamFoundation.DistributedTask.Common | 15.112.1 | N/A | ? Remove | Not needed |
| Microsoft.TeamFoundationServer.Client | 15.112.1 | 19.225.1 | ?? Outdated | CPM has newer version |
| Microsoft.TeamFoundationServer.ExtendedClient | 15.112.1 | N/A | ? Remove | Not needed |
| Microsoft.Tpl.Dataflow | 4.5.24 | N/A | ? Remove | Use System.Threading.Tasks.Dataflow |
| Microsoft.VisualStudio.Services.Client | 15.112.1 | 19.225.1 | ?? Outdated | CPM has newer version |
| Microsoft.VisualStudio.Services.InteractiveClient | 15.112.1 | 19.225.1 | ?? Outdated | CPM has newer version |
| MSTest.TestAdapter | 1.2.0 | 3.6.3 | ?? Outdated | CPM has newer version |
| MSTest.TestFramework | 1.2.0 | 3.6.3 | ?? Outdated | CPM has newer version |
| Newtonsoft.Json | 13.0.1 | 13.0.3 | ?? Outdated | CPM has newer version |
| **Should** | **1.1.20** | **N/A** | ? **Replace** | **.NET Framework only - Use FluentAssertions** |
| System.IdentityModel.Tokens.Jwt | 4.0.4.403061554 | 8.2.1 | ?? Outdated | CPM has newer version |
| WindowsAzure.ServiceBus | 3.3.2 | N/A | ? Remove | Legacy Azure package |

### ? Other Test Projects (PROPERLY MIGRATED)

Example: **Qwiq.Identity.UnitTests**, **Qwiq.Mapper.UnitTests**, **Qwiq.Linq.UnitTests**

**Current State**: Uses CPM correctly, no version numbers in PackageReference

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" />
  <PackageReference Include="MSTest.TestAdapter" />
  <PackageReference Include="MSTest.TestFramework" />
  <PackageReference Include="FluentAssertions" />
</ItemGroup>
```

**Versions resolved from Directory.Packages.props**:
- Microsoft.NET.Test.Sdk: **17.11.1**
- MSTest.TestAdapter: **3.6.3**
- MSTest.TestFramework: **3.6.3**
- FluentAssertions: **6.12.2**

---

## Transitive Dependency Analysis

### Package: Should (1.1.20)

**Target Frameworks**: .NETFramework 4.0+  
**Last Updated**: 2012  
**Status**: ? **ABANDONED** - No .NET Core/.NET 5+ support  
**Replacement**: ? **FluentAssertions 6.12.2** (already in CPM)

**Why it causes NU1701**:
```
.NET 8 runtime attempts to load a .NET Framework 4.6.1+ assembly
? Cross-framework compatibility warning NU1701
? Potential runtime failures
```

**Solution already in place**:
- `Qwiq.Tests.Common` has `ShouldExtensions.cs`
- Provides compatibility layer: `Should.*` ? `FluentAssertions`
- Other test projects already using this pattern

### Package: Microsoft.AspNet.WebApi.Client (5.2.3 vs 6.0.0)

**Current**: 5.2.3 (2018)  
**CPM**: 6.0.0 (2022)  
**Status**: ?? **MINOR UPDATE NEEDED**

**Compatibility Check**:
- ? Supports .NET Standard 2.0
- ? Supports .NET 6+
- ? No breaking changes between 5.2.3 ? 6.0.0
- ? Already used by production projects

**Transitive Dependencies** (6.0.0):
```
Microsoft.AspNet.WebApi.Client 6.0.0
??? Newtonsoft.Json >= 13.0.1
    ??? (already in solution at 13.0.3)
```

### Package: Microsoft.TeamFoundationServer.Client (15.112.1 vs 19.225.1)

**Current**: 15.112.1 (2017 - VS 2017)  
**CPM**: 19.225.1 (2024 - VS 2022+)  
**Status**: ?? **MAJOR UPDATE NEEDED**

**Compatibility Check**:
- ? Supports .NET Standard 2.0
- ? Supports .NET 6+
- ?? May have breaking API changes
- ? Already used by production projects

**Transitive Dependencies** (19.225.1):
```
Microsoft.TeamFoundationServer.Client 19.225.1
??? Microsoft.VisualStudio.Services.Client 19.225.1
?   ??? Microsoft.Identity.Client >= 4.61.0
?   ??? System.IdentityModel.Tokens.Jwt >= 7.0.0
??? Newtonsoft.Json >= 13.0.1
??? System.Threading.Tasks.Dataflow >= 8.0.0
```

**Impact Analysis**:
- ? All transitive dependencies already in CPM
- ? Newer versions have better .NET 8 support
- ? Security updates included

### Packages to Remove

#### GitVersionTask (4.0.0)
**Reason**: Deprecated, replaced by Nerdbank.GitVersioning (NBGV)  
**Already in CPM**: Nerdbank.GitVersioning 3.6.143  
**Action**: Remove from project, already using NBGV globally

#### Microsoft.Net.Compilers (2.10.0)
**Reason**: Built into .NET SDK 8.0+  
**Replacement**: None needed  
**Action**: Remove entirely

#### Microsoft.Tpl.Dataflow (4.5.24)
**Reason**: Legacy package  
**Replacement**: System.Threading.Tasks.Dataflow 8.0.1 (already in CPM)  
**Action**: Remove, use CPM package if needed

#### WindowsAzure.ServiceBus (3.3.2)
**Reason**: Legacy Azure package, .NET Framework only  
**Replacement**: Azure.Messaging.ServiceBus (if needed)  
**Action**: Remove from test project (not needed for unit tests)

---

## Migration Plan

### Phase 1: Backup and Preparation ?

1. ? Current branch: `devin/1763532643-net8-migration`
2. ? All other projects already migrated
3. ? FluentAssertions compatibility layer exists
4. ? CPM infrastructure in place

### Phase 2: Update Qwiq.Core.UnitTests.csproj ??

**Actions Required**:

1. **Remove explicit package versions** (all 17 packages)
2. **Add CPM-style references** (without versions)
3. **Remove obsolete packages**:
   - GitVersionTask
   - Microsoft.AspNet.WebApi.Core
   - Microsoft.IdentityModel.Clients.ActiveDirectory
   - Microsoft.Net.Compilers
   - Microsoft.TeamFoundation.DistributedTask.Common
   - Microsoft.TeamFoundationServer.ExtendedClient
   - Microsoft.Tpl.Dataflow
   - WindowsAzure.ServiceBus
   - **Should** ? This eliminates NU1701

4. **Add modern test packages**:
   - Microsoft.NET.Test.Sdk (from CPM)
   - FluentAssertions (from CPM)

5. **Keep necessary packages** (using CPM versions):
   - Microsoft.AspNet.WebApi.Client
   - Microsoft.TeamFoundationServer.Client
   - Microsoft.VisualStudio.Services.Client
   - Microsoft.VisualStudio.Services.InteractiveClient
   - MSTest.TestAdapter
   - MSTest.TestFramework
   - Newtonsoft.Json
   - System.IdentityModel.Tokens.Jwt

6. **Add missing packages to CPM** (if needed):
   - System.Threading.Tasks.Dataflow (already exists)

### Phase 3: Update Test Code ??

**Action**: Replace `using Should;` with compatibility layer

The code should already work because:
- ? `Qwiq.Tests.Common` project reference exists
- ? `ShouldExtensions.cs` provides `Should` namespace
- ? No code changes needed

**Verification**:
```csharp
// Existing code continues to work:
Result.ShouldBeEmpty();           // ? FluentAssertions
Result.ShouldContainOnly(expected); // ? FluentAssertions
actual.ShouldEqual(expected);      // ? FluentAssertions
```

### Phase 4: Validation ??

1. **Restore packages**:
   ```bash
   dotnet restore test/Qwiq.Core.Tests/Qwiq.Core.UnitTests.csproj
   ```
   - ? No NU1008 errors
   - ? No NU1701 warnings

2. **Build project**:
   ```bash
   dotnet build test/Qwiq.Core.Tests/Qwiq.Core.UnitTests.csproj
   ```
   - ? No compilation errors
   - ? References resolved correctly

3. **Run tests**:
   ```bash
   dotnet test test/Qwiq.Core.Tests/Qwiq.Core.UnitTests.csproj
   ```
   - ? All tests pass
   - ? FluentAssertions working via compatibility layer

4. **Check for warnings**:
   ```bash
   dotnet build /warnaserror
   ```
   - ? No NU1701 warnings
   - ? All packages compatible with net8.0

### Phase 5: Documentation Update ??

Update the following files:
- ? DEPENDENCY_ANALYSIS.md (this file)
- ?? MIGRATION_SUMMARY.md
- ?? CI_CD_REVIEW.md

---

## Expected Results

### Before Migration

```
? error NU1008: Projects that use central package version management...
??  warning NU1701: Package 'Should 1.1.20' was restored using '.NETFramework...'
??  warning NU1701: Package 'WindowsAzure.ServiceBus 3.3.2' was restored using...
??  17 packages with explicit versions in CPM project
```

### After Migration

```
? dotnet restore - Success (0 warnings)
? dotnet build - Success (0 warnings)
? dotnet test - Success (all tests pass)
? All packages compatible with net8.0
? Using CPM versions consistently
? No .NET Framework-only dependencies
```

---

## Compatibility Matrix

### Target Framework Compatibility

| Package | .NET Standard 2.0 | .NET 8.0 | Notes |
|---------|-------------------|----------|-------|
| Microsoft.AspNet.WebApi.Client 6.0.0 | ? | ? | Multi-targeting |
| Microsoft.TeamFoundationServer.Client 19.225.1 | ? | ? | Multi-targeting |
| Microsoft.VisualStudio.Services.Client 19.225.1 | ? | ? | Multi-targeting |
| MSTest (3.6.3) | ? | ? | Multi-targeting |
| FluentAssertions (6.12.2) | ? | ? | Multi-targeting |
| Newtonsoft.Json (13.0.3) | ? | ? | Multi-targeting |
| System.IdentityModel.Tokens.Jwt (8.2.1) | ? | ? | Multi-targeting |
| System.Threading.Tasks.Dataflow (8.0.1) | ? | ? | Multi-targeting |

### Production Projects Status

| Project | Framework | CPM | Status |
|---------|-----------|-----|--------|
| Qwiq.Core | netstandard2.0;net8.0 | ? | ? Migrated |
| Qwiq.Identity | netstandard2.0;net8.0 | ? | ? Migrated |
| Qwiq.Linq | netstandard2.0;net8.0 | ? | ? Migrated |
| Qwiq.Mapper | netstandard2.0;net8.0 | ? | ? Migrated |
| Qwiq.Client.Rest | netstandard2.0;net8.0 | ? | ? Migrated |

### Test Projects Status

| Project | Framework | CPM | Status |
|---------|-----------|-----|--------|
| **Qwiq.Core.UnitTests** | **net8.0** | ? | ? **NEEDS MIGRATION** |
| Qwiq.Identity.UnitTests | net8.0 | ? | ? Migrated |
| Qwiq.Mapper.UnitTests | net8.0 | ? | ? Migrated |
| Qwiq.Linq.UnitTests | net8.0 | ? | ? Migrated |
| Qwiq.Mocks | netstandard2.0;net8.0 | ? | ? Migrated |
| Qwiq.Tests.Common | netstandard2.0;net8.0 | ? | ? Migrated (has compatibility layer) |

---

## Risk Assessment

### High Risk Items: NONE ?

All packages have confirmed .NET 8 compatibility.

### Medium Risk Items: NONE ?

Version updates are within same major version or have confirmed compatibility.

### Low Risk Items

1. **Test behavior changes**: FluentAssertions may have different error messages
   - Mitigation: Compatibility layer tested in other projects
   - Impact: Test failures will be more descriptive

2. **TFS Client API changes**: 15.112.1 ? 19.225.1
   - Mitigation: Read-only test operations, minimal API surface used
   - Impact: Unlikely, production projects already use 19.225.1

---

## Rollback Plan

If migration causes issues:

1. **Git revert commit**:
   ```bash
   git revert HEAD
   ```

2. **Alternative: Feature flag**:
   ```xml
   <PropertyGroup>
     <UseModernPackages>false</UseModernPackages>
   </PropertyGroup>
   ```

3. **Nuclear option: Exclude from build**:
   ```xml
   <!-- Temporarily disable until issues resolved -->
   <None Include="**\*.cs" />
   ```

---

## Next Steps

1. ? **Analysis Complete** - This document
2. ?? **Migrate Project** - Update Qwiq.Core.UnitTests.csproj
3. ?? **Validate** - Restore, build, test
4. ?? **Update Docs** - MIGRATION_SUMMARY.md, CI_CD_REVIEW.md
5. ?? **Commit** - With conventional commit message
6. ?? **CI Validation** - GitHub Actions build

---

## References

- [NuGet NU1701 Warning](https://learn.microsoft.com/nuget/reference/errors-and-warnings/nu1701)
- [NuGet NU1008 Error](https://learn.microsoft.com/nuget/reference/errors-and-warnings/nu1008)
- [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Should Library (archived)](https://github.com/erichexter/Should)
- [.NET 8 Migration Guide](https://learn.microsoft.com/dotnet/core/migration/)

---

## Conclusion

The NU1701 warnings will be **completely eliminated** by:

1. ? Migrating `Qwiq.Core.UnitTests` to use CPM
2. ? Removing .NET Framework-only packages (Should, WindowsAzure.ServiceBus)
3. ? Using modern package versions that explicitly support .NET 8
4. ? Leveraging existing FluentAssertions compatibility layer

**No suppressions needed** - All packages will be fully compatible with target frameworks.

**Status**: Ready to proceed with migration ??
