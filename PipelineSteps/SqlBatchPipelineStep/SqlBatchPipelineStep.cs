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

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The sql batch queue processor.
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
        protected override void Handle(SqlBatchQueueItem workItem)
        {
            int seed = 0;

            foreach (var dataSource in workItem.Loads)
            {
                var queryName = dataSource.Path ?? "Main";
                var queryId = queryName;

                this.AddToOutputQueue(
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

                this.fileWriter.WriteToFile(filepath, $"start: {workItem.Start}, end: {workItem.End}");
            }

            // wait until the other queues are cleared up

            // QueueContext.QueueManager.WaitTillAllQueuesAreCompleted<SqlBatchQueueItem>();
        }

        /// <inheritdoc />
        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        /// <inheritdoc />
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        /// <inheritdoc />
        protected override string GetId(SqlBatchQueueItem workItem)
        {
            return workItem.QueryId;
        }
    }
}
