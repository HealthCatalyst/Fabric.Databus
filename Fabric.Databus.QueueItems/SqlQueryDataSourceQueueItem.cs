// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlQueryDataSourceQueueItem.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   Defines the SqlQueryDataSourceQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The sql import queue item.
    /// </summary>
    public class SqlQueryDataSourceQueueItem : IQueueItem
    {
        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        public IDataSource DataSource { get; set; }

        /// <summary>
        /// Gets or sets the seed.
        /// </summary>
        public int Seed { get; set; }

        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        public string Start { get; set; }

        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        public string End { get; set; }

        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        public int BatchNumber { get; set; }

        /// <summary>
        /// Gets or sets the property types.
        /// </summary>
        public IDictionary<string, string> PropertyTypes { get; set; }
    }
}