namespace SqlGetSchemaQueueProcessor
{
    using System.IO;
    using System.Linq;
    using System.Xml;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Databus.Schema;
    using Fabric.Shared;

    using QueueItems;

    using Serilog;

    public class SqlGetSchemaQueueProcessor : BaseQueueProcessor<SqlGetSchemaQueueItem, SaveSchemaQueueItem>
    {
        private readonly string _folder;

        public SqlGetSchemaQueueProcessor(IQueueContext queueContext, ILogger logger) : base(queueContext, logger)
        {
            this._folder = Path.Combine(Config.LocalSaveFolder, $"{UniqueId}-SqlGetSchema");
            if (Config.WriteDetailedTemporaryFilesToDisk)
            {
                Directory.CreateDirectory(this._folder);
            }
        }

        protected override void Handle(SqlGetSchemaQueueItem workItem)
        {

            var workitemLoads = workItem.Loads;

            var dictionary = SchemaLoader.GetSchemasForLoads(workitemLoads, Config.ConnectionString, Config.TopLevelKeyColumn);

            var mappingItems = dictionary.ToList();

            if (Config.WriteDetailedTemporaryFilesToDisk)
            {
                foreach (var mappingItem in mappingItems)
                {
                    var filePath = Path.Combine(this._folder, $"{mappingItem.PropertyPath ?? "main"}.json");

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

        protected override string GetId(SqlGetSchemaQueueItem workItem)
        {
            return workItem.QueryId;
        }

        protected override string LoggerName => "SqlGetSchema";
    }
}