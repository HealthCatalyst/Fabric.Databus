// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElasticSearchFileUploadPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the ElasticSearchFileUploadPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The file upload queue processor.
    /// </summary>
    public class ElasticSearchFileUploadPipelineStep : BasePipelineStep<FileUploadQueueItem, EndPointQueueItem>
    {
        /// <summary>
        /// The file uploader.
        /// </summary>
        private readonly IElasticSearchUploader elasticSearchUploader;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.PipelineSteps.ElasticSearchFileUploadPipelineStep" /> class.
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
        /// <param name="pipelineStepState"></param>
        public ElasticSearchFileUploadPipelineStep(
            IJobConfig jobConfig, 
            IElasticSearchUploader elasticSearchUploader, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken,
            PipelineStepState pipelineStepState)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken, pipelineStepState)
        {
            this.elasticSearchUploader = elasticSearchUploader;
        }

        /// <inheritdoc />
        protected override string LoggerName => "FileUpload";

        /// <inheritdoc />
        protected override async Task HandleAsync(FileUploadQueueItem workItem)
        {
            await this.UploadFileAsync(workItem);
        }

        /// <inheritdoc />
        protected override async Task BeginAsync(bool isFirstThreadForThisTask)
        {
            if (isFirstThreadForThisTask)
            {
                await this.elasticSearchUploader.StartUploadAsync();
            }
        }

        /// <inheritdoc />
        protected override async Task CompleteBatchAsync(string queryId, bool isLastThreadForThisTask, int batchNumber, IBatchCompletedQueueItem batchCompletedQueueItem)
        {
            if (isLastThreadForThisTask)
            {
                await this.elasticSearchUploader.FinishUploadAsync();
            }
        }

        /// <inheritdoc />
        protected override string GetId(FileUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The upload file.
        /// </summary>
        /// <param name="workItem">
        /// The workItem.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task UploadFileAsync(FileUploadQueueItem workItem)
        {
            await this.elasticSearchUploader.SendDataToHostsAsync(
                    workItem.BatchNumber,
                    workItem.Stream,
                    doLogContent: false,
                    doCompress: this.Config.CompressFiles);

            this.MyLogger.Verbose("Uploaded batch: {BatchNumber}", workItem.BatchNumber);
        }
    }
}
