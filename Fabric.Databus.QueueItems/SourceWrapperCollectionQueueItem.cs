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

        /// <inheritdoc />
        public int BatchNumber { get; set; }

        public int TotalBatches { get; set; }

        /// <summary>
        /// Gets or sets the top level key column.
        /// </summary>
        public string TopLevelKeyColumn { get; set; }
    }
}
