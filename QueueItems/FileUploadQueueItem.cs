// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUploadQueueItem.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the FileUploadQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using System.IO;

    using ElasticSearchSqlFeeder.Interfaces;

    using Newtonsoft.Json;

    public class FileUploadQueueItem : IQueueItem
    {
        public int BatchNumber { get; set; }

        public string PropertyName { get; set; }

        public string QueryId { get; set; }

        [JsonIgnore]
        public Stream Stream { get; set; }
    }
}