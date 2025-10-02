using System;


namespace Qwiq.Client.Rest
{
    public class IdentityDescriptor : Qwiq.IdentityDescriptor
    {
        internal IdentityDescriptor( Microsoft.VisualStudio.Services.Identity.IdentityDescriptor descriptor)
            : base(descriptor.IdentityType, descriptor.Identifier)
        {
            
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
        }
    }
}