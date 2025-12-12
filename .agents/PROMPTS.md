# Copilot Agent Prompts

> **Purpose**: Self-contained prompts for Copilot agent sessions. Each prompt includes
> all context needed - no prior knowledge required.
>
> **Target**: Production v11.0.0 Release for 100+ team members
> **Timeline**: 6-8 weeks
> **Branch**: `chore/modernize-4`

---

## Quick Reference: Current Sprint Priorities

| Tier       | Task ID | Description            | Status                 |
| ---------- | ------- | ---------------------- | ---------------------- |
| 1 CRITICAL | W2.32   | CI Warning Gate        | 📋 Planned             |
| 1 CRITICAL | W2.22   | SHA Digest Pinning     | 📋 Planned             |
| 1 CRITICAL | W3.10   | Package Signing        | ⏸️ BLOCKED (Key Vault) |
| 1 CRITICAL | W5.1    | Security Audit         | 📋 Planned             |
| 2 HIGH     | W3.1    | TFM Expansion          | 📋 Planned             |
| 2 HIGH     | W2.29   | Null Guards            | 📋 Planned             |
| 2 HIGH     | W5.2    | Container Docs         | 📋 Planned             |
| 3 MEDIUM   | W2.33   | NuGet v11.0.0 Publish  | 📋 Planned             |
| 3 MEDIUM   | W5.6    | Migration Guide v10→11 | 📋 Planned             |

---

## Universal Session Start Prompt

Use this prompt to start any session:

````text
# QWIQ Modernization Session

Read the contents of the `.agents` directory before starting work:

1. **FIRST**: Read `.agents/AGENT-INSTRUCTIONS.md` - operational protocol
2. **SECOND**: Read `.agents/HANDOFF.md` - previous session context
3. **THIRD**: Read `.agents/modernize-TODO.md` - task details and current state

## Project Context
- **Repository**: Qwiq - .NET library for Azure DevOps work item queries
- **Branch**: `chore/modernize-4`
- **Target**: Production v11.0.0 for 100+ team members
- **Timeline**: 6-8 weeks
- **Build Status**: 0 errors, 0 warnings (MUST maintain)
- **Test Status**: 208 passed, 1 skipped
- **Code Coverage**: 46.1% (target: 70% for enterprise)

## Session Protocol
1. Create session log: `.agents/sessions/YYYY-MM-DD-session-NN.md`
2. Complete pre-flight checklist from AGENT-INSTRUCTIONS.md
3. Work incrementally with small, conventional commits
4. Check off tasks in modernize-TODO.md as completed
5. Update session log with decisions and challenges

## Build Commands
```powershell
# Build
dotnet build Qwiq.sln -c Release /m:1 /nodeReuse:false

# Test
dotnet test Qwiq.sln -c Release --no-build --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST&TestCategory!=IntegrationTests"

# Lint
dotnet pprettier --write . && dotnet format
````

Continue with tasks from modernize-TODO.md based on current sprint priorities.

````

---

## Universal Session End Prompt

Use this prompt before ending ANY session:

```text
# Session Finalization Checklist

Before ending, complete ALL mandatory steps:

## 1. Documentation Updates
- [ ] All completed tasks checked off in `.agents/modernize-TODO.md`
- [ ] Session log complete at `.agents/sessions/YYYY-MM-DD-session-NN.md`:
  - What was done for each task
  - Decisions made and rationale
  - Challenges encountered and resolutions
  - Files changed and commits made
- [ ] `.agents/HANDOFF.md` updated with:
  - Current state (build/test status)
  - What was completed this session
  - What's next for following session
  - Any blockers or concerns

## 2. Git Operations
- [ ] Stage all documentation: `git add .agents/`
- [ ] Force-add session logs: `git add -f .agents/sessions/*.md`
- [ ] Commit with conventional message

## 3. Verification
- [ ] Lint clean: `dotnet pprettier --write . && dotnet format`
- [ ] Build passes: `dotnet build Qwiq.sln -c Release /m:1 /nodeReuse:false`
- [ ] Tests pass: `dotnet test Qwiq.sln -c Release --no-build --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST&TestCategory!=IntegrationTests"`

## Critical Reminder
The next session has ZERO context except checked-in documentation.
Make documentation complete enough for any agent to continue.
````

---

## Tier 1 CRITICAL: Wave 2 Final Sprint

### W2.32 - CI Warning Gate

````text
# Task: W2.32 - Enforce CI Warning Gate

## Context
Qwiq modernization achieved 0 warnings. This task protects that state by
failing CI builds that introduce new warnings.

## Task Details
- **ID**: W2.32
- **Priority**: CRITICAL (Tier 1)
- **Effort**: S (1-2 hours)
- **Branch**: `chore/modernize-4`

## Requirements
1. Add `/warnaserror` flag to CI build command in `.github/workflows/main.yml`
2. Ensure all target frameworks are covered (net472;net48;net481;net8.0;net9.0;net10.0)
3. Test locally before pushing

## Files to Modify
- `.github/workflows/main.yml` - Add warning-as-error flag to build step

## Acceptance Criteria
- [ ] CI build fails on any warning
- [ ] Build passes with current codebase (0 warnings)
- [ ] All TFMs tested

## Build Commands
```powershell
# Test locally with warnings as errors
dotnet build Qwiq.sln -c Release -warnaserror /m:1 /nodeReuse:false

# Verify test pass
dotnet test Qwiq.sln -c Release --no-build --filter "TestCategory!=localOnly&TestCategory!=Benchmark&TestCategory!=SOAP&TestCategory!=REST&TestCategory!=IntegrationTests"
````

## Documentation

After completing, update:

1. Check off W2.32 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md` with completion status
3. Log in session file

````

### W2.22 - SHA Digest Pinning

```text
# Task: W2.22 - Pin GitHub Actions to SHA Digests

## Context
GitHub Actions using tag-based versions (v4, v5) are vulnerable to supply chain
attacks. Pin all actions to immutable SHA digests for security.

## Task Details
- **ID**: W2.22
- **Priority**: CRITICAL (Tier 1)
- **Effort**: M (2-4 hours)
- **Branch**: `chore/modernize-4`

## Requirements
1. Pin ALL GitHub Actions in ALL workflows to SHA digests
2. Add version comment after each SHA for maintainability
3. Ensure Renovate/Dependabot can update pinned versions

## Files to Review/Modify
- `.github/workflows/main.yml`
- `.github/workflows/release.yml`
- `.github/workflows/dependency-review.yml`
- `.github/workflows/codeql.yml`
- `.github/workflows/dependabot-auto-approve.yml`
- `.github/workflows/dependabot-auto-merge.yml`
- Any other `.github/workflows/*.yml` files

## Pattern
```yaml
# BEFORE (vulnerable)
- uses: actions/checkout@v4

# AFTER (secure)
- uses: actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683 # v4.2.2
````

## Acceptance Criteria

- [ ] All actions pinned to SHA in all workflow files
- [ ] Version comments added for maintainability
- [ ] Renovate config updated to handle SHA updates
- [ ] All workflows still pass

## Documentation

After completing, update:

1. Check off W2.22 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Log in session file

````

### W5.1 - Security Audit Checklist

```text
# Task: W5.1 - Create Security Audit Checklist

## Context
Production deployment for 100+ team members requires passing enterprise security
review. Create comprehensive checklist documenting all security controls.

## Task Details
- **ID**: W5.1
- **Priority**: CRITICAL (Tier 1)
- **Effort**: S-M (2-4 hours)
- **Branch**: `chore/modernize-4`
- **Dependencies**: W2.22 (SHA Pinning), W3.10 (Package Signing - blocked)

## Requirements
Create documentation covering:
1. Supply chain security (SHA pinning, SLSA provenance, SBOM)
2. Code analysis (CodeQL, DevSkim, Gitleaks)
3. Dependency management (vulnerability scanning, license compliance)
4. Secret management procedures
5. Package signing status and plans

## Files to Create
- `docs/security/audit-checklist.md` - Comprehensive security checklist
- Update `SECURITY.md` with compliance summary

## Current Security Controls (Already Implemented)
- ✅ CodeQL integrated (security-extended queries)
- ✅ DevSkim scanning active
- ✅ Gitleaks secret scanning
- ✅ Dependency review with license blocking
- ✅ SLSA Provenance Level 3 in release workflow
- ✅ SBOM generation (dual-pipeline)
- ⏸️ Package signing (BLOCKED - needs Azure Key Vault)

## Acceptance Criteria
- [ ] Security checklist covers all enterprise requirements
- [ ] Supply chain security documented
- [ ] Dependency vulnerability process documented
- [ ] Secret management procedures documented
- [ ] Checklist is self-contained (readable without other docs)

## Documentation
After completing, update:
1. Check off W5.1 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Log in session file
````

---

## Tier 2 HIGH: Framework Modernization

### W3.1 - TFM Expansion

````text
# Task: W3.1 - Expand Target Framework Monikers

## Context
Production deployment requires running in Kubernetes containers on modern .NET.
Expand TFM coverage while maintaining backward compatibility.

## Task Details
- **ID**: W3.1
- **Priority**: HIGH (Tier 2)
- **Effort**: M (4-8 hours)
- **Branch**: `chore/modernize-4`

## Current TFMs
- Core/REST/Identity/Linq/Mapper: `net472;net8.0`
- SOAP projects: `net472` only (Windows SDK constraint)

## Target TFMs
- Core/REST/Identity/Linq/Mapper: `net472;net48;net481;net8.0;net9.0;net10.0`
- SOAP projects: `net472` only (cannot change - SDK hard constraint)
- netstandard2.0: Evaluate if still needed with expanded coverage

## Why These TFMs
| TFM    | Reason                                           |
| ------ | ------------------------------------------------ |
| net472 | Minimum for SOAP SDK (Microsoft.TFS.ExtClient)   |
| net48  | Compiler optimizations, runtime improvements     |
| net481 | Compiler optimizations, latest .NET Framework    |
| net8.0 | LTS until Nov 2026, container support            |
| net9.0 | STS until Nov 2026, current stable               |
| net10.0| LTS until Nov 2028, just released Nov 2025       |

## CANNOT Support
- net462, net47, net471: SDK requires net472+ minimum

## Files to Modify
- `src/Qwiq.Core/Qwiq.Core.csproj`
- `src/Qwiq.Core.Rest/Qwiq.Core.Rest.csproj`
- `src/Qwiq.Identity/Qwiq.Identity.csproj`
- `src/Qwiq.Linq/Qwiq.Linq.csproj`
- `src/Qwiq.Mapper/Qwiq.Mapper.csproj`
- `src/Qwiq.Mapper.Identity/Qwiq.Mapper.Identity.csproj`
- `src/Qwiq.Linq.Identity/Qwiq.Linq.Identity.csproj`
- Test project files (match source TFMs)

## Acceptance Criteria
- [ ] All non-SOAP projects target: net472;net48;net481;net8.0;net9.0;net10.0
- [ ] SOAP projects remain net472 only
- [ ] Build succeeds for all TFMs with 0 warnings
- [ ] Tests pass on all TFMs
- [ ] NuGet packages include all TFMs

## Build Commands
```powershell
# Build all TFMs
dotnet build Qwiq.sln -c Release /m:1 /nodeReuse:false

# Test specific TFM
dotnet test Qwiq.sln -c Release -f net8.0 --no-build --filter "TestCategory!=localOnly"
````

## Documentation

After completing, update:

1. Check off W3.1 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Log in session file

````

### W2.29 - Service Resolution Null Guards

```text
# Task: W2.29 - Add Null Guards for Service Resolution

## Context
Service resolution can return null without guards, causing NullReferenceException
at runtime. Add defensive null checks for production safety.

## Task Details
- **ID**: W2.29
- **Priority**: HIGH (Tier 2)
- **Effort**: M (2-4 hours)
- **Branch**: `chore/modernize-4`

## Requirements
1. Identify all service resolution points that can return null
2. Add null guards with descriptive ArgumentNullException
3. Follow existing null guard patterns in codebase

## Pattern to Follow
```csharp
// Correct pattern (from csharp.instructions.md)
public void Method(SomeType parameter)
{
    if (parameter == null) throw new ArgumentNullException(nameof(parameter));
    // ... rest of method
}

// For constructor base calls
public MyClass(IService service)
    : base(service?.Property ?? throw new ArgumentNullException(nameof(service)))
{
}
````

## Files to Review

- `src/Qwiq.Core/` - Core service resolution
- `src/Qwiq.Core.Rest/` - REST client factory
- `src/Qwiq.Mapper/` - Mapper service resolution

## Known Locations (from codebase analysis)

- Service resolution in factories
- Dependency injection entry points
- Interface implementations that accept nullable parameters

## Acceptance Criteria

- [ ] All service resolution points have null guards
- [ ] Build passes with 0 warnings
- [ ] Tests pass
- [ ] No breaking API changes (guards throw, not return null)

## Documentation

After completing, update:

1. Check off W2.29 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Log in session file

````

### W5.2 - Container Deployment Guide

```text
# Task: W5.2 - Create Container Deployment Documentation

## Context
Production deployment targets Kubernetes. Create comprehensive documentation
for containerizing applications using Qwiq.

## Task Details
- **ID**: W5.2
- **Priority**: HIGH (Tier 2)
- **Effort**: M (4-6 hours)
- **Branch**: `chore/modernize-4`
- **Dependencies**: W3.1 (TFM Expansion to net8.0/net9.0)

## Requirements
Create documentation for:
1. Docker image build instructions
2. Kubernetes deployment YAML examples
3. Resource requirements and recommendations
4. Environment variable configuration
5. Health check patterns (if applicable)

## Files to Create
- `docs/deployment/kubernetes.md` - Kubernetes deployment guide
- `docs/deployment/docker.md` - Docker containerization guide
- `samples/Dockerfile` - Example Dockerfile

## Key Considerations
- REST client is container-ready (no Windows dependencies)
- SOAP client is Windows-only (net472, cannot containerize)
- Document which features work in containers

## Acceptance Criteria
- [ ] Docker build instructions documented
- [ ] Kubernetes deployment YAML examples provided
- [ ] Resource requirements documented
- [ ] Environment configuration guide complete
- [ ] Clear indication of SOAP limitations in containers

## Documentation
After completing, update:
1. Check off W5.2 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Log in session file
````

---

## Tier 3 MEDIUM: Release & Documentation

### W2.33 - NuGet v11.0.0 Publish

```text
# Task: W2.33 - Publish NuGet v11.0.0

## Context
First NuGet release in 7 years! This is a fork of LeCantaloop/Qwiq v10.0.1.
Version 11.0.0 indicates breaking changes from the fork.

## Task Details
- **ID**: W2.33
- **Priority**: MEDIUM (Tier 3) - after security tasks complete
- **Effort**: M (4-6 hours)
- **Branch**: `chore/modernize-4`
- **Dependencies**: W2.32 (CI Gate), W2.22 (SHA Pinning), W3.1 (TFM)

## Pre-Release Checklist
Before publishing, verify:
- [ ] W2.32 CI Warning Gate enabled
- [ ] W2.22 All actions SHA-pinned
- [ ] W3.1 TFM expansion complete
- [ ] All tests pass
- [ ] SLSA provenance configured in release workflow
- [ ] SBOM generation working

## Release Process
1. Create release tag following semantic versioning
2. Trigger release workflow
3. Verify NuGet packages have:
   - Correct version (11.0.0)
   - All TFMs included
   - SLSA attestation
   - SBOM attached

## Files to Verify
- `.github/workflows/release.yml` - Release workflow
- `version.json` - Nerdbank.GitVersioning config
- `Directory.Build.props` - Package metadata

## Acceptance Criteria
- [ ] v11.0.0 packages published to NuGet.org
- [ ] All packages have SLSA provenance
- [ ] SBOM attached to GitHub release
- [ ] Release notes document breaking changes from v10

## Documentation
After completing, update:
1. Check off W2.33 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Update README.md with new version badge
4. Log in session file
```

### W5.6 - Migration Guide v10→v11

```text
# Task: W5.6 - Create Migration Guide from v10 to v11

## Context
Users upgrading from LeCantaloop/Qwiq v10.x need clear migration path to v11.0.0.
Document all breaking changes and provide step-by-step guide.

## Task Details
- **ID**: W5.6
- **Priority**: MEDIUM (Tier 3)
- **Effort**: M (4-6 hours)
- **Branch**: `chore/modernize-4`
- **Dependencies**: W2.33 (v11.0.0 Publish)

## Requirements
Document:
1. All breaking changes from v10 to v11
2. Namespace changes (if any)
3. API changes with before/after examples
4. TFM changes and implications
5. Step-by-step migration checklist

## Files to Create
- `docs/migration/v10-to-v11.md` - Detailed migration guide
- `MIGRATION.md` - Root-level quick reference

## Breaking Changes to Document
- TFM changes (added net8.0+, potentially removed netstandard2.0)
- Any API signature changes
- Nullable reference type annotations
- Package dependencies changes

## Acceptance Criteria
- [ ] All breaking changes documented with examples
- [ ] Before/after code samples provided
- [ ] Step-by-step migration checklist
- [ ] Document is self-contained (no prior knowledge needed)

## Documentation
After completing, update:
1. Check off W5.6 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Log in session file
```

---

## Wave 3: Observability & Configuration

### W3.8 - Observability Overhaul

```text
# Task: W3.8 - Add ILogger and OpenTelemetry Support

## Context
Modern .NET applications expect ILogger integration and OpenTelemetry for
distributed tracing. Add observability hooks without breaking existing code.

## Task Details
- **ID**: W3.8
- **Priority**: HIGH (Tier 2)
- **Effort**: L (8-16 hours)
- **Branch**: `chore/modernize-4`
- **Dependencies**: W3.1 (TFM Expansion)

## Requirements
1. Add ILogger integration for all major operations
2. Add OpenTelemetry activity tracing for queries
3. Structured logging with correlation IDs
4. Backward compatible (existing code without logging still works)

## Files to Create/Modify
- `src/Qwiq.Core/Logging/` - Logging abstractions
- `src/Qwiq.Core/Extensions/ServiceCollectionExtensions.cs` - DI extensions
- Update query execution paths with activity spans

## Design Considerations
- Use Microsoft.Extensions.Logging.Abstractions for ILogger
- Use System.Diagnostics.ActivitySource for OpenTelemetry
- Make logging optional (null logger by default)
- Follow existing patterns in ASP.NET Core libraries

## Acceptance Criteria
- [ ] ILogger integration for major operations
- [ ] OpenTelemetry ActivitySource for query tracing
- [ ] Structured log events with query context
- [ ] No breaking changes to existing API
- [ ] Tests cover logging paths

## Documentation
After completing, update:
1. Check off W3.8 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Create docs for enabling logging
4. Log in session file
```

### W3.9 - IConfiguration Support

````text
# Task: W3.9 - Add IConfiguration Support

## Context
Modern .NET applications use IConfiguration for settings. Add configuration
binding support for connection options.

## Task Details
- **ID**: W3.9
- **Priority**: MEDIUM (Tier 3)
- **Effort**: M (4-8 hours)
- **Branch**: `chore/modernize-4`
- **Dependencies**: W3.8 (Observability)

## Requirements
1. Add configuration binding for AuthenticationOptions
2. Support appsettings.json configuration pattern
3. Backward compatible (manual construction still works)

## Example Configuration Pattern
```json
{
  "Qwiq": {
    "Uri": "https://dev.azure.com/myorg",
    "AuthenticationType": "PersonalAccessToken",
    "Project": "MyProject"
  }
}
````

## Files to Create/Modify

- `src/Qwiq.Core/Configuration/` - Configuration models
- `src/Qwiq.Core/Extensions/` - Configuration extension methods

## Acceptance Criteria

- [ ] AuthenticationOptions bindable from IConfiguration
- [ ] Example configuration in documentation
- [ ] Backward compatible with manual construction
- [ ] Tests cover configuration binding

## Documentation

After completing, update:

1. Check off W3.9 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Log in session file

````

---

## Wave 4: Test Coverage Excellence

### W4.1 - Achieve 70% Code Coverage

```text
# Task: W4.1 - Increase Test Coverage to 70%

## Context
Production deployment for 100+ team members requires high test confidence.
Current coverage is 46.1%, target is 70% for enterprise requirements.

## Task Details
- **ID**: W4.1
- **Priority**: HIGH (Tier 2)
- **Effort**: XL (16-32 hours)
- **Branch**: `chore/modernize-4`

## Current State
- Coverage: 46.1%
- Tests: 208 passed, 1 skipped
- Test framework: MSTest with Shouldly assertions

## Priority Areas for Coverage
1. **LINQ Provider** (highest complexity):
   - WiqlTranslator.cs (397 LOC, 10+ expression handlers)
   - QueryRewriter.cs (160 LOC, ExpressionVisitor)
   - PartialEvaluator.cs (inner classes)
2. **REST Client**: HTTP path coverage
3. **Mapper**: Field mapping edge cases

## Test Patterns
Follow existing patterns in `test/Qwiq.Tests.Common/`:
- Use `ContextSpecification` base class (Given/When/Then)
- Use `MockWorkItem`, `MockRevision` from Qwiq.Mocks
- Use Shouldly for assertions

## Coverage Commands
```powershell
# Run with coverage
dotnet test Qwiq.sln --collect:"XPlat Code Coverage" --settings coverage.runsettings

# Generate HTML report (requires reportgenerator tool)
reportgenerator -reports:artifacts/TestResults/**/coverage.cobertura.xml -targetdir:artifacts/coverage
````

## Acceptance Criteria

- [ ] Overall coverage ≥ 70%
- [ ] LINQ provider coverage ≥ 80% (complexity requires more)
- [ ] All expression visitor paths tested
- [ ] No decrease in existing coverage

## Documentation

After completing, update:

1. Check off W4.1 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Update coverage badge in README
4. Log in session file

````

---

## Wave 2 Remaining: Lower Priority

### W2.3 - Contract Tests

```text
# Task: W2.3 - Create Contract Tests for REST/SOAP Parity

## Context
REST and SOAP clients should return equivalent data for the same queries.
Contract tests verify this parity.

## Task Details
- **ID**: W2.3
- **Priority**: LOW (Tier 3)
- **Effort**: M (4-8 hours)
- **Branch**: `chore/modernize-4`

## Requirements
1. Create tests that execute same query on both REST and SOAP
2. Compare results for equivalence (not necessarily identical)
3. Document known differences

## Known Differences (from codebase analysis)
- REST returns System.AreaLevel1-7 and System.IterationLevel1-7 fields
- SOAP does not return these fields

## Files to Create
- `test/Qwiq.Integration.Tests/ContractTests/` - Parity tests

## Acceptance Criteria
- [ ] Contract tests verify field parity
- [ ] Known differences documented
- [ ] Tests can run against sandbox environment

## Documentation
After completing, update:
1. Check off W2.3 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Log in session file
````

### W2.7 - Update CONTRIBUTING.md

```text
# Task: W2.7 - Update CONTRIBUTING.md

## Context
Contribution guidelines need updating to reflect modern development workflow
and SDK-style project structure.

## Task Details
- **ID**: W2.7
- **Priority**: MEDIUM (Tier 3)
- **Effort**: S (1-2 hours)
- **Branch**: `chore/modernize-4`

## Requirements
Update documentation to include:
1. Modern build commands (dotnet CLI)
2. Test execution with category filters
3. Conventional commit format
4. Code style guidelines (nullable types, etc.)

## Files to Modify
- `CONTRIBUTING.md` - Main contribution guide

## Acceptance Criteria
- [ ] Build instructions current
- [ ] Test instructions with filters
- [ ] Commit format documented
- [ ] Code style requirements documented

## Documentation
After completing, update:
1. Check off W2.7 in `.agents/modernize-TODO.md`
2. Update `.agents/HANDOFF.md`
3. Log in session file
```

---

## Troubleshooting Prompts

### If Agent Forgets Documentation

```text
STOP. Documentation updates are mandatory.

Complete these steps immediately:

1. Check off ALL completed tasks in `.agents/modernize-TODO.md`
2. Update session log at `.agents/sessions/YYYY-MM-DD-session-NN.md`:
   - What was done
   - Decisions made
   - Challenges and resolutions
3. Update `.agents/HANDOFF.md` with current state
4. Commit: `git add .agents/ && git commit -m "docs: update session documentation"`

The next session has ZERO context except documentation.
```

### If Agent Loses Context

```text
Context recovery protocol:

Read these files in order:
1. `.agents/AGENT-INSTRUCTIONS.md` - Process instructions
2. `.agents/HANDOFF.md` - Previous session context
3. `.agents/modernize-TODO.md` - Task details and current state
4. `.agents/sessions/` - Recent session logs (most recent first)

Then continue with current sprint priorities from modernize-TODO.md.
```

### If Build/Tests Fail

````text
Build/test failure recovery:

1. Identify the error:
   ```powershell
   dotnet build Qwiq.sln -c Release /m:1 /nodeReuse:false 2>&1 | Select-String "error"
````

2. Check recent commits:

   ```powershell
   git log --oneline -5
   ```

3. If needed, revert last change:

   ```powershell
   git revert HEAD --no-edit
   ```

4. Document issue and resolution in session log.

CRITICAL: Build must pass with 0 warnings before any commit.

````

### If Package Tests Fail

```text
Package test failure (Verify baseline mismatch):

When NuGet package contents change, baselines need updating:

1. Run package tests to see diff:
   ```powershell
   dotnet test test/Qwiq.Package.Tests/Qwiq.Package.Tests.csproj -c Release
````

2. Review .received._ files vs .verified._ files

3. If changes are expected, accept new baselines:

   ```powershell
   dotnet verify accept -w test/Qwiq.Package.Tests
   ```

4. Commit updated baselines with the package changes.

````

---

## BLOCKED Tasks Reference

### W3.10 - Package Signing (⏸️ BLOCKED)

```text
# Task: W3.10 - NuGet Package Signing

## Status: ⏸️ BLOCKED

## Blocker
Requires Azure Key Vault setup by repository maintainer.

## Prerequisites (for maintainer)
1. Create Azure Key Vault
2. Create code signing certificate
3. Store certificate in Key Vault
4. Create GitHub secret with Key Vault access
5. Update release workflow with signing step

## When Unblocked
Once prerequisites are met, the task involves:
1. Add NuGet signing step to release workflow
2. Configure certificate from Azure Key Vault
3. Verify signed packages

## DO NOT attempt this task until maintainer confirms Key Vault is ready.
````

---

## Document Control

| Version | Date       | Changes                                                                                          |
| ------- | ---------- | ------------------------------------------------------------------------------------------------ |
| 1.0     | 2025-12-06 | Initial prompts                                                                                  |
| 2.0     | 2025-12-12 | Complete rewrite: removed obsolete Phase 2B/2D, added Wave 3-5, tier-based prompts, zero-context |
