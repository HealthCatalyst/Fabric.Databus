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
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;
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
        private readonly IDetailedTemporaryFileWriter fileWriter;

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
            IDetailedTemporaryFileWriter fileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.elasticSearchUploader = elasticSearchUploader ?? throw new ArgumentNullException(nameof(elasticSearchUploader));
            this.schemaLoader = schemaLoader ?? throw new ArgumentNullException(nameof(schemaLoader));
            this.fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            this.folder = this.fileWriter.CombinePath(this.Config.LocalSaveFolder, $"{this.UniqueId}-{LoggerName}");
            this.fileWriter.CreateDirectory(this.folder);
        }

        /// <summary>
        /// The logger name.
        /// </summary>
        protected override string LoggerName => "SqlGetSchema";

        /// <inheritdoc />
        protected override async Task HandleAsync(SqlJobQueueItem workItem)
        {
            var workItemLoads = workItem.Job.Data.DataSources;

            var dictionary = this.schemaLoader.GetSchemasForLoads(workItemLoads);

            var mappingItems = dictionary.ToList();

            foreach (var mappingItem in mappingItems)
            {
                var filePath = this.fileWriter.CombinePath(this.folder, $"{mappingItem.PropertyPath ?? "main"}.json");

                if (this.fileWriter.IsWritingEnabled)
                {
                    await this.fileWriter.WriteToFileAsync(filePath, mappingItem.ToJsonPretty());
                }
            }

            await this.AddToOutputQueueAsync(new SaveSchemaQueueItem { Mappings = mappingItems, Job = workItem.Job });
        }

        /// <inheritdoc />
        protected override async Task BeginAsync(bool isFirstThreadForThisTask)
        {
            if (isFirstThreadForThisTask)
            {
                await this.elasticSearchUploader.DeleteIndex();
            }
        }

        /// <inheritdoc />
        protected override string GetId(SqlJobQueueItem workItem)
        {
            return workItem.QueryId;
        }
    }
}