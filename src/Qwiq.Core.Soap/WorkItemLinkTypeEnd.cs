using System;


using Tfs = Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Qwiq.Client.Soap
{
    public class WorkItemLinkTypeEnd : Qwiq.WorkItemLinkTypeEnd, IIdentifiable<int>
    {
        internal WorkItemLinkTypeEnd( Tfs.WorkItemLinkTypeEnd end)
            : base(end.ImmutableName, new Lazy<IWorkItemLinkTypeEnd>(() => new WorkItemLinkTypeEnd(end.OppositeEnd)))
        {

            if (end == null) throw new ArgumentNullException(nameof(end));
            Id = end.Id;
            LinkType = new WorkItemLinkType(end.LinkType);
            IsForwardLink = end.IsForwardLink;
            Name = end.Name;
        }

        /// <inheritdoc />
        public int Id { get; }
    }
}