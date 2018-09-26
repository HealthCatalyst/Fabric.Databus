namespace QueueItems
{
    using ElasticSearchSqlFeeder.Interfaces;

    using Newtonsoft.Json.Linq;

    public interface IJsonObjectQueueItem : IQueueItem
    {
        string Id { get; set; }

        JObject Document { get; set; }

        int BatchNumber { get; set; }
    }
}