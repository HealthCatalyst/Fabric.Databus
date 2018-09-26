namespace QueueItems
{
    using System.IO;

    using ElasticSearchSqlFeeder.Interfaces;

    public class MappingUploadQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }
        public Stream Stream { get; set; }
        public int SequenceNumber { get; set; }
    }
}