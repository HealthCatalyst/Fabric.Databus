namespace DummyMappingUploadQueueProcessor
{
    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Shared;

    using QueueItems;

    public class DummyMappingUploadQueueProcessor : BaseQueueProcessor<MappingUploadQueueItem, EndPointQueueItem>
    {
        public DummyMappingUploadQueueProcessor(QueueContext queueContext) : base(queueContext)
        {
        }

        protected override void Handle(MappingUploadQueueItem workItem)
        {
            // do nothing
        }

        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        protected override string GetId(MappingUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }

        protected override string LoggerName => "NullMappingUpload";
    }
}