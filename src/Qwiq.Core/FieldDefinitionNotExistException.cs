﻿using System;

namespace Microsoft.Qwiq
{
    public class FieldDefinitionNotExistException : InvalidOperationException
    {
        public FieldDefinitionNotExistException(string name)
            : base($"TF26027: A field definition {name} in the work item type definition file does not exist. Add a definition for this field or remove the reference to the field and try again.")
        {

        }
    }
}
