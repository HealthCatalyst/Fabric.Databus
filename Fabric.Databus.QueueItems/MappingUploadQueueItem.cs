// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingUploadQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MappingUploadQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using System.IO;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Queues;

    using Newtonsoft.Json;

    /// <inheritdoc />
    /// <summary>
    /// The mapping upload queue item.
    /// </summary>
    public class MappingUploadQueueItem : IQueueItem
    {
        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the stream.
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the job.
        /// </summary>
        [JsonIgnore]
        public IJob Job { get; set; }
    }
}