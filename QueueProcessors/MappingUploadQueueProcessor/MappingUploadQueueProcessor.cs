// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingUploadQueueProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MappingUploadQueueProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MappingUploadQueueProcessor
{
    using System;
    using System.Threading;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The mapping upload queue processor.
    /// </summary>
    public class MappingUploadQueueProcessor : BaseQueueProcessor<MappingUploadQueueItem, SqlJobQueueItem>
    {
        /// <summary>
        /// The file uploader.
        /// </summary>
        private readonly IElasticSearchUploader elasticSearchUploader;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MappingUploadQueueProcessor.MappingUploadQueueProcessor" /> class.
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
        public MappingUploadQueueProcessor(
            IJobConfig jobConfig,
            ILogger logger,
            IElasticSearchUploader elasticSearchUploader,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.elasticSearchUploader = elasticSearchUploader ?? throw new ArgumentNullException(nameof(elasticSearchUploader));
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
        protected override void Handle(MappingUploadQueueItem workItem)
        {
            this.UploadFiles(workItem);

            this.AddToOutputQueue(new SqlJobQueueItem { Job = workItem.Job });
        }

        /// <inheritdoc />
        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        /// <inheritdoc />
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            if (isLastThreadForThisTask)
            {
                // set up aliases
                this.elasticSearchUploader.SetupAlias().Wait();
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
        private void UploadFiles(MappingUploadQueueItem wt)
        {
            if (string.IsNullOrEmpty(wt.PropertyName))
            {
                this.elasticSearchUploader.SendMainMappingFileToHosts(1, wt.Stream, doLogContent: true, doCompress: false).Wait();
            }
            else
            {
                this.elasticSearchUploader.SendNestedMappingFileToHosts(1, wt.Stream, doLogContent: true, doCompress: false).Wait();
            }

            this.MyLogger.Verbose($"Uploaded mapping file: {wt.PropertyName} ");
        }
    }
}