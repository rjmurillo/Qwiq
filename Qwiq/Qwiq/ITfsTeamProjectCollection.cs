﻿namespace Microsoft.IE.Qwiq
{
    public interface ITfsTeamProjectCollection
    {
        IIdentityManagementService IdentityManagementService { get; }
        ICommonStructureService ICommonStructureService { get; }
    }
}
