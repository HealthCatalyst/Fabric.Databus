// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveJsonToFilePipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Reads a IJsonObjectQueueItem, saves it to file using the {id}.json and then passes the IJsonObjectQueueItem to the next step
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private readonly IEntityJsonWriter entityJsonWriter;

        /// <summary>
        /// The entity saved to json logger.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private readonly IEntitySavedToJsonLogger entitySavedToJsonLogger;

        /// <inheritdoc />
        public SaveJsonToFilePipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            ITemporaryFileWriter fileWriter,
            IEntityJsonWriter entityJsonWriter,
            IEntitySavedToJsonLogger entitySavedToJsonLogger,
            CancellationToken cancellationToken,
            PipelineStepState pipelineStepState)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken, pipelineStepState)
        {
            this.fileWriter = fileWriter ?? throw new System.ArgumentNullException(nameof(fileWriter));
            this.entityJsonWriter = entityJsonWriter ?? throw new System.ArgumentNullException(nameof(entityJsonWriter));
            this.entitySavedToJsonLogger = entitySavedToJsonLogger ?? throw new System.ArgumentNullException(nameof(entitySavedToJsonLogger));
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

            if (this.Config.LocalSaveFolder != null || this.entitySavedToJsonLogger.IsWritingEnabled)
            {
                await this.entityJsonWriter.WriteToStreamAsync(workItem.Document, stream);
            }

            if (this.entitySavedToJsonLogger.IsWritingEnabled)
            {
                stream.Seek(0, SeekOrigin.Begin);
                await this.entitySavedToJsonLogger.LogSavedEntityAsync(workItem.Id, stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            if (this.Config.LocalSaveFolder != null)
            {
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
