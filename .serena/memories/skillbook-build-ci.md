# Build & CI Skills

## Skill-Build-001

**Entity Type**: Skill
**Statement**: Use `/p:ContinuousIntegrationBuild=true /p:UseSharedCompilation=false /m:1 /nodeReuse:false` for local builds to match CI analyzer strictness
**Atomicity**: 96%
**Category**: Build
**Context**: Before pushing changes that passed local builds
**Evidence**: Session 39 - Fixed CA1711, CA1001, CA1861 errors that passed locally but failed CI
**Tag**: helpful
**Impact**: 9
**Validated**: 1

**Summary**: Local builds without CI flags don't enable strict analyzer rules. CI pipeline enables these flags by default. Mismatch causes "works locally, fails in CI" situations. Common errors caught: CA1711 (type name suffix), CA1001 (disposable fields), CA1861 (const arrays).

**Application Example**:

```powershell
# WRONG - may pass but CI will fail
dotnet build Qwiq.sln -c Release /m:1 /nodeReuse:false

# CORRECT - matches CI behavior
dotnet build Qwiq.sln -c Release /p:ContinuousIntegrationBuild=true /p:UseSharedCompilation=false /m:1 /nodeReuse:false
```

---

## Skill-Build-002

**Entity Type**: Skill
**Statement**: Set `BuildInParallel=false` and `ProduceReferenceAssembly=false` for Windows multi-framework builds
**Atomicity**: 93%
**Category**: Build
**Context**: When experiencing CS0006 "metadata file not found" errors in multi-targeting
**Evidence**: Session CS0006-fix - Directory.Build.props lines 88-96
**Tag**: helpful
**Impact**: 8
**Validated**: 1

**Summary**: Inner-build parallelism causes reference assembly conflicts on Windows. Multi-targeting builds (.NET Framework + .NET Core) need special handling. Error manifests as "The metadata file X cannot be found". Infrastructure fix in Directory.Build.props prevents race conditions.

---

## Skill-CI-001

**Entity Type**: Skill
**Statement**: CI should verify lint rules without auto-fix to catch commits bypassing pre-commit hooks
**Atomicity**: 95%
**Category**: CI
**Context**: When configuring CI pipeline for markdown-heavy repositories
**Evidence**: Session Linting Automation - Added check-only step to main.yml
**Tag**: helpful
**Impact**: 9
**Validated**: 1

**Summary**: Pre-commit hooks auto-fix issues locally (developer experience). CI should verify rules WITHOUT auto-fixing (catch bypasses). Prevents developers from bypassing local hooks. Two-layer approach: auto-fix locally, verify in CI.

---

## Skill-CI-002

**Entity Type**: Skill
**Statement**: CI must validate all files (`**/*.md`), not just changes, as final safety net
**Atomicity**: 92%
**Category**: CI
**Context**: When configuring CI lint checks for repositories with incremental local hooks
**Evidence**: Session markdown-lint-incident 2025-12-14 - 321 errors caught by CI that hooks missed
**Tag**: helpful
**Impact**: 10
**Validated**: 1

**Summary**: Pre-commit hooks validate only staged files for performance. Files committed before hooks enabled are never locally validated. Files committed with `--no-verify` bypass local validation entirely. CI must scan entire repository to catch accumulated issues.

---

## Skill-CI-003

**Entity Type**: Skill
**Statement**: Pre-commit hooks validating only staged files miss pre-existing issues; run baseline validation when enabling hooks
**Atomicity**: 88%
**Category**: CI
**Context**: When enabling git hooks on an existing codebase
**Evidence**: Session markdown-lint-incident 2025-12-14 - 321 errors accumulated before CI detected them
**Tag**: helpful
**Impact**: 9
**Validated**: 1

**Summary**: Git hooks that check only staged files have a blind spot. Files that existed before hook enablement are never validated. This creates a "technical debt" of unvalidated files. Solution: Run full repository scan before enabling incremental hooks.
