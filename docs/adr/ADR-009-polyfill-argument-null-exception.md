# ADR-009: Polyfill Strategy for ArgumentNullException.ThrowIfNull

## Status
Accepted

## Context

- The repository targets multiple TFMs: `net472`, `netstandard2.0`, and `net8.0`.
- Modern guard APIs like `ArgumentNullException.ThrowIfNull` are only available in .NET 6+.
- Earlier experiments tried C# 14 extension members syntax (`extension(ArgumentNullException)`) patterned after the SimonCropp/Polyfill project, but the current .NET 10 SDK (10.0.100) and cross-assembly usage led to ambiguity and build failures for `net472` consumers.
- File-level locks (e.g., Microsoft Defender) complicated incremental builds; stability and determinism were prioritized.

### What we tried (and why it failed)
- **C# 14 extension members syntax** (inspired by SimonCropp/Polyfill):
  - Implemented `Polyfill.extension(ArgumentNullException).ThrowIfNull` with global usings.
  - Worked in-assembly but caused **CS0121 ambiguous call** errors across projects/assemblies targeting `net472`.
  - Extension members for static types are not yet reliable across assembly boundaries in the current toolchain.
- **Making Polyfill public/internal**:
  - Tried toggling visibility to reduce ambiguity; issues persisted because multiple linked copies still surfaced competing extension candidates.
- **GlobalUsings for Polyfills**:
  - Reduced call-site noise but did not eliminate ambiguity when the same extension surfaced from multiple referenced assemblies.

### Why not ship the SimonCropp Polyfill package directly?
- The repo already ships custom compatibility shims (e.g., `NullableAttributes.cs`) to avoid conflicts with VSS Client polyfills.
- Adding the package would risk namespace/type clashes with existing TFS/VSS dependencies.

## Decision

- **Stop using ThrowIfNull polyfill** and **replace all usages** with the traditional guard pattern:
  ```csharp
  if (param == null) throw new ArgumentNullException(nameof(param));
  ```
- **Remove `ArgumentNullExceptionPolyfill.cs`** and any project links to it.
- **Retain** other compatibility shims only where needed (e.g., `CallerArgumentExpressionAttribute`, `ArgumentOutOfRangeExceptionPolyfill`), keeping them **internal** and project-local to avoid cross-assembly ambiguity.
- Prefer simple, explicit guards over experimental language features for cross-TFM stability.

## Consequences

### Positive
- Eliminates CS0117/CS0121 ambiguity across assemblies for `net472`.
- More predictable multi-targeting builds; fewer surprises from compiler feature drift.
- Call-site readability remains high (one-line guard), no hidden magic.
- Avoids coupling to preview language features and reduces risk of future SDK regressions.

### Negative / Trade-offs
- Slightly more verbose than `ThrowIfNull`, no call-site auto-inlining hint, though JITs typically inline trivial guards.
- We lose parameter-name capture improvements from `CallerArgumentExpression` on older TFMs (already unavailable on `net472`).

## Implementation Notes

- **Repository-wide change**: 67 usages replaced across 62 files (source + tests) with explicit null guards.
- **Removed file**: `src/Qwiq.Core/Compatibility/ArgumentNullExceptionPolyfill.cs`.
- **Removed project links** to the polyfill in:
  - `src/Qwiq.Linq/Qwiq.Linq.csproj`
  - `src/Qwiq.Identity/Qwiq.Identity.csproj`
  - `src/Qwiq.Mapper/Qwiq.Mapper.csproj`
  - `src/Qwiq.Core.Rest/Qwiq.Client.Rest.csproj`
- **Kept** `CallerArgumentExpressionAttribute.cs` because it is required by `ArgumentOutOfRangeExceptionPolyfill.cs` (for `CallerArgumentExpression` on down-level TFMs).
- **C# syntax choice**: reverted to classic null checks; no extension members for static types.
- **Build flags**: `Directory.Build.rsp` enforces `/m:1 /nodeReuse:false` for stability on Windows; Defender exclusions recommended to reduce file-lock noise.

## Alternatives Considered

1) **Continue with C# 14 extension members** and add stricter namespacing or `using static` scoping.
   - Rejected: still ambiguous across assemblies; feature is preview and not reliable in SDK 10.0.100 for `net472`.

2) **Ship SimonCropp/Polyfill NuGet** and rely on its source generation/targets.
   - Rejected: risk of conflicts with VSS/TFS client polyfills and existing custom shims.

3) **Leave call sites unchanged and polyfill only at the boundary** (e.g., a single shared helper).
   - Rejected: still requires extension surface and cross-assembly resolution; explicit guards are simpler and clearer.

## Future Work

- If/when C# extension members for static types stabilize across assemblies (post C#14) and are supported in all target TFMs, we may revisit a lightweight, single-source polyfill.
- Consider a small Roslyn analyzer/code fix to enforce the standard guard pattern and prevent reintroduction of `ThrowIfNull` in down-level targets.
- Monitor SDK release notes for improved cross-assembly extension lowering; re-evaluate after .NET/SDK updates.

## References

- SimonCropp/Polyfill `ArgumentNullExceptionPolyfill.cs` (commit e78ac432695270075490e9fbee211e25e4fccb70)
- C# Extension Members (C# 14 proposal): https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-14.0/extensions
- Qwiq repository compatibility shims (e.g., `NullableAttributes.cs`, `CallerArgumentExpressionAttribute.cs`)
