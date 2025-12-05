// Polyfill for ArgumentNullException.ThrowIfNull for .NET Framework and .NET Standard 2.0
//
// This file provides a compatibility shim for ArgumentNullException.ThrowIfNull which is only available
// in .NET 6+. For net472 and netstandard2.0 targets, we provide our own implementation.

#if NETFRAMEWORK || NETSTANDARD2_0

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    // Polyfill CallerArgumentExpression attribute
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

namespace System
{
    internal static partial class ArgumentNullException
    {
        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static void ThrowIfNull(object? argument, [Runtime.CompilerServices.CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}

#endif
