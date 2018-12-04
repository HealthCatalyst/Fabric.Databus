// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUploadQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the FileUploadQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using System.IO;

    using Fabric.Databus.Interfaces.Queues;

    using Newtonsoft.Json;

    /// <inheritdoc />
    /// <summary>
    /// The file upload queue item.
    /// </summary>
    public class FileUploadQueueItem : IQueueItem
    {
        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        public int BatchNumber { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <summary>
        /// Gets or sets the stream.
        /// </summary>
        [JsonIgnore]
        public Stream Stream { get; set; }
    }
}