# Mutation Testing Baseline - Qwiq.Core

## Overview

This document tracks mutation testing baselines for the Qwiq project using Stryker.NET.

## Configuration Summary

| Setting           | Value             | Rationale                                   |
| ----------------- | ----------------- | ------------------------------------------- |
| Tool              | Stryker.NET 4.8.1 | De facto standard for .NET mutation testing |
| Target Framework  | net8.0            | Cross-platform CI, modern runtime           |
| Mutation Level    | Standard          | Balanced coverage vs execution time         |
| Coverage Analysis | perTest           | Only run relevant tests per mutant          |
| Threshold Break   | 0                 | Baseline mode - establish metrics first     |

## Expected Initial Baseline

Based on multi-agent analysis (Plan, C# Expert, Architecture, Independent Thinker):

| Metric            | Expected Range | Notes                                             |
| ----------------- | -------------- | ------------------------------------------------- |
| Mutation Score    | 40-55%         | Coverage (71%) != mutation score; expect ~20% gap |
| Mutants Generated | 800-1,500      | Standard level on ~5,000 LOC                      |
| Execution Time    | 15-45 minutes  | Depends on test concurrency                       |

## Why Expect Lower Than Coverage

1. **Assertion weakness**: Tests verify output but not intermediate calculations
2. **Branch coverage gaps**: Some branches covered but not all paths tested
3. **Equivalent mutants**: Some mutations produce semantically identical code
4. **String/null handling**: Tests may not distinguish null vs empty

## Baseline Data

| Date       | Score      | Killed | Survived | Timeout | No Coverage | Run Time |
| ---------- | ---------- | ------ | -------- | ------- | ----------- | -------- |
| 2025-12-13 | **43.96%** | 656    | 354      | 17      | 504         | 11 min   |

### High Performers (≥75%)

| File                              | Score   | Notes                     |
| --------------------------------- | ------- | ------------------------- |
| WorkItemTypeCollection.cs         | 100%    | All mutants killed        |
| TeamFoundationIdentityComparer.cs | 100%    | All mutants killed        |
| IWorkItemLinkType.Extensions.cs   | 100%    | All mutants killed        |
| CoreFieldRefNames.cs              | 97.14%  | 1 no coverage mutant      |
| ExceptionMapper.cs                | 79.17%  | Good exception handling   |
| WorkItemLinkInfo.cs               | 77.78%  | Solid test coverage       |
| FieldCollection.cs                | 75.47%  | Well-tested collection    |

### Priority Improvements (Low scores, high impact)

| File                        | Score  | Survived | No Coverage | Priority |
| --------------------------- | ------ | -------- | ----------- | -------- |
| IWorkItem.Extensions.cs     | 0%     | 0        | 47          | HIGH     |
| CredentialsFactory.cs       | 0%     | 0        | 32          | HIGH     |
| GenericComparer.cs          | 31.91% | 14       | 18          | HIGH     |
| IdentityFieldValue.cs       | 49%    | 26       | 25          | MEDIUM   |
| TypeParser.cs               | 57.75% | 21       | 9           | MEDIUM   |
| WorkItemLinkTypeComparer.cs | 18.75% | 23       | 3           | MEDIUM   |

## Threshold Progression Plan

| Phase    | Timeline  | Break      | Low | High | Goal               |
| -------- | --------- | ---------- | --- | ---- | ------------------ |
| Baseline | Week 1-2  | 0          | 50  | 70   | Establish metrics  |
| Ratchet  | Week 3-8  | baseline-5 | 55  | 75   | Prevent regression |
| Target   | Week 9-16 | 55         | 60  | 75   | Production quality |

## How to Run Locally

```powershell
# Restore tools
dotnet tool restore

# Run mutation testing (full)
dotnet stryker --config-file stryker-config.json

# Run on specific files (faster)
dotnet stryker --config-file stryker-config.json --mutate "src/Qwiq.Core/Comparers/**/*.cs"

# View report
Start-Process "StrykerOutput/reports/mutation-report.html"
```

## How to Trigger CI Run

1. Go to Actions tab in GitHub
2. Select "Main build" workflow
3. Click "Run workflow"
4. Check "Run mutation testing (slow, ~30 min)"
5. Click "Run workflow"

Or wait for the weekly scheduled run (Monday 2:30 AM UTC).

## Analysis Guidelines

When reviewing mutation reports:

1. **Focus on survived mutants** - These indicate weak test assertions
2. **Ignore equivalent mutants** - Some mutations don't change behavior
3. **Prioritize by file** - Focus on high-value business logic first
4. **Don't chase 100%** - Diminishing returns above 80%

## Document History

- 2025-12-13: Initial creation (Session 35)
