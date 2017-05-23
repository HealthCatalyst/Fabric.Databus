using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using ElasticSearchSqlFeeder.Shared;
using NLog;

namespace ElasticSearchApiCaller
{
    public class FileSaveQueueProcessor : BaseQueueProcessor<FileUploadQueueItem, FileUploadQueueItem>
    {
        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public FileSaveQueueProcessor(QueueContext queueContext)
            : base(queueContext)
        {
        }


        private void SaveFile(FileUploadQueueItem wt)
        {
            if (Config.WriteTemporaryFilesToDisk)
            {
                var fileExtension = Config.CompressFiles ? @".json.gz" : @".json";

                var path = Path.Combine(Config.LocalSaveFolder, $@"data-{wt.BatchNumber}{fileExtension}");

                lock (_locks.GetOrAdd(path, s => new object()))
                {
                    MyLogger.Trace($"Saving file: {path} ");

                    if (Config.CompressFiles)
                    {
                        using (var fileStream = File.Create(path))
                        {
                            using (var zipStream = new GZipStream(fileStream, CompressionMode.Compress, false))
                            {
                                wt.Stream.Seek(0, SeekOrigin.Begin);
                                wt.Stream.CopyTo(zipStream);

                                fileStream.Flush();
                            }
                        }
                    }
                    else
                    {
                        using (var fileStream = File.Create(path))
                        {
                            wt.Stream.Seek(0, SeekOrigin.Begin);
                            wt.Stream.CopyTo(fileStream);

                            fileStream.Flush();
                        }
                    }

                    MyLogger.Trace($"Saved file: {path} ");
                }
            }

            AddToOutputQueue(wt);
        }

        protected override void Handle(FileUploadQueueItem workitem)
        {
            SaveFile(workitem);
        }

        protected override void Complete(string queryId)
        {

        }

        protected override string GetId(FileUploadQueueItem workitem)
        {
            return workitem.QueryId;
        }

        protected override string LoggerName => "FileSave";

        public static void CleanOutputFolder(string configLocalSaveFolder)
        {
            DeleteDirectory(configLocalSaveFolder);
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

        }
    }

}
