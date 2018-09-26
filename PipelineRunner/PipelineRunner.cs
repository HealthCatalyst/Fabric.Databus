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
        private const int TimeoutInMilliseconds = 5 * 60 * 60 * 60;

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
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The init.
        /// </summary>
        public void Init()
        {
            this.container = new UnityContainer();
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
        /// <param name="cancellationToken">
        /// The cancellation Token.
        /// </param>
        public void RunPipeline(IJob config, IProgressMonitor progressMonitor, IJobStatusTracker jobStatusTracker, CancellationToken cancellationToken)
        {
            jobStatusTracker.TrackStart();
            try
            {
                this.RunPipeline(config, progressMonitor, cancellationToken);
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
        /// <param name="cancellationToken">cancellation token</param>
        public void RunPipeline(IJob job, IProgressMonitor progressMonitor, CancellationToken cancellationToken)
        {
            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

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
                DocumentDictionary = documentDictionary
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

            var tasks = new List<Task>();
            // ReSharper disable once JoinDeclarationAndInitializer
            IList<Task> newTasks;

            newTasks = this.RunAsync(() => this.container.Resolve<SqlBatchQueueProcessor>(), 1);
            tasks.AddRange(newTasks);

            newTasks = this.RunAsync(() => this.container.Resolve<SqlImportQueueProcessor>(), 2);
            tasks.AddRange(newTasks);

            newTasks = this.RunAsync(() => this.container.Resolve<ConvertDatabaseRowToJsonQueueProcessor>(), 1);
            tasks.AddRange(newTasks);

            newTasks = this.RunAsync(() => this.container.Resolve<JsonDocumentMergerQueueProcessor>(), 1);
            tasks.AddRange(newTasks);

            newTasks = this.RunAsync(() => this.container.Resolve<CreateBatchItemsQueueProcessor>(), 2);
            tasks.AddRange(newTasks);

            newTasks = this.RunAsync(() => this.container.Resolve<SaveBatchQueueProcessor>(), 2);
            tasks.AddRange(newTasks);

            if (config.WriteTemporaryFilesToDisk)
            {
                newTasks = this.RunAsync(() => this.container.Resolve<FileSaveQueueProcessor>(), 2);
                tasks.AddRange(newTasks);
            }

            if (config.UploadToElasticSearch)
            {
                newTasks = this.RunAsync(() => this.container.Resolve<FileUploadQueueProcessor>(), 1);
                tasks.AddRange(newTasks);
            }

            Task.WaitAll(tasks.ToArray(), TimeoutInMilliseconds, cancellationToken);

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

                // Logger.Trace($"Start: {cmd.CommandText}");
                var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

                var list = new List<string>();

                while (reader.Read())
                {
                    var obj = reader.GetValue(0);
                    list.Add(Convert.ToString(obj));
                }

                // Logger.Trace($"Finish: {cmd.CommandText}");
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
            var fileUploader = new FileUploader(
                queueContext.Config.ElasticSearchUserName,
                queueContext.Config.ElasticSearchPassword,
                job.Config.KeepIndexOnline);

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
                .CreateInputQueue<SqlGetSchemaQueueItem>(this.stepNumber + 1);

            sqlGetSchemaQueue.Add(new SqlGetSchemaQueueItem
            {
                Loads = job.Data.DataSources
            });

            sqlGetSchemaQueue.CompleteAdding();

            newTasks = this.RunAsync(() => this.container.Resolve<SqlGetSchemaQueueProcessor>(), 1);
            tasks.AddRange(newTasks);

            newTasks = this.RunAsync(() => this.container.Resolve<SaveSchemaQueueProcessor>(), 1);
            tasks.AddRange(newTasks);

            newTasks = config.UploadToElasticSearch
                           ? this.RunAsync(() => this.container.Resolve<MappingUploadQueueProcessor>(), 1)
                           : this.RunAsync(() => this.container.Resolve<DummyMappingUploadQueueProcessor>(), 1);

            tasks.AddRange(newTasks);

            Task.WaitAll(tasks.ToArray());

            // set up aliases
            if (config.UploadToElasticSearch)
            {
                fileUploader.SetupAlias(config.Urls, config.Index, config.Alias).Wait();
            }
        }

        /// <summary>
        /// The run async.
        /// </summary>
        /// <param name="functionQueueProcessor">
        /// The fn queue processor.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        private IList<Task> RunAsync(Func<IBaseQueueProcessor> functionQueueProcessor, int count)
        {
            IList<Task> tasks = new List<Task>();

            this.stepNumber++;

            var thisStepNumber = this.stepNumber;

            bool isFirst = true;

            for (var i = 0; i < count; i++)
            {
                var queueProcessor = functionQueueProcessor();

                if (isFirst)
                {
                    queueProcessor.CreateOutQueue(thisStepNumber);
                    isFirst = false;
                }

                queueProcessor.InitializeWithStepNumber(thisStepNumber);

                var task = Task.Factory.StartNew(
                    (o) =>
                        {
                            queueProcessor.MonitorWorkQueue();
                            return 1;
                        },
                    thisStepNumber);

                tasks.Add(task);
            }

            var taskArray = tasks.ToArray();

            Task.WhenAll(taskArray).ContinueWith(
                t =>
                    {
                        if (t.IsFaulted)
                        {
                            this.cancellationTokenSource.Cancel();
                        }
                    },
                this.cancellationTokenSource.Token,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current).ContinueWith(
                task =>
                    {
                        if (!task.IsFaulted)
                        {
                            functionQueueProcessor().MarkOutputQueueAsCompleted(thisStepNumber);
                        }
                    });

            return tasks;
        }
    }
}