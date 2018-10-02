// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveSchemaPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SaveSchemaPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SaveSchemaPipelineStep
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces;

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
        private readonly IEntityJsonWriter entityJsonWriter;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:SaveSchemaPipelineStep.SaveSchemaPipelineStep" /> class.
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
        public SaveSchemaPipelineStep(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            ITemporaryFileWriter fileWriter,
            IEntityJsonWriter entityJsonWriter,
            CancellationToken cancellationToken) 
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            this.entityJsonWriter = entityJsonWriter ?? throw new ArgumentNullException(nameof(entityJsonWriter));
        }

        /// <inheritdoc />
        protected override string LoggerName => "SaveSchema";

        /// <inheritdoc />
        protected override async System.Threading.Tasks.Task HandleAsync(SaveSchemaQueueItem workItem)
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

            string path = Path.Combine(
                this.Config.LocalSaveFolder,
                propertyPath != null ? $@"mapping-{mapping.SequenceNumber}-{propertyPath}.json" : "mainmapping.json");

            await this.fileWriter.WriteStreamAsync(path, stream);
        }
    }
}