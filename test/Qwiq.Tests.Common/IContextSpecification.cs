using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qwiq.Tests.Common
{
    public interface IContextSpecification
    {
        void TestInitialize();

        void TestCleanup();

        void Given();

        void When();

        void Cleanup();
    }
}
