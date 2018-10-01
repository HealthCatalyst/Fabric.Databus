namespace FileSaveQueueProcessor
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.IO.Compression;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using QueueItems;

    using Serilog;

    public class FileSaveQueueProcessor : BaseQueueProcessor<FileUploadQueueItem, FileUploadQueueItem>
    {
        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileSaveQueueProcessor.FileSaveQueueProcessor" /> class.
        /// </summary>
        /// <param name="queueContext">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager">
        /// The queue manager.
        /// </param>
        /// <param name="progressMonitor">
        /// The progress monitor.
        /// </param>
        public FileSaveQueueProcessor(IQueueContext queueContext, ILogger logger, IQueueManager queueManager, IProgressMonitor progressMonitor)
            : base(queueContext, logger, queueManager, progressMonitor)
        {
        }

        /// <summary>
        /// The save file.
        /// </summary>
        /// <param name="wt">
        /// The wt.
        /// </param>
        private void SaveFile(FileUploadQueueItem wt)
        {
            if (this.Config.WriteTemporaryFilesToDisk)
            {
                var fileExtension = this.Config.CompressFiles ? @".json.gz" : @".json";

                var path = Path.Combine(this.Config.LocalSaveFolder, $@"data-{wt.BatchNumber}{fileExtension}");

                lock (this._locks.GetOrAdd(path, s => new object()))
                {
                    this.MyLogger.Verbose($"Saving file: {path} ");

                    if (this.Config.CompressFiles)
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

                    this.MyLogger.Verbose($"Saved file: {path} ");
                }
            }

            this.AddToOutputQueue(wt);
        }

        /// <inheritdoc />
        protected override void Handle(FileUploadQueueItem workItem)
        {
            this.SaveFile(workItem);
        }

        /// <inheritdoc />
        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        /// <inheritdoc />
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {

        }

        /// <inheritdoc />
        protected override string GetId(FileUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <inheritdoc />
        protected override string LoggerName => "FileSave";

        /// <summary>
        /// The clean output folder.
        /// </summary>
        /// <param name="configLocalSaveFolder">
        /// The config local save folder.
        /// </param>
        public static void CleanOutputFolder(string configLocalSaveFolder)
        {
            DeleteDirectory(configLocalSaveFolder);
        }

        /// <summary>
        /// The delete directory.
        /// </summary>
        /// <param name="target_dir">
        /// The target_dir.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public static void DeleteDirectory(string target_dir)
        {
            if (!Directory.Exists(target_dir))
            {
                throw new Exception($"Folder {target_dir} does not exist");
            }

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
