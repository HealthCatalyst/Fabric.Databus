using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Config;

namespace SqlImporter
{
    public class SqlBatchQueueProcessor : BaseQueueProcessor<SqlBatchQueueItem, SqlImportQueueItem>
    {
        public SqlBatchQueueProcessor(QueueContext queueContext) : base(queueContext)
        {
        }

        protected override void Handle(SqlBatchQueueItem workitem)
        {
            int seed = 0;

            workitem.Loads
                .ForEach(c =>
                {
                    var queryName = c.Path ?? "Main";
                    var queryId = queryName;
                    //var queryId = JsonDocumentMergerQueueProcessor.RegisterQuery(seed, queryName);

                    AddToOutputQueue(new SqlImportQueueItem
                    {
                        BatchNumber = workitem.BatchNumber,
                        QueryId = queryId,
                        PropertyName = c.Path,
                        Seed = seed,
                        DataSource = c,
                        Start = workitem.Start,
                        End = workitem.End,
                    });
                });

            // wait until the other queues are cleared up
            //QueueContext.QueueManager.WaitTillAllQueuesAreCompleted<SqlBatchQueueItem>();
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        protected override string GetId(SqlBatchQueueItem workitem)
        {
            return workitem.QueryId;
        }

        protected override string LoggerName => "SqlBatch";
    }

    public class SqlBatchQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public List<DataSource> Loads { get; set; }
        public int BatchNumber { get; set; }
    }
}
