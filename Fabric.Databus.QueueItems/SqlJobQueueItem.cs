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

        public int BatchNumber { get; set; }

        public int TotalBatches { get; set; }

        /// <summary>
        /// Gets or sets the job.
        /// </summary>
        public IJob Job { get; set; }
    }
}
