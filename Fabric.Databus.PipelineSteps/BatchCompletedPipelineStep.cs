// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchCompletedPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the BatchCompletedPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The batch completed pipeline step.
    /// </summary>
    public class BatchCompletedPipelineStep : BasePipelineStep<EndPointQueueItem, EndPointQueueItem>
    {
        /// <summary>
        /// The batch completed logger.
        /// </summary>
        private readonly IBatchEventsLogger batchEventsLogger;

        /// <inheritdoc />
        public BatchCompletedPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            IBatchEventsLogger batchEventsLogger,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.batchEventsLogger = batchEventsLogger ?? throw new ArgumentNullException(nameof(batchEventsLogger));
        }

        /// <inheritdoc />
        protected override string LoggerName => "BatchCompleted";

        /// <inheritdoc />
        protected override Task HandleAsync(EndPointQueueItem workItem)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override async Task CompleteBatchAsync(string queryId, bool isLastThreadForThisTask, int batchNumber, IBatchCompletedQueueItem batchCompletedQueueItem)
        {
            this.batchEventsLogger.BatchCompleted(batchCompletedQueueItem);

            await base.CompleteBatchAsync(queryId, isLastThreadForThisTask, batchNumber, batchCompletedQueueItem);
        }

        /// <inheritdoc />
        protected override string GetId(EndPointQueueItem workItem)
        {
            return Convert.ToString(workItem.BatchNumber);
        }
    }
}
