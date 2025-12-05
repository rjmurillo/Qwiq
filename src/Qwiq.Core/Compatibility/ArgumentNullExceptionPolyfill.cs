// Polyfill for ArgumentNullException.ThrowIfNull for .NET Framework and .NET Standard 2.0
//
// This file provides a compatibility shim for ArgumentNullException.ThrowIfNull which is only available
// in .NET 6+. For net472 and netstandard2.0 targets, we provide our own implementation using C# 14
// extension blocks.
//
// Implementation based on: https://github.com/SimonCropp/Polyfill
// We cannot use the Polyfill NuGet package directly due to conflicts with VSS Client polyfills.
//
// Requires: .NET 9+ SDK (for C# 14 compiler) with LangVersion=preview in Directory.Build.props

#if NETFRAMEWORK || NETSTANDARD2_0

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    // Polyfill CallerArgumentExpression attribute for older frameworks
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}

#endif

#if !NET6_0_OR_GREATER

// ReSharper disable once CheckNamespace
namespace System
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Polyfill extension for <see cref="ArgumentNullException"/> to provide ThrowIfNull on older frameworks.
    /// </summary>
    internal static class ArgumentNullExceptionPolyfill
    {
        extension(ArgumentNullException)
        {
            /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
            /// <param name="argument">The reference type argument to validate as non-null.</param>
            /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
            public static void ThrowIfNull(
                [NotNull] object? argument,
                [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            {
                if (argument is null)
                {
                    throw new ArgumentNullException(paramName);
                }
            }
        }
    }
}

#endif
