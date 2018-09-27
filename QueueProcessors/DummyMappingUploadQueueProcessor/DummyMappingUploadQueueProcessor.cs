namespace DummyMappingUploadQueueProcessor
{
    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using QueueItems;

    using Serilog;

    public class DummyMappingUploadQueueProcessor : BaseQueueProcessor<MappingUploadQueueItem, EndPointQueueItem>
    {
        public DummyMappingUploadQueueProcessor(IQueueContext queueContext, ILogger logger) : base(queueContext, logger)
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