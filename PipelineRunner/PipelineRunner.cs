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
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using ConvertDatabaseRowToJsonQueueProcessor;

    using CreateBatchItemsQueueProcessor;

    using DummyMappingUploadQueueProcessor;

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

    using SqlJobQueueProcessor;

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
        /// The unity container.
        /// </summary>
        private readonly IUnityContainer container;

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The step number.
        /// </summary>
        private int stepNumber;

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

            var documentDictionary = new DocumentDictionary(MaximumDocumentsInQueue);

            var stopwatch = new Stopwatch();
            stopwatch.Start();


            var queueContext = new QueueContext
            {
                Config = config,
                CancellationToken = this.cancellationTokenSource.Token
            };

            var queueManager = new QueueManager();
            this.container.RegisterInstance<IQueueManager>(queueManager);
            this.container.RegisterInstance<IProgressMonitor>(progressMonitor);
            this.container.RegisterInstance<IQueueContext>(queueContext);
            this.container.RegisterInstance<IDocumentDictionary>(documentDictionary);

            IElasticSearchUploaderFactory elasticSearchUploaderFactory = this.container.Resolve<IElasticSearchUploaderFactory>();
            IElasticSearchUploader elasticSearchUploader = elasticSearchUploaderFactory.Create(config.ElasticSearchUserName, config.ElasticSearchPassword, false);
            this.container.RegisterInstance(elasticSearchUploader);
           
            int loadNumber = 0;

            // add sequence number to every load
            foreach (var load in job.Data.DataSources)
            {
                load.SequenceNumber = ++loadNumber;
            }

            // add job to the first queue
            var sqlJobQueue = queueManager
                .CreateInputQueue<SqlJobQueueItem>(++this.stepNumber);

            sqlJobQueue.Add(new SqlJobQueueItem
                                {
                                    Job = job
                                });

            sqlJobQueue.CompleteAdding();

            var processors = new List<QueueProcessorInfo>();

            if (config.DropAndReloadIndex)
            {
                processors.AddRange(
                    new List<QueueProcessorInfo>
                        {
                            new QueueProcessorInfo { Type = typeof(SqlGetSchemaQueueProcessor), Count = 1 },
                            new QueueProcessorInfo { Type = typeof(SaveSchemaQueueProcessor), Count = 1 },
                            new QueueProcessorInfo
                                {
                                    Type = config.UploadToElasticSearch
                                               ? typeof(MappingUploadQueueProcessor)
                                               : typeof(DummyMappingUploadQueueProcessor),
                                    Count = 1
                                }
                        });
            }

            processors.AddRange(
                new List<QueueProcessorInfo>
                    {
                        new QueueProcessorInfo { Type = typeof(SqlJobQueueProcessor), Count = 1 },
                        new QueueProcessorInfo { Type = typeof(SqlBatchQueueProcessor), Count = 1 },
                        new QueueProcessorInfo { Type = typeof(SqlImportQueueProcessor), Count = 1 },
                        new QueueProcessorInfo { Type = typeof(ConvertDatabaseRowToJsonQueueProcessor), Count = 1 },
                        new QueueProcessorInfo { Type = typeof(JsonDocumentMergerQueueProcessor), Count = 1 },
                        new QueueProcessorInfo { Type = typeof(CreateBatchItemsQueueProcessor), Count = 1 },
                        new QueueProcessorInfo { Type = typeof(SaveBatchQueueProcessor), Count = 1 }
                    });

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

            var pipelineExecutorFactory = this.container.Resolve<IPipelineExecutorFactory>();

            var pipelineExecutor = pipelineExecutorFactory.Create(this.container, this.cancellationTokenSource);

            pipelineExecutor.RunPipelineTasks(config, processors, TimeoutInMilliseconds);

            var stopwatchElapsed = stopwatch.Elapsed;
            stopwatch.Stop();
            Console.WriteLine(stopwatchElapsed);
        }
    }
}