using System.Collections.Generic;
using System.Linq;


namespace Qwiq.Mocks
{
    public class MockRevision : Revision
    {
        public MockRevision( Dictionary<string, object> dictionary, int index)
            :base(new MockFieldDefinitionCollection(dictionary.Keys.Select(MockFieldDefinition.Create)), index)
        {

            foreach (var kvp in dictionary)
            {
                var fd = FieldDefinitions[kvp.Key];
                SetFieldValue(fd.Id, kvp.Value);
            }
        }

        public MockRevision( Dictionary<string, object> dictionary)
            : this(dictionary, (int)dictionary["Index"])
        {
           
        }
    }
}
