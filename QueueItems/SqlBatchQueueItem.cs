// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlBatchQueueItem.cs" company="Health Catalyst">
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

    /// <summary>
    /// The sql batch queue item.
    /// </summary>
    public class SqlBatchQueueItem : IQueueItem
    {
        /// <summary>
        /// Gets or sets the query id.
        /// </summary>
        public string QueryId { get; set; }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        public string Start { get; set; }

        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        public string End { get; set; }

        /// <summary>
        /// Gets or sets the loads.
        /// </summary>
        public IList<IDataSource> Loads { get; set; }

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