// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Class1.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SendToRestApiPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Http;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The send to rest api pipeline step.
    /// </summary>
    public class SendToRestApiPipelineStep : BasePipelineStep<IJsonObjectQueueItem, EndPointQueueItem>
    {
        /// <summary>
        /// The file uploader.
        /// </summary>
        private readonly IFileUploader fileUploader;

        /// <summary>
        /// The detailed temporary file writer.
        /// </summary>
        private readonly IDetailedTemporaryFileWriter detailedTemporaryFileWriter;

        /// <summary>
        /// The entity json writer.
        /// </summary>
        private readonly IEntityJsonWriter entityJsonWriter;

        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <inheritdoc />
        public SendToRestApiPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            IFileUploader fileUploader,
            IDetailedTemporaryFileWriter detailedTemporaryFileWriter,
            IEntityJsonWriter entityJsonWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.fileUploader = fileUploader ?? throw new ArgumentNullException(nameof(fileUploader));
            this.detailedTemporaryFileWriter = detailedTemporaryFileWriter ?? throw new ArgumentNullException(nameof(detailedTemporaryFileWriter));
            this.entityJsonWriter = entityJsonWriter ?? throw new ArgumentNullException(nameof(entityJsonWriter));
            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.Config.LocalSaveFolder != null)
            {
                this.folder = this.detailedTemporaryFileWriter.CombinePath(this.Config.LocalSaveFolder, $"{this.UniqueId}-{this.LoggerName}");
            }
        }

        /// <inheritdoc />
        protected override sealed string LoggerName => "SendToRestApi";

        /// <inheritdoc />
        protected override async Task HandleAsync(IJsonObjectQueueItem workItem)
        {
            var stream = new MemoryStream();

            await this.entityJsonWriter.WriteToStreamAsync(workItem.Document, stream);

            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.folder != null)
            {
                var path = this.detailedTemporaryFileWriter.CombinePath(
                    this.detailedTemporaryFileWriter.CombinePath(this.folder, $"{workItem.BatchNumber}.txt"),
                    Convert.ToString(workItem.BatchNumber));

                this.detailedTemporaryFileWriter.CreateDirectory(path);
            }

            // now send to Rest Api
            await this.fileUploader.SendStreamToHosts(string.Empty, 1, stream, false, false);
        }

        /// <inheritdoc />
        protected override string GetId(IJsonObjectQueueItem workItem)
        {
            return workItem.Id;
        }
    }
}
