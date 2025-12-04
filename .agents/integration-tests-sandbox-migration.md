# Integration Tests Sandbox Migration Plan

## Overview

This document outlines the migration of Qwiq integration tests from the legacy environment to the new `qwiq-sandbox.visualstudio.com/WIT` sandbox environment.

## Sandbox Environment Configuration (Verified)

### Organization Details
- **Organization URL**: `https://qwiq-sandbox.visualstudio.com`
- **Project Name**: `WIT`
- **Project ID**: `0a4c0240-1a67-45de-93db-fc1de9f54ffb`
- **Process Template**: `WIT_TEST`
- **Version Control**: Git

### Team Configuration
- **Default Team**: WIT Team
- **Team ID**: `dc6a7cf3-e433-435c-adc5-89124efbda08`

### Test User
- **Display Name**: Richard Murillo
- **Email/UPN**: `rjmurillo@msn.com`
- **Identity ID**: `9d6ac35f-9834-46a4-aa39-f786fef3a204`
- **Descriptor**: `msa.Mjg2MmQ5MDMtZjk3ZC03MTVhLTk2YjctNTBlNTBjYTk1YzIy`

### Work Items Created

| ID | Type | Title | Purpose | Links |
|----|------|-------|---------|-------|
| 1 | Bug | Integration Test | Basic test work item | None |
| 2 | Task | Child Task for Integration Tests | Child of ID 3 (hierarchy) | Parent: 3 |
| 3 | User Story | Parent Story for Integration Tests | Parent for hierarchy tests | Children: 2, 6 |
| 4 | Bug | Bug for Mapper Integration Tests | Mapper test work item | None |
| 5 | Bug | Work Item with Links for Integration Tests | Link testing | Related: 1, 4 |
| 6 | Task | Child Task 2 for Hierarchy | Second child of ID 3 | Parent: 3 |

### Hierarchy Structure
```
User Story (ID: 3) - "Parent Story for Integration Tests"
├── Task (ID: 2) - "Child Task for Integration Tests"
└── Task (ID: 6) - "Child Task 2 for Hierarchy"
```

## Current State Analysis

### Configuration Files

| File | Current Value | New Value | Status |
|------|---------------|-----------|--------|
| `IntegrationSettings.cs` URL | `https://qwiq-sandbox.visualstudio.com/WIT` | (same) | ✅ Correct |
| `ProjectGuid` | `8d47e068-03c8-4cdc-aa9b-fc6929290322` | `0a4c0240-1a67-45de-93db-fc1de9f54ffb` | ❌ Needs update |
| `TenantId` | `72F988BF-86F1-41AF-91AB-2D7CD011DB47` | Keep or remove | ⚠️ Review |
| `Domains` | `{ "microsoft.com" }` | `{ "msn.com" }` | ❌ Needs update |

### Hardcoded Work Item IDs

| Work Item ID | Files Using It | Purpose |
|--------------|----------------|---------|
| `10726528` | 7 files | Primary test work item |
| `6413554` | 1 file | Work item with links (147 related, 14 external, 8 hyper, 8 attached) |
| `10726623` | 1 file | Large hierarchy parent |
| `8663955` | 1 file | Bug work item for mapper tests |
| `1` | 1 file | Simple work item test |

#### Files with Work Item ID References:

1. **`WorkItemStore/WorkItem/IntegrationContextSpecificationSpecification.cs`**
   - Line 14: `private const int Id = 10726528;`

2. **`WorkItemStore/WorkItem/SingleIdTests.cs`**
   - Line 10: `private const int Id = 10726528;`
   - Line 22: `private const int Id = 10726528;`

3. **`WorkItemStore/WorkItem/MultipleIdTests.cs`**
   - Line 11: `private const int Id = 10726528;`
   - Line 35: `private const int Id = 10726528;`

4. **`WorkItemStore/WorkItem/WiqlFlatQueryTests.cs`**
   - Line 14: `private const int Id = 10726528;`

5. **`WorkItemStore/WiqlHierarchyQueryTests.cs`**
   - Line 19: `[Source].[System.ID] = 10726528`

6. **`WorkItemStore/LargeHierarchyContextSpecification.cs`**
   - Line 12: `Source.[System.Id] IN (10726623)`

7. **`WorkItemStore/WorkItem/WorkItemWithLinksContextSpecification.cs`**
   - Line 9: `private const int Id = 6413554;`

8. **`WorkItemStore/WorkItem/WorkItemTests.cs`**
   - Line 12: `private const int Id = 1;`

9. **`Mapper/WiqlAttributeMapperContextSpecification.cs`**
   - Line 46: `_ids = new[] { 8663955 };`

### Hardcoded Identity References

| Identity | Files Using It | Context |
|----------|----------------|---------|
| `rimuri@microsoft.com` / `rimuri` | 3 files | LINQ queries, identity mapping |
| `pelavall@microsoft.com` / `Peter Lavallee` | 1 file | Display name conversion |
| `jweber@microsoft.com` / `Jason Weber` | 1 file | Display name conversion (multiple identities test) |

#### Files with Identity References:

1. **`WorkItemStore/Linq/LinqTests.cs`**
   - Lines 17-18: `rimuri@microsoft.com`
   - Lines 37-38: `rimuri`

2. **`Identity/Soap/IdentityManagementServiceTests.cs`**
   - Line 19: `rimuri@microsoft.com`

3. **`Identity/IdentityMapperTests.cs`**
   - Lines 20-21: `rimuri` to `rimuri@microsoft.com`

4. **`Mapper/Identity/DisplayNameToAliasConverterTests.cs`**
   - Lines 16, 47: `Peter Lavallee`, `Jason Weber`
   - Lines 57, 89, 109: `pelavall@microsoft.com`
   - Lines 57, 120, 153: `jweber@microsoft.com`

### Project-Specific References

| Reference | Location | Issue |
|-----------|----------|-------|
| `'OS'` project name | `WiqlHierarchyQueryTests.cs:18` | Wrong project - should be `WIT` |
| `"Shared Queries" then "WPT - Web Platform"` | `ProjectTests.cs:78-79` | Query folder does not exist in sandbox |
| `ProjectGuid` | `IntegrationSettings.cs:19` | Needs sandbox project GUID |

## Required Sandbox Configuration

### 1. Project Setup

The sandbox needs a project named `WIT` (or verify the existing URL structure).

### 2. Work Items to Create

#### Basic Test Work Item (replaces ID 10726528)
- Type: Bug or Task
- Title: "Integration Test Work Item"
- State: Active
- Area Path: WIT
- Iteration Path: WIT
- Assigned To: Test user
- Fields: All core fields populated

#### Work Item with Links (replaces ID 6413554)
- Type: Bug
- Title: "Work Item with Multiple Links"
- Related Links: At least 10+
- External Links: At least 5+
- Hyperlinks: At least 5+
- Attachments: At least 5+

#### Hierarchy Parent (replaces ID 10726623)
- Type: Epic or Feature
- Title: "Parent for Hierarchy Tests"
- Children: Create 5-10 child work items of type "Scenario"

#### Simple Work Item (replaces ID 1)
- First work item created in the project

#### Bug for Mapper (replaces ID 8663955)
- Type: Bug
- Title: "Bug for Mapper Tests"
- State: Active

### 3. Users and Identities to Create

The sandbox needs test users that can be resolved:
- A user with UPN pattern for LINQ tests
- Users for display name resolution tests

### 4. Shared Query Structure

Create folder structure:
```
Shared Queries/
  WPT - Web Platform/
    (sample queries)
```

## Implementation Plan

### Phase 1: Sandbox Environment Setup
1. Verify or create WIT project in qwiq-sandbox
2. Get project GUID for IntegrationSettings
3. Create test work items with required characteristics
4. Create query folder structure
5. Document test users available in sandbox

### Phase 2: Update IntegrationSettings.cs
1. Update `ProjectGuid` with sandbox value
2. Update `TenantId` if different from Microsoft tenant
3. Update `Domains` array if needed
4. Consider externalizing settings to environment variables or config file

### Phase 3: Update Test Files
1. Create `TestData.cs` with centralized work item IDs
2. Update all files to use centralized constants
3. Replace hardcoded identity references

### Phase 4: Create Configuration System
1. Add `IntegrationTestSettings.json` for environment-specific values
2. Add support for environment variable overrides
3. Update IntegrationSettings.cs to load from config

### Phase 5: Documentation and Verification
1. Document sandbox setup requirements
2. Create test data seeding script
3. Run full integration test suite
4. Update CI/CD if needed

## Implementation Details

### Centralized Test Data Constants

Create `test/Qwiq.Integration.Tests/TestData.cs`:
```csharp
namespace Qwiq
{
    /// <summary>
    /// Centralized test data constants for integration tests against qwiq-sandbox.visualstudio.com/WIT
    /// </summary>
    public static class TestData
    {
        #region Work Item IDs

        /// <summary>
        /// Basic Bug work item (ID: 1) - "Integration Test"
        /// Replaces legacy ID: 10726528
        /// </summary>
        public const int BasicWorkItemId = 1;

        /// <summary>
        /// Bug with Related links to IDs 1 and 4 (ID: 5) - "Work Item with Links for Integration Tests"
        /// Replaces legacy ID: 6413554
        /// </summary>
        public const int WorkItemWithLinksId = 5;

        /// <summary>
        /// User Story parent with 2 Task children (ID: 3) - "Parent Story for Integration Tests"
        /// Replaces legacy ID: 10726623
        /// </summary>
        public const int HierarchyParentId = 3;

        /// <summary>
        /// Child Task under HierarchyParentId (ID: 2) - "Child Task for Integration Tests"
        /// </summary>
        public const int HierarchyChildId = 2;

        /// <summary>
        /// Bug for mapper tests (ID: 4) - "Bug for Mapper Integration Tests"
        /// Replaces legacy ID: 8663955
        /// </summary>
        public const int MapperBugId = 4;

        #endregion

        #region Test User Identity

        /// <summary>
        /// Test user email/UPN for identity tests
        /// </summary>
        public const string TestUserUpn = "rjmurillo@msn.com";

        /// <summary>
        /// Test user alias (username portion of email)
        /// </summary>
        public const string TestUserAlias = "rjmurillo";

        /// <summary>
        /// Test user display name
        /// </summary>
        public const string TestUserDisplayName = "Richard Murillo";

        #endregion

        #region Project Configuration

        /// <summary>
        /// Project name in Azure DevOps
        /// </summary>
        public const string ProjectName = "WIT";

        #endregion
    }
}
```

### Updated IntegrationSettings.cs

```csharp
using System;
using System.Collections.Generic;

namespace Qwiq
{
    public static class IntegrationSettings
    {
        // Sandbox environment URL
        private static readonly Uri Uri = new Uri(
            Environment.GetEnvironmentVariable("QWIQ_TEST_URL")
            ?? "https://qwiq-sandbox.visualstudio.com/WIT");

        // WIT project GUID in qwiq-sandbox
        public static Guid ProjectGuid = Guid.Parse(
            Environment.GetEnvironmentVariable("QWIQ_PROJECT_GUID")
            ?? "0a4c0240-1a67-45de-93db-fc1de9f54ffb");

        // Azure AD tenant (may not be needed for MSA accounts)
        public static Guid TenantId = Guid.Parse("72F988BF-86F1-41AF-91AB-2D7CD011DB47");

        public static HashSet<string> Domains => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "msn.com"
        };

        public static Uri TfsUri => Uri;
    }
}
```

### Files Requiring Work Item ID Updates

| File | Line | Current ID | New ID/Constant |
|------|------|------------|-----------------|
| `IntegrationContextSpecificationSpecification.cs` | 14 | `10726528` | `TestData.BasicWorkItemId` (1) |
| `SingleIdTests.cs` | 10, 22 | `10726528` | `TestData.BasicWorkItemId` (1) |
| `MultipleIdTests.cs` | 11, 35 | `10726528` | `TestData.BasicWorkItemId` (1) |
| `WiqlFlatQueryTests.cs` | 14 | `10726528` | `TestData.BasicWorkItemId` (1) |
| `WiqlHierarchyQueryTests.cs` | 19 | `10726528` | `TestData.HierarchyParentId` (3) |
| `LargeHierarchyContextSpecification.cs` | 12 | `10726623` | `TestData.HierarchyParentId` (3) |
| `WorkItemWithLinksContextSpecification.cs` | 9 | `6413554` | `TestData.WorkItemWithLinksId` (5) |
| `WorkItemTests.cs` | 12 | `1` | `TestData.BasicWorkItemId` (1) ✅ |
| `WiqlAttributeMapperContextSpecification.cs` | 46 | `8663955` | `TestData.MapperBugId` (4) |

### Files Requiring Identity Updates

| File | Current Value | New Value |
|------|---------------|-----------|
| `LinqTests.cs` | `rimuri@microsoft.com` | `TestData.TestUserUpn` |
| `LinqTests.cs` | `rimuri` | `TestData.TestUserAlias` |
| `IdentityManagementServiceTests.cs` | `rimuri@microsoft.com` | `TestData.TestUserUpn` |
| `IdentityMapperTests.cs` | `rimuri`, `rimuri@microsoft.com` | `TestData.TestUserAlias`, `TestData.TestUserUpn` |
| `DisplayNameToAliasConverterTests.cs` | `Peter Lavallee`, `pelavall@microsoft.com` | `TestData.TestUserDisplayName`, `TestData.TestUserUpn` |

### Files Requiring Project Name Updates

| File | Line | Current Value | New Value |
|------|------|---------------|-----------|
| `WiqlHierarchyQueryTests.cs` | 18 | `'OS'` | `'WIT'` |

## Manual Setup Required

### Query Folder Structure
Create in Azure DevOps UI at https://qwiq-sandbox.visualstudio.com/WIT/_queries:
```
Shared Queries/
└── WPT - Web Platform/
    └── (any sample query)
```

## Notes

- The current tests use `[TestCategory("localOnly")]` which excludes them from CI
- SOAP tests require Windows with .NET Framework 4.7.2
- REST tests can run cross-platform in theory but still excluded from CI
- Tests compare SOAP vs REST implementations for consistency
- The sandbox uses MSA (Microsoft Account) authentication, not Azure AD
- PAT authentication is required for CI/CD scenarios

## PAT Authentication for CI/CD

For CI/CD, use environment variable:
```
AZURE_DEVOPS_EXT_PAT=<your-pat-token>
```

The PAT needs the following scopes:
- Work Items (Read & Write)
- Project and Team (Read)
- Identity (Read)