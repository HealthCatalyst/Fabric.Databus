// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlBatchPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   The sql batch queue processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SqlBatchPipelineStep
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// Reads a SqlBatchQueueItem with a number of queries and splits it into one SqlImportQueueItem for each query
    /// </summary>
    public class SqlBatchPipelineStep : BasePipelineStep<SqlBatchQueueItem, SqlImportQueueItem>
    {
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
        /// Initializes a new instance of the <see cref="T:SqlBatchPipelineStep.SqlBatchPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager"></param>
        /// <param name="progressMonitor"></param>
        /// <param name="fileWriter"></param>
        /// <param name="cancellationToken"></param>
        public SqlBatchPipelineStep(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            IDetailedTemporaryFileWriter fileWriter,
            CancellationToken cancellationToken) 
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            this.folder = Path.Combine(this.Config.LocalSaveFolder, $"{this.UniqueId}-SqlBatch");
        }

        /// <inheritdoc />
        protected override string LoggerName => "SqlBatch";

        /// <inheritdoc />
        protected override async Task HandleAsync(SqlBatchQueueItem workItem)
        {
            int seed = 0;

            foreach (var dataSource in workItem.Loads)
            {
                var queryName = dataSource.Path ?? "Main";
                var queryId = queryName;

                await this.AddToOutputQueueAsync(
                    new SqlImportQueueItem
                        {
                            BatchNumber = workItem.BatchNumber,
                            QueryId = queryId,
                            PropertyName = dataSource.Path,
                            Seed = seed,
                            DataSource = dataSource,
                            Start = workItem.Start,
                            End = workItem.End,
                            PropertyTypes = workItem.PropertyTypes
                        });
            }

            foreach (var workItemLoad in workItem.Loads)
            {
                var queryName = workItemLoad.Path ?? "Main";
                var queryId = queryName;

                var path = Path.Combine(this.folder, queryId);

                this.fileWriter.CreateDirectory(path);

                var filepath = Path.Combine(path, Convert.ToString(workItem.BatchNumber) + ".txt");

                await this.fileWriter.WriteToFileAsync(filepath, $"start: {workItem.Start}, end: {workItem.End}");
            }

            // wait until the other queues are cleared up

            // QueueContext.QueueManager.WaitTillAllQueuesAreCompleted<SqlBatchQueueItem>();
        }

        /// <inheritdoc />
        protected override string GetId(SqlBatchQueueItem workItem)
        {
            return workItem.QueryId;
        }
    }
}
