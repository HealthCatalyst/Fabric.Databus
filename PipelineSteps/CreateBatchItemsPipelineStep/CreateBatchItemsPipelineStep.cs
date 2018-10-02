﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateBatchItemsPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the CreateBatchItemsPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CreateBatchItemsPipelineStep
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The create batch items queue processor.
    /// </summary>
    public class CreateBatchItemsPipelineStep : BasePipelineStep<IJsonObjectQueueItem, SaveBatchQueueItem>
    {
        /// <summary>
        /// The temporary cache.
        /// </summary>
        private readonly Queue<IJsonObjectQueueItem> temporaryCache = new Queue<IJsonObjectQueueItem>();

        /// <inheritdoc />
        public CreateBatchItemsPipelineStep(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor, 
            CancellationToken cancellationToken) 
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
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
            if (this.temporaryCache.Count > this.Config.EntitiesPerUploadFile)
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