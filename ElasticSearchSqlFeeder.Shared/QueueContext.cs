// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueContext.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the QueueContext type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Shared
{
    using System.Collections.Generic;
    using System.Threading;

    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    using QueueItems;

    /// <summary>
    /// The queue context.
    /// </summary>
    public class QueueContext : IQueueContext
    {
        public IProgressMonitor ProgressMonitor { get; set; }

        public IQueueManager QueueManager { get; set; }
        public string BulkUploadRelativeUrl { get; set; }
        public string MainMappingUploadRelativeUrl { get; set; }
        public string SecondaryMappingUploadRelativeUrl { get; set; }
        public IQueryConfig Config { get; set; }
        public Dictionary<string, string> PropertyTypes { get; set; }

        public IMeteredConcurrentDictionary<string, IJsonObjectQueueItem> DocumentDictionary { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }
}
