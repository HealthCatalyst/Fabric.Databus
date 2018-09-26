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

    public class SaveSchemaQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }
        public List<MappingItem> Mappings { get; set; }
    }
}