// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonObjectQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JsonObjectQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using Fabric.Databus.Interfaces.Queues;

    using Newtonsoft.Json.Linq;

    /// <inheritdoc />
    /// <summary>
    /// The json object queue item.
    /// </summary>
    public class JsonObjectQueueItem : IJsonObjectQueueItem
    {
        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public JObject Document { get; set; }

        /// <inheritdoc />
        public int BatchNumber { get; set; }

        public int TotalBatches { get; set; }
    }
}
