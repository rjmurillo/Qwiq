using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Qwiq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should.Core.Assertions;

namespace Should
{
    [DebuggerStepThrough]
    public static class ShouldExtensions
    {
        public static void ShouldContainOnly<T>(this IEnumerable<T> collection, IEnumerable<T> expected)
        {
            ShouldContainOnly(collection, expected, GenericComparer<T>.Default);
        }

        public static void ShouldContainOnly<T>(this IEnumerable<T> collection, params T[] expected)
        {
            ShouldContainOnly(collection, expected, GenericComparer<T>.Default);
        }

        public static void ShouldContainOnly<T>(this IEnumerable<T> collection, IEnumerable<T> expected, IEqualityComparer<T> comparer)
        {
            var source = new List<T>(collection);
            var noContain = new List<T>();

            foreach (var item in expected)
            {
                if (!source.Contains(item, comparer)) noContain.Add(item);
                else source.Remove(item);
            }

            if (noContain.Any() || source.Any())
            {
                var message = $"Should contain only: {string.Join(", ", expected)} \r\nentire list: {string.Join(", ", collection)}";

                if (noContain.Any()) message += "\ndoes not contain: " + string.Join(", ", noContain);

                if (source.Any()) message += "\ndoes contain but shouldn't: " + string.Join(", ", source);

                throw new AssertFailedException(message);
            }
        }
    }
}