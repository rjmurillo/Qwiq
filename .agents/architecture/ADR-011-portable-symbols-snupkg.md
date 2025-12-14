# ADR-011: Portable Debug Symbols with Symbol Packages

**Status**: Accepted
**Date**: 2025-12-14
**Deciders**: Architecture Team
**Context**: v11.0.0 NuGet Release Preparation

## Context and Problem Statement

QWIQ is preparing for its v11.0.0 NuGet release. The project uses `DotNet.ReproducibleBuilds` for deterministic builds, which defaults to `embedded` debug symbols. We need to decide whether to accept this default or override to `portable` symbols with separate `.snupkg` symbol packages.

**Key Questions**:

- Should debug symbols be embedded in assemblies or distributed separately?
- What impact does this have on package consumers?
- How does this align with industry practices for public NuGet libraries?

## Decision Drivers

### Technical Factors

1. **Package Size Impact**:

   - Embedded PDBs increase assembly size by approximately 20-30%
   - QWIQ has 9 packable projects with 6 target frameworks each
   - Size penalty applies to ALL consumers, not just debuggers

2. **Distribution Model**:

   - NuGet.org has a dedicated symbol server (`symbols.nuget.org`)
   - Symbol packages (`.snupkg`) are indexed automatically when pushed alongside `.nupkg`
   - Pay-to-play model: only developers who debug download symbols

3. **DotNet.ReproducibleBuilds Default**:

   - Defaults to `DebugType=embedded` for simplicity
   - Must be explicitly overridden for portable + snupkg approach

4. **Debugging Experience**:
   - Embedded: Zero configuration - symbols travel with assemblies
   - Portable: One-time IDE configuration to enable NuGet.org symbol server

### Industry Practices

Survey of popular .NET libraries:

| Library             | DebugType  | Symbol Distribution | Notes                                  |
| ------------------- | ---------- | ------------------- | -------------------------------------- |
| **dotnet/runtime**  | `portable` | snupkg              | Microsoft's own base class libraries   |
| **dotnet/aspnetcore** | (default)  | `IncludeSymbols`    | Uses symbol packages                   |
| **Serilog**         | (default)  | snupkg              | Popular logging library                |
| **AutoMapper**      | (default)  | snupkg              | Popular mapping library                |
| **Newtonsoft.Json** | N/A        | No symbols          | Users have requested PDBs              |

**Key observation**: Microsoft's own libraries use `portable` + snupkg for public packages.

## Considered Options

### Option 1: Portable Symbols with snupkg (Recommended)

**Configuration**:

```xml
<PropertyGroup>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>

<PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
  <DebugType>portable</DebugType>
</PropertyGroup>
```

**Pros**:

- Smaller package size for all consumers
- Pay-to-play model respects consumer bandwidth
- Follows Microsoft guidance for public libraries
- Industry standard approach (Serilog, AutoMapper, etc.)
- NuGet.org symbol server is optimized for this workflow

**Cons**:

- Consumers must configure symbol server in IDE (one-time)
- Corporate firewalls may block symbol downloads
- Two packages to publish instead of one
- Must maintain `DebugType` override in `Directory.Build.props`
- CI must publish both `.nupkg` and `.snupkg`

### Option 2: Embedded Symbols

**Configuration**:

```xml
<PropertyGroup>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <DebugType>embedded</DebugType>
</PropertyGroup>
```

**Pros**:

- Zero configuration debugging - "just works"
- No network dependency for symbols
- Single package to manage
- Simpler CI pipeline (one package type)
- Works in corporate environments with symbol server blocks

**Cons**:

- ALL consumers pay 20-30% size penalty, even if they never debug
- Slower restore times for large dependency graphs
- Contradicts Microsoft guidance for public libraries
- Exposes debug info in production deployments

## Decision

Adopt Option 1: Portable Symbols with snupkg.

### Rationale

1. **Microsoft Guidance**: Microsoft's own `dotnet/runtime` explicitly uses `portable` to "override arcade sdk which uses embedded for local builds" - they deliberately choose portable for public distribution.

2. **Bandwidth Respect**: Most QWIQ consumers will never debug into QWIQ source code. Making everyone download embedded symbols violates the principle of not wasting consumer resources.

3. **Industry Standard**: Popular libraries (Serilog, AutoMapper) use snupkg. Developers expect this pattern and have symbol servers configured.

4. **NuGet.org Optimization**: The NuGet.org symbol server is specifically designed for this workflow, with tight integration into `dotnet nuget push`.

5. **Professionalism**: Separating concerns (code distribution vs. debug info) is the more architecturally sound approach for a public library.

## Consequences

### Headaches We Accept

1. **Consumer IDE Configuration**:

   - Visual Studio: Tools > Options > Debugging > Symbols > Add `https://symbols.nuget.org/download/symbols`
   - JetBrains Rider: Works automatically
   - VS Code: Requires `CopyDebugSymbolFilesFromPackages=true` for .NET 7+
   - **Mitigation**: Document in README; one-time setup

2. **Corporate Firewall Blocks**:

   - Some enterprises block external symbol servers
   - **Mitigation**: Users in such environments can use Source Link with authenticated GitHub access, or accept debugging without symbols

3. **Dual Package Publishing**:

   - CI must push both `.nupkg` and `.snupkg`
   - **Mitigation**: `dotnet nuget push *.nupkg --source nuget.org` handles both automatically

4. **Configuration Override Maintenance**:

   - Must maintain `<DebugType>portable</DebugType>` override in `Directory.Build.props`
   - **Mitigation**: Document in ADR; unlikely to conflict with future changes

5. **Symbol Server Availability**:
   - NuGet.org symbol server downtime affects debugging
   - **Mitigation**: Source Link provides fallback to GitHub source browsing

### Benefits We Gain

1. **Smaller Packages**: Base size without 20-30% symbol overhead
2. **Faster Restores**: Consumers download less data during `dotnet restore`
3. **Professional Standard**: Aligns with Microsoft and popular library practices
4. **Pay-to-Play Model**: Only debuggers incur symbol download cost
5. **Source Link Integration**: Full debugging experience with GitHub source navigation

### Neutral Outcomes

1. **Build Time**: No significant difference between portable and embedded
2. **CI Complexity**: Minimal - `dotnet pack` produces both formats automatically
3. **NuGet.org Storage**: Microsoft hosts symbol packages at no additional cost

## Implementation

### Current Configuration (Verified)

The following is already in `Directory.Build.props`:

```xml
<!-- Source Link Configuration -->
<PropertyGroup>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>

<PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
  <DebugType>portable</DebugType>
  <!-- ... -->
</PropertyGroup>
```

### CI Publishing

```bash
# Push both nupkg and snupkg to NuGet.org
dotnet nuget push "artifacts/**/*.nupkg" --source nuget.org --api-key $NUGET_API_KEY
# Note: snupkg is automatically pushed alongside nupkg
```

### Consumer Documentation

Add to README.md or package README:

```markdown
## Debugging

QWIQ packages include Source Link support for full debugging with source code navigation.

**Visual Studio Setup** (one-time):

1. Tools > Options > Debugging > Symbols
2. Check "NuGet.org Symbol Server"
3. Enable "Load only specified modules" for faster debugging (optional)

**JetBrains Rider**: Works automatically.
```

## Validation

### Success Criteria

- [x] `Directory.Build.props` configured for portable + snupkg
- [ ] v11.0.0 packages publish successfully to NuGet.org
- [ ] Symbol packages indexed on NuGet.org symbol server
- [ ] Debugging into QWIQ source works in Visual Studio with symbol server configured
- [ ] Package size is baseline (no embedded symbol bloat)

### Monitoring

- NuGet.org package size metrics (compare to previous versions with embedded)
- GitHub issues tagged with `debugging` or `symbols`
- User feedback on debugging experience

## Related Decisions

- **ADR-005**: Central Package Management - Package versioning approach
- **Analysis-001**: Embedded vs Portable Symbols Analysis (`.agents/analysis/001-embedded-vs-portable-symbols-analysis.md`)
- **DotNet.ReproducibleBuilds**: External package providing deterministic build defaults

## References

- [Microsoft Learn: NuGet and .NET libraries](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/nuget)
- [Microsoft Learn: Symbol packages (.snupkg)](https://learn.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg)
- [NuGet Package Debugging & Symbols Improvements](https://github.com/NuGet/Home/wiki/NuGet-Package-Debugging-&-Symbols-Improvements)
- [dotnet/sdk Issue #2679: Discussion on embedded default](https://github.com/dotnet/sdk/issues/2679)
- [Ken Muse: What Every Developer Should Know About PDBs](https://www.kenmuse.com/blog/what-every-developer-should-know-about-pdbs/)
- [dotnet/runtime Directory.Build.props](https://github.com/dotnet/runtime/blob/main/Directory.Build.props)
