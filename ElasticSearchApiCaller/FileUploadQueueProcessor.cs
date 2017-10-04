using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.Shared;
using Newtonsoft.Json;

namespace ElasticSearchApiCaller
{
    public class FileUploadQueueProcessor : BaseQueueProcessor<FileUploadQueueItem, EndPointQueueItem>
    {
        readonly FileUploader _fileUploader;
        private readonly string _relativeUrlForPosting;

        public FileUploadQueueProcessor(QueueContext queueContext)
            : base(queueContext)
        {
            _fileUploader = new FileUploader(queueContext.Config.ElasticSearchUserName,
                queueContext.Config.ElasticSearchPassword);
            _relativeUrlForPosting = queueContext.BulkUploadRelativeUrl;
        }

        private void UploadFile(FileUploadQueueItem wt)
        {
            try
            {
                Task.Run(async () =>
                    {
                        await _fileUploader.SendStreamToHosts(Config.Urls, _relativeUrlForPosting,
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

        protected override void Handle(FileUploadQueueItem workitem)
        {
            UploadFile(workitem);
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            if (isLastThreadForThisTask)
            {
                Task.Run(async () =>
                {
                    await _fileUploader.FinishUpload(Config.Urls, Config.Index, Config.Alias);
                })
                .Wait();
            }
        }

        protected override string GetId(FileUploadQueueItem workitem)
        {
            return workitem.QueryId;
        }

        protected override string LoggerName => "FileUpload";

    }

    public class FileUploadQueueItem : IQueueItem
    {
        public int BatchNumber { get; set; }

        public string PropertyName { get; set; }

        public string QueryId { get; set; }

        [JsonIgnore]
        public Stream Stream { get; set; }
    }
}
