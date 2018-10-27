// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConvertDatabaseRowToJsonPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the ConvertDatabaseRowToJsonPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ConvertDatabaseRowToJsonPipelineStep
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Shared;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// Reads a ConvertDatabaseToJsonQueueItem and converts the data from rows into a json structure and stores it in JsonDocumentMergerQueueItem
    /// </summary>
    public class ConvertDatabaseRowToJsonPipelineStep : BasePipelineStep<ConvertDatabaseToJsonQueueItem, JsonDocumentMergerQueueItem>
    {
        /// <summary>
        /// The sequence barrier.
        /// </summary>
        private static readonly SequenceBarrier SequenceBarrier = new SequenceBarrier();

        /// <summary>
        /// The entity json writer.
        /// </summary>
        private readonly IEntityJsonWriter entityJsonWriter;

        /// <summary>
        /// The file writer.
        /// </summary>
        private readonly IDetailedTemporaryFileWriter fileWriter;

        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ConvertDatabaseRowToJsonPipelineStep.ConvertDatabaseRowToJsonPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager"></param>
        /// <param name="progressMonitor"></param>
        /// <param name="entityJsonWriter"></param>
        /// <param name="fileWriter"></param>
        /// <param name="cancellationToken"></param>
        public ConvertDatabaseRowToJsonPipelineStep(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            IEntityJsonWriter entityJsonWriter,
            IDetailedTemporaryFileWriter fileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.entityJsonWriter = entityJsonWriter ?? throw new ArgumentNullException(nameof(entityJsonWriter));
            this.fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            this.folder = Path.Combine(this.Config.LocalSaveFolder, $"{this.UniqueId}-ConvertToJson");
        }

        /// <inheritdoc />
        protected override string LoggerName => "ConvertDatabaseRow";

        /// <inheritdoc />
        protected override async Task HandleAsync(ConvertDatabaseToJsonQueueItem workItem)
        {
            await this.WriteObjectToJsonAsync(workItem);
        }

        /// <inheritdoc />
        protected override async Task CompleteAsync(string queryId, bool isLastThreadForThisTask)
        {
            await SequenceBarrier.CompleteQuery(queryId);
        }

        /// <inheritdoc />
        protected override string GetId(ConvertDatabaseToJsonQueueItem workItem)
        {
            return Convert.ToString(workItem.JoinColumnValue);
        }

        /// <summary>
        /// The write object to json.
        /// </summary>
        /// <param name="wt">
        /// The wt.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task WriteObjectToJsonAsync(ConvertDatabaseToJsonQueueItem wt)
        {
            var id = this.GetId(wt);

            var jsonForRows = await this.entityJsonWriter.GetJsonForRowForMergeAsync(
                wt.Columns,
                wt.Rows,
                wt.PropertyName,
                wt.PropertyTypes);

            var path = Path.Combine(this.folder, wt.PropertyName ?? "main");

            this.fileWriter.CreateDirectory(path);

            var sb = new StringBuilder();

            foreach (var jsonForRow in jsonForRows)
            {
                sb.AppendLine(jsonForRow.ToString());
            }

            if (this.fileWriter.IsWritingEnabled)
            {
                await this.fileWriter.WriteToFileAsync(Path.Combine(path, $"{id}.json"), sb.ToString());
            }

            await this.AddToOutputQueueAsync(
                new JsonDocumentMergerQueueItem
                    {
                        BatchNumber = wt.BatchNumber,
                        QueryId = wt.QueryId,
                        Id = id,
                        PropertyName = wt.PropertyName,
                        //JoinColumnValue = workItem.JoinColumnValue,
                        NewJObjects = jsonForRows
                    });

            var minimumEntityIdProcessed = SequenceBarrier.UpdateMinimumEntityIdProcessed(wt.QueryId, id);

            this.MyLogger.Verbose($"Add to queue: {id}");

            this.CleanListIfNeeded(wt.QueryId, minimumEntityIdProcessed);
        }

        /// <summary>
        /// The clean list if needed.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="minimumEntityIdProcessed">
        /// The minimum entity id processed.
        /// </param>
        private void CleanListIfNeeded(string queryId, string minimumEntityIdProcessed)
        {
        }
    }
}
