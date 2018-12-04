// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlGetSchemaQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SqlGetSchemaQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using System.Collections.Generic;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The sql get schema queue item.
    /// </summary>
    public class SqlGetSchemaQueueItem : IQueueItem
    {
        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the loads.
        /// </summary>
        public List<DataSource> Loads { get; set; }

    }
}
