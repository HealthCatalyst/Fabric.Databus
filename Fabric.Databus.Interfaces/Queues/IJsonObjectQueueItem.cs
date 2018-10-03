// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJsonObjectQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IJsonObjectQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Queues
{
    using Newtonsoft.Json.Linq;

    /// <inheritdoc />
    /// <summary>
    /// The JsonObjectQueueItem interface.
    /// </summary>
    public interface IJsonObjectQueueItem : IQueueItem
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        JObject Document { get; set; }

        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        int BatchNumber { get; set; }
    }
}