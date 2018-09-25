using System.Collections.Generic;
using ElasticSearchSqlFeeder.Interfaces;
using Fabric.Databus.Config;

namespace ElasticSearchSqlFeeder.Shared
{
    public class QueueContext : IQueueContext
    {
        public IProgressMonitor ProgressMonitor { get; set; }

        public IQueueManager QueueManager { get; set; }
        public string BulkUploadRelativeUrl { get; set; }
        public string MainMappingUploadRelativeUrl { get; set; }
        public string SecondaryMappingUploadRelativeUrl { get; set; }
        public QueryConfig Config { get; set; }
        public Dictionary<string, string> PropertyTypes { get; set; }

        public IMeteredConcurrentDictionary<string, JsonObjectQueueItem> DocumentDictionary { get; set; }
    }
}
