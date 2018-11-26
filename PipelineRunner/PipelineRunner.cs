// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineRunner.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   The sql import runner simple.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using ConvertDatabaseRowToJsonPipelineStep;

    using CreateBatchItemsPipelineStep;

    using DummyMappingUploadPipelineStep;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.Importers;
    using Fabric.Databus.Domain.Jobs;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.ElasticSearch;
    using Fabric.Databus.Http;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Http;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.Json;
    using Fabric.Databus.Schema;
    using Fabric.Databus.Shared;
    using Fabric.Databus.Shared.FileWriters;
    using Fabric.Databus.Shared.Loggers;
    using Fabric.Databus.Shared.Queues;

    using FileSavePipelineStep;

    using FileUploadPipelineStep;

    using JsonDocumentMergerPipelineStep;

    using MappingUploadPipelineStep;

    using QueueItems;

    using SaveBatchPipelineStep;

    using SaveJsonToFilePipelineStep;

    using SaveSchemaPipelineStep;

    using SendToRestApiPipelineStep;

    using Serilog;

    using SqlBatchPipelineStep;

    using SqlGetSchemaPipelineStep;

    using SqlImportPipelineStep;

    using SqlJobPipelineStep;

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

        /// <inheritdoc />
        public void RunElasticSearchPipeline(IJob config, IJobStatusTracker jobStatusTracker)
        {
            jobStatusTracker.TrackStart();
            try
            {
                this.RunElasticSearchPipeline(config);
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
        public void RunElasticSearchPipeline(IJob job)
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
            this.InitContainer(config);

            if (config.WriteTemporaryFilesToDisk)
            {
                this.container.Resolve<IFileWriter>().DeleteDirectory(config.LocalSaveFolder);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int loadNumber = 0;

            // add sequence number to every load
            foreach (var load in job.Data.DataSources)
            {
                load.SequenceNumber = ++loadNumber;

                // support older syntax
                if (load.Path == null)
                {
                    load.Path = "$";
                }
                else if (!load.Path.StartsWith("$"))
                {
                    load.Path = $"$.{load.Path}";
                }
            }
           
            var sqlJobQueue = this.container.Resolve<IQueueManager>()
                .CreateInputQueue<SqlJobQueueItem>(++this.stepNumber);

            sqlJobQueue.Add(new SqlJobQueueItem
            {
                Job = job
            });

            sqlJobQueue.CompleteAdding();

            var processors = new List<PipelineStepInfo>();

            if (config.DropAndReloadIndex)
            {
                processors.AddRange(
                    new List<PipelineStepInfo>
                        {
                            new PipelineStepInfo { Type = typeof(SqlGetSchemaPipelineStep), Count = 1 },
                            new PipelineStepInfo { Type = typeof(SaveSchemaPipelineStep), Count = 1 },
                            new PipelineStepInfo
                                {
                                    Type = config.UploadToElasticSearch
                                               ? typeof(MappingUploadPipelineStep)
                                               : typeof(DummyMappingUploadPipelineStep),
                                    Count = 1
                                }
                        });
            }

            processors.AddRange(
                new List<PipelineStepInfo>
                    {
                        new PipelineStepInfo { Type = typeof(SqlJobPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(SqlBatchPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(SqlImportPipelineStep), Count = 15 },
                        new PipelineStepInfo { Type = typeof(SqlCombineSourceWrappersPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(WriteSourceWrapperCollectionToJsonPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(CreateBatchItemsPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(SaveBatchPipelineStep), Count = 1 }
                    });

            if (config.WriteTemporaryFilesToDisk)
            {
                processors.Add(new PipelineStepInfo
                {
                    Type = typeof(FileSavePipelineStep),
                    Count = 1
                });
            }

            if (config.UploadToElasticSearch)
            {
                processors.Add(new PipelineStepInfo
                {
                    Type = typeof(FileUploadPipelineStep),
                    Count = 1
                });
            }

            var elasticSearchUploaderFactory = this.container.Resolve<IElasticSearchUploaderFactory>();
            IElasticSearchUploader elasticSearchUploader = elasticSearchUploaderFactory.Create(
                                                            config.ElasticSearchUserName,
                                                            config.ElasticSearchPassword,
                                                            false,
                                                            config.Urls,
                                                            config.Index,
                                                            config.Alias,
                                                            config.EntityType);

            this.container.RegisterInstance(elasticSearchUploader);

            var pipelineExecutorFactory = this.container.Resolve<IPipelineExecutorFactory>();

            var pipelineExecutor = pipelineExecutorFactory.Create(this.container, this.cancellationTokenSource);

            pipelineExecutor.RunPipelineTasks(config, processors, TimeoutInMilliseconds);

            var stopwatchElapsed = stopwatch.Elapsed;
            stopwatch.Stop();
            Console.WriteLine(stopwatchElapsed);
        }

        /// <summary>
        /// The run pipeline.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        public void RunRestApiPipeline(IJob job)
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
            this.InitContainer(config);

            if (config.WriteTemporaryFilesToDisk)
            {
                this.container.Resolve<IFileWriter>().DeleteDirectory(config.LocalSaveFolder);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int loadNumber = 0;

            // add sequence number to every load
            foreach (var load in job.Data.DataSources)
            {
                load.SequenceNumber = ++loadNumber;

                // support older syntax
                if (load.Path == null)
                {
                    load.Path = "$";
                }
                else if (!load.Path.StartsWith("$"))
                {
                    load.Path = $"$.{load.Path}";
                }
            }

            // add job to the first queue
            var sqlJobQueue = this.container.Resolve<IQueueManager>()
                .CreateInputQueue<SqlJobQueueItem>(++this.stepNumber);

            sqlJobQueue.Add(new SqlJobQueueItem
            {
                Job = job
            });

            sqlJobQueue.CompleteAdding();

            var processors = new List<PipelineStepInfo>();

            processors.AddRange(
                new List<PipelineStepInfo>
                    {
                        new PipelineStepInfo { Type = typeof(SqlJobPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(SqlBatchPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(SqlImportPipelineStep), Count = 5 },
                        new PipelineStepInfo { Type = typeof(SqlCombineSourceWrappersPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(WriteSourceWrapperCollectionToJsonPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(SaveJsonToFilePipelineStep), Count = 1 }
                    });

            if (config.UploadToElasticSearch)
            {
                processors.Add(new PipelineStepInfo { Type = typeof(SendToRestApiPipelineStep), Count = 1 });
            }

            var fileUploaderFactory = this.container.Resolve<IFileUploaderFactory>();
            var fileUploader = fileUploaderFactory.Create(config.ElasticSearchUserName, config.ElasticSearchPassword, config.Urls);
            this.container.RegisterInstance(fileUploader);

            var pipelineExecutorFactory = this.container.Resolve<IPipelineExecutorFactory>();

            var pipelineExecutor = pipelineExecutorFactory.Create(this.container, this.cancellationTokenSource);

            pipelineExecutor.RunPipelineTasks(config, processors, TimeoutInMilliseconds);

            var stopwatchElapsed = stopwatch.Elapsed;
            stopwatch.Stop();
            Console.WriteLine(stopwatchElapsed);
        }

        /// <summary>
        /// The init container.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        private void InitContainer(IQueryConfig config)
        {
            var documentDictionary = new DocumentDictionary(MaximumDocumentsInQueue);

            var queueManager = new QueueManager();
            this.container.RegisterInstance<IQueueManager>(queueManager);
            this.container.RegisterInstance<IDocumentDictionary>(documentDictionary);
            this.container.RegisterInstance<IJobConfig>(config);

            var schemaLoader = new SchemaLoader(config.ConnectionString, config.TopLevelKeyColumn);
            this.container.RegisterInstance<ISchemaLoader>(schemaLoader);

            this.container.RegisterType<IEntityJsonWriter, EntityJsonWriter>();

            if (config.WriteDetailedTemporaryFilesToDisk)
            {
                this.container.RegisterType<IDetailedTemporaryFileWriter, FileWriter>();
            }
            else
            {
                this.container.RegisterType<IDetailedTemporaryFileWriter, NullFileWriter>();
            }

            if (config.WriteTemporaryFilesToDisk)
            {
                this.container.RegisterType<ITemporaryFileWriter, FileWriter>();
            }
            else
            {
                this.container.RegisterType<ITemporaryFileWriter, NullFileWriter>();
            }

            this.container.RegisterType<IFileWriter, FileWriter>();

            this.InitContainerWithDefaults(config);
        }

        /// <summary>
        /// The init container with defaults.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        private void InitContainerWithDefaults(IQueryConfig config)
        {
            if (!this.container.IsRegistered<IProgressMonitor>())
            {
                IProgressMonitor progressMonitor = new ProgressMonitor(new NullProgressLogger());
                this.container.RegisterInstance<IProgressMonitor>(progressMonitor);
            }

            if (!this.container.IsRegistered<IDatabusSqlReader>())
            {
                var databusSqlReader = new DatabusSqlReader(config.ConnectionString, 0);
                this.container.RegisterInstance<IDatabusSqlReader>(databusSqlReader);
            }
            if (!this.container.IsRegistered<ILogger>())
            {
                ILogger logger = new LoggerConfiguration().MinimumLevel.Verbose().CreateLogger();

                this.container.RegisterInstance(logger);
            }

            if (!this.container.IsRegistered<IElasticSearchUploaderFactory>())
            {
                this.container.RegisterType<IElasticSearchUploaderFactory, ElasticSearchUploaderFactory>();
            }

            if (!this.container.IsRegistered<IElasticSearchUploaderFactory>())
            {
                this.container.RegisterType<IElasticSearchUploaderFactory, ElasticSearchUploaderFactory>();
            }

            if (!this.container.IsRegistered<IFileUploaderFactory>())
            {
                this.container.RegisterType<IFileUploaderFactory, FileUploaderFactory>();
            }

            if (!this.container.IsRegistered<IElasticSearchUploader>())
            {
                this.container.RegisterType<IElasticSearchUploader, ElasticSearchUploader>();
            }

            if (!this.container.IsRegistered<IFileUploader>())
            {
                this.container.RegisterType<IFileUploader, FileUploader>();
            }

            if (!this.container.IsRegistered<IHttpClientFactory>())
            {
                this.container.RegisterType<IHttpClientFactory, HttpClientFactory>();
            }

            if (config.UseMultipleThreads)
            {
                if (!this.container.IsRegistered<IPipelineExecutorFactory>())
                {
                    this.container.RegisterType<IPipelineExecutorFactory, MultiThreadedPipelineExecutorFactory>();
                }
            }
            else
            {
                if (!this.container.IsRegistered<IPipelineExecutorFactory>())
                {
                    this.container.RegisterType<IPipelineExecutorFactory, SingleThreadedPipelineExecutorFactory>();
                }
            }
        }
    }
}