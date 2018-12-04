// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlJobQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SqlJobQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The sql job queue item.
    /// </summary>
    public class SqlJobQueueItem : IQueueItem
    {
        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the job.
        /// </summary>
        public IJob Job { get; set; }
    }
}
