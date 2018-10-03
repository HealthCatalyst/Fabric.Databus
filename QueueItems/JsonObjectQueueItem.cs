namespace QueueItems
{
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Queues;

    using Newtonsoft.Json.Linq;

    public class JsonObjectQueueItem : IQueueItem, IJsonObjectQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }
        public string Id { get; set; }
        public JObject Document { get; set; }
        public int BatchNumber { get; set; }
    }
}
