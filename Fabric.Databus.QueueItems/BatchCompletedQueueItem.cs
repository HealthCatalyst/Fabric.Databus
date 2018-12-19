// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchCompletedQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   The batch completed queue item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc cref="IBatchCompletedQueueItem" />
    /// <summary>
    /// The batch completed queue item.
    /// </summary>
    public class BatchCompletedQueueItem : IBatchCompletedQueueItem
    {
        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <inheritdoc />
        public int BatchNumber { get; set; }

        public int TotalBatches { get; set; }

        /// <inheritdoc />
        public string Start { get; set; }

        /// <inheritdoc />
        public string End { get; set; }

        /// <inheritdoc />
        public int NumberOfEntities { get; set; }

        /// <inheritdoc />
        public int NumberOfEntitiesUploaded { get; set; }
    }
}
