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
    using System.IO;
    using System.Linq;
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
        private static int numberOfEntitiesInBatch;

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
        public WriteSourceWrapperCollectionToJsonPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
        }

        /// <inheritdoc />
        protected override string LoggerName => "WriteSourceWrapperToJson";

        /// <inheritdoc />
        protected override Task HandleAsync(SourceWrapperCollectionQueueItem workItem)
        {
            workItem.SourceWrapperCollection.SortAll();

            using (var textWriter = new StringWriter())
            {
                using (var writer = new JsonTextWriter(textWriter))
                {
                    workItem.SourceWrapperCollection.WriteToJson(writer);
                }

                var result = textWriter.ToString();
                var actualJson = JArray.Parse(result);

                foreach (var entity in actualJson)
                {
                    Interlocked.Increment(ref numberOfEntitiesInBatch);

                    this.AddToOutputQueueAsync(
                        new JsonObjectQueueItem
                            {
                                BatchNumber = workItem.BatchNumber,
                                Id = entity.Children().First().First().ToString(),
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
            batchCompletedQueueItem.NumberOfEntities = numberOfEntitiesInBatch;
            numberOfEntitiesInBatch = 0;
            return base.CompleteBatchAsync(queryId, isLastThreadForThisTask, batchNumber, batchCompletedQueueItem);
        }
    }
}
