// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveSchemaQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SaveSchemaQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using System.Collections.Generic;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;

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