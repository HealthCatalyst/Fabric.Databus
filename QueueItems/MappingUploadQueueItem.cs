namespace QueueItems
{
    using System.IO;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Queues;

    public class MappingUploadQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }
        public Stream Stream { get; set; }
        public int SequenceNumber { get; set; }

        public IJob Job { get; set; }
    }
}