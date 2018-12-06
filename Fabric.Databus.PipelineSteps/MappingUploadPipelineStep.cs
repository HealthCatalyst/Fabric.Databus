// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingUploadPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the MappingUploadPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The mapping upload queue processor.
    /// </summary>
    public class MappingUploadPipelineStep : BasePipelineStep<MappingUploadQueueItem, SqlJobQueueItem>
    {
        /// <summary>
        /// The file uploader.
        /// </summary>
        private readonly IElasticSearchUploader elasticSearchUploader;

        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.PipelineSteps.MappingUploadPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="elasticSearchUploader"></param>
        /// <param name="queueManager"></param>
        /// <param name="progressMonitor"></param>
        /// <param name="cancellationToken"></param>
        public MappingUploadPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IElasticSearchUploader elasticSearchUploader,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.elasticSearchUploader = elasticSearchUploader ?? throw new ArgumentNullException(nameof(elasticSearchUploader));
            this.folder = Path.Combine(this.Config.LocalSaveFolder, $"{this.UniqueId}-{this.LoggerName}");
            if (this.Config.WriteDetailedTemporaryFilesToDisk)
            {
                Directory.CreateDirectory(this.folder);
            }

        }

        /// <inheritdoc />s
        /// <summary>
        /// The logger name.
        /// </summary>
        protected override string LoggerName => "MappingUpload";

        /// <inheritdoc />
        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        protected override async Task HandleAsync(MappingUploadQueueItem workItem)
        {
            await this.UploadFilesAsync(workItem);

            await this.AddToOutputQueueAsync(new SqlJobQueueItem { Job = workItem.Job });
        }

        /// <inheritdoc />
        protected override async Task CompleteAsync(string queryId, bool isLastThreadForThisTask)
        {
            if (isLastThreadForThisTask)
            {
                // set up aliases
                await this.elasticSearchUploader.SetupAliasAsync();
            }
        }

        /// <inheritdoc />
        protected override string GetId(MappingUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The upload files.
        /// </summary>
        /// <param name="wt">
        /// The wt.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task UploadFilesAsync(MappingUploadQueueItem wt)
        {
            if (string.IsNullOrEmpty(wt.PropertyName))
            {
                await this.elasticSearchUploader.SendMainMappingFileToHostsAsync(1, wt.Stream, doLogContent: true, doCompress: false);
            }
            else
            {
                await this.elasticSearchUploader.SendNestedMappingFileToHostsAsync(1, wt.Stream, doLogContent: true, doCompress: false);
            }

            this.MyLogger.Verbose("Uploaded mapping file: {PropertyName} {@wt}", wt.PropertyName, wt);
        }
    }
}