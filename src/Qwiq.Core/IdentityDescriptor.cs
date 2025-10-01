using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Services.Common;

namespace Qwiq
{
    /// <summary>
    /// Simple identity type mapper to replace the legacy IdentityTypeMapper
    /// </summary>
    internal static class SimpleIdentityTypeMapper
    {
        private static readonly Dictionary<string, byte> TypeNameToId = new Dictionary<string, byte>
        {
            { "Microsoft.IdentityModel.Claims.ClaimsIdentity", 1 },
            { "Microsoft.TeamFoundation.Identity", 2 },
            { "Microsoft.TeamFoundation.ServiceIdentity", 3 },
            { "System.Security.Principal.WindowsIdentity", 4 }
        };

        private static readonly Dictionary<byte, string> TypeIdToName = new Dictionary<byte, string>
        {
            { 1, "Microsoft.IdentityModel.Claims.ClaimsIdentity" },
            { 2, "Microsoft.TeamFoundation.Identity" },
            { 3, "Microsoft.TeamFoundation.ServiceIdentity" },
            { 4, "System.Security.Principal.WindowsIdentity" }
        };

        public static byte GetTypeIdFromName(string typeName)
        {
            return TypeNameToId.TryGetValue(typeName, out var id) ? id : (byte)0;
        }

        public static string GetTypeNameFromId(byte typeId)
        {
            return TypeIdToName.TryGetValue(typeId, out var name) ? name : string.Empty;
        }
    }

    public class IdentityDescriptor : IIdentityDescriptor, IComparable<IdentityDescriptor>, IEquatable<IdentityDescriptor>
    {
        [NotNull] private string _identifier;
        [NotNull] private string _identityType;

        /// <summary>
        /// </summary>
        /// <param name="identityType"></param>
        /// <param name="identifier"></param>
        /// <example>
        ///     User:
        ///     "Microsoft.IdentityModel.Claims.ClaimsIdentity", "2fa3a376-370f-4226-9fbb-d778e4b5bf74\\ftotten@fabrikam.com"
        ///     Service:
        ///     "Microsoft.TeamFoundation.ServiceIdentity",
        ///     "d9454f90-6587-4699-9357-3e83e331580a:Build:f2200ea9-52cf-4343-8c80-af2cfa409984"
        ///     TFS Identity:
        ///     "Microsoft.TeamFoundation.Identity",
        ///     "S-1-9-1234567890-1234567890-123456789-1234567890-1234567890-1-1234567890-1234567890-1234567890-1234567890"
        /// </example>
        public IdentityDescriptor([NotNull] string identityType, [NotNull] string identifier)
        {
            IdentityType = identityType;
            Identifier = identifier;
        }

        public string Identifier
        {
            get => _identifier;
            private set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
                if (value.Length > IdentityConstants.MaxIdLength) throw new ArgumentOutOfRangeException(nameof(value));
                _identifier = value;
            }
        }

        public string IdentityType
        {
            get => _identityType;
            private set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
                if (value.Length > IdentityConstants.MaxTypeLength) throw new ArgumentOutOfRangeException(nameof(value));
                _identityType = value;
                IdentityTypeId = SimpleIdentityTypeMapper.GetTypeIdFromName(value);
            }
        }

        protected internal byte IdentityTypeId { get; private set; }

        public int CompareTo(IdentityDescriptor other)
        {
            if (this == other) return 0;
            if (this == null && other != null) return -1;
            if (this != null && other == null) return 1;

            var num = 0;
            if (IdentityTypeId > other.IdentityTypeId) num = 1;
            else if (IdentityTypeId < other.IdentityTypeId) num = -1;

            if (num == 0) num = StringComparer.OrdinalIgnoreCase.Compare(Identifier, other.Identifier);
            return num;
        }

        public override int GetHashCode()
        {
            return IdentityTypeId ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Identifier);
        }

        /// <inheritdoc />
        public bool Equals(IdentityDescriptor other)
        {
            return CompareTo(other) == 0;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as IdentityDescriptor);
        }

        public override string ToString()
        {
            return IdentityType + ";" + _identifier;
        }
    }
}