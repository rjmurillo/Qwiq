# Feature Request: Support .snupkg Symbol Package Verification

**Target Repository**: [MattKotsenas/Verify.Nupkg](https://github.com/MattKotsenas/Verify.Nupkg/issues/38)  
**Status**: Pending upstream implementation  
**Created**: 2025-12-05  
**Priority**: Medium

---

## Problem Statement

The Verify.Nupkg plugin currently only supports `.nupkg` (NuGet package) file verification via the `VerifyFile()` API. It does not recognize or process `.snupkg` (symbol package) files, which have a different structure and purpose but share similar verification needs.

When attempting to baseline `.snupkg` files with `VerifyFile()`, the plugin fails to recognize the file extension and cannot extract/verify the package contents.

## Use Case: Qwiq Repository

The [Qwiq repository](https://github.com/rjmurillo/Qwiq) generates both `.nupkg` and `.snupkg` packages for all 10 library projects. Our package test suite validates package structure and manifest consistency using snapshot testing.

**Current Workaround:**
- Baseline only `.nupkg` files using Verify.Nupkg
- Skip `.snupkg` verification with logged message referencing this issue
- Rely on CI-level `dotnet sourcelink test` for symbol package validation (limited scope)

**Code Reference (PackageTests.cs):**
```csharp
private static TheoryData<string> GetPackages()
{
    var theoryData = new TheoryData<string>();
    var packageFiles = Directory.GetFiles(
        Path.Combine(RepoRootHelper.RepoRoot, "src"),
        "*.nupkg",  // Only .nupkg, not *.snupkg
        SearchOption.AllDirectories
    )
    .Where(file => file.Contains(Path.Combine("bin", "Release")))
    .Select(file => new FileInfo(file))
    .GroupBy(GetPackageDiscriminator, StringComparer.OrdinalIgnoreCase)
    .Select(group => group.OrderByDescending(fileInfo => fileInfo.LastWriteTimeUtc).First())
    .Select(fileInfo => fileInfo.FullName)
    .ToList();

    // Log skipped symbol packages
    var symbolPackageCount = Directory.GetFiles(
        Path.Combine(RepoRootHelper.RepoRoot, "src"),
        "*.snupkg",
        SearchOption.AllDirectories
    )
    .Count(file => file.Contains(Path.Combine("bin", "Release")));

    if (symbolPackageCount > 0)
    {
        Trace.TraceInformation(
            $"Skipping baseline verification for {symbolPackageCount} symbol packages " +
            "pending Verify.Nupkg support. See https://github.com/MattKotsenas/Verify.Nupkg/issues/38."
        );
    }

    foreach (var package in packageFiles)
    {
        theoryData.Add(package);
    }

    return theoryData;
}
```

## Desired Behavior

Extend Verify.Nupkg to support `.snupkg` files with the same verification capabilities as `.nupkg`:

1. **Extension Recognition**: Accept `*.snupkg` file paths in `VerifyFile()` API
2. **Content Extraction**: Extract ZIP contents from symbol package
3. **Manifest Parsing**: Parse embedded `.nuspec` manifest (if present)
4. **ASCII Tree Generation**: Generate file tree structure for snapshot
5. **Scrubbing Support**: Apply existing `ScrubNuspec()` logic to symbol packages

**Expected API Usage:**
```csharp
[Theory]
[MemberData(nameof(GetPackages))]
public Task Baseline(string packagePath)
{
    var package = new FileInfo(packagePath);
    var settings = new VerifySettings();
    settings.UseTextForParameters(GetPackageDiscriminator(package.Name));
    
    // Should work for both .nupkg AND .snupkg
    return VerifyFile(package, settings).ScrubNuspec();
}
```

## Proposed Solution

### Option 1: Extend Existing VerifyFile() (Recommended)

Modify `VerifyFile()` to detect file extension and handle `.snupkg` files:

```csharp
public static Task VerifyFile(FileInfo file, VerifySettings? settings = null)
{
    var extension = file.Extension.ToLowerInvariant();
    
    if (extension == ".nupkg")
    {
        return VerifyNupkg(file, settings);
    }
    else if (extension == ".snupkg")
    {
        return VerifySnupkg(file, settings);  // New method
    }
    
    throw new NotSupportedException($"Unsupported package extension: {extension}");
}

private static Task VerifySnupkg(FileInfo file, VerifySettings? settings)
{
    // Similar to VerifyNupkg but account for symbol package structure
    // - May not have .nuspec manifest
    // - Contains PDB files and source files
    // - Different content organization
}
```

### Option 2: New VerifySymbolPackage() Method

Add dedicated method for symbol packages:

```csharp
public static Task VerifySymbolPackage(FileInfo file, VerifySettings? settings = null)
{
    // Dedicated symbol package verification logic
}
```

**Trade-offs:**
- ✅ Clearer API surface
- ❌ Requires users to know package type upfront
- ❌ More code duplication

## Alternative Solutions Considered

1. **Custom ZIP Verification**: Implement custom ZIP extraction and tree generation
   - ❌ 150+ lines of code to maintain
   - ❌ Duplicates Verify.Nupkg functionality
   - ❌ Less reliable than upstream plugin

2. **Skip Symbol Package Testing**: Only verify `.nupkg` files
   - ❌ Loses coverage of symbol package structure
   - ❌ Misses potential packaging issues
   - ✅ Currently implemented as temporary workaround

3. **CI-Only Validation**: Use `dotnet sourcelink test` in CI
   - ✅ Validates Source Link metadata
   - ❌ Doesn't verify package structure
   - ❌ No snapshot baseline for regression detection

## Acceptance Criteria

- [ ] `VerifyFile()` accepts both `.nupkg` and `.snupkg` file paths
- [ ] Symbol package ZIP contents extracted correctly
- [ ] ASCII tree generated for symbol package structure
- [ ] `.nuspec` scrubbing works (if manifest present)
- [ ] Existing `.nupkg` verification behavior unchanged
- [ ] Documentation updated with symbol package examples
- [ ] Tests demonstrate both package types

## Impact on Qwiq Repository

Once implemented, Qwiq will:

1. **Remove Workaround Code**: Delete skip logging in `GetPackages()`
2. **Restore Symbol Baselines**: Create 9 `.snupkg.verified` baseline files (one per library)
3. **Improve Coverage**: Snapshot testing for all package artifacts (18 total tests: 9 .nupkg + 9 .snupkg)
4. **Simplify Maintenance**: Unified verification approach for both package types

**Migration Path:**
```csharp
// Update GetPackages() to include .snupkg
private static TheoryData<string> GetPackages()
{
    var theoryData = new TheoryData<string>();
    var packageFiles = Directory.GetFiles(
        Path.Combine(RepoRootHelper.RepoRoot, "src"),
        "*.nupkg",  // Add: "*.snupkg" once supported
        SearchOption.AllDirectories
    )
    // ... rest of deduplication logic ...
    
    var symbolPackageFiles = Directory.GetFiles(
        Path.Combine(RepoRootHelper.RepoRoot, "src"),
        "*.snupkg",
        SearchOption.AllDirectories
    )
    .Where(file => file.Contains(Path.Combine("bin", "Release")))
    .Select(file => new FileInfo(file))
    .GroupBy(GetPackageDiscriminator, StringComparer.OrdinalIgnoreCase)
    .Select(group => group.OrderByDescending(fileInfo => fileInfo.LastWriteTimeUtc).First())
    .Select(fileInfo => fileInfo.FullName);
    
    foreach (var package in packageFiles.Concat(symbolPackageFiles))
    {
        theoryData.Add(package);
    }
    
    return theoryData;
}
```

## Additional Context

- **Symbol Package Format**: `.snupkg` is a ZIP file containing PDB files, source files, and optionally a `.nuspec` manifest
- **Source Link Integration**: Symbol packages include Source Link metadata for debugger integration
- **NuGet.org Publishing**: Both `.nupkg` and `.snupkg` published together for complete debugging experience
- **Structure Differences**:
  - `.nupkg`: `/lib`, `/content`, `/build`, `/tools`, `.nuspec`
  - `.snupkg`: PDB files, source files under original paths, optional `.nuspec`

## References

- [NuGet Symbol Packages Documentation](https://learn.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg)
- [Qwiq Package Tests Implementation](https://github.com/rjmurillo/Qwiq/blob/copilot/start-wave-1-task-w1-1/test/Qwiq.Package.Tests/PackageTests.cs)
- [Source Link Configuration in Qwiq](https://github.com/rjmurillo/Qwiq/blob/copilot/start-wave-1-task-w1-1/Directory.Build.props)

---

**Next Steps:**
1. File issue in MattKotsenas/Verify.Nupkg with this content
2. Reference issue #38 in Qwiq codebase (already done in `PackageTests.cs`)
3. Monitor upstream for implementation
4. Update Qwiq package tests once support is available
