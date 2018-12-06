// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndPointQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the EndPointQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The end point queue item.
    /// </summary>
    public class EndPointQueueItem : IQueueItem
    {
        /// <inheritdoc />
        public string PropertyName { get; set; }

        public int BatchNumber { get; set; }

        /// <inheritdoc />
        public string QueryId { get; set; }
    }
}
