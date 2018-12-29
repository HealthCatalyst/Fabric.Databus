// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobCompletedPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JobCompletedPipelineStep type.
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
    public class JobCompletedPipelineStep : BasePipelineStep<EndPointQueueItem, EndPointQueueItem>
    {
        /// <summary>
        /// The job events logger.
        /// </summary>
        private readonly IJobEventsLogger jobEventsLogger;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.PipelineSteps.JobCompletedPipelineStep" /> class.
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
        /// <param name="jobEventsLogger"></param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        public JobCompletedPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            IJobEventsLogger jobEventsLogger,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.jobEventsLogger = jobEventsLogger ?? throw new ArgumentNullException(nameof(jobEventsLogger));
        }

        /// <inheritdoc />
        protected override string LoggerName => "JobCompleted";

        /// <inheritdoc />
        protected override Task HandleAsync(EndPointQueueItem workItem)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override string GetId(EndPointQueueItem workItem)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        protected override Task CompleteJobAsync(string queryId, bool isLastThreadForThisTask, IJobCompletedQueueItem jobCompletedQueueItem)
        {
            this.jobEventsLogger.JobCompleted(jobCompletedQueueItem);

            return base.CompleteJobAsync(queryId, isLastThreadForThisTask, jobCompletedQueueItem);
        }
    }
}
