// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveBatchQueueItem.cs" company="">
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
        public IEnumerable<JsonObjectQueueItem> ItemsToSave { get; set; }

        public string PropertyName { get; set; }

        public string QueryId { get; set; }
    }
}