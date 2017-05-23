using System.Collections.Generic;
using ElasticSearchSqlFeeder.Interfaces;
using Fabric.Databus.Config;

namespace SqlImporter
{
    public class SqlGetSchemaQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }
        public List<DataSource> Loads { get; set; }

    }
}
