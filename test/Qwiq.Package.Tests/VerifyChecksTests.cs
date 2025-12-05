using System.Threading.Tasks;
using VerifyTests;
using Xunit;

namespace Qwiq.Package.Tests;

public partial class VerifyChecksTests
{
    [Fact]
    public Task Run() =>
        VerifyChecks.Run();
}
