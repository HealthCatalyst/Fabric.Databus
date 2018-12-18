// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateJsonBatchesPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the CreateJsonBatchesPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The create batch items queue processor.
    /// </summary>
    public class CreateJsonBatchesPipelineStep : BasePipelineStep<IJsonObjectQueueItem, SaveBatchQueueItem>
    {
        /// <summary>
        /// The temporary cache.
        /// </summary>
        private readonly Queue<IJsonObjectQueueItem> temporaryCache = new Queue<IJsonObjectQueueItem>();

        /// <inheritdoc />
        public CreateJsonBatchesPipelineStep(
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
        protected override string LoggerName => "CreateBatchesOfJson";

        /// <summary>
        /// The flush all documents.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task FlushAllDocuments()
        {
            while (this.temporaryCache.Any())
            {
                await this.FlushDocumentsToLimit(this.temporaryCache.Count);
            }
        }

        /// <inheritdoc />
        protected override async Task HandleAsync(IJsonObjectQueueItem workItem)
        {
            this.temporaryCache.Enqueue(workItem);
            await this.FlushDocumentsIfBatchSizeReachedAsync();
        }

        /// <inheritdoc />
        protected override async Task CompleteBatchAsync(string queryId, bool isLastThreadForThisTask, int batchNumber, IBatchCompletedQueueItem batchCompletedQueueItem)
        {
            await this.FlushAllDocuments();
        }

        /// <inheritdoc />
        protected override string GetId(IJsonObjectQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The flush documents if batch size reached.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task FlushDocumentsIfBatchSizeReachedAsync()
        {
            // see if there are enough to create a batch
            if (this.temporaryCache.Count > this.Config.EntitiesPerUploadFile)
            {
                await this.FlushDocumentsAsync();
            }
        }

        /// <summary>
        /// The flush documents.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task FlushDocumentsAsync()
        {
            await this.FlushDocumentsWithoutLockAsync();
        }

        /// <summary>
        /// The flush documents without lock.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task FlushDocumentsWithoutLockAsync()
        {
            while (this.temporaryCache.Count > this.Config.EntitiesPerUploadFile)
            {
                await this.FlushDocumentsToLimit(this.Config.EntitiesPerUploadFile);
            }
        }

        /// <summary>
        /// The flush documents to limit.
        /// </summary>
        /// <param name="limit">
        /// The limit.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task FlushDocumentsToLimit(int limit)
        {
            var docsToSave = new List<IJsonObjectQueueItem>();
            for (int i = 0; i < limit; i++)
            {
                IJsonObjectQueueItem cacheItem = this.temporaryCache.Dequeue();
                docsToSave.Add(cacheItem);
            }

            if (docsToSave.Any())
            {
                this.MyLogger.Verbose(
                    "Saved Batch: count:{DocumentsToSave} from {FirstDocumentId} to {LastDocumentId}, inQueue:{InQueueCount} ",
                    docsToSave.Count,
                    docsToSave.First().Id,
                    docsToSave.Last().Id,
                    this.InQueue.Count);

                await this.AddToOutputQueueAsync(new SaveBatchQueueItem { ItemsToSave = docsToSave });
            }
        }
    }
}