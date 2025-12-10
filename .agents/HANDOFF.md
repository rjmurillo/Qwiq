# Handoff Document

> **Last Updated**: 2025-12-10 by Copilot Agent (Session File Cleanup)
> **Current Phase**: Wave 1 ✅ COMPLETE | Wave 2 Phase 2C (PARTIAL) | Maintenance
> **Branch**: `chore/modernize-wave-2`

---

## Current State

**Build Status**: ✅ Passing (Release /m:1 /nodeReuse:false on 2025-12-10)
**Test Status**: ✅ 206/207 passing, 1 skipped (filtered suite per standard categories)
**WireMock Tests**: ⚠ Not rerun this session (last known: ✅ 9 passing with captured ADO traffic)
**Package Validation**: ⚠ Not rerun this session (last known: ✅ All 10 packages produced)

**Last Commit**: `3eaeb170` - docs(agents): tidy session logs and links

### Session Summary (Session File Cleanup - 2025-12-10)

**Purpose**: Align session documentation after relocating session logs into `.agents/sessions/` and fixing stale links.

**Work Completed**:
1. ✅ Confirmed all session markdown files live under `.agents/sessions/` (renamed from root).
2. ✅ Updated internal references to the new paths, including the remaining location note in `session-handoff-test-failures.md`.
3. ✅ Added session log `2025-12-10-phase-maintenance-sessions.md` and refreshed `modernize-TODO.md` Session Activity + Last Updated metadata.

**Verification**:
- Build: ✅ Release /m:1 /nodeReuse:false (2025-12-10).
- Tests: ✅ Filtered suite passed (206 succeeded, 1 skipped).

**Notes/Next Steps**:
- Stage and commit `.agents/` changes (`git add .agents/`; force-add session logs if needed).
- If further work resumes, rerun build/tests to reconfirm baseline.


### Session Summary (Package Validation Fix - 2025-12-10)

**Purpose**: Fix GitHub Actions run #20110086011 where package validation was failing.

**Root Cause**:
- The SDK places packages in `artifacts/package/{Configuration}` when `ArtifactsPath` is set
- Both `Validate-PackageOutput.ps1` and `PackageTests.cs` were searching in `src/**/bin/Release`
- Packages exist in correct location but scripts/tests were looking elsewhere

**Solution Implemented**:
1. ✅ Fixed `build/scripts/Validate-PackageOutput.ps1` to search in `artifacts/package/{Configuration}`
2. ✅ Fixed `test/Qwiq.Package.Tests/PackageTests.cs` to search in `artifacts/package/release`
3. ✅ Added `Qwiq.Mocks` package baselines (newly packable project)

**Files Changed**:
- `build/scripts/Validate-PackageOutput.ps1` - Fixed package search path
- `test/Qwiq.Package.Tests/PackageTests.cs` - Fixed package search path
- `test/Qwiq.Package.Tests/PackageTests.Baseline_Qwiq.Mocks#*.verified.*` - New baselines

**Verification**:
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ 206 passed, 1 skipped
- Package validation: ✅ All 10 packages found

**Commits This Session**:
1. `07287637` - fix(ci): update package validation to use centralized artifacts directory
2. `[new]` - fix(test): update package tests to use centralized artifacts directory

See: `.agents/sessions/2025-12-10-package-validation-fix.md` for full details.

---

### Session Summary (CS0006 CI Build Fix - 2025-12-10)

**Purpose**: Investigate and fix CS0006 "Metadata file not found" errors in GitHub Actions CI run 20109171974.

**Root Cause**:
- `/m:1` only limits solution-level parallelism, NOT inner-build parallelism
- MSBuild's `DispatchToInnerBuilds` runs net472 and net8.0 inner builds in parallel
- Reference assemblies accessed before fully written = race condition

**Solution Implemented**:
1. ✅ Verified fix already in `Directory.Build.props` (lines 88-96): `BuildInParallel=false`, `MSBuildBuildInParallel=false`, `ProduceReferenceAssembly=false`
2. ✅ Added `Microsoft.NETFramework.ReferenceAssemblies` 1.0.3 for cross-platform net472 builds

**Files Changed**:
- `Directory.Packages.props` - Added package version
- `Directory.Build.props` - Added conditional PackageReference for net472
- `.agents/TASKS-cs0006-fix.md` - Created comprehensive task plan (454 lines)
- `.agents/sessions/2025-12-10-cs0006-fix.md` - Session log

**Subagent Consultations**:
| Agent | Purpose | Key Insights |
|-------|---------|--------------|
| csharp-expert | Technical MSBuild analysis | Inner-build parallelism explanation |
| feature-request-review | Solution validation | Confirmed approach, documented tradeoffs |
| independent-thinker | Devil's advocate | Alternative approaches, risks |
| generate-tasks | Task breakdown | Comprehensive plan generation |

**Verification**:
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ 186 passed, 1 skipped

**Next Steps**:
1. Commit and push to trigger CI
2. Verify CI run passes without CS0006 errors
3. Update Solutions Repository in copilot-instructions.md if successful

See: `.agents/sessions/2025-12-10-cs0006-fix.md` for full details.

---

### Session Summary (Phase 2C Evaluation - 2025-12-09)

**Purpose**: Evaluate Phase 2C work in branch `copilot/sub-pr-65` against the modernize-TODO.md plan.

**Findings**:
- ✅ **W2.4 - Benchmark CI Integration**: COMPLETE (all 3 benchmark projects compile in CI)
- ✅ **W2.16 Phase 1 (REST offline)**: COMPLETE (9 WireMock tests passing with real ADO traffic)
- ⏸️ **W2.16 Phase 2 (SOAP offline)**: NOT STARTED (Windows-only, Moq-based)
- ⏸️ **W2.3 (Contract Tests)**: BLOCKED by W2.16 Phase 2

**Wave 2 Progress**: 9/14 fully complete + 1 partial (W2.16) = 64% complete (71% including partial)

**Key Artifacts**:
- `.agents/sessions/2025-12-09-phase-2c-evaluation.md` - Full evaluation session log
- `docs/adr/008-wiremock-offline-rest-testing.md` - Architectural decision for WireMock approach
- `test/Qwiq.Integration.Tests/WireMock/` - WireMock test infrastructure

**Next Recommended Work**:
1. Resolve CS7069 TimeZone type forwarding errors (pre-existing, not Phase 2C related)
2. Implement W2.16 Phase 2 (SOAP offline tests with Moq - Windows-only)
3. Implement W2.3 (Contract Tests) after W2.16 Phase 2 complete

See: `.agents/sessions/2025-12-09-phase-2c-evaluation.md` for full details.

---

### Session Summary (Polyfill SOAP Projects - 2025-12-09)

**Completed**:
1. ✅ Added polyfill file links to `Qwiq.Client.Soap.csproj` and `Qwiq.Identity.Soap.csproj`
2. ✅ Replaced 14 traditional null checks with `ArgumentNullException.ThrowIfNull` in SOAP projects
3. ✅ Individual SOAP project builds succeed

**Files Changed** (10 files, 25 insertions, 14 deletions):
- `src/Qwiq.Core.Soap/Qwiq.Client.Soap.csproj` - Added polyfill links
- `src/Qwiq.Identity.Soap/Qwiq.Identity.Soap.csproj` - Added polyfill links
- `src/Qwiq.Core.Soap/WorkItemStore.cs` - 4 ThrowIfNull replacements
- `src/Qwiq.Core.Soap/WorkItemStoreFactory.cs` - 1 ThrowIfNull replacement
- `src/Qwiq.Core.Soap/WorkItemType.cs` - 1 ThrowIfNull replacement
- `src/Qwiq.Core.Soap/WorkItemLinkTypeEnd.cs` - 1 ThrowIfNull replacement
- `src/Qwiq.Core.Soap/WorkItemLinkType.cs` - 1 ThrowIfNull replacement
- `src/Qwiq.Core.Soap/QueryFactory.cs` - 1 ThrowIfNull replacement
- `src/Qwiq.Identity.Soap/IdentityManagementService.cs` - 2 ThrowIfNull replacements
- `src/Qwiq.Identity.Soap/Extensions.cs` - 3 ThrowIfNull replacements

**Known Issues**:
- ⚠️ CS0006 errors - Solution build fails with missing reference assembly errors (parallel build issue)
- ⚠️ CS0436 warning - `MaybeNullWhenAttribute` conflict between linked file and Qwiq.Core export

See: `.agents/sessions/2025-12-09-polyfill-soap.md` for full details.

---

### Session Summary (Compilation Fixes - 2025-12-09 earlier)

**Completed**:
1. ✅ Fixed `NotNullAttribute` accessibility - Added `NullableAttributes.cs` to Qwiq.Identity and Qwiq.Linq
2. ✅ Fixed `MaybeNullWhenAttribute` accessibility - Added `NullableAttributes.cs` to Qwiq.Core.Soap
3. ✅ Fixed `System.Runtime` version conflicts - Added conditional package reference for net472 builds
4. ✅ Fixed `ArgumentNullException.ThrowIfNull` polyfill access - Resolved via transitive references

See: `.agents/sessions/2025-12-09-compilation-fixes.md` for full details.

---

### Session Summary (Wave 1 Completion - 2025-12-08)

**Completed Wave 1 Tasks**:
1. ✅ **W1.22** - Document Testing Matrix (verified already complete)
2. ✅ **W1.23** - Configure ArtifactsPath (created Artifacts.props, imported in Directory.Build.props)
3. ✅ **W1.24** - Add Cross-Platform CI Matrix (verified already complete)
4. ✅ **W1.16** - Enable remaining P1 Reliability Rules (CA2213, CA2215 verified enabled)

**Wave 1 Status**: ✅ **COMPLETE** (27/27 tasks, 100%)

**Key Achievements**:
- All Phase 1E Build Quality Gates complete
- All P1 Reliability Rules enabled and passing (5/5)
- ArtifactsPath infrastructure ready for .NET 10+ upgrade (skipping .NET 9 STS, adopting .NET 10 LTS)
- Cross-platform CI validated on Windows and Linux
- Comprehensive documentation in place

See: `.agents/sessions/2025-12-08-wave1-completion.md` for full details.

### Session Summary (WireMock Offline REST Testing)

**Completed**:
1. ✅ Implemented WireMock-based offline REST tests (9 passing) using real ADO traffic
2. ✅ Captured HAR (1.7 MB) → Converted to stubs (1 MB, 5 mappings)
3. ✅ Added infrastructure: WireMock context, base spec, stub loader
4. ✅ Added tests: `WireMockQueryTests` (single, multiple, empty scenarios)
5. ✅ Added PowerShell tooling: `Convert-HarToWireMock.ps1`, `Capture-WireMockTraffic.ps1`
6. ✅ Documented via ADR-008 and `.agents/WIREMOCK-IMPLEMENTATION-COMPLETE.md`
7. ✅ Updated ADR index; tests runnable with `dotnet test --filter "TestCategory=WireMock"`

**Architectural Notes**:
- IdentityDescriptor must be string format (captured traffic), not object
- Fiddler system proxy required for capture; WireMock Cloud recording bypassed by SDK
- Uses Newtonsoft.Json for .NET Framework 4.7.2 compatibility

**Blockers**:
- SOAP offline tests (W2.16 Phase 2) remain TODO
- Expand stub coverage (multiple IDs, empty queries, error cases) suggested but not required

### Session 19 Summary (SBOM Tool Fix)

Fixed SBOM generation in GitHub Actions. The `microsoft/sbom-tool` GitHub Action is a container action that only works on Linux, causing Windows builds to fail. Solution:

1. Added `microsoft.sbom.dotnettool` v4.1.4 to `.config/dotnet-tools.json`
2. Use `dotnet sbom-tool generate` CLI instead of container action
3. Use nbgv version for SBOM package version
4. Run SBOM on both Windows and Linux (cross-platform)
5. DRYed out workflows - release.yml now downloads SBOM from main.yml build
6. Standardized all shells to `pwsh` for consistency

See: `.agents/sessions/2025-12-06-sbom-tool-fix.md` for full details.


## What Was Completed

### Wave 1 (Code Quality & Contribution Enablement) ✅ COMPLETE
- ✅ **W1.22** - Document Testing Matrix (verified complete)
- ✅ **W1.23** - Configure ArtifactsPath (Artifacts.props created)
- ✅ **W1.24** - Add Cross-Platform CI Matrix (verified complete)
- ✅ **W1.16** - Enable remaining P1 Reliability Rules (CA2213, CA2215 verified)
- **Wave 1 Total**: 27/27 tasks complete (100%)

### Wave 2 Phase 2A (Sessions 14-16 - 2025-12-06)
  - Created 6 comprehensive ADRs (49.1 KB total documentation)
  - Established ADR template and guidelines
  - Documented: Factory Pattern, Interface-First Design, REST/SOAP Strategy, Multi-Targeting, CPM, NRT Migration
  - Populated PublicAPI.Unshipped.txt for all 9 packable projects (1,268 total API entries)
  - Created framework-specific files for net472 polyfill types (Qwiq.Core, Qwiq.Identity)
  - Added local pragma suppressions for RS0026/RS0027 (optional parameter warnings)
  - Added .gitattributes rules for PublicAPI file line endings
  - Created migration script: `build/scripts/Migrate-PublicApiToShipped.ps1`
  - **Build passes with 0 RS00xx warnings**
  - Enhanced Dependabot configuration with scheduling, grouping, and labels
  - Created Renovate configuration with `helpers:pinGitHubActionDigests` preset
  - Renovate will automatically pin actions to commit SHAs via PR
  - Configured package grouping for NuGet dependencies
  - Enabled for all 9 packable projects
  - Configured strict mode for TFM and framework compatibility
  - Baseline version deferred until next release
  - Added `workflow_call` trigger to main.yml for DRY reuse
  - Created release.yml that reuses main.yml build/test/pack pipeline
  - Configured NuGet publishing with `--skip-duplicate`
  - Added environment approval gate (`production-nuget`)
  - Supports: workflow_dispatch, release events, and v* tags

### API Baseline Migration (Session 17 Bonus)
  - Created reusable script: `build/scripts/Migrate-PublicApiToShipped.ps1`
  - Executed migration across 11 PublicAPI.Shipped.txt files
  - All current API signatures now marked as "shipped" baseline
  - Enables breaking change detection in future releases
  - Build verified: 0 warnings, 0 errors


## What Was Completed

### Phase 2C: Testing Enhancements (Updated)
- ✅ **W2.4** - Benchmark CI Integration (verified benchmarks compile in CI)
- ✅ **W2.16 Phase 1 (REST offline)** - WireMock-based tests implemented and passing
- ✅ **ADR-008** - WireMock-based offline REST testing decision
- ⏸️ **W2.16 Phase 2 (SOAP offline)** - Not started (Windows-only, Moq-based)
- ⏸️ **W2.3** - Blocked by W2.16 Phase 2

## What's Next

### Immediate Priority: Complete Compilation Fixes

**Remaining Issues**:
1. **TimeZone Type Forwarding** - Some CS7069 errors remain in:
   - `Qwiq.Core.Rest` (VssConnectionAdapter.cs, WorkItemStore.cs)
   - `Qwiq.Core.Soap` (TfsTeamProjectCollection.cs, WorkItemStore.cs)
   - `Qwiq.Mocks` (MockTfsTeamProjectCollection.cs, MockWorkItemStore.cs)

2. **XmlElement Type Forwarding** - CS7069 error in:
   - `Qwiq.Core.Soap/CommonStructureService.cs`

**Potential Solutions**:
- Use explicit type aliases: `using TimeZone = System.TimeZone;` and `using XmlElement = System.Xml.XmlElement;`
- Add binding redirects in app.config/web.config
- Investigate if TFS client library updates would resolve type forwarding

**Next Steps**:
1. Address remaining TimeZone/XmlElement type forwarding issues
2. Run full test suite to verify no regressions
3. Verify build passes completely

### Phase 2A: ✅ COMPLETE (5/5 tasks)

1. ✅ ~~**W2.5** - Architecture Decision Records~~ (COMPLETE)
2. ✅ ~~**W2.2** - API Compatibility Baselines~~ (COMPLETE)
3. ✅ ~~**W2.15** - Pin GitHub Actions by SHA + Dependabot/Renovate~~ (COMPLETE)
4. ✅ ~~**W2.18** - Enable Package Validation~~ (COMPLETE)
5. ✅ ~~**W2.11** - Create Release Workflow~~ (COMPLETE)

### Phase 2B: Supply Chain Security ✅ COMPLETE (3/3 tasks)

1. ✅ ~~**W2.17** - SLSA Provenance Generation~~ (COMPLETE - commit c4077d5)
2. ✅ ~~**W2.13** - SBOM Generation (dual pipeline)~~ (COMPLETE - commit e569bb5)
3. ✅ ~~**W2.14** - Dependency Review Action~~ (COMPLETE - commit 5222a66)

### Phase 2C: Testing Enhancements (1 fully complete, 1 partial)

1. ✅ ~~**W2.4** - Benchmark CI Integration~~ (COMPLETE)
2. 🔄 **W2.16** - REST/SOAP Unit Test Coverage - **HIGH** - Phase 1 (REST offline) ✅ COMPLETE, Phase 2 (SOAP offline) pending
3. ⏸️ **W2.3** - Contract Tests for REST/SOAP Parity - **LOW** - Blocked by W2.16 Phase 2

### Phase 2D: Security Hardening (NEXT RECOMMENDED)

1. **W2.19** - CodeQL Advanced Security - **MEDIUM** - Integrate CodeQL into main.yml
2. **W2.20** - Secrets Scanning - **MEDIUM** - Add secrets scanning to CI

### Phase 2E: Documentation

1. **W2.7** - Update CONTRIBUTING.md - **MEDIUM** - Document new workflows and patterns


## Blockers & Concerns

| Issue | Impact | Mitigation |
|-------|--------|------------|
| W2.16 architectural challenge | Cannot implement REST unit tests without refactoring | ADR-007 created proposing factory method pattern with internal overload. Requires architectural review before implementation. |
| W2.3 blocked by W2.16 | Contract tests depend on REST unit test infrastructure | Defer W2.3 until W2.16 architectural decision approved and implemented. |

### Previous Blockers (Resolved)

| Issue | Impact | Mitigation |
|-------|--------|------------|
| None | - | - |


## Quick Verification

```powershell
# Verify current state
git status
git log --oneline -5

# Verify build (check for actual compilation errors, not reference assembly errors)
dotnet build Qwiq.sln -c Release /m:1 /nodeReuse:false 2>&1 | Select-String "error CS[0-9]{4}:" | Where-Object { $_ -notmatch "CS0006" }

# Check for RS00xx warnings (should be 0)
dotnet build Qwiq.sln -c Release 2>&1 | Select-String "RS00"

# Verify WireMock tests (should pass)
dotnet test Qwiq.sln -c Release --no-build --filter "TestCategory=WireMock"

# Verify source projects build successfully for net472
dotnet build src/Qwiq.Core/Qwiq.Core.csproj -f net472 -c Release
dotnet build src/Qwiq.Core.Rest/Qwiq.Client.Rest.csproj -f net472 -c Release
dotnet build src/Qwiq.Core.Soap/Qwiq.Client.Soap.csproj -c Release
dotnet build src/Qwiq.Linq/Qwiq.Linq.csproj -f net472 -c Release
dotnet build src/Qwiq.Mapper/Qwiq.Mapper.csproj -f net472 -c Release

# Check remaining TimeZone/XmlElement errors
dotnet build Qwiq.sln -c Release 2>&1 | Select-String "CS7069.*TimeZone|CS7069.*XmlElement"
```

**Note**:
- CS0006 errors (missing reference assemblies) are cascading from other compilation errors
- Focus on fixing CS7069 (type forwarding) and CS0122/CS0012 errors first
- Pre-existing issues in `Qwiq.Core.Tests` and package test baselines should be addressed separately


## Session History

| Date | Phase | Tasks | Status |
|------|-------|-------|--------|
| 2025-12-09 | 2C | Phase 2C Evaluation (W2.4, W2.16, W2.3 status review) | ✅ Complete |
| 2025-12-09 | Maintenance | Polyfill SOAP projects (14 ThrowIfNull replacements) | ✅ Complete |
| 2025-12-09 | Maintenance | Compilation fixes (NotNullAttribute, System.Runtime, polyfills) | 🔄 Partial |
| 2025-12-08 | 1E | Wave 1 Completion (W1.22, W1.23, W1.24, W1.16) | ✅ Complete |
| 2025-12-06 | Planning | Wave 2 restructure (Session 12-13) | ✅ Complete |
| 2025-12-06 | 2A | W2.5 (ADRs), W2.2 (API infra) - Session 14 | ✅ Complete |
| 2025-12-06 | 2A | W2.2 (API baselines populated) - Session 15 | ✅ Complete |
| 2025-12-06 | 2A | W2.15 (Deps), W2.18 (Validation) - Session 16 | ✅ Complete |
| 2025-12-06 | 2A | W2.11 (Release Workflow) + API Migration - Session 17 | ✅ Complete |
| 2025-12-06 | 2B | W2.17, W2.13, W2.14 (Supply Chain Security) - Session 18 | ✅ Complete |
| 2025-12-06 | 2B | SBOM Tool Fix - Session 19 | ✅ Complete |
| 2025-12-06 | 2C | W2.4, W2.16 infrastructure, ADR-007 - Session 20 | 🔄 Partial |


## Files to Review

If you need context, read these files in order:
1. `.agents/AGENT-INSTRUCTIONS.md` - **READ FIRST** - Process instructions
2. `.agents/modernize-TODO.md` - Task details and acceptance criteria
3. `.agents/modernize-explainer.md` - Architecture and design decisions
4. `.github/copilot-instructions.md` - Repository coding standards


## Important Notes for Next Session

1. **Phase 2A COMPLETE**: All 5 tasks done (W2.5, W2.2, W2.15, W2.18, W2.11) ✅
   - Plus bonus: API Migration (1,296 entries) completed in Session 17

2. **Phase 2B COMPLETE**: All 3 tasks done (W2.17, W2.13, W2.14) ✅
   - SLSA Level 3 provenance generation
   - Dual-pipeline SBOM (SPDX 2.3)
   - Enhanced dependency review with license policy

3. **Phase 2C PARTIAL**: 1/3 tasks complete
   - ✅ W2.4 - Benchmark CI Integration (verified working)
   - ⏸️ W2.16 - Blocked pending ADR-007 architectural decision
   - ⏸️ W2.3 - Blocked by W2.16

4. **Wave 2 Progress**: 9/14 tasks fully complete, 1 partial (W2.16 Phase 1) (64% complete, 71% with partial)

5. **CRITICAL DECISION REQUIRED**: ADR-007 REST Client Testability
   - Review `docs/adr/007-rest-client-testability.md`
   - Option 3a (Factory Method with Internal Overload) is recommended
   - Decision needed before W2.16 implementation can proceed

6. **NEXT RECOMMENDED**: Phase 2D - Security Hardening
   - W2.19 - CodeQL Advanced Security (simpler, no blockers)
   - W2.20 - Secrets Scanning (simpler, no blockers)
   - Both can proceed independently while ADR-007 is under review

4. **Release Workflow Manual Setup Required**:
   - Create `production-nuget` environment in GitHub repo settings
   - Add `NUGET_API_KEY` secret to the repository
   - Test with `workflow_dispatch` before relying on tag triggers

5. **API Baselines Complete** ✅:
   - W2.2: 1,268 API entries documented in Unshipped files
   - Session 17: 1,296 entries migrated to Shipped (establishes stable baseline)
   - Script available: `build/scripts/Migrate-PublicApiToShipped.ps1` for future releases

6. **Dependency Management Complete** ✅:
   - Renovate will automatically pin GitHub Actions to SHAs via PR
   - Dependabot and Renovate both configured with proper grouping
   - Dependency review blocks vulnerable packages

7. **Package Validation Enabled** ✅:
   - All 9 projects configured
   - Baseline version will be set after next release
   - Breaking changes will be detected automatically

8. **ADRs Complete** ✅: 6 comprehensive ADRs documented (49.1 KB total)

9. **Supply Chain Security Complete** ✅ (Session 18):
   - SLSA Level 3 provenance with verification docs
   - Dual-pipeline SBOM generation (SPDX 2.3)
   - Dependency review with license policy enforcement
   - Complete transparency for release artifacts


## Package Versions to Use

When adding packages for Wave 2:

```xml
<!-- API Analyzers (W2.2) -->
<PackageVersion Include="Microsoft.CodeAnalysis.PublicApiAnalyzers" Version="3.3.4" />

<!-- Testing (W2.16 - future) -->
<PackageVersion Include="WireMock.Net" Version="1.5.40" />
<PackageVersion Include="Moq" Version="4.16.0" />
<PackageVersion Include="Moq.Analyzers" Version="0.4.0" />
```


## SBOM Tool Configuration

The SBOM tool is now configured as a local .NET tool:

```json
// .config/dotnet-tools.json
"microsoft.sbom.dotnettool": {
  "version": "4.1.4",
  "commands": ["sbom-tool"]
}
```

**Usage**: `dotnet sbom-tool generate -b <buildDropPath> -bc <buildComponentPath> -pn <packageName> -pv <version> -ps <supplier> -nsb <namespaceBase> -m <manifestDirPath>`

**Note**: The `-m` directory must exist before running the tool.

---

## End of Handoff

### Wave 1 Completion Summary (2025-12-08)

**Wave 1: ✅ COMPLETE (27/27 tasks, 100%)**

All remaining Wave 1 tasks completed in this session:
- ✅ W1.22 - Document Testing Matrix (verified already complete)
- ✅ W1.23 - Configure ArtifactsPath (Artifacts.props created and imported)
- ✅ W1.24 - Add Cross-Platform CI Matrix (verified already complete)
- ✅ W1.16 - Enable remaining P1 Reliability Rules (CA2213, CA2215 verified)

**Key Achievements**:
- All Phase 1E Build Quality Gates complete
- All P1 Reliability Rules enabled and passing (5/5: CA1062, CA2000, CA2007, CA2213, CA2215)
- ArtifactsPath infrastructure ready for .NET 10+ upgrade (skipping .NET 9 STS, adopting .NET 10 LTS)
- Cross-platform CI validated on Windows and Linux
- Comprehensive documentation in place

**Session Commits**:
- `72196f4b` - W1.23: ArtifactsPath configuration
- `d11aee8b` - W1.16: P1 Reliability Rules complete
- `a92245a3` - Wave 1 documentation updates
- `b9090d00` - Fix duplicate SBOM section
- `066a08e2` - Fix Wave 2 task counts

### Next Session Recommendations

The next Copilot session should:

**Option 1: Continue Wave 2 Phase 2D (Security Hardening)** - Recommended
1. Read `AGENT-INSTRUCTIONS.md` completely
2. Create session log: `.agents/sessions/2025-12-XX-phase-2d.md`
3. Execute Phase 2D tasks:
   - W2.19 - CodeQL Advanced Security (integrate into main.yml)
   - W2.20 - Secrets Scanning (GitHub native or Gitleaks)
4. Update this HANDOFF.md before ending

**Option 2: Continue Wave 2 Phase 2C (Testing Enhancements)**
1. Review ADR-007 for REST Client Testability decision
2. If approved, implement W2.16 Phase 2 (SOAP offline tests)
3. Then proceed with W2.3 (Contract Tests)

**Option 3: Wave 2 Phase 2E (Documentation)**
1. W2.7 - Update CONTRIBUTING.md with new workflows and patterns
