# .NET 10 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 10 SDK required for this upgrade is installed on the machine and if not, help to get it installed
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10 upgrade
3. Upgrade src\Qwiq.Core\Qwiq.Core.csproj
4. Upgrade src\Qwiq.Linq\Qwiq.Linq.csproj
5. Upgrade src\Qwiq.Identity\Qwiq.Identity.csproj
6. Upgrade test\Qwiq.Tests.Common\Qwiq.Tests.Common.csproj
7. Upgrade src\Qwiq.Mapper\Qwiq.Mapper.csproj
8. Upgrade src\Qwiq.Core.Rest\Qwiq.Client.Rest.csproj
9. Upgrade test\Qwiq.Mocks\Qwiq.Mocks.csproj
10. Upgrade test\Qwiq.Benchmark\Qwiq.Benchmark.csproj
11. Upgrade src\Qwiq.Mapper.Identity\Qwiq.Mapper.Identity.csproj
12. Upgrade src\Qwiq.Linq.Identity\Qwiq.Linq.Identity.csproj
13. Upgrade test\Qwiq.Integration.Tests\Qwiq.IntegrationTests.csproj
14. Upgrade test\Qwiq.Mapper.Benchmark.Tests\Qwiq.Mapper.BenchmarkTests.csproj
15. Upgrade test\Qwiq.Identity.Benchmark.Tests\Qwiq.Identity.BenchmarkTests.csproj
16. Upgrade test\Qwiq.Identity.Tests\Qwiq.Identity.UnitTests.csproj
17. Upgrade test\Qwiq.Linq.Tests\Qwiq.Linq.UnitTests.csproj
18. Upgrade test\Qwiq.Mapper.Tests\Qwiq.Mapper.UnitTests.csproj
19. Upgrade test\Qwiq.Core.Tests\Qwiq.Core.UnitTests.csproj

## Settings

This section contains settings and data used by execution steps.

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                       | Current Version | New Version | Description                                           |
|:-----------------------------------|:---------------:|:-----------:|:------------------------------------------------------|
| Newtonsoft.Json                    | 13.0.3          | 13.0.4      | Recommended package update for .NET 10                |
| System.Threading.Tasks.Dataflow    | 8.0.1           | 10.0.0      | Recommended package update for .NET 10                |

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### src\Qwiq.Core\Qwiq.Core.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `netstandard2.0;net8.0` to `netstandard2.0;net8.0;net10.0`

NuGet packages changes:
  - Newtonsoft.Json should be updated from `13.0.3` to `13.0.4` (*recommended for .NET 10*)

#### src\Qwiq.Linq\Qwiq.Linq.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `netstandard2.0;net8.0` to `netstandard2.0;net8.0;net10.0`

#### src\Qwiq.Identity\Qwiq.Identity.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `netstandard2.0;net8.0` to `netstandard2.0;net8.0;net10.0`

NuGet packages changes:
  - Newtonsoft.Json should be updated from `13.0.3` to `13.0.4` (*recommended for .NET 10*)

#### test\Qwiq.Tests.Common\Qwiq.Tests.Common.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### src\Qwiq.Mapper\Qwiq.Mapper.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `netstandard2.0;net8.0` to `netstandard2.0;net8.0;net10.0`

#### src\Qwiq.Core.Rest\Qwiq.Client.Rest.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `netstandard2.0;net8.0` to `netstandard2.0;net8.0;net10.0`

NuGet packages changes:
  - Newtonsoft.Json should be updated from `13.0.3` to `13.0.4` (*recommended for .NET 10*)
  - System.Threading.Tasks.Dataflow should be updated from `8.0.1` to `10.0.0` (*recommended for .NET 10*)

#### test\Qwiq.Mocks\Qwiq.Mocks.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Newtonsoft.Json should be updated from `13.0.3` to `13.0.4` (*recommended for .NET 10*)

#### test\Qwiq.Benchmark\Qwiq.Benchmark.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### src\Qwiq.Mapper.Identity\Qwiq.Mapper.Identity.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `netstandard2.0;net8.0` to `netstandard2.0;net8.0;net10.0`

#### src\Qwiq.Linq.Identity\Qwiq.Linq.Identity.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `netstandard2.0;net8.0` to `netstandard2.0;net8.0;net10.0`

#### test\Qwiq.Integration.Tests\Qwiq.IntegrationTests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### test\Qwiq.Mapper.Benchmark.Tests\Qwiq.Mapper.BenchmarkTests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### test\Qwiq.Identity.Benchmark.Tests\Qwiq.Identity.BenchmarkTests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### test\Qwiq.Identity.Tests\Qwiq.Identity.UnitTests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### test\Qwiq.Linq.Tests\Qwiq.Linq.UnitTests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### test\Qwiq.Mapper.Tests\Qwiq.Mapper.UnitTests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### test\Qwiq.Core.Tests\Qwiq.Core.UnitTests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Newtonsoft.Json should be updated from `13.0.3` to `13.0.4` (*recommended for .NET 10*)
