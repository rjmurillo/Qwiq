using System.Linq;

namespace Qwiq
{
    internal class WorkItemTypeCollectionComparer : GenericComparer<IWorkItemTypeCollection>
    {
        internal new static WorkItemTypeCollectionComparer Default => Nested.Instance;

        public override bool Equals(IWorkItemTypeCollection? x, IWorkItemTypeCollection? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            // Note: We intentionally don't check x.Count == y.Count here because
            // REST API (x) may return additional work item types that SOAP (y) doesn't expose.
            // We iterate over y (SOAP/expected) and verify all types exist and match in x (REST/actual).
            // This allows REST to have additional types that SOAP doesn't have.

            foreach (var wit in y)
            {
                var witName = wit.Name;
                if (witName == null || !x.Contains(witName)) return false;
                var tw = x[witName];
                if (!WorkItemTypeComparer.Default.Equals(tw, wit)) return false;
            }

            // We don't fail if x (REST) has extra items - REST API can return more types than SOAP
            return true;
        }

        public override int GetHashCode(IWorkItemTypeCollection obj)
        {
            if (ReferenceEquals(obj, null)) return 0;

            // IMPORTANT: The collection must be in the same order to produce the same hash
            var hash = 27;
            foreach (var wit in obj.OrderBy(p => p.Name)) hash = (13 * hash) ^ wit.GetHashCode();
            return hash;
        }

        // ReSharper disable ClassNeverInstantiated.Local
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        private class Nested
        // ReSharper restore ClassNeverInstantiated.Local
        {
            // ReSharper disable MemberHidesStaticFromOuterClass
            internal static readonly WorkItemTypeCollectionComparer Instance = new WorkItemTypeCollectionComparer();

            // ReSharper restore MemberHidesStaticFromOuterClass

            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
            static Nested()
            {
            }
        }
    }
}