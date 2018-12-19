// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveBatchQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SaveBatchQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The save batch queue item.
    /// </summary>
    public class SaveBatchQueueItem : IQueueItem
    {
        /// <summary>
        /// Gets or sets the items to save.
        /// </summary>
        public List<IJsonObjectQueueItem> ItemsToSave { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        public int BatchNumber { get; set; }

        public int TotalBatches { get; set; }

        /// <inheritdoc />
        public string QueryId { get; set; }
    }
}