using System.Runtime.CompilerServices;

namespace Qwiq.Package.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyNupkg.Initialize();
    }
}
