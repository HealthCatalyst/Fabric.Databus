﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSavePipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   The file save queue processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

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
        private readonly ITemporaryFileWriter fileWriter;

        /// <summary>
        /// The _locks.
        /// </summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.PipelineSteps.FileSavePipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        ///     The queue context.
        /// </param>
        /// <param name="logger">
        ///     The logger.
        /// </param>
        /// <param name="queueManager">
        ///     The queue manager.
        /// </param>
        /// <param name="progressMonitor">
        ///     The progress monitor.
        /// </param>
        /// <param name="fileWriter"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="pipelineStepState"></param>
        public FileSavePipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            ITemporaryFileWriter fileWriter,
            CancellationToken cancellationToken,
            PipelineStepState pipelineStepState)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken, pipelineStepState)
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
        protected override async Task HandleAsync(FileUploadQueueItem workItem)
        {
            await this.SaveFile(workItem);
        }

        /// <inheritdoc />
        protected override string GetId(FileUploadQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The save file.
        /// </summary>
        /// <param name="workItem">
        /// The workItem.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task SaveFile(FileUploadQueueItem workItem)
        {
            if (this.Config.WriteTemporaryFilesToDisk)
            {
                var fileExtension = this.Config.CompressFiles ? @".json.gz" : @".json";

                var path = this.fileWriter.CombinePath(this.Config.LocalSaveFolder, $@"data-{workItem.BatchNumber}{fileExtension}");

                var semaphoreSlim = this.locks.GetOrAdd(path, s => new SemaphoreSlim(1, 1));

                // Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
                await semaphoreSlim.WaitAsync();
                try
                {
                    this.MyLogger.Verbose("Saving file: {path}", path);

                    if (this.Config.CompressFiles)
                    {
                        using (var fileStream = this.fileWriter.CreateFile(path))
                        {
                            using (var zipStream = new GZipStream(fileStream, CompressionMode.Compress, false))
                            {
                                workItem.Stream.Seek(0, SeekOrigin.Begin);
                                await workItem.Stream.CopyToAsync(zipStream);

                                fileStream.Flush();
                            }
                        }
                    }
                    else
                    {
                        using (var fileStream = this.fileWriter.CreateFile(path))
                        {
                            workItem.Stream.Seek(0, SeekOrigin.Begin);
                            await workItem.Stream.CopyToAsync(fileStream);

                            fileStream.Flush();
                        }
                    }

                    this.MyLogger.Verbose("Saved file: {path}", path);
                }
                finally
                {
                    // When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                    // This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                    semaphoreSlim.Release();
                }
            }

            await this.AddToOutputQueueAsync(workItem);
        }
    }
}
