// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlDataLoadedQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SqlDataLoadedQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.QueueItems
{
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;

    /// <inheritdoc />
    /// <summary>
    /// The convert database to json queue item.
    /// </summary>
    public class SqlDataLoadedQueueItem : IQueueItem
    {
        /// <inheritdoc />
        public string QueryId { get; set; }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        public List<ColumnInfo> Columns { get; set; }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the join column value.
        /// </summary>
        public string JoinColumnValue { get; set; }

        /// <summary>
        /// Gets or sets the rows.
        /// </summary>
        public List<object[]> Rows { get; set; }

        /// <summary>
        /// Gets or sets the property type.
        /// </summary>
        public string PropertyType { get; set; }

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