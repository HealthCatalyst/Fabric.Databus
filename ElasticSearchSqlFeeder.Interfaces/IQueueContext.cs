using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System.Threading;

    using QueueItems;

    public interface IQueueContext
    {
        IProgressMonitor ProgressMonitor { get; set; }

        IQueueManager QueueManager { get; set; }

        string BulkUploadRelativeUrl { get; set; }

        string MainMappingUploadRelativeUrl { get; set; }

        string SecondaryMappingUploadRelativeUrl { get; set; }

        IQueryConfig Config { get; set; }

        Dictionary<string, string> PropertyTypes { get; set; }

        IMeteredConcurrentDictionary<string, IJsonObjectQueueItem> DocumentDictionary { get; set; }

        CancellationToken CancellationToken { get; set; }
    }
}
