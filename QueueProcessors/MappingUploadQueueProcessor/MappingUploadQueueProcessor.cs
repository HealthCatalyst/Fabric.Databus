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
    using System.IO;

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
        /// The file uploader factory.
        /// </summary>
        private readonly IFileUploaderFactory fileUploaderFactory;

        /// <summary>
        /// The main mapping upload relative url.
        /// </summary>
        private readonly string mainMappingUploadRelativeUrl;

        /// <summary>
        /// The secondary mapping upload relative url.
        /// </summary>
        private readonly string secondaryMappingUploadRelativeUrl;

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
        public MappingUploadQueueProcessor(IQueueContext queueContext, ILogger logger, IFileUploaderFactory fileUploaderFactory)
            : base(queueContext, logger)
        {
            this.fileUploaderFactory = fileUploaderFactory ?? throw new ArgumentNullException(nameof(fileUploaderFactory));
            this.mainMappingUploadRelativeUrl = queueContext.MainMappingUploadRelativeUrl;
            this.secondaryMappingUploadRelativeUrl = queueContext.SecondaryMappingUploadRelativeUrl;
        }

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
                    var fileUploader = this.fileUploaderFactory.Create(
                        this.QueueContext.Config.ElasticSearchUserName,
                        this.QueueContext.Config.ElasticSearchPassword,
                        this.Config.KeepIndexOnline);

                    fileUploader.SetupAlias(this.QueueContext.Config.Urls, this.QueueContext.Config.Index, this.QueueContext.Config.Alias).Wait();
                }
            }
        }

        /// <inheritdoc />
        protected override string GetId(MappingUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The upload single file.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="relativeUrl">
        /// The relative url.
        /// </param>
        private void UploadSingleFile(Stream stream, string relativeUrl)
        {
            var fileUploader = this.fileUploaderFactory.Create(
                this.QueueContext.Config.ElasticSearchUserName,
                this.QueueContext.Config.ElasticSearchPassword,
                this.Config.KeepIndexOnline);

            fileUploader.SendStreamToHosts(this.Config.Urls, relativeUrl, 1, stream, doLogContent: true, doCompress: false).Wait();
        }

        /// <summary>
        /// The upload files.
        /// </summary>
        /// <param name="wt">
        /// The wt.
        /// </param>
        private void UploadFiles(MappingUploadQueueItem wt)
        {
            var relativeUrl = string.IsNullOrEmpty(wt.PropertyName) ? this.mainMappingUploadRelativeUrl : this.secondaryMappingUploadRelativeUrl;

            this.UploadSingleFile(wt.Stream, relativeUrl);

            this.MyLogger.Verbose($"Uploaded mapping file: {wt.PropertyName} ");
        }
    }
}