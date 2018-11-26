// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonDocumentMergerQueueItem.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   Defines the JsonDocumentMergerQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using Fabric.Databus.Interfaces.Queues;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The json document merger queue item.
    /// </summary>
    public class JsonDocumentMergerQueueItem : IQueueItem
    {
        /// <summary>
        /// Gets or sets the query id.
        /// </summary>
        public string QueryId { get; set; }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the new j objects.
        /// </summary>
        public JObject[] NewJObjects { get; set; }

        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        public int BatchNumber { get; set; }

        // public string JoinColumnValue { get; set; }
    }
}