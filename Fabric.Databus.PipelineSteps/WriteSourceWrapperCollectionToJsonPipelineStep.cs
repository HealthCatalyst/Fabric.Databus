// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WriteSourceWrapperCollectionToJsonPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the WriteSourceWrapperCollectionToJsonPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The write source wrapper collection to json pipeline step.
    /// </summary>
    public class WriteSourceWrapperCollectionToJsonPipelineStep : BasePipelineStep<SourceWrapperCollectionQueueItem, IJsonObjectQueueItem>
    {
        /// <summary>
        /// The number of entities in batch.
        /// </summary>
        private int numberOfEntitiesInBatch;

        /// <summary>
        /// The number of entities in job.
        /// </summary>
        private int numberOfEntitiesInJob;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.PipelineSteps.WriteSourceWrapperCollectionToJsonPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The job config.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager">
        /// The queue manager.
        /// </param>
        /// <param name="progressMonitor">
        /// The progress monitor.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <param name="pipelineStepState"></param>
        public WriteSourceWrapperCollectionToJsonPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken,
            PipelineStepState pipelineStepState)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken, pipelineStepState)
        {
        }

        /// <inheritdoc />
        protected override string LoggerName => "WriteSourceWrapperToJson";

        /// <inheritdoc />
        protected override Task HandleAsync(SourceWrapperCollectionQueueItem workItem)
        {
            if (workItem.TopLevelKeyColumn == null)
            {
                throw new ArgumentNullException(nameof(workItem.TopLevelKeyColumn));
            }

            workItem.SourceWrapperCollection.SortAll();

            using (var textWriter = new StringWriter())
            {
                using (var writer = new JsonTextWriter(textWriter))
                {
                    workItem.SourceWrapperCollection.WriteToJson(writer);
                }

                var result = textWriter.ToString();
                var actualJson = JArray.Parse(result);

                int i = 0;

                foreach (var entity in actualJson)
                {
                    Interlocked.Increment(ref this.numberOfEntitiesInBatch);
                    Interlocked.Increment(ref this.numberOfEntitiesInJob);

                    var itemId = entity.SelectToken(workItem.TopLevelKeyColumn)?.ToString() ?? $"{workItem.BatchNumber}-{++i}";

                    this.AddToOutputQueueAsync(
                        new JsonObjectQueueItem
                            {
                                BatchNumber = workItem.BatchNumber,
                                TotalBatches = workItem.TotalBatches,
                                Id = itemId,
                                Document = (JObject)entity
                            });
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override string GetId(SourceWrapperCollectionQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <inheritdoc />
        protected override Task CompleteBatchAsync(string queryId, bool isLastThreadForThisTask, int batchNumber, IBatchCompletedQueueItem batchCompletedQueueItem)
        {
            batchCompletedQueueItem.NumberOfEntities = this.numberOfEntitiesInBatch;
            this.numberOfEntitiesInBatch = 0;
            return base.CompleteBatchAsync(queryId, isLastThreadForThisTask, batchNumber, batchCompletedQueueItem);
        }

        /// <inheritdoc />
        protected override Task CompleteJobAsync(string queryId, bool isLastThreadForThisTask, IJobCompletedQueueItem jobCompletedQueueItem)
        {
            jobCompletedQueueItem.NumberOfEntities = this.numberOfEntitiesInJob;
            this.numberOfEntitiesInJob = 0;
            return base.CompleteJobAsync(queryId, isLastThreadForThisTask, jobCompletedQueueItem);
        }
    }
}
