// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullSendToRestApiPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NullSendToRestApiPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The null send to rest api pipeline step.
    /// </summary>
    public class NullSendToRestApiPipelineStep : BasePipelineStep<IJsonObjectQueueItem, EndPointQueueItem>
    {
        /// <inheritdoc />
        public NullSendToRestApiPipelineStep(IJobConfig jobConfig, ILogger logger, IQueueManager queueManager, IProgressMonitor progressMonitor, CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
        }

        /// <inheritdoc />
        protected override string LoggerName => "NullSendToRestApi";

        /// <inheritdoc />
        protected override Task HandleAsync(IJsonObjectQueueItem workItem)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override string GetId(IJsonObjectQueueItem workItem)
        {
            return workItem.Id;
        }
    }
}
