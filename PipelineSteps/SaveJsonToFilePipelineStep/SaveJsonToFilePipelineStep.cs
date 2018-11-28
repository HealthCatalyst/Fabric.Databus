// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveJsonToFilePipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Reads a IJsonObjectQueueItem, saves it to file using the {id}.json and then passes the IJsonObjectQueueItem to the next step
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SaveJsonToFilePipelineStep
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// Reads a IJsonObjectQueueItem, saves it to file using the {id}.json and then passes the IJsonObjectQueueItem to the next step
    /// </summary>
    public class SaveJsonToFilePipelineStep : BasePipelineStep<IJsonObjectQueueItem, IJsonObjectQueueItem>
    {
        /// <summary>
        /// The file writer.
        /// </summary>
        private readonly ITemporaryFileWriter fileWriter;

        /// <summary>
        /// The entity json writer.
        /// </summary>
        private readonly IEntityJsonWriter entityJsonWriter;

        /// <inheritdoc />
        public SaveJsonToFilePipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            ITemporaryFileWriter fileWriter,
            IEntityJsonWriter entityJsonWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.fileWriter = fileWriter ?? throw new System.ArgumentNullException(nameof(fileWriter));
            this.entityJsonWriter = entityJsonWriter ?? throw new System.ArgumentNullException(nameof(entityJsonWriter));
        }

        /// <inheritdoc />
        protected override string LoggerName => "SaveJsonToFile";

        /// <inheritdoc />
        protected override string GetId(IJsonObjectQueueItem workItem)
        {
            return workItem.Id;
        }

        /// <inheritdoc />
        protected override async Task HandleAsync(IJsonObjectQueueItem workItem)
        {
            var stream = new MemoryStream();

            if (this.Config.LocalSaveFolder != null)
            {
                await this.entityJsonWriter.WriteToStreamAsync(workItem.Document, stream);

                this.fileWriter.CreateDirectory(this.Config.LocalSaveFolder);

                string path = this.fileWriter.CombinePath(this.Config.LocalSaveFolder, $"{workItem.Id}.json");

                if (this.fileWriter.IsWritingEnabled)
                {
                    await this.fileWriter.WriteStreamAsync(path, stream);
                }
            }

            await this.AddToOutputQueueAsync(workItem);
        }
    }
}
