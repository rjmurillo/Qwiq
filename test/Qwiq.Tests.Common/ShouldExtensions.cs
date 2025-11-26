using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Qwiq;

namespace Should
{
    /// <summary>
    /// Compatibility extensions to migrate from Should library to FluentAssertions.
    /// This allows existing tests to work with FluentAssertions without changing test code.
    /// </summary>
    [DebuggerStepThrough]
    public static class ShouldExtensions
    {
        public static void ShouldEqual<T>(this T actual, T expected)
        {
            actual.Should().Be(expected);
        }

        public static void ShouldEqual<T>(this T actual, T expected, string message)
        {
            actual.Should().Be(expected, message);
        }

        public static void ShouldNotEqual<T>(this T actual, T expected)
        {
            actual.Should().NotBe(expected);
        }

        public static void ShouldBeNull<T>(this T actual) where T : class
        {
            actual.Should().BeNull();
        }

        public static void ShouldNotBeNull<T>(this T actual) where T : class
        {
            actual.Should().NotBeNull();
        }

        public static void ShouldBeTrue(this bool actual)
        {
            actual.Should().BeTrue();
        }

        public static void ShouldBeFalse(this bool actual)
        {
            actual.Should().BeFalse();
        }

        public static void ShouldBeType<T>(this object actual)
        {
            actual.Should().BeOfType<T>();
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection, T expected)
        {
            collection.Should().Contain(expected);
        }

        public static void ShouldContainOnly<T>(this IEnumerable<T> collection, IEnumerable<T> expected)
        {
            collection.Should().BeEquivalentTo(expected);
        }

        public static void ShouldContainOnly<T>(this IEnumerable<T> collection, params T[] expected)
        {
            collection.Should().BeEquivalentTo(expected);
        }

        public static void ShouldContainOnly<T>(this IEnumerable<T> collection, IEnumerable<T> expected, IEqualityComparer<T> comparer)
        {
            using (new AssertionScope())
            {
                var source = new List<T>(collection);
                var expectedList = expected.ToList();
                var noContain = new List<T>();

                foreach (var item in expectedList)
                {
                    var found = source.FirstOrDefault(s => comparer.Equals(s, item));
                    if (found == null || !comparer.Equals(found, default(T)))
                    {
                        if (found != null) source.Remove(found);
                        else noContain.Add(item);
                    }
                    else
                    {
                        noContain.Add(item);
                    }
                }

                if (noContain.Any())
                {
                    Execute.Assertion.FailWith($"Expected collection to contain {string.Join(", ", expectedList)}, but could not find {string.Join(", ", noContain)}");
                }

                if (source.Any())
                {
                    Execute.Assertion.FailWith($"Expected collection to only contain {string.Join(", ", expectedList)}, but also found {string.Join(", ", source)}");
                }
            }
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> collection)
        {
            collection.Should().BeEmpty();
        }

        public static void ShouldNotBeEmpty<T>(this IEnumerable<T> collection)
        {
            collection.Should().NotBeEmpty();
        }
    }
}
