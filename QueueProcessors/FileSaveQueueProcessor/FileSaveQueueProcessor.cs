﻿namespace FileSaveQueueProcessor
{
    using System.Collections.Concurrent;
    using System.IO;
    using System.IO.Compression;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using QueueItems;

    public class FileSaveQueueProcessor : BaseQueueProcessor<FileUploadQueueItem, FileUploadQueueItem>
    {
        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public FileSaveQueueProcessor(IQueueContext queueContext)
            : base(queueContext)
        {
        }


        private void SaveFile(FileUploadQueueItem wt)
        {
            if (Config.WriteTemporaryFilesToDisk)
            {
                var fileExtension = Config.CompressFiles ? @".json.gz" : @".json";

                var path = Path.Combine(Config.LocalSaveFolder, $@"data-{wt.BatchNumber}{fileExtension}");

                lock (this._locks.GetOrAdd(path, s => new object()))
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

        protected override void Handle(FileUploadQueueItem workItem)
        {
            this.SaveFile(workItem);
        }

        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {

        }

        protected override string GetId(FileUploadQueueItem workItem)
        {
            return workItem.QueryId;
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
