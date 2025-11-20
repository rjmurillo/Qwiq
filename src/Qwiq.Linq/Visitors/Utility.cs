using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Qwiq.Linq.Visitors
{
    public static class Utility
    {
        public static string ToConcatenatedString<T>(this IEnumerable<T> source, Func<T, string> selector, string separator)
        {
            var b = new StringBuilder();
            var needSeparator = false;

            foreach (var item in source)
            {
                if (needSeparator) b.Append(separator);

                b.Append(selector(item));
                needSeparator = true;
            }

            return b.ToString();
        }

        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> source)
        {
            return new LinkedList<T>(source);
        }

        /// <summary>
        ///     Creates a SHA-256 fingerprint of the string.
        /// </summary>
        /// <remarks>
        ///     This method uses SHA-256 instead of MD5 for stronger cryptographic security.
        ///     The implementation is optimized for low allocations by:
        ///     - Reusing a static SHA256 instance
        ///     - Avoiding ToCharArray() allocation by passing the string directly
        ///     - Using a pre-sized StringBuilder
        /// </remarks>
        public static string ToSha256Fingerprint(this string s)
        {
            // SHA256 produces 32 bytes, which becomes 64 hex characters
            var bytes = Encoding.Unicode.GetBytes(s);
            var hash = SHA256.HashData(bytes);

            // Pre-allocate StringBuilder with exact capacity (32 bytes * 2 chars per byte)
            var sb = new StringBuilder(64);
            foreach (var b in hash)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}