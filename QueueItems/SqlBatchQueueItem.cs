// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlBatchQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlBatchQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using System.Collections.Generic;

    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    public class SqlBatchQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public List<DataSource> Loads { get; set; }
        public int BatchNumber { get; set; }
    }
}