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

    /// <inheritdoc />
    /// <summary>
    /// The batch completed queue item.
    /// </summary>
    public class BatchCompletedQueueItem : IQueueItem
    {
        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        public int BatchNumber { get; set; }
    }
}
