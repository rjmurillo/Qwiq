using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Qwiq.Core.Tests.Mocks
{
    public class MockQuery : IQuery
    {
        public MockQuery(string wiql, bool dayPrecision, ICollection<string> wiqls)
        {
            wiqls.Add(wiql);
        }

        public IEnumerable<IWorkItem> RunQuery()
        {
            return Enumerable.Empty<IWorkItem>();
        }

        public IEnumerable<IWorkItemLinkInfo> RunLinkQuery()
        {
            return Enumerable.Empty<IWorkItemLinkInfo>();
        }

        public IEnumerable<IWorkItemLinkTypeEnd> GetLinkTypes()
        {
            return Enumerable.Empty<IWorkItemLinkTypeEnd>();
        }
    }
}
