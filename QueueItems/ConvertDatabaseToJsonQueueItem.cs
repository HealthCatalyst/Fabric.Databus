// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConvertDatabaseToJsonQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ConvertDatabaseToJsonQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using System.Collections.Generic;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    /// <summary>
    /// The convert database to json queue item.
    /// </summary>
    public class ConvertDatabaseToJsonQueueItem : IQueueItem
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
        /// Gets or sets the json value writer.
        /// </summary>
        public IJsonValueWriter JsonValueWriter { get; set; }

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