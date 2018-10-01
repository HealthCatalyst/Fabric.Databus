// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlGetSchemaQueueProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlGetSchemaQueueProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SqlGetSchemaQueueProcessor
{
    using System;
    using System.IO;
    using System.Linq;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Schema;
    using Fabric.Shared;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The sql get schema queue processor.
    /// </summary>
    public class SqlGetSchemaQueueProcessor : BaseQueueProcessor<SqlJobQueueItem, SaveSchemaQueueItem>
    {
        /// <summary>
        /// The file uploader.
        /// </summary>
        private readonly IElasticSearchUploader elasticSearchUploader;

        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <inheritdoc />
        public SqlGetSchemaQueueProcessor(
            IQueueContext queueContext,
            ILogger logger,
            IElasticSearchUploader elasticSearchUploader,
            IQueueManager queueManager) : base(queueContext, logger, queueManager)
        {
            this.elasticSearchUploader = elasticSearchUploader ?? throw new ArgumentNullException(nameof(elasticSearchUploader));
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
        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        protected override void Handle(SqlJobQueueItem workItem)
        {
            var workItemLoads = workItem.Job.Data.DataSources;

            var dictionary = SchemaLoader.GetSchemasForLoads(workItemLoads, this.Config.ConnectionString, this.Config.TopLevelKeyColumn);

            var mappingItems = dictionary.ToList();

            if (this.Config.WriteDetailedTemporaryFilesToDisk)
            {
                foreach (var mappingItem in mappingItems)
                {
                    var filePath = Path.Combine(this.folder, $"{mappingItem.PropertyPath ?? "main"}.json");

                    File.AppendAllText(filePath, mappingItem.ToJsonPretty());
                }
            }

            this.AddToOutputQueue(new SaveSchemaQueueItem
            {
                Mappings = mappingItems,
                Job = workItem.Job
            });
        }

        /// <summary>
        /// The begin.
        /// </summary>
        /// <param name="isFirstThreadForThisTask">
        /// The is first thread for this task.
        /// </param>
        protected override void Begin(bool isFirstThreadForThisTask)
        {
            if (isFirstThreadForThisTask)
            {
                var config = this.QueueContext.Config;
                if (config.UploadToElasticSearch && config.DropAndReloadIndex)
                {
                    this.elasticSearchUploader.DeleteIndex(
                        config.Urls,
                        config.Index,
                        config.Alias).Wait();
                }
            }
        }

        /// <summary>
        /// The complete.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        /// The is last thread for this task.
        /// </param>
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        /// <summary>
        /// The get id.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected override string GetId(SqlJobQueueItem workItem)
        {
            return workItem.QueryId;
        }
    }
}