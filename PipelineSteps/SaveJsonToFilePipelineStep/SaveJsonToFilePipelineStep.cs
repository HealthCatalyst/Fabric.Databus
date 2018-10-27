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

namespace SaveJsonToFilePipelineStep
{
    /// <summary>
    /// Reads a IJsonObjectQueueItem, saves it to file using the {id}.json and then passes the IJsonObjectQueueItem to the next step
    /// </summary>
    public class SaveJsonToFilePipelineStep : BasePipelineStep<IJsonObjectQueueItem, IJsonObjectQueueItem>
    {
        private readonly ITemporaryFileWriter fileWriter;
        private readonly IEntityJsonWriter entityJsonWriter;

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

        protected override string LoggerName => "SaveJsonToFile";

        protected override string GetId(IJsonObjectQueueItem workItem)
        {
            return workItem.Id;
        }

        protected override async Task HandleAsync(IJsonObjectQueueItem workItem)
        {
            var stream = new MemoryStream();

            await this.entityJsonWriter.WriteToStreamAsync(workItem.Document, stream);

            fileWriter.CreateDirectory(this.Config.LocalSaveFolder);

            string path = Path.Combine(this.Config.LocalSaveFolder, $"{workItem.Id}.json");

            if (this.fileWriter.IsWritingEnabled)
            {
                await fileWriter.WriteStreamAsync(path, stream);
            }

            await this.AddToOutputQueueAsync(workItem);
        }
    }
}
