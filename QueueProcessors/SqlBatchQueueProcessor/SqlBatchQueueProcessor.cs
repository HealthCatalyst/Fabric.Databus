﻿// --------------------------------------------------------------------------------------------------------------------
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

    /// <summary>
    /// The sql batch queue processor.
    /// </summary>
    public class SqlBatchQueueProcessor : BaseQueueProcessor<SqlBatchQueueItem, SqlImportQueueItem>
    {
        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlBatchQueueProcessor"/> class.
        /// </summary>
        /// <param name="queueContext">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public SqlBatchQueueProcessor(IQueueContext queueContext, ILogger logger) : base(queueContext, logger)
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
                    //var queryId = JsonDocumentMergerQueueProcessor.RegisterQuery(seed, queryName);

                    this.AddToOutputQueue(new SqlImportQueueItem
                    {
                        BatchNumber = workItem.BatchNumber,
                        QueryId = queryId,
                        PropertyName = dataSource.Path,
                        Seed = seed,
                        DataSource = dataSource,
                        Start = workItem.Start,
                        End = workItem.End,
                    });
                });

            if (this.Config.WriteDetailedTemporaryFilesToDisk)
            {
                foreach (var workitemLoad in workItem.Loads)
                {
                    var queryName = workitemLoad.Path ?? "Main";
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