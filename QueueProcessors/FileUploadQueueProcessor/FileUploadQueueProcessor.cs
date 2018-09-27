// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUploadQueueProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the FileUploadQueueProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FileUploadQueueProcessor
{
    using System;

    using BaseQueueProcessor;

    using ElasticSearchApiCaller;

    using ElasticSearchSqlFeeder.Interfaces;

    using QueueItems;

    using Serilog;

    /// <summary>
    /// The file upload queue processor.
    /// </summary>
    public class FileUploadQueueProcessor : BaseQueueProcessor<FileUploadQueueItem, EndPointQueueItem>
    {
        /// <summary>
        /// The file uploader.
        /// </summary>
        private readonly IFileUploader fileUploader;

        /// <summary>
        /// The relative url for posting.
        /// </summary>
        private readonly string relativeUrlForPosting;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadQueueProcessor"/> class.
        /// </summary>
        /// <param name="queueContext">
        /// The queue context.
        /// </param>
        /// <param name="fileUploaderFactory">fileUploader Factory</param>
        public FileUploadQueueProcessor(IQueueContext queueContext, IFileUploaderFactory fileUploaderFactory, ILogger logger)
            : base(queueContext, logger)
        {
            if (fileUploaderFactory == null)
            {
                throw new ArgumentNullException(nameof(fileUploaderFactory));
            }

            this.fileUploader = fileUploaderFactory.Create(
                queueContext.Config.ElasticSearchUserName,
                queueContext.Config.ElasticSearchPassword,
                this.Config.KeepIndexOnline);

            this.relativeUrlForPosting = queueContext.BulkUploadRelativeUrl;
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
                this.fileUploader.StartUpload(Config.Urls, Config.Index, Config.Alias).Wait();
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
                this.fileUploader.FinishUpload(Config.Urls, Config.Index, Config.Alias).Wait();
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
            this.fileUploader.SendStreamToHosts(
                    this.Config.Urls,
                    this.relativeUrlForPosting,
                    wt.BatchNumber,
                    wt.Stream,
                    doLogContent: false,
                    doCompress: this.Config.CompressFiles)
                .Wait();

            this.MyLogger.Verbose($"Uploaded batch: {wt.BatchNumber} ");

        }
    }
}
