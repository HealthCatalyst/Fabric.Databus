using ElasticSearchSqlFeeder.Shared;

namespace ElasticSearchApiCaller
{
    public class DummyMappingUploadQueueProcessor : BaseQueueProcessor<MappingUploadQueueItem, EndPointQueueItem>
    {
        public DummyMappingUploadQueueProcessor(QueueContext queueContext) : base(queueContext)
        {
        }

        protected override void Handle(MappingUploadQueueItem workitem)
        {
            // do nothing
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        protected override string GetId(MappingUploadQueueItem workitem)
        {
            return workitem.QueryId;
        }

        protected override string LoggerName => "NullMappingUpload";
    }
}