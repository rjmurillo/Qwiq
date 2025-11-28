using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.Services.Identity
{
    /// <summary>
    /// A compatibility shim for <c>IdentityTypeMapper</c> which was removed from newer versions
    /// of <c>Microsoft.VisualStudio.Services.Client</c> (v19+).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class provides a mapping between identity type names and their byte IDs, which is
    /// essential for identity serialization/deserialization in Team Foundation Server and Azure DevOps.
    /// </para>
    /// <para>
    /// <strong>Why this exists:</strong> The original <c>IdentityTypeMapper</c> class was part of
    /// the TFS SDK but was removed in version 19.x of <c>Microsoft.VisualStudio.Services.Client</c>.
    /// This shim restores the functionality for compatibility with existing code that depends on it.
    /// </para>
    /// <para>
    /// <strong>When to remove:</strong> This shim can be removed when:
    /// <list type="bullet">
    /// <item><description>Qwiq no longer needs to support TFS SDK v19+ identity type mapping, OR</description></item>
    /// <item><description>Microsoft restores this class to the official SDK</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This class is thread-safe. The singleton instance is created
    /// lazily using <see cref="Lazy{T}"/>, and dictionary modifications are protected by a lock.
    /// </para>
    /// </remarks>
    internal sealed class IdentityTypeMapper
    {
        private static readonly Lazy<IdentityTypeMapper> LazyInstance =
            new Lazy<IdentityTypeMapper>(() => new IdentityTypeMapper());

        /// <summary>
        /// Gets the singleton instance of the <see cref="IdentityTypeMapper"/>.
        /// </summary>
        internal static IdentityTypeMapper Instance => LazyInstance.Value;

        private readonly object _lock = new object();

        // Common identity types based on TFS/Azure DevOps identity system
        // These mappings are based on the original Microsoft implementation
        private readonly Dictionary<byte, string> _idToName = new Dictionary<byte, string>
        {
            { 0, "System.Security.Principal.WindowsIdentity" },
            { 1, "Microsoft.TeamFoundation.Identity" },
            { 2, "Microsoft.TeamFoundation.ServiceIdentity" },
            { 3, "Microsoft.TeamFoundation.UnauthenticatedIdentity" },
            { 4, "Microsoft.IdentityModel.Claims.ClaimsIdentity" },
            { 5, "Microsoft.TeamFoundation.Framework.Server.TeamFoundationApplicationGroup" },
            { 6, "Microsoft.TeamFoundation.GroupIdentity" },
            { 7, "Microsoft.TeamFoundation.BindPendingIdentity" },
            { 8, "Microsoft.TeamFoundation.ImportedIdentity" },
            { 9, "Microsoft.TeamFoundation.AggregateIdentity" },
            { 10, "Microsoft.TeamFoundation.ServerIdentity" },
            { 11, "Microsoft.TeamFoundation.CertificateIdentity" },
            { 12, "Microsoft.TeamFoundation.SystemIdentity" },
            { 13, "Microsoft.TeamFoundation.ServicePrincipal" },
            { 14, "Microsoft.VisualStudio.Services.Identity.AadUser" },
            { 15, "Microsoft.VisualStudio.Services.Identity.MsaUser" },
            { 16, "Microsoft.VisualStudio.Services.Identity.VssUser" },
        };

        private readonly Dictionary<string, byte> _nameToId = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        private IdentityTypeMapper()
        {
            foreach (var kvp in _idToName)
            {
                _nameToId[kvp.Value] = kvp.Key;
            }
        }

        /// <summary>
        /// Gets the identity type name from its byte ID.
        /// </summary>
        /// <param name="typeId">The byte ID of the identity type.</param>
        /// <returns>The identity type name, or a formatted placeholder string if unknown.</returns>
        public string GetTypeNameFromId(byte typeId)
        {
            lock (_lock)
            {
                if (_idToName.TryGetValue(typeId, out var name))
                {
                    return name;
                }
            }

            // For unknown types, return a placeholder
            return $"UnknownIdentityType_{typeId}";
        }

        /// <summary>
        /// Gets the byte ID for an identity type name.
        /// </summary>
        /// <param name="typeName">The identity type name.</param>
        /// <returns>The byte ID of the identity type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the maximum number of identity types (255) has been exceeded.</exception>
        public byte GetTypeIdFromName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            lock (_lock)
            {
                if (_nameToId.TryGetValue(typeName, out var id))
                {
                    return id;
                }

                // Try to register unknown types dynamically
                // Find the next available ID
                byte nextId = (byte)(_idToName.Count);
                while (_idToName.ContainsKey(nextId) && nextId < byte.MaxValue)
                {
                    nextId++;
                }

                if (nextId == byte.MaxValue && _idToName.ContainsKey(nextId))
                {
                    throw new InvalidOperationException("Maximum number of identity types exceeded.");
                }

                // Register the new type
                _idToName[nextId] = typeName;
                _nameToId[typeName] = nextId;

                return nextId;
            }
        }
    }
}
