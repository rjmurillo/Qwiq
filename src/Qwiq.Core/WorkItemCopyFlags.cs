using System;

namespace Qwiq
{
    /// <summary>
    /// Flags specifying optional work item data that should be copied.
    /// </summary>
    /// <remarks>
    /// This enum mirrors the values from the TFS Client OM's WorkItemCopyFlags.
    /// </remarks>
    [Flags]
    public enum WorkItemCopyFlags
    {
        None = 0,
        CopyFiles = 1,
        CopyLinks = 2,
    }
}
