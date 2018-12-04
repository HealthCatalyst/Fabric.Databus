// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceWrapperCollectionQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SourceWrapperCollectionQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Shared;

    using Newtonsoft.Json;

    /// <inheritdoc />
    /// <summary>
    /// The source wrapper collection queue item.
    /// </summary>
    public class SourceWrapperCollectionQueueItem : IQueueItem
    {
        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the source wrapper collection.
        /// </summary>
        [JsonIgnore]
        public SourceWrapperCollection SourceWrapperCollection { get; set; }

        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        public int BatchNumber { get; set; }
    }
}
