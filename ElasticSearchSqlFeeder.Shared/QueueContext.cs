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

    /// <inheritdoc />
    /// <summary>
    /// The queue context.
    /// </summary>
    public class QueueContext : IQueueContext
    {
        /// <inheritdoc />
        public IQueryConfig Config { get; set; }

        /// <inheritdoc />
        public CancellationToken CancellationToken { get; set; }
    }
}
