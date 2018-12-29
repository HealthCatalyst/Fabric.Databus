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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
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
        /// The number of entities uploaded.
        /// </summary>
        private int numberOfEntitiesUploadedForBatch;

        /// <summary>
        /// The number of entities uploaded for job.
        /// </summary>
        private int numberOfEntitiesUploadedForJob;

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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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

            // now send to Rest Api
            var fileUploadResult = await this.fileUploader.SendStreamToHostsAsync(string.Empty, workItem.Id, stream, false, this.Config.CompressFiles);

            await this.WriteDiagnosticsAsync(workItem, fileUploadResult);

            if (fileUploadResult.StatusCode == HttpStatusCode.OK)
            {
                Interlocked.Increment(ref numberOfEntitiesUploadedForBatch);
                Interlocked.Increment(ref numberOfEntitiesUploadedForJob);
            }
        }

        /// <inheritdoc />
        protected override string GetId(IJsonObjectQueueItem workItem)
        {
            return workItem.Id;
        }

        /// <inheritdoc />
        protected override Task CompleteBatchAsync(
            string queryId,
            bool isLastThreadForThisTask,
            int batchNumber,
            IBatchCompletedQueueItem batchCompletedQueueItem)
        {
            batchCompletedQueueItem.NumberOfEntitiesUploaded = numberOfEntitiesUploadedForBatch;
            numberOfEntitiesUploadedForBatch = 0;
            return base.CompleteBatchAsync(queryId, isLastThreadForThisTask, batchNumber, batchCompletedQueueItem);
        }

        /// <inheritdoc />
        protected override Task CompleteJobAsync(string queryId, bool isLastThreadForThisTask, IJobCompletedQueueItem jobCompletedQueueItem)
        {
            jobCompletedQueueItem.NumberOfEntitiesUploaded = numberOfEntitiesUploadedForJob;
            numberOfEntitiesUploadedForJob = 0;
            return base.CompleteJobAsync(queryId, isLastThreadForThisTask, jobCompletedQueueItem);
        }

        /// <summary>
        /// The write diagnostics.
        /// </summary>
        /// <param name="workItem">
        /// The work item.  
        /// </param>
        /// <param name="fileUploadResult">
        /// file upload result
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task WriteDiagnosticsAsync(IQueueItem workItem, IFileUploadResult fileUploadResult)
        {
            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.folder != null)
            {
                var path = this.detailedTemporaryFileWriter.CombinePath(this.folder, $"{workItem.BatchNumber}");

                this.detailedTemporaryFileWriter.CreateDirectory(path);

                await this.detailedTemporaryFileWriter.WriteToFileAsync(
                    this.detailedTemporaryFileWriter.CombinePath(path, "url.txt"),
                    $"{fileUploadResult.HttpMethod} {fileUploadResult.Uri}");

                await this.detailedTemporaryFileWriter.WriteToFileAsync(
                    this.detailedTemporaryFileWriter.CombinePath(path, "request.txt"),
                    fileUploadResult.RequestContent);

                await this.detailedTemporaryFileWriter.WriteToFileAsync(
                    this.detailedTemporaryFileWriter.CombinePath(path, $"response-{fileUploadResult.StatusCode}.txt"),
                    await fileUploadResult.ResponseContent.ReadAsStringAsync());
            }
        }
    }
}
