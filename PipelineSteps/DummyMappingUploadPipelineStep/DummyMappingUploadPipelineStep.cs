// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyMappingUploadPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the DummyMappingUploadPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DummyMappingUploadPipelineStep
{
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;

    using QueueItems;

    using Serilog;

    /// <summary>
    /// The dummy mapping upload queue processor.
    /// </summary>
    public class DummyMappingUploadPipelineStep : BasePipelineStep<MappingUploadQueueItem, SqlJobQueueItem>
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:DummyMappingUploadPipelineStep.DummyMappingUploadPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager">
        /// The queue Manager.
        /// </param>
        /// <param name="progressMonitor"></param>
        /// <param name="cancellationToken"></param>
        public DummyMappingUploadPipelineStep(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken) 
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
        }

        /// <summary>
        /// The logger name.
        /// </summary>
        protected override string LoggerName => "NullMappingUpload";

        /// <inheritdoc />
        protected override Task HandleAsync(MappingUploadQueueItem workItem)
        {
            // do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        /// The get id.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected override string GetId(MappingUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }
    }
}