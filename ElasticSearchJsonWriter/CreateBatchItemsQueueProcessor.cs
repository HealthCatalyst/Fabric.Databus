using System.Collections.Generic;
using System.Linq;
using ElasticSearchSqlFeeder.Shared;

namespace ElasticSearchJsonWriter
{
    public class CreateBatchItemsQueueProcessor : BaseQueueProcessor<JsonObjectQueueItem, SaveBatchQueueItem>
    {
        private readonly Queue<JsonObjectQueueItem> _items = new Queue<JsonObjectQueueItem>();

        public CreateBatchItemsQueueProcessor(QueueContext queueContext) : base(queueContext)
        {
        }

        protected override void Handle(JsonObjectQueueItem workitem)
        {
            _items.Enqueue(workitem);
            FlushDocumentsIfBatchSizeReached();
        }

        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        public void FlushAllDocuments()
        {
            while (_items.Any())
            {
                FlushDocumentsToLimit(_items.Count);
            }
        }
        private void FlushDocumentsIfBatchSizeReached()
        {
            // see if there are enough to create a batch
            if (_items.Count > Config.EntitiesPerUploadFile)
            {
                FlushDocuments();
            }
        }

        private void FlushDocuments()
        {
            FlushDocumentsWithoutLock();
        }

        private void FlushDocumentsWithoutLock()
        {
            while (_items.Count > Config.EntitiesPerUploadFile)
            {
                FlushDocumentsToLimit(Config.EntitiesPerUploadFile);
            }
        }

        private void FlushDocumentsToLimit(int limit)
        {
            var docsToSave = new List<JsonObjectQueueItem>();
            for (int i = 0; i < limit; i++)
            {
                JsonObjectQueueItem cacheItem = _items.Dequeue();
                docsToSave.Add(cacheItem);
            }

            if (docsToSave.Any())
            {
                MyLogger.Trace($"Saved Batch: count:{docsToSave.Count} from {docsToSave.First().Id} to {docsToSave.Last().Id}, inQueue:{_inQueue.Count} ");
                AddToOutputQueue(new SaveBatchQueueItem { ItemsToSave = docsToSave });

            }
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            FlushAllDocuments();
        }

        protected override string GetId(JsonObjectQueueItem workitem)
        {
            return workitem.QueryId;
        }

        protected override string LoggerName => "CreateBatchItems";
    }

}