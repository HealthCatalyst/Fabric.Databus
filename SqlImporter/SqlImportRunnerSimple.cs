using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElasticSearchApiCaller;
using ElasticSearchJsonWriter;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Config;
using Fabric.Databus.Domain.Importers;
using Fabric.Databus.Domain.Jobs;

namespace SqlImporter
{
    public class SqlImportRunnerSimple : IImportRunner
    {
        private const int MaximumDocumentsInQueue = 1 * 1000;

        private int _stepNumber = 0;
        
        
        public void ReadFromDatabase(Job config, IProgressMonitor progressMonitor, IJobStatusTracker jobStatusTracker)
        {

            jobStatusTracker.TrackStart();
            try
            {
                ReadFromDatabase(config, progressMonitor);
            }
            catch (Exception e)
            {
                jobStatusTracker.TrackError(e);
                throw;
            }
            jobStatusTracker.TrackCompletion();
        }

        public void ReadFromDatabase(Job job, IProgressMonitor progressMonitor)
        {
            var config = job.Config;

            if (config.WriteTemporaryFilesToDisk)
            {
                FileSaveQueueProcessor.CleanOutputFolder(config.LocalSaveFolder);
            }

            var documentDictionary =
                new MeteredConcurrentDictionary<string, JsonObjectQueueItem>(MaximumDocumentsInQueue);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            CancellationTokenSource cts = new CancellationTokenSource();

            var queueContext = new QueueContext
            {
                Config = config,
                QueueManager = new QueueManager(),
                ProgressMonitor = progressMonitor,
                BulkUploadRelativeUrl = $"/{config.Index}/{config.EntityType}/_bulk?pretty",
                MainMappingUploadRelativeUrl = $"/{config.Index}",
                SecondaryMappingUploadRelativeUrl = $"/{config.Index}/_mapping/{config.EntityType}",
                PropertyTypes = job.Data.DataSources.Where(a => a.Path != null).ToDictionary(a => a.Path, a => a.PropertyType), 
            };

            int loadNumber = 0;

            foreach (var load in job.Data.DataSources)
            {
                load.SequenceNumber = ++loadNumber;
            }

            if (config.DropAndReloadIndex)
            {
                ReadAndSetSchema(config, queueContext, job);
            }

            var sqlBatchQueue = queueContext.QueueManager
                .CreateInputQueue<SqlBatchQueueItem>(_stepNumber + 1);

            //int seed = 0;


            var ranges = CalculateRanges(config, job);

            int currentBatchNumber = 1;

            foreach (var range in ranges)
            {
                sqlBatchQueue.Add(new SqlBatchQueueItem
                {
                    BatchNumber = currentBatchNumber++,
                    Start = range.Item1,
                    End = range.Item2,
                    Loads = job.Data.DataSources,
                });
            }

            sqlBatchQueue.CompleteAdding();

            var tasks = new List<Task>();
            // ReSharper disable once JoinDeclarationAndInitializer
            IList<Task> newTasks;

            newTasks = RunAsync(() => new SqlBatchQueueProcessor(queueContext), 1, queueContext);
            tasks.AddRange(newTasks);


            newTasks = RunAsync(() => new SqlImportQueueProcessor(queueContext), 2, queueContext);
            tasks.AddRange(newTasks);

            newTasks = RunAsync(() => new ConvertDatabaseRowToJsonQueueProcessor(queueContext, 0), 1, queueContext);
            tasks.AddRange(newTasks);

            newTasks = RunAsync(() => new JsonDocumentMergerQueueProcessor(documentDictionary, queueContext), 1,
                queueContext);
            tasks.AddRange(newTasks);

            newTasks = RunAsync(() => new CreateBatchItemsQueueProcessor(queueContext), 2, queueContext);
            tasks.AddRange(newTasks);

            newTasks = RunAsync(() => new SaveBatchQueueProcessor(queueContext), 2, queueContext);
            tasks.AddRange(newTasks);

            if (config.WriteTemporaryFilesToDisk)
            {
                newTasks = RunAsync(() => new FileSaveQueueProcessor(queueContext), 2, queueContext);
                tasks.AddRange(newTasks);
            }

            if (config.UploadToElasticSearch)
            {
                newTasks = RunAsync(() => new FileUploadQueueProcessor(queueContext), 1, queueContext);
                tasks.AddRange(newTasks);
            }

            Task.WaitAll(tasks.ToArray());

            var stopwatchElapsed = stopwatch.Elapsed;
            stopwatch.Stop();
            Console.WriteLine(stopwatchElapsed);
        }

        private void ReadAndSetSchema(QueryConfig config, QueueContext queueContext, Job job)
        {
            var fileUploader = new FileUploader(queueContext.Config.ElasticSearchUserName,
                queueContext.Config.ElasticSearchPassword);

            if (config.UploadToElasticSearch && config.DropAndReloadIndex)
            {
                Task.Run(
                    async () =>
                    {
                        await fileUploader.DeleteIndex(config.Urls, queueContext.MainMappingUploadRelativeUrl, config.Index, config.Alias);
                    })
                .Wait();
            }

            var tasks = new List<Task>();
            // ReSharper disable once JoinDeclarationAndInitializer
            IList<Task> newTasks;

            var sqlGetSchemaQueue = queueContext.QueueManager
                .CreateInputQueue<SqlGetSchemaQueueItem>(_stepNumber + 1);

            sqlGetSchemaQueue.Add(new SqlGetSchemaQueueItem
            {
                Loads = job.Data.DataSources
            });

            sqlGetSchemaQueue.CompleteAdding();

            newTasks = RunAsync(() => new SqlGetSchemaQueueProcessor(queueContext), 1, queueContext);
            tasks.AddRange(newTasks);

            newTasks = RunAsync(() => new SaveSchemaQueueProcessor(queueContext), 1, queueContext);
            tasks.AddRange(newTasks);

            if (config.UploadToElasticSearch)
            {
                newTasks = RunAsync(() => new MappingUploadQueueProcessor(queueContext), 1, queueContext);
            }
            else
            {
                newTasks = RunAsync(() => new DummyMappingUploadQueueProcessor(queueContext), 1, queueContext);
            }
            tasks.AddRange(newTasks);

            Task.WaitAll(tasks.ToArray());

            // set up aliases
            if (config.UploadToElasticSearch)
            {
                fileUploader.SetupAlias(config.Urls, config.Index, config.Alias).Wait();
            }
        }

        private List<Tuple<string, string>> CalculateRanges(QueryConfig config, Job job)
        {
            var list = GetListOfEntityKeys(config,job);

            var itemsLeft = list.Count;

            var start = 1;

            var ranges = new List<Tuple<string, string>>();

            while (itemsLeft > 0)
            {
                var end = start + (itemsLeft > config.EntitiesPerBatch ? (config.EntitiesPerBatch) : itemsLeft) - 1;
                ranges.Add(new Tuple<string, string>(list[start - 1], list[end - 1]));
                itemsLeft = list.Count - end;
                start = end + 1;
            }

            return ranges;
        }

        private static List<string> GetListOfEntityKeys(QueryConfig config, Job job)
        {
            var load = job.Data.DataSources.First(c => c.Path == null);

            using (var conn = new SqlConnection(config.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (job.Config.SqlCommandTimeoutInSeconds != 0)
                    cmd.CommandTimeout = job.Config.SqlCommandTimeoutInSeconds;

                if (config.MaximumEntitiesToLoad > 0)
                {
                    cmd.CommandText = $";WITH CTE AS ( {load.Sql} )  SELECT TOP {config.MaximumEntitiesToLoad} {config.TopLevelKeyColumn} from CTE ORDER BY {config.TopLevelKeyColumn} ASC;";
                }
                else
                {
                    cmd.CommandText = $";WITH CTE AS ( {load.Sql} )  SELECT {config.TopLevelKeyColumn} from CTE ORDER BY {config.TopLevelKeyColumn} ASC;";
                }

                //Logger.Trace($"Start: {cmd.CommandText}");
                var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

                var list = new List<string>();

                while (reader.Read())
                {
                    var obj = reader.GetValue(0);
                    list.Add(Convert.ToString(obj));
                }
                //Logger.Trace($"Finish: {cmd.CommandText}");


                return list;
            }
        }

        private IList<Task> RunAsync(Func<IBaseQueueProcessor> fnQueueProcessor, int count, QueueContext queueContext)
        {
            IList<Task> tasks = new List<Task>();

            _stepNumber++;

            var thisStepNumber = _stepNumber;

            bool isFirst = true;

            for (var i = 0; i < count; i++)
            {
                var queueProcessor = fnQueueProcessor();

                if (isFirst)
                {
                    queueProcessor.CreateOutQueue(thisStepNumber);
                    isFirst = false;
                }

                queueProcessor.InitializeWithStepNumber(thisStepNumber);

                var task =
                Task.Factory.StartNew((o) =>
                {
                    queueProcessor.MonitorWorkQueue();
                    return 1;
                }, thisStepNumber);

                tasks.Add(task);
            }

            var taskArray = tasks.ToArray();

            Task.WhenAll(taskArray).ContinueWith(a =>
                fnQueueProcessor().MarkOutputQueueAsCompleted(thisStepNumber)
            );

            return tasks;
        }

    }
}