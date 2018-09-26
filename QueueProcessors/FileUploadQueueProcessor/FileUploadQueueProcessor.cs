namespace FileUploadQueueProcessor
{
    using System;
    using System.Threading.Tasks;

    using BaseQueueProcessor;

    using ElasticSearchApiCaller;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using QueueItems;

    public class FileUploadQueueProcessor : BaseQueueProcessor<FileUploadQueueItem, EndPointQueueItem>
    {
        readonly FileUploader _fileUploader;
        private readonly string _relativeUrlForPosting;

        public FileUploadQueueProcessor(IQueueContext queueContext)
            : base(queueContext)
        {
            this._fileUploader = new FileUploader(queueContext.Config.ElasticSearchUserName,
                queueContext.Config.ElasticSearchPassword, Config.KeepIndexOnline);
            this._relativeUrlForPosting = queueContext.BulkUploadRelativeUrl;
        }

        private void UploadFile(FileUploadQueueItem wt)
        {
            try
            {
                Task.Run(async () =>
                    {
                        await this._fileUploader.SendStreamToHosts(Config.Urls, this._relativeUrlForPosting,
                            wt.BatchNumber, wt.Stream, doLogContent: false, doCompress: Config.CompressFiles);
                    })
                    .Wait();

                MyLogger.Trace($"Uploaded batch: {wt.BatchNumber} ");

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
                    MyLogger.Error(x);
                    return false; // Let anything else stop the application.
                });
            }
        }

        protected override void Handle(FileUploadQueueItem workItem)
        {
            this.UploadFile(workItem);
        }

        protected override void Begin(bool isFirstThreadForThisTask)
        {
            if (isFirstThreadForThisTask)
            {
                Task.Run(async () =>
                    {
                        await this._fileUploader.StartUpload(Config.Urls, Config.Index, Config.Alias);
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
                    await this._fileUploader.FinishUpload(Config.Urls, Config.Index, Config.Alias);
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
