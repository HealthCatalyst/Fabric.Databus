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

    using ElasticSearchSqlFeeder.Interfaces;

    public class SaveBatchQueueItem : IQueueItem
    {
        public List<IJsonObjectQueueItem> ItemsToSave { get; set; }

        public string PropertyName { get; set; }

        public string QueryId { get; set; }
    }
}