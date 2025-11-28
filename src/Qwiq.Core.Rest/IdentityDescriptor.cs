using System;

namespace Qwiq.Client.Rest
{
    public class IdentityDescriptor : Qwiq.IdentityDescriptor
    {
        internal IdentityDescriptor(Microsoft.VisualStudio.Services.Identity.IdentityDescriptor descriptor)
            : base(
                (descriptor ?? throw new ArgumentNullException(nameof(descriptor))).IdentityType,
                descriptor.Identifier)
        {
        }
    }
}