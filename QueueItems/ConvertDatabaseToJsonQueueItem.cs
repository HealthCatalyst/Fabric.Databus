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

    public class ConvertDatabaseToJsonQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public List<ColumnInfo> Columns { get; set; }
        public string PropertyName { get; set; }
        public IJsonValueWriter JsonValueWriter { get; set; }
        public string JoinColumnValue { get; set; }
        public List<object[]> Rows { get; set; }
        public string PropertyType { get; set; }
        public int BatchNumber { get; set; }
    }
}