// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSavePipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   The file save queue processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FileSavePipelineStep
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;

    using BasePipelineStep;

    using ElasticSearchSqlFeeder.Interfaces;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The file save queue processor.
    /// </summary>
    public class FileSavePipelineStep : BasePipelineStep<FileUploadQueueItem, FileUploadQueueItem>
    {
        /// <summary>
        /// The file writer.
        /// </summary>
        private readonly IFileWriter fileWriter;

        /// <summary>
        /// The _locks.
        /// </summary>
        private readonly ConcurrentDictionary<string, object> locks = new ConcurrentDictionary<string, object>();

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileSavePipelineStep.FileSavePipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
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
        /// <param name="fileWriter"></param>
        /// <param name="cancellationToken"></param>
        public FileSavePipelineStep(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            IFileWriter fileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        }

        /// <inheritdoc />
        protected override string LoggerName => "FileSave";

        /// <summary>
        /// The clean output folder.
        /// </summary>
        /// <param name="configLocalSaveFolder">
        /// The config local save folder.
        /// </param>
        public void CleanOutputFolder(string configLocalSaveFolder)
        {
            this.fileWriter.DeleteDirectory(configLocalSaveFolder);
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

                lock (this.locks.GetOrAdd(path, s => new object()))
                {
                    this.MyLogger.Verbose($"Saving file: {path} ");

                    if (this.Config.CompressFiles)
                    {
                        using (var fileStream = this.fileWriter.CreateFile(path))
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
                        using (var fileStream = this.fileWriter.CreateFile(path))
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
    }
}
