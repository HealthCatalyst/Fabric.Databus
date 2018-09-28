// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveSchemaQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SaveSchemaQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using System.Collections.Generic;

    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    /// <summary>
    /// The save schema queue item.
    /// </summary>
    public class SaveSchemaQueueItem : IQueueItem
    {
        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        public List<MappingItem> Mappings { get; set; }

        /// <summary>
        /// Gets or sets the job.
        /// </summary>
        public IJob Job { get; set; }
    }
}