// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Class1.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SendToRestApiPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SendToRestApiPipelineStep
{
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    using Newtonsoft.Json;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The send to rest api pipeline step.
    /// </summary>
    public class SendToRestApiPipelineStep : BasePipelineStep<IJsonObjectQueueItem, EndPointQueueItem>
    {
        /// <inheritdoc />
        public SendToRestApiPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
        }

        /// <inheritdoc />
        protected override string LoggerName => "SendToRestApi";

        /// <inheritdoc />
        protected override async Task HandleAsync(IJsonObjectQueueItem workItem)
        {
            var stream = new MemoryStream();

            using (var textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            using (var writer = new JsonTextWriter(textWriter))
            {
                await workItem.Document.WriteToAsync(writer);
            }

            // now send to Rest Api
        }

        /// <inheritdoc />
        protected override string GetId(IJsonObjectQueueItem workItem)
        {
            return workItem.Id;
        }
    }
}
