using System.Collections.Generic;
using System.Linq;

using Qwiq.Identity.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Should;

namespace Qwiq.Identity.Soap
{
    [TestClass]
    public class when_ReadIdentities_using_a_search_factor_for_an_identity_that_doesnt_exist : IdentityManagementServiceContextSpecification<KeyValuePair<string, IEnumerable<ITeamFoundationIdentity>>>
    {
        private static readonly string[] SearchValues = { "I Do Not Exist" };

        public override void When()
        {
            Actual = Service.ReadIdentities(IdentitySearchFactor.AccountName, SearchValues);
        }

        [TestMethod]
        [TestCategory("SOAP")]
        public void a_null_is_returned_instead_of_a_TeamFoundationIdentity()
        {
            Actual.Single().Value.Single().ShouldBeNull();
        }
    }

    [TestClass]
    public class when_ReadIdentities_using_an_IIdentityDescriptor_for_an_identity_that_doesnt_exist :
        IdentityManagementServiceContextSpecification<ITeamFoundationIdentity>
    {
        private static readonly IIdentityDescriptor[] Descriptors = { new MockIdentityDescriptor() };

        public override void When()
        {
            Actual = Service.ReadIdentities(Descriptors);
        }

        [TestMethod]
        [TestCategory("SOAP")]
        public void a_null_is_returned_instead_of_a_TeamFoundationIdentity()
        {
            Actual.Single().ShouldBeNull();
        }
    }
}

