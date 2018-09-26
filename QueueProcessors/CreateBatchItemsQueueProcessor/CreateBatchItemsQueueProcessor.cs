namespace CreateBatchItemsQueueProcessor
{
    using System.Collections.Generic;
    using System.Linq;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Shared;

    using QueueItems;

    public class CreateBatchItemsQueueProcessor : BaseQueueProcessor<JsonObjectQueueItem, SaveBatchQueueItem>
    {
        private readonly Queue<JsonObjectQueueItem> _items = new Queue<JsonObjectQueueItem>();

        public CreateBatchItemsQueueProcessor(QueueContext queueContext) : base(queueContext)
        {
        }

        protected override void Handle(JsonObjectQueueItem workItem)
        {
            this._items.Enqueue(workItem);
            this.FlushDocumentsIfBatchSizeReached();
        }

        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        public void FlushAllDocuments()
        {
            while (this._items.Any())
            {
                this.FlushDocumentsToLimit(this._items.Count);
            }
        }
        private void FlushDocumentsIfBatchSizeReached()
        {
            // see if there are enough to create a batch
            if (this._items.Count > Config.EntitiesPerUploadFile)
            {
                this.FlushDocuments();
            }
        }

        private void FlushDocuments()
        {
            this.FlushDocumentsWithoutLock();
        }

        private void FlushDocumentsWithoutLock()
        {
            while (this._items.Count > Config.EntitiesPerUploadFile)
            {
                this.FlushDocumentsToLimit(Config.EntitiesPerUploadFile);
            }
        }

        private void FlushDocumentsToLimit(int limit)
        {
            var docsToSave = new List<JsonObjectQueueItem>();
            for (int i = 0; i < limit; i++)
            {
                JsonObjectQueueItem cacheItem = this._items.Dequeue();
                docsToSave.Add(cacheItem);
            }

            if (docsToSave.Any())
            {
                MyLogger.Trace($"Saved Batch: count:{docsToSave.Count} from {docsToSave.First().Id} to {docsToSave.Last().Id}, inQueue:{this.InQueue.Count} ");
                AddToOutputQueue(new SaveBatchQueueItem { ItemsToSave = docsToSave });

            }
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            this.FlushAllDocuments();
        }

        protected override string GetId(JsonObjectQueueItem workItem)
        {
            return workItem.QueryId;
        }

        protected override string LoggerName => "CreateBatchItems";
    }

}