// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlBatchQueueProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   The sql batch queue processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SqlBatchQueueProcessor
{
    using System;
    using System.IO;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The sql batch queue processor.
    /// </summary>
    public class SqlBatchQueueProcessor : BaseQueueProcessor<SqlBatchQueueItem, SqlImportQueueItem>
    {
        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:SqlBatchQueueProcessor.SqlBatchQueueProcessor" /> class.
        /// </summary>
        /// <param name="queueContext">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager"></param>
        /// <param name="progressMonitor"></param>
        public SqlBatchQueueProcessor(IQueueContext queueContext, ILogger logger, IQueueManager queueManager, IProgressMonitor progressMonitor) 
            : base(queueContext, logger, queueManager, progressMonitor)
        {
            this.folder = Path.Combine(this.Config.LocalSaveFolder, $"{this.UniqueId}-SqlBatch");
        }

        /// <inheritdoc />
        protected override void Handle(SqlBatchQueueItem workItem)
        {
            int seed = 0;

            workItem.Loads
                .ForEach(dataSource =>
                {
                    var queryName = dataSource.Path ?? "Main";
                    var queryId = queryName;

                    this.AddToOutputQueue(new SqlImportQueueItem
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
                });

            if (this.Config.WriteDetailedTemporaryFilesToDisk)
            {
                foreach (var workItemLoad in workItem.Loads)
                {
                    var queryName = workItemLoad.Path ?? "Main";
                    var queryId = queryName;

                    var path = Path.Combine(this.folder, queryId);

                    Directory.CreateDirectory(path);

                    var filepath = Path.Combine(path, Convert.ToString(workItem.BatchNumber) + ".txt");

                    using (var file = File.OpenWrite(filepath))
                    {
                        using (var stream = new StreamWriter(file))
                        {
                            stream.WriteLine($"start: {workItem.Start}, end: {workItem.End}");
                        }
                    }
                }
            }
            // wait until the other queues are cleared up
            //QueueContext.QueueManager.WaitTillAllQueuesAreCompleted<SqlBatchQueueItem>();
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

        protected override string LoggerName => "SqlBatch";
    }
}
