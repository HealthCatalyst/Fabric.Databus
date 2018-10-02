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

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Shared;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The convert database row to json queue processor.
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

        private readonly IFileWriter fileWriter;

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
            IFileWriter fileWriter,
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
        protected override void Handle(ConvertDatabaseToJsonQueueItem workItem)
        {
            this.WriteObjectToJson(workItem);
        }

        /// <inheritdoc />
        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        /// <inheritdoc />
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            SequenceBarrier.CompleteQuery(queryId);
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
        private void WriteObjectToJson(ConvertDatabaseToJsonQueueItem wt)
        {
            var id = this.GetId(wt);

            var jsonForRows = this.entityJsonWriter.GetJsonForRowForMerge(wt.Columns, wt.Rows, wt.PropertyName, wt.PropertyTypes);

            if (this.Config.WriteDetailedTemporaryFilesToDisk)
            {
                var path = Path.Combine(this.folder, wt.PropertyName ?? "main");

                this.fileWriter.CreateDirectory(path);

                var sb = new StringBuilder();

                foreach (var jsonForRow in jsonForRows)
                {
                    sb.AppendLine(jsonForRow.ToString());
                }

                this.fileWriter.WriteToFile(Path.Combine(path, $"{id}.json"), sb.ToString());
            }

            this.AddToOutputQueue(new JsonDocumentMergerQueueItem
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
