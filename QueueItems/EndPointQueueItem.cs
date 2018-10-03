// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndPointQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the EndPointQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Queues;

    public class EndPointQueueItem : IQueueItem
    {
        public string PropertyName { get; set; }

        public string QueryId { get; set; }
    }
}
