// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Queues
{
    /// <summary>
    /// The QueueItem interface.
    /// </summary>
    public interface IQueueItem
    {
        /// <summary>
        /// Gets or sets the query id.
        /// </summary>
        string QueryId { get; set; }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        int BatchNumber { get; set; }

        /// <summary>
        /// Gets or sets the total batches.
        /// </summary>
        int TotalBatches { get; set; }
    }
}
