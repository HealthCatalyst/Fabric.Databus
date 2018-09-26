namespace MappingUploadQueueProcessor
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using BaseQueueProcessor;

    using ElasticSearchApiCaller;

    using ElasticSearchSqlFeeder.Shared;

    using QueueItems;

    public class MappingUploadQueueProcessor : BaseQueueProcessor<MappingUploadQueueItem, EndPointQueueItem>
    {
        readonly FileUploader _fileUploader;
        private readonly string _mainMappingUploadRelativeUrl;
        private readonly string _secondaryMappingUploadRelativeUrl;

        public MappingUploadQueueProcessor(QueueContext queueContext)
            : base(queueContext)
        {
            this._fileUploader = new FileUploader(queueContext.Config.ElasticSearchUserName,
                queueContext.Config.ElasticSearchPassword, Config.KeepIndexOnline);
            this._mainMappingUploadRelativeUrl = queueContext.MainMappingUploadRelativeUrl;
            this._secondaryMappingUploadRelativeUrl = queueContext.SecondaryMappingUploadRelativeUrl;
        }

        private void UploadFiles(MappingUploadQueueItem wt)
        {

            var relativeUrl = string.IsNullOrEmpty(wt.PropertyName) ? this._mainMappingUploadRelativeUrl : this._secondaryMappingUploadRelativeUrl;

            this.UploadSingleFile(wt.Stream, relativeUrl ).Wait();

            MyLogger.Trace($"Uploaded mapping file: {wt.PropertyName} ");

        }

        private Task UploadSingleFile(Stream stream, string relativeUrl)
        {
            try
            {
                return Task.Run(
                        async () =>
                        {
                            await this._fileUploader.SendStreamToHosts(Config.Urls, relativeUrl, 1, stream, doLogContent: true, doCompress: false);
                        });

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
                    return false;
                });

                throw;
            }
        }

        protected override void Handle(MappingUploadQueueItem workItem)
        {
            this.UploadFiles(workItem);
        }

        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        protected override string GetId(MappingUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }

        protected override string LoggerName => "MappingUpload";
    }
}