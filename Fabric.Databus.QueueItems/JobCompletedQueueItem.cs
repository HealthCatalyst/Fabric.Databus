// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobCompletedQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JobCompletedQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The job completed queue item.
    /// </summary>
    public class JobCompletedQueueItem : IJobCompletedQueueItem
    {
        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <inheritdoc />
        public int BatchNumber { get; set; }

        public int TotalBatches { get; set; }

        /// <inheritdoc />
        public int NumberOfEntities { get; set; }

        /// <inheritdoc />
        public int NumberOfEntitiesUploaded { get; set; }
    }
}
