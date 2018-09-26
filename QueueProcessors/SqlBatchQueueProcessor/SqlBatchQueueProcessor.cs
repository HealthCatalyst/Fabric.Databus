namespace SqlBatchQueueProcessor
{
    using System;
    using System.IO;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Shared;

    using QueueItems;

    public class SqlBatchQueueProcessor : BaseQueueProcessor<SqlBatchQueueItem, SqlImportQueueItem>
    {
        private readonly string _folder;

        public SqlBatchQueueProcessor(QueueContext queueContext) : base(queueContext)
        {
            this._folder = Path.Combine(this.Config.LocalSaveFolder, $"{this.UniqueId}-SqlBatch");

        }

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

                    var path = Path.Combine(this._folder, queryId);

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

        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        protected override string GetId(SqlBatchQueueItem workItem)
        {
            return workItem.QueryId;
        }

        protected override string LoggerName => "SqlBatch";
    }
}
