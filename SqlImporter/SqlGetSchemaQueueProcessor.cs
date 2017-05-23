using System.Linq;
using ElasticSearchJsonWriter;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Schema;

namespace SqlImporter
{
    public class SqlGetSchemaQueueProcessor : BaseQueueProcessor<SqlGetSchemaQueueItem, SaveSchemaQueueItem>
    {
        public SqlGetSchemaQueueProcessor(QueueContext queueContext) : base(queueContext)
        {
        }

        protected override void Handle(SqlGetSchemaQueueItem workitem)
        {

            var workitemLoads = workitem.Loads;

            var dictionary = SchemaLoader.GetSchemasForLoads(workitemLoads, Config.ConnectionString, Config.TopLevelKeyColumn);

            AddToOutputQueue(new SaveSchemaQueueItem
            {
                Mappings = dictionary.ToList()
            });
        }


        protected override void Complete(string queryId)
        {
        }

        protected override string GetId(SqlGetSchemaQueueItem workitem)
        {
            return workitem.QueryId;
        }

        protected override string LoggerName => "SqlGetSchema";
    }
}