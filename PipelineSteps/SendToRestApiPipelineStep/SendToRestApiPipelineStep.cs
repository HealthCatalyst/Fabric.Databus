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
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Http;
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
        private readonly IFileUploader fileUploader;
        private readonly IEntityJsonWriter entityJsonWriter;

        /// <inheritdoc />
        public SendToRestApiPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            IFileUploader fileUploader,
            IEntityJsonWriter entityJsonWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.fileUploader = fileUploader ?? throw new System.ArgumentNullException(nameof(fileUploader));
            this.entityJsonWriter = entityJsonWriter ?? throw new System.ArgumentNullException(nameof(entityJsonWriter));
        }

        /// <inheritdoc />
        protected override string LoggerName => "SendToRestApi";

        /// <inheritdoc />
        protected override async Task HandleAsync(IJsonObjectQueueItem workItem)
        {
            var stream = new MemoryStream();

            await this.entityJsonWriter.WriteToStreamAsync(workItem.Document, stream);

            // now send to Rest Api
            await fileUploader.SendStreamToHosts(string.Empty, 1, stream, false, false);
        }

        /// <inheritdoc />
        protected override string GetId(IJsonObjectQueueItem workItem)
        {
            return workItem.Id;
        }
    }
}
