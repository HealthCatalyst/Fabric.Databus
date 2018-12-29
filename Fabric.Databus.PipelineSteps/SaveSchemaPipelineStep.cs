// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveSchemaPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SaveSchemaPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The save schema queue processor.
    /// </summary>
    public class SaveSchemaPipelineStep : BasePipelineStep<SaveSchemaQueueItem, MappingUploadQueueItem>
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
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.PipelineSteps.SaveSchemaPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager">
        /// The queue Manager.
        /// </param>
        /// <param name="progressMonitor"></param>
        /// <param name="fileWriter"></param>
        /// <param name="entityJsonWriter"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="pipelineStepState"></param>
        public SaveSchemaPipelineStep(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            ITemporaryFileWriter fileWriter,
            IEntityJsonWriter entityJsonWriter,
            CancellationToken cancellationToken,
            PipelineStepState pipelineStepState) 
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken, pipelineStepState)
        {
            this.fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            this.entityJsonWriter = entityJsonWriter ?? throw new ArgumentNullException(nameof(entityJsonWriter));
            this.folder = this.fileWriter.CombinePath(this.Config.LocalSaveFolder, $"{this.UniqueId}-{this.LoggerName}");
            this.fileWriter.CreateDirectory(this.folder);
        }

        /// <inheritdoc />
        protected override sealed string LoggerName => "SaveSchema";

        /// <inheritdoc />
        protected override async Task HandleAsync(SaveSchemaQueueItem workItem)
        {
            foreach (var mapping in workItem.Mappings.OrderBy(m => m.SequenceNumber).ToList())
            {
                await this.SendMapping(mapping, workItem.Job);
            }
        }

        /// <inheritdoc />
        protected override string GetId(SaveSchemaQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The send mapping.
        /// </summary>
        /// <param name="mapping">
        /// The mapping.
        /// </param>
        /// <param name="workItemJob">
        /// The work item job.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task SendMapping(MappingItem mapping, IJob workItemJob)
        {
            var stream = new MemoryStream(); // do not use using since we'll pass it to next queue

            var propertyPath = mapping.PropertyPath == string.Empty ? null : mapping.PropertyPath;

            using (var textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                await this.entityJsonWriter.WriteMappingToStreamAsync(
                    mapping.Columns,
                    propertyPath,
                    textWriter,
                    mapping.PropertyType,
                    this.Config.EntityType);
            }

            await this.AddToOutputQueueAsync(
                new MappingUploadQueueItem
                    {
                        PropertyName = mapping.PropertyPath,
                        SequenceNumber = mapping.SequenceNumber,
                        Stream = stream,
                        Job = workItemJob
                    });

            string path = this.fileWriter.CombinePath(
                this.folder,
                propertyPath != null ? $@"mapping-{mapping.SequenceNumber}-{propertyPath}.json" : "mainmapping.json");

            if (this.fileWriter.IsWritingEnabled)
            {
                await this.fileWriter.WriteStreamAsync(path, stream);
            }
        }
    }
}