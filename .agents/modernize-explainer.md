# Qwiq Repository Modernization Explainer

> **Document Purpose**: Comprehensive Product Requirements Document (PRD) for modernizing the Qwiq repository.
> This document serves as the single source of truth for modernization planning and coordination.
>
> **Last Updated**: December 4, 2025 (Session 2)
> **Status**: Active Planning

---

## Executive Summary

**Qwiq** (Quick Work Item Query) is a .NET library providing a simplified API for querying Azure DevOps / Team Foundation Server work items. The repository has already completed significant modernization work including migration to SDK-style projects, .NET 8 support, Central Package Management, and GitHub Actions CI/CD.

### Current Modernization Status

| Wave | Description | Status |
|------|-------------|--------|
| Wave 0 | .NET 8 adoption, SDK-style projects | ✅ **Complete** |
| Wave 1 | Code quality baselines, contribution enablement | 🔄 **In Progress** |
| Wave Next | Observability, package ecosystem, advanced quality | 📋 **Planned** |

### Key Decisions Made

- **Target Frameworks**: Maintain `net472`, `netstandard2.0`, `net8.0` for maximum compatibility
- **SOAP Client**: Maintenance-only mode (bug fixes only, no new features)
- **REST Client**: Active development, cross-platform focus
- **Nullable Migration**: Phased project-by-project cleanup
- **Risk Tolerance**: Low - each change must be independently revertible

---

## Session Handoff Summary (December 4, 2025)

| Area | Update |
|------|--------|
| Documentation | Verified the modernization explainer & TODO remain the canonical sources inside `.agents/`. No secondary copies are maintained. |
| Progress Notes | No modernization tasks were completed in this session; focus was on documentation readiness and handoff preparations. |
| Validation | `dotnet test Qwiq.sln --configuration Release --no-build --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST&TestCategory!=IntegrationTests"` (Dec 4, 2025) — all targeted tests passed; integration suite filtered out by design. |
| Next Steps | Continue with Wave 1 items, starting with `W1.1` (SDK upgrade) or the highest-priority nullable cleanup task as capacity allows. |

> **Where to look next:** Continue using the `.agents` folder as the single source of truth for modernization planning. No mirrors exist elsewhere in the repository.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Application Layer                            │
│                    (Consumer Applications)                           │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       Integration Layer                              │
│  ┌────────────────┐  ┌─────────────────┐  ┌──────────────────┐     │
│  │ Qwiq.Linq     │  │ Qwiq.Mapper     │  │ Qwiq.Identity    │     │
│  │ .Identity     │  │ .Identity       │  │ .Soap            │     │
│  └───────┬───────┘  └────────┬────────┘  └────────┬─────────┘     │
└──────────┼───────────────────┼───────────────────┼──────────────────┘
           │                   │                   │
           ▼                   ▼                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        Extension Layer                               │
│  ┌────────────────┐  ┌─────────────────┐  ┌──────────────────┐     │
│  │ Qwiq.Linq     │  │ Qwiq.Mapper     │  │ Qwiq.Identity    │     │
│  │ LINQ→WIQL     │  │ Object Mapping  │  │ Identity Mgmt    │     │
│  └───────┬───────┘  └────────┬────────┘  └────────┬─────────┘     │
└──────────┼───────────────────┼───────────────────┼──────────────────┘
           │                   │                   │
           └───────────────────┼───────────────────┘
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                          Client Layer                                │
│  ┌─────────────────────────────┐  ┌────────────────────────────┐   │
│  │ Qwiq.Core.Rest              │  │ Qwiq.Core.Soap             │   │
│  │ net472/netstandard2.0/net8.0│  │ net472 only (Windows)      │   │
│  │ ✅ Active Development       │  │ 🔧 Maintenance Only        │   │
│  └──────────────┬──────────────┘  └─────────────┬──────────────┘   │
└─────────────────┼───────────────────────────────┼───────────────────┘
                  │                               │
                  ▼                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                           Core Layer                                 │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ Qwiq.Core - Interfaces: IWorkItem, IWorkItemStore, IQuery   │   │
│  │ net472 / netstandard2.0 / net8.0                            │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       External Systems                               │
│  ┌─────────────────────────┐  ┌────────────────────────────────┐   │
│  │ Azure DevOps Services   │  │ TFS On-Premises                │   │
│  │ (REST API)              │  │ (SOAP API)                     │   │
│  └─────────────────────────┘  └────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Wave 0: .NET 8 Adoption ✅ COMPLETE

### Status Summary

| Item | Status | Evidence |
|------|--------|----------|
| `global.json` pinned SDK | ✅ | v8.0.100 (update recommended → 8.0.404) |
| `nuget.config` configured | ✅ | Standard nuget.org source |
| SDK-style projects | ✅ | All 14 projects migrated |
| Supported TFMs | ✅ | net472, netstandard2.0, net8.0 |
| Central Package Management | ✅ | `Directory.Packages.props` |
| Nerdbank.GitVersioning | ✅ | v3.6.143 via `version.json` |

### Remaining Items

| Item | Status | Priority | Notes |
|------|--------|----------|-------|
| Update SDK to 8.0.404+ | 🔄 | Medium | Security/bug fixes |
| AnyCPU validation | ✅ | Done | All projects target AnyCPU |
| ARM64 testing | 📋 | Low | Future infrastructure concern |

---

## Wave 1: Code Quality & Contribution Enablement 🔄 IN PROGRESS

### 1.1 Dotfiles Status

| File | Status | Notes |
|------|--------|-------|
| `.gitignore` | ✅ | Standard .NET patterns |
| `.gitattributes` | ⚠️ | Verify/update for consistency |
| `.editorconfig` | ✅ | Comprehensive rules configured |
| `OWNERS` / `CODEOWNERS` | ❌ | Not present - create |

### 1.2 Documentation Status

| File | Status | Needs |
|------|--------|-------|
| `README.md` | 🔄 | Update badges (AppVeyor → GitHub Actions) |
| `CONTRIBUTING.md` | ⚠️ | Verify current, add ADRs |
| `TESTING.md` | ✅ | Comprehensive guide exists |
| `CODE_OF_CONDUCT.md` | ❌ | Not present - create |
| `SECURITY.md` | ❌ | Not present - create |

### 1.3 Code Quality Gates

| Item | Current | Target | Gap |
|------|---------|--------|-----|
| `TreatWarningsAsErrors` | ✅ Enabled | ✅ | None |
| Code coverage in CI | ❌ | 70% new code | Add coverlet + reporting |
| Nullable warnings | ~300+ suppressed | 0 | Phased cleanup |
| CA analyzer rules | ~150+ suppressed | Prioritized fix | Technical debt reduction |

---

## Wave 1 Detailed Gaps

### Gap 1: Code Coverage Not Published in CI

**Current State**: `coverlet.collector` is referenced in test projects but coverage is not:
- Collected during CI builds
- Published as artifacts
- Reported to PR checks

**Required Changes**:
1. Add `--collect:"XPlat Code Coverage"` to test command
2. Upload coverage reports as artifacts
3. Configure coverage reporting (e.g., Codecov, Coveralls, or built-in)
4. Set minimum threshold (70% for new code)

**Files to Modify**:
- `.github/workflows/main.yml`

---

### Gap 2: Source Link Not Configured

**Current State**: No Source Link configuration for debugging support.

**Required Changes**:
1. Add Source Link package to `Directory.Packages.props`:
   ```xml
   <PackageVersion Include="Microsoft.SourceLink.GitHub" Version="8.0.0" />
   ```
2. Configure in `Directory.Build.props`:
   ```xml
   <PublishRepositoryUrl>true</PublishRepositoryUrl>
   <EmbedUntrackedSources>true</EmbedUntrackedSources>
   <IncludeSymbols>true</IncludeSymbols>
   <SymbolPackageFormat>snupkg</SymbolPackageFormat>
   ```
3. Enable symbol publishing in CI

---

### Gap 3: PackageReadme Not Authored

**Current State**: NuGet packages lack embedded README.

**Required Changes**:
1. Create `docs/package-readme/` directory with per-package READMEs
2. Add to each packable project:
   ```xml
   <PackageReadmeFile>README.md</PackageReadmeFile>
   ```
3. Include content:
   - Quick start guide
   - Authentication examples
   - API surface overview
   - Link to full documentation

---

### Gap 4: Nullable Reference Types (300+ Suppressions)

**Current State**: Nullable enabled globally but extensive suppressions in `.editorconfig`:
- CS8600-CS8769 (17 rules suppressed)
- Indicates ~300+ potential violations

**Phased Cleanup Plan**:

| Phase | Projects | Priority | Effort |
|-------|----------|----------|--------|
| 1 | `Qwiq.Core` | High | M |
| 2 | `Qwiq.Core.Rest` | High | M |
| 3 | `Qwiq.Mocks` | Medium | S |
| 4 | `Qwiq.Linq` | Medium | L |
| 5 | `Qwiq.Mapper` | Medium | M |
| 6 | `Qwiq.Identity` | Low | S |
| 7 | `Qwiq.Core.Soap` | Low | S |
| 8 | `Qwiq.Identity.Soap` | Low | S |

**Approach**:
0. Wait for PR #56 to merge (https://github.com/rjmurillo/Qwiq/pull/56) and then evaluate remaining warnings.
1. Remove suppression for one CS86xx rule at a time
2. Fix violations in that rule category
3. Commit as atomic change
4. Repeat until all nullable annotations correct

---

### Gap 5: Analyzer Debt (~150+ Rules Suppressed)

**Current State**: Many CA rules suppressed in `.editorconfig` as technical debt.

**Prioritized Enablement Plan**:

| Category | Rules | Priority | Impact |
|----------|-------|----------|--------|
| Performance | CA18xx | High | Runtime efficiency |
| Reliability | CA2xxx | High | Stability |
| Security | CA3xxx-CA5xxx | High | Vulnerability prevention |
| Maintainability | CA15xx | Medium | Code health |
| Design | CA1xxx | Low | API quality |
| Naming | CA17xx | Low | Convention compliance |

---

## Wave Next: Parking Lot 📋 PLANNED

### Observability

| Item | Priority | Effort | Notes |
|------|----------|--------|-------|
| OpenTelemetry basic tracing | Medium | M | Query duration, work item counts |
| Standard runtime metrics | Low | S | CPU, memory, GC |
| Architecture-aware telemetry | Low | S | Add dimensions for client type |

### Package Ecosystem

| Item | Priority | Effort | Notes |
|------|----------|--------|-------|
| PackageReadme authoring | High | M | Per-package documentation |
| Source Link configuration | High | S | Debug symbol support |
| Symbol server publishing | Medium | S | nuget.org symbols |
| Semantic Versioning validation | Low | S | Already using GitVersion |

### Build/Deploy

| Item | Priority | Effort | Notes |
|------|----------|--------|-------|
| Artifacts output layout | Low | S | Standardize output paths |
| SDK version update (8.0.404+) | Medium | S | In global.json |
| .NET 9 evaluation | Low | M | Future framework support |

---

## Dependencies & Prerequisites

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        DEPENDENCY MAP                                    │
│                                                                         │
│  ┌──────────────────┐                                                   │
│  │ SDK Version      │──┐                                                │
│  │ Update (8.0.404) │  │                                                │
│  └──────────────────┘  │                                                │
│                        ▼                                                │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐     │
│  │ Source Link      │  │ Code Coverage    │  │ Nullable Cleanup │     │
│  │ Configuration    │  │ in CI            │  │ Phase 1 (Core)   │     │
│  └────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘     │
│           │                     │                     │                 │
│           │   ┌─────────────────┘                     │                 │
│           │   │                                       │                 │
│           ▼   ▼                                       ▼                 │
│  ┌──────────────────┐                    ┌──────────────────┐          │
│  │ PackageReadme    │                    │ Nullable Cleanup │          │
│  │ Authoring        │                    │ Phase 2-8        │          │
│  └────────┬─────────┘                    └────────┬─────────┘          │
│           │                                       │                     │
│           │              ┌────────────────────────┘                     │
│           │              │                                              │
│           ▼              ▼                                              │
│  ┌──────────────────────────────────────┐                              │
│  │ Analyzer Debt Reduction              │                              │
│  │ (Enable CA rules incrementally)      │                              │
│  └──────────────────────────────────────┘                              │
│                        │                                                │
│                        ▼                                                │
│  ┌──────────────────────────────────────┐                              │
│  │ OpenTelemetry Integration            │                              │
│  │ (Requires stable API surface)        │                              │
│  └──────────────────────────────────────┘                              │
│                                                                         │
│  PARALLEL TRACKS (No Dependencies):                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                  │
│  │ CODEOWNERS   │  │ README       │  │ SECURITY.md  │                  │
│  │ Creation     │  │ Badge Update │  │ Creation     │                  │
│  └──────────────┘  └──────────────┘  └──────────────┘                  │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Nullable changes break consumers | Medium | High | Phased rollout, extensive testing |
| TFS SDK updates break SOAP client | Low | Medium | Pin versions, maintenance-only mode |
| Code coverage gates block PRs | Low | Medium | Start with warnings, graduate to gates |
| Community PRs conflict with modernization | Medium | Low | Clear contribution guidelines, labeled issues |
| .NET 9 introduces breaking changes | Low | Medium | Evaluate after .NET 8 stabilization |
| ARM64 testing infrastructure gaps | Low | Low | Document as future work |
| OpenTelemetry API changes | Low | Low | Start with stable APIs only |
| Symbol server integration issues | Low | Low | Test in preview before release |

---

## Success Metrics

### Wave 1 Completion Criteria

| Metric | Baseline | Target | Measurement |
|--------|----------|--------|-------------|
| Nullable warnings | ~300+ | 0 | Build output count |
| Code coverage | Unknown | 70%+ (new code) | CI pipeline |
| CA rules suppressed | ~150 | <50 priority rules | .editorconfig count |
| Documentation completeness | 60% | 100% | Checklist audit |
| CI pipeline health | Passing | Green + artifacts | GitHub Actions |

### Definition of Done

Modernization is **complete** when:

1. ✅ All Wave 0 items verified
2. ✅ All Wave 1 items completed
3. ✅ Zero nullable warnings (or documented exceptions)
4. ✅ Code coverage published in every PR
5. ✅ Source Link functional (verified in debugger)
6. ✅ All NuGet packages have PackageReadme
7. ✅ CODEOWNERS, SECURITY.md, CODE_OF_CONDUCT.md exist
8. ✅ README badges reflect current CI status
9. ✅ Priority CA rules enabled (performance, reliability, security)
10. ✅ All changes documented in MIGRATION_NOTES.md

---

## Effort Estimates

| Item | Effort | Time (Solo) | Parallelizable |
|------|--------|-------------|----------------|
| SDK version update | S | 1-2 hours | Yes |
| Source Link config | S | 2-4 hours | Yes |
| Code coverage in CI | M | 4-8 hours | Yes |
| PackageReadme (all) | M | 1-2 days | Yes |
| CODEOWNERS creation | S | 1 hour | Yes |
| SECURITY.md creation | S | 1-2 hours | Yes |
| CODE_OF_CONDUCT.md | S | 1 hour | Yes |
| README badge update | S | 30 min | Yes |
| Nullable Phase 1 (Core) | M | 2-3 days | No |
| Nullable Phase 2 (Rest) | M | 2-3 days | After Phase 1 |
| Nullable Phases 3-8 | L | 1-2 weeks | Sequential |
| Analyzer debt (Priority) | L | 2-3 weeks | After nullable |
| OpenTelemetry basic | M | 2-3 days | After analyzers |

**Effort Key**: S = Small (< 4 hours), M = Medium (4-16 hours), L = Large (> 16 hours), XL = Extra Large (> 1 week)

---

## Appendix A: Project Target Frameworks

| Project | net472 | netstandard2.0 | net8.0 |
|---------|--------|----------------|--------|
| Qwiq.Core | ✅ | ✅ | ✅ |
| Qwiq.Core.Rest | ✅ | ✅ | ✅ |
| Qwiq.Core.Soap | ✅ | ❌ | ❌ |
| Qwiq.Linq | ✅ | ❌ | ✅ |
| Qwiq.Mapper | ✅ | ❌ | ✅ |
| Qwiq.Identity | ✅ | ❌ | ✅ |
| Qwiq.Identity.Soap | ✅ | ❌ | ❌ |
| Qwiq.Linq.Identity | ✅ | ❌ | ✅ |
| Qwiq.Mapper.Identity | ✅ | ❌ | ✅ |
| Qwiq.Mocks | ✅ | ❌ | ✅ |
| Qwiq.Tests.Common | ✅ | ❌ | ✅ |
| Qwiq.Core.Tests | ✅ | ❌ | ✅ |
| Qwiq.Linq.Tests | ✅ | ❌ | ✅ |
| Qwiq.Mapper.Tests | ✅ | ❌ | ✅ |
| Qwiq.Identity.Tests | ✅ | ❌ | ✅ |
| Qwiq.IntegrationTests | ✅ | ❌ | ❌ |
| Qwiq.Package.Tests | ❌ | ❌ | ✅ |

---

## Appendix B: Key Configuration Files

| File | Purpose | Modernization Relevance |
|------|---------|------------------------|
| `global.json` | SDK version pinning | Update to 8.0.404+ |
| `Directory.Build.props` | Shared MSBuild properties | Add Source Link config |
| `Directory.Build.targets` | Shared build targets | N/A |
| `Directory.Packages.props` | Central Package Management | Add new packages here |
| `.editorconfig` | Code style + analyzer severity | Remove suppressions |
| `version.json` | Nerdbank.GitVersioning config | Semantic versioning |
| `nuget.config` | NuGet sources | Standard config |
| `.github/workflows/main.yml` | CI/CD pipeline | Add coverage, artifacts |

---

## Appendix C: Suppressed Analyzer Categories

### Nullable (CS86xx) - 17 rules suppressed
- CS8600, CS8601, CS8602, CS8603, CS8604, CS8605
- CS8618, CS8619, CS8620, CS8625, CS8629
- CS8764, CS8765, CS8766, CS8767, CS8769

### Design (CA1xxx) - ~40 rules suppressed
### Globalization (CA13xx) - 8 rules suppressed
### Performance (CA18xx) - ~50 rules suppressed
### Reliability (CA2xxx) - ~25 rules suppressed
### Security (CA3xxx-CA5xxx) - ~20 rules suppressed
### IDE/Style (IDE0xxx) - ~30 rules suppressed

**Total**: ~150+ analyzer rules currently suppressed as technical debt

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Dec 4, 2025 | Claudette | Initial comprehensive PRD |
