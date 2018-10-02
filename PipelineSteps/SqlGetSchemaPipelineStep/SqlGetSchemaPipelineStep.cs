// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlGetSchemaPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SqlGetSchemaPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SqlGetSchemaPipelineStep
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Shared;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The sql get schema queue processor.
    /// </summary>
    public class SqlGetSchemaPipelineStep : BasePipelineStep<SqlJobQueueItem, SaveSchemaQueueItem>
    {
        /// <summary>
        /// The file uploader.
        /// </summary>
        private readonly IElasticSearchUploader elasticSearchUploader;

        /// <summary>
        /// The schema loader.
        /// </summary>
        private readonly ISchemaLoader schemaLoader;

        /// <summary>
        /// The file writer.
        /// </summary>
        private readonly IFileWriter fileWriter;

        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <inheritdoc />
        public SqlGetSchemaPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IElasticSearchUploader elasticSearchUploader,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            ISchemaLoader schemaLoader,
            IFileWriter fileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.elasticSearchUploader = elasticSearchUploader ?? throw new ArgumentNullException(nameof(elasticSearchUploader));
            this.schemaLoader = schemaLoader ?? throw new ArgumentNullException(nameof(schemaLoader));
            this.fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            this.folder = Path.Combine(this.Config.LocalSaveFolder, $"{this.UniqueId}-SqlGetSchema");
            if (this.Config.WriteDetailedTemporaryFilesToDisk)
            {
                Directory.CreateDirectory(this.folder);
            }
        }

        /// <summary>
        /// The logger name.
        /// </summary>
        protected override string LoggerName => "SqlGetSchema";

        /// <inheritdoc />
        protected override void Handle(SqlJobQueueItem workItem)
        {
            var workItemLoads = workItem.Job.Data.DataSources;

            var dictionary = this.schemaLoader.GetSchemasForLoads(workItemLoads);

            var mappingItems = dictionary.ToList();

            if (this.Config.WriteDetailedTemporaryFilesToDisk)
            {
                foreach (var mappingItem in mappingItems)
                {
                    var filePath = Path.Combine(this.folder, $"{mappingItem.PropertyPath ?? "main"}.json");

                    this.fileWriter.WriteToFile(filePath, mappingItem.ToJsonPretty());
                }
            }

            this.AddToOutputQueue(new SaveSchemaQueueItem
            {
                Mappings = mappingItems,
                Job = workItem.Job
            });
        }

        /// <inheritdoc />
        protected override void Begin(bool isFirstThreadForThisTask)
        {
            if (isFirstThreadForThisTask)
            {
                this.elasticSearchUploader.DeleteIndex().Wait();
            }
        }

        /// <inheritdoc />
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        /// <inheritdoc />
        protected override string GetId(SqlJobQueueItem workItem)
        {
            return workItem.QueryId;
        }
    }
}