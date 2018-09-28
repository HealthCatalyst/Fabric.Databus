// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineRunner.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   The sql import runner simple.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunner
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ConvertDatabaseRowToJsonQueueProcessor;

    using CreateBatchItemsQueueProcessor;

    using DummyMappingUploadQueueProcessor;

    using ElasticSearchApiCaller;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.Importers;
    using Fabric.Databus.Domain.Jobs;

    using FileSaveQueueProcessor;

    using FileUploadQueueProcessor;

    using JsonDocumentMergerQueueProcessor;

    using MappingUploadQueueProcessor;

    using QueueItems;

    using SaveBatchQueueProcessor;

    using SaveSchemaQueueProcessor;

    using SqlBatchQueueProcessor;

    using SqlGetSchemaQueueProcessor;

    using SqlImportQueueProcessor;

    using Unity;

    /// <summary>
    /// The sql import runner simple.
    /// </summary>
    public class PipelineRunner : IImportRunner
    {
        /// <summary>
        /// The maximum documents in queue.
        /// </summary>
        private const int MaximumDocumentsInQueue = 1 * 1000;

        /// <summary>
        /// The timeout in milliseconds.
        /// </summary>
        private const int TimeoutInMilliseconds = 30 * 60 * 1000; // 5 * 60 * 60 * 1000;

        /// <summary>
        /// The step number.
        /// </summary>
        private int stepNumber;

        /// <summary>
        /// The unity container.
        /// </summary>
        private IUnityContainer container;

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineRunner"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        public PipelineRunner(IUnityContainer container, CancellationToken cancellationToken)
        {
            this.container = container;
            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        /// <summary>
        /// The run pipeline.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="progressMonitor">
        /// The progress monitor.
        /// </param>
        /// <param name="jobStatusTracker">
        /// The job status tracker.
        /// </param>
        public void RunPipeline(IJob config, IProgressMonitor progressMonitor, IJobStatusTracker jobStatusTracker)
        {
            jobStatusTracker.TrackStart();
            try
            {
                this.RunPipeline(config, progressMonitor);
            }
            catch (Exception e)
            {
                jobStatusTracker.TrackError(e);
                throw;
            }

            jobStatusTracker.TrackCompletion();
        }

        /// <summary>
        /// The run pipeline.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <param name="progressMonitor">
        /// The progress monitor.
        /// </param>
        public void RunPipeline(IJob job, IProgressMonitor progressMonitor)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }
            if (job.Config == null)
            {
                throw new ArgumentNullException(nameof(job.Config));
            }

            var config = job.Config;


            if (config.WriteTemporaryFilesToDisk)
            {
                FileSaveQueueProcessor.CleanOutputFolder(config.LocalSaveFolder);
            }

            var documentDictionary =
                new MeteredConcurrentDictionary<string, IJsonObjectQueueItem>(MaximumDocumentsInQueue);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var queueContext = new QueueContext
            {
                Config = config,
                QueueManager = new QueueManager(),
                ProgressMonitor = progressMonitor,
                BulkUploadRelativeUrl = $"/{config.Index}/{config.EntityType}/_bulk?pretty",
                MainMappingUploadRelativeUrl = $"/{config.Index}",
                SecondaryMappingUploadRelativeUrl = $"/{config.Index}/_mapping/{config.EntityType}",
                PropertyTypes = job.Data.DataSources.Where(a => a.Path != null).ToDictionary(a => a.Path, a => a.PropertyType),
                DocumentDictionary = documentDictionary,
                CancellationToken = this.cancellationTokenSource.Token
            };

            this.container.RegisterInstance<IQueueContext>(queueContext);

            int loadNumber = 0;

            foreach (var load in job.Data.DataSources)
            {
                load.SequenceNumber = ++loadNumber;
            }

            if (config.DropAndReloadIndex)
            {
                this.ReadAndSetSchema(config, queueContext, job);
            }

            var sqlBatchQueue = queueContext.QueueManager
                .CreateInputQueue<SqlBatchQueueItem>(this.stepNumber + 1);

            if (queueContext.Config.EntitiesPerBatch <= 0)
            {
                sqlBatchQueue.Add(new SqlBatchQueueItem
                {
                    BatchNumber = 1,
                    Start = null,
                    End = null,
                    Loads = job.Data.DataSources,
                });
            }
            else
            {
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
            }

            sqlBatchQueue.CompleteAdding();

            var pipelineExecutorFactory = this.container.Resolve<IPipelineExecutorFactory>();

            var pipelineExecutor = pipelineExecutorFactory.Create(this.container, this.cancellationTokenSource);

            var processors = new List<QueueProcessorInfo>
                                 {
                                     new QueueProcessorInfo { Type = typeof(SqlBatchQueueProcessor), Count = 1 },
                                     new QueueProcessorInfo { Type = typeof(SqlImportQueueProcessor), Count = 1 },
                                     new QueueProcessorInfo
                                         {
                                             Type = typeof(ConvertDatabaseRowToJsonQueueProcessor), Count = 1
                                         },
                                     new QueueProcessorInfo
                                         {
                                             Type = typeof(JsonDocumentMergerQueueProcessor), Count = 1
                                         },
                                     new QueueProcessorInfo
                                         {
                                             Type = typeof(CreateBatchItemsQueueProcessor), Count = 1
                                         },
                                     new QueueProcessorInfo { Type = typeof(SaveBatchQueueProcessor), Count = 1 }
                                 };

            if (config.WriteTemporaryFilesToDisk)
            {
                processors.Add(new QueueProcessorInfo
                {
                    Type = typeof(FileSaveQueueProcessor),
                    Count = 1
                });
            }

            if (config.UploadToElasticSearch)
            {
                processors.Add(new QueueProcessorInfo
                                   {
                                       Type = typeof(FileUploadQueueProcessor),
                                       Count = 1
                                   });
            }

            pipelineExecutor.RunPipelineTasks(config, processors, TimeoutInMilliseconds);

            var stopwatchElapsed = stopwatch.Elapsed;
            stopwatch.Stop();
            Console.WriteLine(stopwatchElapsed);
        }

        /// <summary>
        /// The calculate ranges.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private static IEnumerable<Tuple<string, string>> CalculateRanges(IQueryConfig config, IJob job)
        {
            var list = GetListOfEntityKeys(config, job);

            var itemsLeft = list.Count;

            var start = 1;

            var ranges = new List<Tuple<string, string>>();

            while (itemsLeft > 0)
            {
                var end = start + (itemsLeft > config.EntitiesPerBatch ? config.EntitiesPerBatch : itemsLeft) - 1;
                ranges.Add(new Tuple<string, string>(list[start - 1], list[end - 1]));
                itemsLeft = list.Count - end;
                start = end + 1;
            }

            return ranges;
        }

        /// <summary>
        /// The get list of entity keys.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private static List<string> GetListOfEntityKeys(IQueryConfig config, IJob job)
        {
            var load = job.Data.DataSources.First(c => c.Path == null);

            using (var conn = new SqlConnection(config.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (job.Config.SqlCommandTimeoutInSeconds != 0)
                {
                    cmd.CommandTimeout = job.Config.SqlCommandTimeoutInSeconds;
                }

                cmd.CommandText = config.MaximumEntitiesToLoad > 0
                                      ? $";WITH CTE AS ( {load.Sql} )  SELECT TOP {config.MaximumEntitiesToLoad} {config.TopLevelKeyColumn} from CTE ORDER BY {config.TopLevelKeyColumn} ASC;"
                                      : $";WITH CTE AS ( {load.Sql} )  SELECT {config.TopLevelKeyColumn} from CTE ORDER BY {config.TopLevelKeyColumn} ASC;";

                // Logger.Verbose($"Start: {cmd.CommandText}");
                var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

                var list = new List<string>();

                while (reader.Read())
                {
                    var obj = reader.GetValue(0);
                    list.Add(Convert.ToString(obj));
                }

                // Logger.Verbose($"Finish: {cmd.CommandText}");
                return list;
            }
        }

        /// <summary>
        /// The read and set schema.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="queueContext">
        /// The queue context.
        /// </param>
        /// <param name="job">
        /// The job.
        /// </param>
        private void ReadAndSetSchema(IQueryConfig config, IQueueContext queueContext, IJob job)
        {
            //var fileUploader = new FileUploader(
            //    queueContext.Config.ElasticSearchUserName,
            //    queueContext.Config.ElasticSearchPassword,
            //    job.Config.KeepIndexOnline);

            //if (config.UploadToElasticSearch && config.DropAndReloadIndex)
            //{
            //    Task.Run(
            //        async () =>
            //        {
            //            await fileUploader.DeleteIndex(config.Urls, queueContext.MainMappingUploadRelativeUrl, config.Index, config.Alias);
            //        })
            //    .Wait();
            //}

            //var tasks = new List<Task>();
            //// ReSharper disable once JoinDeclarationAndInitializer
            //IList<Task> newTasks;

            //var sqlGetSchemaQueue = queueContext.QueueManager
            //    .CreateInputQueue<SqlGetSchemaQueueItem>(this.stepNumber + 1);

            //sqlGetSchemaQueue.Add(new SqlGetSchemaQueueItem
            //{
            //    Loads = job.Data.DataSources
            //});

            //sqlGetSchemaQueue.CompleteAdding();

            //newTasks = this.RunAsync(() => this.container.Resolve<SqlGetSchemaQueueProcessor>(), 1);
            //tasks.AddRange(newTasks);

            //newTasks = this.RunAsync(() => this.container.Resolve<SaveSchemaQueueProcessor>(), 1);
            //tasks.AddRange(newTasks);

            //newTasks = config.UploadToElasticSearch
            //               ? this.RunAsync(() => this.container.Resolve<MappingUploadQueueProcessor>(), 1)
            //               : this.RunAsync(() => this.container.Resolve<DummyMappingUploadQueueProcessor>(), 1);

            //tasks.AddRange(newTasks);

            //Task.WaitAll(tasks.ToArray());

            //// set up aliases
            //if (config.UploadToElasticSearch)
            //{
            //    fileUploader.SetupAlias(config.Urls, config.Index, config.Alias).Wait();
            //}
        }
    }
}