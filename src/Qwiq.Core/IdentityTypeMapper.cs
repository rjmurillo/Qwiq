using System.Collections.Generic;

namespace Qwiq
{
    internal class IdentityTypeMapper
    {
        private static readonly Dictionary<string, byte> TypeNameToId = new Dictionary<string, byte>
        {
            { "Microsoft.IdentityModel.Claims.ClaimsIdentity", 1 },
            { "Microsoft.TeamFoundation.Identity", 2 },
            { "Microsoft.TeamFoundation.ServiceIdentity", 3 },
            { "System.Security.Principal.WindowsIdentity", 4 },
            { "Microsoft.TeamFoundation.UnauthenticatedIdentity", 5 }
        };

        private static readonly Dictionary<byte, string> TypeIdToName = new Dictionary<byte, string>
        {
            { 1, "Microsoft.IdentityModel.Claims.ClaimsIdentity" },
            { 2, "Microsoft.TeamFoundation.Identity" },
            { 3, "Microsoft.TeamFoundation.ServiceIdentity" },
            { 4, "System.Security.Principal.WindowsIdentity" },
            { 5, "Microsoft.TeamFoundation.UnauthenticatedIdentity" }
        };

        private static readonly IdentityTypeMapper _instance = new IdentityTypeMapper();

        public static IdentityTypeMapper Instance => _instance;

        private IdentityTypeMapper() { }

        public string GetTypeNameFromId(byte id)
        {
            return TypeIdToName.TryGetValue(id, out var name) ? name : "Unknown";
        }

        public byte GetTypeIdFromName(string name)
        {
            return TypeNameToId.TryGetValue(name, out var id) ? id : (byte)0;
        }
    }
}
