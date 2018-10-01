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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The create batch items queue processor.
    /// </summary>
    public class CreateBatchItemsQueueProcessor : BaseQueueProcessor<IJsonObjectQueueItem, SaveBatchQueueItem>
    {
        /// <summary>
        /// The temporary cache.
        /// </summary>
        private readonly Queue<IJsonObjectQueueItem> temporaryCache = new Queue<IJsonObjectQueueItem>();

        /// <inheritdoc />
        public CreateBatchItemsQueueProcessor(IQueueContext queueContext, ILogger logger, IQueueManager queueManager) : base(queueContext, logger, queueManager)
        {
            if (this.Config.EntitiesPerUploadFile < 1)
            {
                throw new ArgumentException(nameof(this.Config.EntitiesPerUploadFile));
            }
        }

        /// <inheritdoc />
        protected override string LoggerName => "CreateBatchItems";

        /// <summary>
        /// The flush all documents.
        /// </summary>
        public void FlushAllDocuments()
        {
            while (this.temporaryCache.Any())
            {
                this.FlushDocumentsToLimit(this.temporaryCache.Count);
            }
        }

        /// <inheritdoc />
        protected override void Handle(IJsonObjectQueueItem workItem)
        {
            this.temporaryCache.Enqueue(workItem);
            this.FlushDocumentsIfBatchSizeReached();
        }

        /// <inheritdoc />
        protected override void Begin(bool isFirstThreadForThisTask)
        {
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

        /// <summary>
        /// The flush documents if batch size reached.
        /// </summary>
        private void FlushDocumentsIfBatchSizeReached()
        {
            // see if there are enough to create a batch
            if (this.temporaryCache.Count > Config.EntitiesPerUploadFile)
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
            while (this.temporaryCache.Count > this.Config.EntitiesPerUploadFile)
            {
                this.FlushDocumentsToLimit(this.Config.EntitiesPerUploadFile);
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
                IJsonObjectQueueItem cacheItem = this.temporaryCache.Dequeue();
                docsToSave.Add(cacheItem);
            }

            if (docsToSave.Any())
            {
                this.MyLogger.Verbose($"Saved Batch: count:{docsToSave.Count} from {docsToSave.First().Id} to {docsToSave.Last().Id}, inQueue:{this.InQueue.Count} ");
                this.AddToOutputQueue(new SaveBatchQueueItem { ItemsToSave = docsToSave });
            }
        }
    }
}