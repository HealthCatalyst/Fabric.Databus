// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlGetSchemaQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SqlGetSchemaQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using System.Collections.Generic;

    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    public class SqlGetSchemaQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }
        public List<DataSource> Loads { get; set; }

    }
}
