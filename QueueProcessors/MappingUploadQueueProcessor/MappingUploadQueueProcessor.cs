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
        /// <param name="queueContext">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="elasticSearchUploader"></param>
        public MappingUploadQueueProcessor(IQueueContext queueContext, ILogger logger, IElasticSearchUploader elasticSearchUploader)
            : base(queueContext, logger)
        {
            this.elasticSearchUploader = elasticSearchUploader ?? throw new ArgumentNullException(nameof(elasticSearchUploader));
        }

        /// <inheritdoc />
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
                if (this.QueueContext.Config.UploadToElasticSearch)
                {
                    this.elasticSearchUploader.SetupAlias(this.QueueContext.Config.Urls, this.QueueContext.Config.Index, this.QueueContext.Config.Alias).Wait();
                }
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
                this.elasticSearchUploader.SendMainMappingFileToHosts(this.Config.Urls, this.Config.Index, 1, wt.Stream, doLogContent: true, doCompress: false).Wait();
            }
            else
            {
                this.elasticSearchUploader.SendNestedMappingFileToHosts(this.Config.Urls, this.Config.Index, this.Config.EntityType, 1, wt.Stream, doLogContent: true, doCompress: false).Wait();
            }

            this.MyLogger.Verbose($"Uploaded mapping file: {wt.PropertyName} ");
        }
    }
}