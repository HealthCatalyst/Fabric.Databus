// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateBatchItemsQueueProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the CreateBatchItemsQueueProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CreateBatchItemsQueueProcessor
{
    using System.Collections.Generic;
    using System.Linq;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using QueueItems;

    using Serilog;

    /// <summary>
    /// The create batch items queue processor.
    /// </summary>
    public class CreateBatchItemsQueueProcessor : BaseQueueProcessor<IJsonObjectQueueItem, SaveBatchQueueItem>
    {
        private readonly Queue<IJsonObjectQueueItem> _items = new Queue<IJsonObjectQueueItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBatchItemsQueueProcessor"/> class.
        /// </summary>
        /// <param name="queueContext">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public CreateBatchItemsQueueProcessor(IQueueContext queueContext, ILogger logger) : base(queueContext, logger)
        {
        }

        /// <inheritdoc />
        protected override void Handle(IJsonObjectQueueItem workItem)
        {
            this._items.Enqueue(workItem);
            this.FlushDocumentsIfBatchSizeReached();
        }

        /// <inheritdoc />
        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        /// <summary>
        /// The flush all documents.
        /// </summary>
        public void FlushAllDocuments()
        {
            while (this._items.Any())
            {
                this.FlushDocumentsToLimit(this._items.Count);
            }
        }

        /// <summary>
        /// The flush documents if batch size reached.
        /// </summary>
        private void FlushDocumentsIfBatchSizeReached()
        {
            // see if there are enough to create a batch
            if (this._items.Count > Config.EntitiesPerUploadFile)
            {
                this.FlushDocuments();
            }
        }

        /// <summary>
        /// The flush documents.
        /// </summary>
        private void FlushDocuments()
        {
            this.FlushDocumentsWithoutLock();
        }

        /// <summary>
        /// The flush documents without lock.
        /// </summary>
        private void FlushDocumentsWithoutLock()
        {
            while (this._items.Count > Config.EntitiesPerUploadFile)
            {
                this.FlushDocumentsToLimit(Config.EntitiesPerUploadFile);
            }
        }

        /// <summary>
        /// The flush documents to limit.
        /// </summary>
        /// <param name="limit">
        /// The limit.
        /// </param>
        private void FlushDocumentsToLimit(int limit)
        {
            var docsToSave = new List<IJsonObjectQueueItem>();
            for (int i = 0; i < limit; i++)
            {
                IJsonObjectQueueItem cacheItem = this._items.Dequeue();
                docsToSave.Add(cacheItem);
            }

            if (docsToSave.Any())
            {
                MyLogger.Verbose($"Saved Batch: count:{docsToSave.Count} from {docsToSave.First().Id} to {docsToSave.Last().Id}, inQueue:{this.InQueue.Count} ");
                AddToOutputQueue(new SaveBatchQueueItem { ItemsToSave = docsToSave });

            }
        }

        /// <inheritdoc />
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            this.FlushAllDocuments();
        }

        /// <inheritdoc />
        protected override string GetId(IJsonObjectQueueItem workItem)
        {
            return workItem.QueryId;
        }

        protected override string LoggerName => "CreateBatchItems";
    }

}