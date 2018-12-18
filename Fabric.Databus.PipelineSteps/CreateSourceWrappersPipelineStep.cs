// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateSourceWrappersPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the CreateSourceWrappersPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Shared;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The sql combine source wrappers pipeline step.
    /// </summary>
    public class CreateSourceWrappersPipelineStep : BasePipelineStep<SqlDataLoadedQueueItem, SourceWrapperCollectionQueueItem>
    {
        /// <summary>
        /// The detailed temporary file writer.
        /// </summary>
        private readonly IDetailedTemporaryFileWriter detailedTemporaryFileWriter;

        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <summary>
        /// The source wrapper collection.
        /// </summary>
        private SourceWrapperCollection sourceWrapperCollection = new SourceWrapperCollection();

        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        private int batchNumber;

        /// <summary>
        /// The top level key column.
        /// </summary>
        private string topLevelKeyColumn;

        /// <inheritdoc />
        public CreateSourceWrappersPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            IDetailedTemporaryFileWriter detailedTemporaryFileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.detailedTemporaryFileWriter = detailedTemporaryFileWriter ?? throw new ArgumentNullException(nameof(detailedTemporaryFileWriter));

            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.Config.LocalSaveFolder != null)
            {
                this.folder = this.detailedTemporaryFileWriter.CombinePath(this.Config.LocalSaveFolder, $"{this.UniqueId}-{this.LoggerName}");
            }
        }

        /// <inheritdoc />
        protected override sealed string LoggerName => "CreateSourceWrappers";

        /// <inheritdoc />
        protected override async Task HandleAsync(SqlDataLoadedQueueItem workItem)
        {
            var keyLevels = workItem.PropertyName.Count(c => c == '.');

            var keys = new List<string>();
            for (var i = 0; i < keyLevels + 1; i++)
            {
                var keyLevelColumnName = $"KeyLevel{i + 1}";
                if (workItem.Columns.Any(c => c.Name.Equals(keyLevelColumnName)))
                {
                    keys.Add(keyLevelColumnName);
                }
            }

            this.sourceWrapperCollection.Add(
                new SourceWrapper(
                    workItem.QueryId,
                    workItem.Columns,
                    workItem.PropertyName,
                    workItem.Rows,
                    keys,
                    workItem.PropertyType != "object",
                    this.Config.KeepTemporaryLookupColumnsInOutput));

            await this.WriteDiagnostics(workItem);

            this.batchNumber = workItem.BatchNumber;

            this.topLevelKeyColumn = workItem.TopLevelKeyColumn;
        }

        /// <inheritdoc />
        /// <summary>
        /// The complete async.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        /// The is last thread for this task.
        /// </param>
        /// <param name="batchNumber1">
        /// batch number
        /// </param>
        /// <param name="batchCompletedQueueItem">
        /// batch completed queue item
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        protected override Task CompleteBatchAsync(string queryId, bool isLastThreadForThisTask, int batchNumber1, IBatchCompletedQueueItem batchCompletedQueueItem)
        {
            if (this.sourceWrapperCollection.Any())
            {
                this.AddToOutputQueueAsync(new SourceWrapperCollectionQueueItem
                {
                    BatchNumber = this.batchNumber,
                    SourceWrapperCollection = this.sourceWrapperCollection,
                    TopLevelKeyColumn = this.topLevelKeyColumn
                });
            }

            // start a new collection for next batch
            this.sourceWrapperCollection = new SourceWrapperCollection();

            return base.CompleteBatchAsync(queryId, isLastThreadForThisTask, batchNumber1, batchCompletedQueueItem);
        }

        /// <inheritdoc />
        protected override string GetId(SqlDataLoadedQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The write diagnostics.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task WriteDiagnostics(SqlDataLoadedQueueItem workItem)
        {
            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.folder != null)
            {
                var filepath = this.detailedTemporaryFileWriter.CombinePath(this.folder, $"{workItem.QueryId}.csv");

                this.detailedTemporaryFileWriter.CreateDirectory(this.folder);

                using (var stream = this.detailedTemporaryFileWriter.OpenStreamForWriting(filepath))
                {
                    using (var streamWriter = new StreamWriter(stream))
                    {
                        var columns = workItem.Columns.Select(c => c.Name).ToList();
                        var text = $@"""Key""," + string.Join(",", columns.Select(c => $@"""{c}"""));

                        await streamWriter.WriteLineAsync(text);

                        var lines = workItem.Rows.Select(c => string.Join(",", c.Select(c1 => $@"""{c1}"""))).ToList();
                        foreach (var line in lines)
                        {
                            await streamWriter.WriteLineAsync(line);
                        }
                    }
                }
            }
        }
    }
}
