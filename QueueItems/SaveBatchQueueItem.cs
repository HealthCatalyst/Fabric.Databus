// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveBatchQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SaveBatchQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Queues;

    public class SaveBatchQueueItem : IQueueItem
    {
        public List<IJsonObjectQueueItem> ItemsToSave { get; set; }

        public string PropertyName { get; set; }

        public string QueryId { get; set; }
    }
}