// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUploadPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the FileUploadPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FileUploadPipelineStep
{
    using System.Threading;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;

    using QueueItems;

    using Serilog;

    /// <summary>
    /// The file upload queue processor.
    /// </summary>
    public class FileUploadPipelineStep : BasePipelineStep<FileUploadQueueItem, EndPointQueueItem>
    {
        /// <summary>
        /// The file uploader.
        /// </summary>
        private readonly IElasticSearchUploader elasticSearchUploader;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileUploadPipelineStep.FileUploadPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The queue context.
        /// </param>
        /// <param name="elasticSearchUploader">
        /// The elasticSearchUploader
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager">
        /// The queue Manager.
        /// </param>
        /// <param name="progressMonitor"></param>
        /// <param name="cancellationToken"></param>
        public FileUploadPipelineStep(
            IJobConfig jobConfig, 
            IElasticSearchUploader elasticSearchUploader, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.elasticSearchUploader = elasticSearchUploader;
        }

        /// <summary>
        /// The logger name.
        /// </summary>
        protected override string LoggerName => "FileUpload";

        /// <inheritdoc />
        protected override void Handle(FileUploadQueueItem workItem)
        {
            this.UploadFile(workItem);
        }

        /// <inheritdoc />
        protected override void Begin(bool isFirstThreadForThisTask)
        {
            if (isFirstThreadForThisTask)
            {
                this.elasticSearchUploader.StartUpload().Wait();
            }
        }

        /// <summary>
        /// The complete.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        /// The is last thread for this task.
        /// </param>
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            if (isLastThreadForThisTask)
            {
                this.elasticSearchUploader.FinishUpload().Wait();
            }
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
        protected override string GetId(FileUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The upload file.
        /// </summary>
        /// <param name="wt">
        /// The wt.
        /// </param>
        private void UploadFile(FileUploadQueueItem wt)
        {
            this.elasticSearchUploader.SendDataToHosts(
                    wt.BatchNumber,
                    wt.Stream,
                    doLogContent: false,
                    doCompress: this.Config.CompressFiles)
                .Wait();

            this.MyLogger.Verbose($"Uploaded batch: {wt.BatchNumber} ");

        }
    }
}
