using System.IO;
using System.Linq;
using ElasticSearchJsonWriter;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Schema;
using Fabric.Shared;

namespace SqlImporter
{
    public class SqlGetSchemaQueueProcessor : BaseQueueProcessor<SqlGetSchemaQueueItem, SaveSchemaQueueItem>
    {
        private readonly string _folder;

        public SqlGetSchemaQueueProcessor(QueueContext queueContext) : base(queueContext)
        {
            _folder = Path.Combine(Config.LocalSaveFolder, $"{UniqueId}-SqlGetSchema");
            Directory.CreateDirectory(_folder);
        }

        protected override void Handle(SqlGetSchemaQueueItem workitem)
        {

            var workitemLoads = workitem.Loads;

            var dictionary = SchemaLoader.GetSchemasForLoads(workitemLoads, Config.ConnectionString, Config.TopLevelKeyColumn);

            var mappingItems = dictionary.ToList();

            if (Config.WriteDetailedTemporaryFilesToDisk)
            {
                foreach (var mappingItem in mappingItems)
                {
                    var filePath = Path.Combine(_folder, $"{mappingItem.PropertyPath ?? "main"}.json");

                    File.AppendAllText(filePath, mappingItem.ToJsonPretty());
                }
            }
            
            AddToOutputQueue(new SaveSchemaQueueItem
            {
                Mappings = mappingItems
            });
        }

        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }


        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        protected override string GetId(SqlGetSchemaQueueItem workitem)
        {
            return workitem.QueryId;
        }

        protected override string LoggerName => "SqlGetSchema";
    }
}