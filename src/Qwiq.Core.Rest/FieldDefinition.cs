using System;
using System.Diagnostics.Contracts;


using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Qwiq.Client.Rest
{
    internal class FieldDefinition : Qwiq.FieldDefinition
    {
        internal FieldDefinition( WorkItemFieldReference field)
            :base(field.ReferenceName, field.Name)
        {
            Contract.Requires(field != null);

            if (field == null) throw new ArgumentNullException(nameof(field));
        }
    }
}