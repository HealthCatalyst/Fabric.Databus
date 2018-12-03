// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlJobQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SqlJobQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Queues;

    /// <summary>
    /// The sql job queue item.
    /// </summary>
    public class SqlJobQueueItem : IQueueItem
    {
        /// <summary>
        /// Gets or sets the query id.
        /// </summary>
        public string QueryId { get; set; }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the job.
        /// </summary>
        public IJob Job { get; set; }
    }
}
