# ADR-012: Embedded Debug Symbols

**Status**: Accepted  
**Date**: 2025-12-14  
**Deciders**: Multi-Agent Consensus (Architect, DevOps, Independent Thinker, QA)  
**Context**: v11.0.0 NuGet Release Preparation  
**Supersedes**: ADR-011

## Context and Problem Statement

Following the initial analysis in ADR-011, which recommended portable debug symbols with separate `.snupkg` symbol packages, the maintainer raised concerns about operational complexity ("pain in the ass factor"). This prompted a multi-agent consensus review to reconsider the decision in light of QWIQ's actual scale, audience, and maintainer capacity.

**Key Question**: Should QWIQ optimize for theoretical bandwidth efficiency (portable symbols) or maintainer simplicity and debugging UX (embedded symbols)?

## Decision Drivers

### Scale Reality

- **QWIQ's Download Rate**: ~1 download/day (<1000 total downloads)
- **Microsoft's Download Rate**: Millions per day
- **Bandwidth Impact**: 200KB × 365 days = ~73MB/year total overhead
- **Conclusion**: Bandwidth optimization is premature at QWIQ's scale

### Maintainer Constraints

- **Single part-time maintainer** with limited capacity
- **CI/CD complexity**: Dual-package publishing adds cognitive overhead and failure modes
- **Support burden**: Symbol server configuration requires user documentation and support
- **Sustainability**: Simpler operations = more time for features/fixes

### Audience Characteristics

- **Target Users**: Enterprise Azure DevOps/TFS users
- **Network Environment**: Disproportionately behind corporate firewalls
- **Symbol Server Access**: May be blocked by IT policy
- **Debugging Pattern**: When users debug, "just works" > configuration steps

### Technical Considerations

- **Package Size Impact**: 20-30% increase (~200-300KB per package)
- **Absolute Size**: Still under 2MB per package
- **Modern Hardware**: Size increase is negligible on developer workstations
- **Source Link**: Provides GitHub source navigation regardless of symbol type

## Considered Options

### Option 1: Continue with Portable + snupkg (ADR-011)

**Pros**:

- Aligns with Microsoft's public library guidance
- Smaller package size for majority who never debug
- Industry standard for popular libraries
- Pay-to-play bandwidth model

**Cons**:

- Dual-package CI/CD complexity
- Symbol server configuration required for debugging
- Corporate firewalls may block symbol servers
- Maintainer overhead for minimal benefit at QWIQ's scale

### Option 2: Switch to Embedded Symbols (Recommended)

**Pros**:

- Zero-configuration debugging experience
- Single package simplifies CI/CD pipeline
- No network dependency for debugging
- Works in all enterprise environments
- Consistent experience across all IDEs
- Reduces maintainer operational burden

**Cons**:

- 20-30% larger packages (~200-300KB increase)
- Deviates from Microsoft's guidance for public libraries
- All consumers download symbols whether they debug or not

## Decision

**Adopt Option 2: Embedded Debug Symbols**

This decision explicitly reverses ADR-011 based on pragmatic assessment of QWIQ's specific constraints.

### Rationale

1. **Maintainer Sustainability**: Single maintainer's time is the scarce resource, not consumer bandwidth. Simpler CI/CD operations directly benefit project longevity.

2. **Scale-Appropriate Optimization**: At 1 download/day, the bandwidth "savings" from portable symbols (~73MB/year) is background noise. Optimizing for this is premature.

3. **Enterprise Reality**: QWIQ's Azure DevOps/TFS audience is disproportionately behind corporate firewalls where `symbols.nuget.org` may be blocked. Embedded symbols bypass this entirely.

4. **Debugging UX**: Zero-configuration debugging benefits both consumers debugging issues and contributors working on QWIQ. The "just works" experience outweighs theoretical efficiency.

5. **Professional Decision-Making**: Following Microsoft's approach without understanding their constraints (millions of downloads, dedicated support teams) is cargo culting. QWIQ's constraints are fundamentally different.

## Consequences

### Accepted Trade-offs

1. **Package Size**: 20-30% increase (~200-300KB per package)

   - **Impact**: Low - still under 2MB per package
   - **Mitigation**: CI validation enforces 2MB threshold

2. **Industry Deviation**: Differs from Microsoft and popular libraries

   - **Impact**: May raise questions in code reviews
   - **Mitigation**: Clear documentation of rationale in this ADR

3. **Bandwidth "Waste"**: All consumers download symbols
   - **Impact**: Negligible at 1 download/day scale
   - **Reevaluation Trigger**: If downloads exceed 100/day

### Benefits Gained

1. **Simplified CI/CD**: Single package type, fewer failure modes
2. **Zero-Config Debugging**: Works immediately in all IDEs
3. **Enterprise Compatibility**: No symbol server network dependency
4. **Contributor Experience**: Consistent debugging without setup
5. **Maintainer Time**: Reduced operational overhead

## Implementation

### Configuration (Already Applied)

`Directory.Build.props` has been updated:

```xml
<PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
  <!-- Use embedded symbols for simplicity and enterprise compatibility -->
  <DebugType>embedded</DebugType>
  <Optimize>true</Optimize>
  <DefineConstants>$(DefineConstants);TRACE</DefineConstants>
</PropertyGroup>
```

**Note**: `IncludeSymbols` and `SymbolPackageFormat` are NOT set, preventing `.snupkg` generation.

### CI/CD Simplification

Release workflow publishes only `.nupkg` files (symbols embedded):

```powershell
# Push nupkg files (now contain embedded symbols)
foreach ($file in $nupkgFiles) {
    dotnet nuget push $file.FullName --api-key "$env:NUGET_API_KEY" `
        --source https://api.nuget.org/v3/index.json --skip-duplicate
}
```

### Validation

Package size regression test added to CI:

```powershell
# Fail if any package exceeds 2MB threshold
$maxNupkgSize = 2MB
foreach ($nupkg in $nupkgFiles) {
    if ($nupkg.Length -gt $maxNupkgSize) {
        Write-Error "Package $($nupkg.Name) exceeds size threshold"
    }
}
```

## Monitoring and Reevaluation

### Success Criteria

- [x] `DebugType=embedded` configured in Directory.Build.props
- [ ] v11.0.0 packages published successfully
- [ ] Package sizes under 2MB threshold
- [ ] Debugging works without configuration in VS, Rider, and VS Code
- [ ] No user complaints about package size in first 30 days

### Reevaluation Triggers

Reconsider this decision if:

- **Download count exceeds 100/day** (scaling assumption changes)
- **Multiple users report package size issues** within first release
- **Corporate environments report embedded symbols as security concern**

Monitor:

- NuGet.org download statistics
- GitHub issues tagged with `debugging` or `package-size`
- Package size trends in CI builds

## Multi-Agent Consensus

This decision resulted from consultation with 4 specialized agents:

| Agent                   | Vote        | Key Reasoning                                    |
| ----------------------- | ----------- | ------------------------------------------------ |
| **Architect**           | EMBEDDED ✓  | Maintainer time > bandwidth at QWIQ's scale      |
| **DevOps**              | EMBEDDED ✓  | CI/CD simplification measurable (10-15 min/rel)  |
| **Independent Thinker** | EMBEDDED ✓  | Following Microsoft is cargo culting at our scale |
| **QA**                  | PORTABLE ⚠️ | Technically superior but pragmatically accepts   |

**Consensus**: 3/4 favor embedded (strong majority). QA's technical preference for portable symbols acknowledged but overridden by pragmatic factors.

### Dissenting Opinion

**QA's Reservation**: Portable+snupkg remains technically superior for public libraries in abstract. Embedded is a deliberate tradeoff for maintainer pragmatism, not the "correct" technical choice. If QWIQ's adoption grows significantly, this should be reevaluated.

## Related Decisions

- **ADR-011**: Superseded by this decision - originally recommended portable symbols
- **DotNet.ReproducibleBuilds**: External package providing deterministic build support
- **ADR-005**: Central Package Management - Package versioning approach

## References

- **Consensus Documents**:
  - `.agents/architecture/002-symbols-consensus-recommendation.md` - Full analysis
  - `.agents/architecture/DECISION-SUMMARY-embedded-symbols.md` - Executive summary
  - `.agents/sessions/2025-12-14-debug-symbol-consensus.md` - Session log
- **Implementation**: PR #124 - Multi-agent consensus: Switch to embedded debug symbols
- **Microsoft Guidance**:
  - [NuGet and .NET libraries](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/nuget)
  - [Symbol packages (.snupkg)](https://learn.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg)
- **Technical Context**:
  - [Ken Muse: What Every Developer Should Know About PDBs](https://www.kenmuse.com/blog/what-every-developer-should-know-about-pdbs/)
  - [dotnet/sdk Issue #2679: Discussion on embedded default](https://github.com/dotnet/sdk/issues/2679)

## Superseded By

None
