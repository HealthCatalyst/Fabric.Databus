// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullElasticSearchFileUploadPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NullElasticSearchFileUploadPipelineStep type.
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
    public class NullElasticSearchFileUploadPipelineStep : BasePipelineStep<FileUploadQueueItem, EndPointQueueItem>
    {
        /// <inheritdoc />
        public NullElasticSearchFileUploadPipelineStep(IJobConfig jobConfig, ILogger logger, IQueueManager queueManager, IProgressMonitor progressMonitor, CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
        }

        /// <inheritdoc />
        protected override string LoggerName => "NullElasticSearchFileUpload";

        /// <inheritdoc />
        protected override Task HandleAsync(FileUploadQueueItem workItem)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override string GetId(FileUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }
    }
}
