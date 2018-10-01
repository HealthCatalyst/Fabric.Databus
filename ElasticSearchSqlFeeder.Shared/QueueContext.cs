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
    using System.Threading;

    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    using QueueItems;

    /// <inheritdoc />
    /// <summary>
    /// The queue context.
    /// </summary>
    public class QueueContext : IQueueContext
    {
        /// <inheritdoc />
        public IProgressMonitor ProgressMonitor { get; set; }

        /// <inheritdoc />
        public IQueryConfig Config { get; set; }

        /// <inheritdoc />
        public IMeteredConcurrentDictionary<string, IJsonObjectQueueItem> DocumentDictionary { get; set; }

        /// <inheritdoc />
        public CancellationToken CancellationToken { get; set; }
    }
}
