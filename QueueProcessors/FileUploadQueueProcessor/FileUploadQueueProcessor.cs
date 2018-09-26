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
    using System.Threading.Tasks;

    using BaseQueueProcessor;

    using ElasticSearchApiCaller;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using QueueItems;

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
        public FileUploadQueueProcessor(IQueueContext queueContext, IFileUploaderFactory fileUploaderFactory)
            : base(queueContext)
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
        /// The upload file.
        /// </summary>
        /// <param name="wt">
        /// The wt.
        /// </param>
        private void UploadFile(FileUploadQueueItem wt)
        {
            try
            {
                Task.Run(async () =>
                            {
                                await this.fileUploader.SendStreamToHosts(
                                    this.Config.Urls,
                                    this.relativeUrlForPosting,
                                    wt.BatchNumber,
                                    wt.Stream,
                                    doLogContent: false,
                                    doCompress: this.Config.CompressFiles);
                            })
                    .Wait();

                this.MyLogger.Trace($"Uploaded batch: {wt.BatchNumber} ");

            }
            catch (AggregateException ae)
            {
                ae.Handle((x) =>
                {
                    //if (x is UnauthorizedAccessException) // This we know how to handle.
                    //{
                    //    Console.WriteLine("You do not have permission to access all folders in this path.");
                    //    Console.WriteLine("See your network administrator or try another path.");
                    //    return true;
                    //}

                    this.MyLogger.Error(x);
                    return false; // Let anything else stop the application.
                });
            }
        }

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
                Task.Run(async () =>
                    {
                        await this.fileUploader.StartUpload(Config.Urls, Config.Index, Config.Alias);
                    })
                    .Wait();
            }
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            if (isLastThreadForThisTask)
            {
                Task.Run(async () =>
                {
                    await this.fileUploader.FinishUpload(Config.Urls, Config.Index, Config.Alias);
                })
                .Wait();
            }
        }

        protected override string GetId(FileUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }

        protected override string LoggerName => "FileUpload";

    }
}
