// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlImportQueueItem.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   Defines the SqlImportQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    public class SqlImportQueueItem : IQueueItem
    {
        public string PropertyName { get; set; }

        public string QueryId { get; set; }

        public DataSource DataSource { get; set; }

        public int Seed { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public int BatchNumber { get; set; }
    }
}