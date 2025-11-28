using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.Services.Identity
{
    /// <summary>
    /// A compatibility shim for IdentityTypeMapper which was removed from newer versions
    /// of Microsoft.VisualStudio.Services.Client (v19+).
    /// This provides a mapping between identity type names and their byte IDs.
    /// </summary>
    internal sealed class IdentityTypeMapper
    {
        private static readonly Lazy<IdentityTypeMapper> LazyInstance =
            new Lazy<IdentityTypeMapper>(() => new IdentityTypeMapper());

        public static IdentityTypeMapper Instance => LazyInstance.Value;

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
        /// <returns>The identity type name, or a formatted string if unknown.</returns>
        public string GetTypeNameFromId(byte typeId)
        {
            if (_idToName.TryGetValue(typeId, out var name))
            {
                return name;
            }

            // For unknown types, return a placeholder
            return $"UnknownIdentityType_{typeId}";
        }

        /// <summary>
        /// Gets the byte ID for an identity type name.
        /// </summary>
        /// <param name="typeName">The identity type name.</param>
        /// <returns>The byte ID of the identity type.</returns>
        /// <exception cref="ArgumentException">Thrown when the type name is not recognized.</exception>
        public byte GetTypeIdFromName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName));
            }

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
