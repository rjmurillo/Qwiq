using System;


namespace Qwiq
{
    public class RegisteredLinkType : IRegisteredLinkType
    {
        public RegisteredLinkType( string name)
        {
            Name = name != null ? string.Intern(name) : throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
    }
}