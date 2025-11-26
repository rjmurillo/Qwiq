 QWIQ
=======

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/LeCantaloop/Qwiq/blob/master/LICENSE) [![Build status](https://github.com/rjmurillo/Qwiq/actions/workflows/main.yml/badge.svg)](https://github.com/rjmurillo/Qwiq/actions/workflows/main.yml)

[![MyGet](https://img.shields.io/myget/qwiq/v/Qwiq.Core.svg)](https://myget.org/feed/qwiq/package/nuget/Qwiq.Core) [![MyGet](https://img.shields.io/myget/qwiq/vpre/Qwiq.Core.svg)](https://myget.org/feed/qwiq/package/nuget/Qwiq.Core)

QWIQ is a **Q**uick **W**ork **I**tem **Q**uery library for Team Foundation Server and Azure DevOps. If you do a lot of reading or writing of work items, this package is for you!

## Features

- **Easy to consume** - No complex setup, just install the NuGet package
- **Easy to test** - Everything has an interface, easy to mock
- **Easy to understand** - Clean, focused APIs
- **Multi-client** - Supports both SOAP (legacy TFS) and REST (Azure DevOps)
- **LINQ support** - Write queries using LINQ syntax
- **Strong typing** - Map work items to your own classes
- **Identity helpers** - Simplify identity operations

## Requirements

### For Using Qwiq
- **.NET Standard 2.0** or higher
- **.NET Framework 4.6.1** or higher (for .NET Framework apps)
- **.NET 6, 7, or 8** (for modern .NET apps)

### For Building from Source
- **.NET 8 SDK**
- **Git**

?? See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed build instructions.

## Installation

### Stable Releases (NuGet.org)

```
PM> Install-Package Qwiq.Core
PM> Install-Package Qwiq.Client.Soap
```

Or via the UI: [Qwiq.Core](https://www.nuget.org/packages/Qwiq.Core/), [Qwiq.Client.Soap](https://www.nuget.org/packages/Qwiq.Client.Soap/)

### Preview Releases (MyGet)

Add MyGet feed: `https://www.myget.org/F/qwiq/api/v3/index.json`

```
PM> Install-Package Qwiq.Core -Source https://www.myget.org/F/qwiq/api/v3/index.json
```

## Quick Start

### C# Example

```csharp
using Qwiq;
using Qwiq.Credentials;
using Microsoft.VisualStudio.Services.Client;

// Configure authentication
var uri = new Uri("https://dev.azure.com/yourorg/DefaultCollection");
var options = new AuthenticationOptions(uri, AuthenticationTypes.Windows);

// Create work item store
var store = WorkItemStoreFactory.Default.Create(options);

// Execute WIQL query
var items = store.Query(@"
    SELECT [System.Id] 
    FROM WorkItems 
    WHERE [System.WorkItemType] = 'Bug' 
      AND [System.State] = 'Active'");

foreach (var item in items)
{
    Console.WriteLine($"Bug {item.Id}: {item.Title}");
}
```

### PowerShell Example

```powershell
[Reflection.Assembly]::LoadFrom(".\Qwiq.Core.dll")
[Reflection.Assembly]::LoadFrom(".\Qwiq.Client.Soap.dll")

$uri = [Uri]"https://dev.azure.com/yourorg/DefaultCollection"
$options = New-Object Qwiq.Credentials.AuthenticationOptions $uri,Windows
$store = [Qwiq.Client.Soap.WorkItemStoreFactory]::Default.Create($options)

$items = $store.Query("SELECT [System.Id] FROM WorkItems WHERE [System.State] = 'Active'")
```

### Authentication Options

Qwiq supports multiple authentication methods:
- **OAuth2**
- **Personal Access Token (PAT)**
- **Username and password (BASIC)**
- **Windows credentials (NTLM or Federated)**
- **Anonymous**

## Available Packages

- **Qwiq.Core** - Core abstractions and interfaces
- **Qwiq.Client.Soap** - SOAP client for legacy TFS
- **Qwiq.Client.Rest** - REST client for Azure DevOps
- **Qwiq.Identity** - Identity conversion helpers
- **Qwiq.Linq** - LINQ query provider
- **Qwiq.Mapper** - Work item to POCO mapping
- **Qwiq.Mocks** - Test mocks and helpers

## Building from Source

```bash
# Clone and build
git clone https://github.com/rjmurillo/Qwiq.git
cd Qwiq
dotnet tool restore
dotnet build

# Run tests
dotnet test
```

For detailed build and contribution instructions, see [CONTRIBUTING.md](CONTRIBUTING.md).

## Contributing

We welcome contributions! To get started:

1. **Read the guide** - [CONTRIBUTING.md](CONTRIBUTING.md) has everything you need
2. **Fork and clone** the repository
3. **Create a feature branch**
4. **Make your changes** following the coding standards
5. **Submit a pull request**

See [CONTRIBUTING.md](CONTRIBUTING.md) for complete details on:
- Building from source
- Development workflow
- Coding standards
- Testing guidelines
- Commit conventions
- Pull request process

## Documentation

- ?? [Contributing Guide](CONTRIBUTING.md) - Build, develop, and contribute
- ?? [Issue Tracker](https://github.com/rjmurillo/Qwiq/issues) - Report bugs or request features
- ?? [Discussions](https://github.com/rjmurillo/Qwiq/discussions) - Ask questions and share ideas

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) for details.

