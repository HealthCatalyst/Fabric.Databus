﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ConfigValidators;
    using Fabric.Databus.Domain.Importers;
    using Fabric.Databus.Domain.Jobs;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.ElasticSearch;
    using Fabric.Databus.Http;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.Exceptions;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Http;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Pipeline;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.Json;
    using Fabric.Databus.PipelineSteps;
    using Fabric.Databus.Schema;
    using Fabric.Databus.Shared;
    using Fabric.Databus.Shared.FileWriters;
    using Fabric.Databus.Shared.Loggers;
    using Fabric.Databus.Shared.Queues;
    using Fabric.Databus.SqlGenerator;
    using Fabric.Shared.ReliableHttp.Interceptors;
    using Fabric.Shared.ReliableHttp.Interfaces;

    using QueueItems;

    using Serilog;

    using Unity;

    /// <inheritdoc />
    /// <summary>
    /// The sql import runner simple.
    /// </summary>
    public class PipelineRunner : IImportRunner
    {
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
        public async Task RunPipelineAsync(IJob job, IJobStatusTracker jobStatusTracker)
        {
            jobStatusTracker.TrackStart();
            try
            {
                await this.RunPipelineAsync(job);
            }
            catch (Exception e)
            {
                jobStatusTracker.TrackError(e);
                throw;
            }

            jobStatusTracker.TrackCompletion();
        }

        /// <inheritdoc />
        public async Task RunPipelineAsync(IJob job)
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

            if (job.Data.TopLevelDataSource == null)
            {
                throw new DatabusValidationException("job.Data.TopLevelDataSource cannot be null");
            }

            this.InitContainerWithDefaults(job);

            var logger = this.container.Resolve<ILogger>();

            if (config.WriteTemporaryFilesToDisk)
            {
                this.container.Resolve<IFileWriter>().DeleteDirectory(config.LocalSaveFolder);
            }

            var temporaryFileWriter = this.container.Resolve<ITemporaryFileWriter>();
            if (!string.IsNullOrWhiteSpace(job.Config.LocalSaveFolder) && temporaryFileWriter.IsWritingEnabled)
            {
                // temporaryFileWriter.WriteToFileAsync(temporaryFileWriter.CombinePath(job.Config.LocalSaveFolder, "job.json"), job.ToJsonPretty());
                await temporaryFileWriter.WriteToFileAsync(
                    temporaryFileWriter.CombinePath(job.Config.LocalSaveFolder, "job.xml"),
                    new ConfigReader().WriteXml(job));
            }

            this.container.Resolve<IConfigValidator>().ValidateJob(job, logger);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int loadNumber = 0;

            // add sequence number to every load
            foreach (var dataSource in job.Data.DataSources)
            {
                dataSource.SequenceNumber = ++loadNumber;

                // support older syntax
                if (dataSource.Path == null)
                {
                    dataSource.Path = "$";
                }
                else if (!dataSource.Path.StartsWith("$"))
                {
                    dataSource.Path = $"$.{dataSource.Path}";
                }

                RelationshipInheritor.InheritRelationships(
                    job.Data.DataSources,
                    dataSource,
                    job.Data.TopLevelDataSource.TableOrView);
            }

            await this.container.Resolve<IConfigValidator>().ValidateDataSourcesAsync(job, logger);

            // add job to the first queue
            var sqlJobQueue = this.container.Resolve<IQueueManager>()
                .CreateInputQueue<SqlJobQueueItem>(++this.stepNumber);

            sqlJobQueue.Add(new SqlJobQueueItem
            {
                Job = job
            });

            sqlJobQueue.AddBatchCompleted(new JobCompletedQueueItem());

            sqlJobQueue.CompleteAdding();

            var processors = this.GetPipelineByName(config.Pipeline, config);

            var fileUploaderFactory = this.container.Resolve<IFileUploaderFactory>();
            var fileUploader = fileUploaderFactory.Create(config.Urls, config.UrlMethod, this.cancellationTokenSource.Token);
            this.container.RegisterInstance(fileUploader);

            var pipelineExecutorFactory = this.container.Resolve<IPipelineExecutorFactory>();

            var pipelineExecutor = pipelineExecutorFactory.Create(this.container, this.cancellationTokenSource);

            logger.Information("Starting pipeline {config} {processors} {TimeoutInMilliseconds}", config, processors, TimeoutInMilliseconds);

            await pipelineExecutor.RunPipelineTasksAsync(config, processors, TimeoutInMilliseconds);

            var stopwatchElapsed = stopwatch.Elapsed;
            stopwatch.Stop();
            Console.WriteLine(stopwatchElapsed);

            logger.Information("Finished pipeline");
            Log.CloseAndFlush();
        }

        /// <summary>
        /// The get pipeline by name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <returns>
        /// The <see cref="IList{T}"/>.
        /// </returns>
        private IList<PipelineStepInfo> GetPipelineByName(PipelineNames name, IQueryConfig config)
        {
            var processors = new List<PipelineStepInfo>();

            if (name == PipelineNames.ElasticSearch)
            {
                if (config.DropAndReloadIndex)
                {
                    processors.AddRange(
                        new List<PipelineStepInfo>
                            {
                                new PipelineStepInfo { Type = typeof(SqlGetSchemaPipelineStep), Count = 1 },
                                new PipelineStepInfo { Type = typeof(SaveSchemaPipelineStep), Count = 1 },
                                new PipelineStepInfo
                                    {
                                        Type = config.UploadToUrl
                                                   ? typeof(MappingUploadPipelineStep)
                                                   : typeof(DummyMappingUploadPipelineStep),
                                        Count = 1
                                    }
                            });
                }
            }

            processors.AddRange(
                new List<PipelineStepInfo>
                    {
                        new PipelineStepInfo { Type = typeof(CreateBatchesPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(CreateBatchesForEachDataSourcePipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(QuerySqlPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(CreateSourceWrappersPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(WriteSourceWrapperCollectionToJsonPipelineStep), Count = 1 },
                        new PipelineStepInfo { Type = typeof(SaveJsonToFilePipelineStep), Count = 1 }
                    });

            if (name == PipelineNames.ElasticSearch)
            {
                processors.AddRange(
                    new List<PipelineStepInfo>
                        {
                            new PipelineStepInfo { Type = typeof(CreateJsonBatchesPipelineStep), Count = 1 },
                            new PipelineStepInfo { Type = typeof(SaveJsonBatchesPipelineStep), Count = 1 }
                        });
            }

            if (!string.IsNullOrWhiteSpace(config.Url) && config.UploadToUrl)
            {
                if (name == PipelineNames.ElasticSearch)
                {
                    if (config.WriteTemporaryFilesToDisk)
                    {
                        processors.Add(new PipelineStepInfo { Type = typeof(FileSavePipelineStep), Count = 1 });
                    }

                    processors.Add(new PipelineStepInfo { Type = typeof(ElasticSearchFileUploadPipelineStep), Count = 1 });
                }
                else
                {
                    processors.Add(new PipelineStepInfo { Type = typeof(SendToRestApiPipelineStep), Count = 1 });
                }
            }
            else
            {
                processors.Add(
                    name == PipelineNames.ElasticSearch
                        ? new PipelineStepInfo { Type = typeof(NullElasticSearchFileUploadPipelineStep), Count = 1 }
                        : new PipelineStepInfo { Type = typeof(NullSendToRestApiPipelineStep), Count = 1 });
            }

            if (name == PipelineNames.ElasticSearch)
            {
                var elasticSearchUploaderFactory = this.container.Resolve<IElasticSearchUploaderFactory>();
                IElasticSearchUploader elasticSearchUploader = elasticSearchUploaderFactory.Create(
                    false,
                    config.Urls,
                    config.Index,
                    config.Alias,
                    config.EntityType,
                    config.UrlMethod);

                this.container.RegisterInstance(elasticSearchUploader);
            }

            processors.Add(new PipelineStepInfo { Type = typeof(BatchCompletedPipelineStep), Count = 1 });
            processors.Add(new PipelineStepInfo { Type = typeof(JobCompletedPipelineStep), Count = 1 });

            return processors;
        }

        /// <summary>
        /// The init container with defaults.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private void InitContainerWithDefaults(IJob job)
        {
            if (!this.container.IsRegistered<ILogger>())
            {
                ILogger logger = new LoggerConfiguration().CreateLogger();
                this.container.RegisterInstance(logger);
            }

            if (!this.container.IsRegistered<ISqlGeneratorFactory>())
            {
                this.container.RegisterType<ISqlGeneratorFactory, SqlGeneratorFactory>();
            }

            if (!this.container.IsRegistered<IQueueFactory>())
            {
                if (job.Config.UseMultipleThreads)
                {
                    this.container.RegisterType<IQueueFactory, InMemoryQueueFactory>();
                }
                else
                {
                    this.container.RegisterType<IQueueFactory, InMemoryQueueWithoutBlockingFactory>();
                }
            }

            if (!this.container.IsRegistered<IQueueManager>())
            {
                var queueManager = new QueueManager(this.container.Resolve<IQueueFactory>());
                this.container.RegisterInstance<IQueueManager>(queueManager);
            }

            if (!this.container.IsRegistered<IJobConfig>())
            {
                this.container.RegisterInstance<IJobConfig>(job.Config);
            }

            if (!this.container.IsRegistered<IConfigValidator>())
            {
                this.container.RegisterType<IConfigValidator, ConfigValidator>();
            }

            if (!this.container.IsRegistered<IEntityJsonWriter>())
            {
                this.container.RegisterType<IEntityJsonWriter, EntityJsonWriter>();
            }

            if (!this.container.IsRegistered<IDetailedTemporaryFileWriter>())
            {
                if (job.Config.WriteDetailedTemporaryFilesToDisk)
                {
                    this.container.RegisterType<IDetailedTemporaryFileWriter, FileWriter>();
                }
                else
                {
                    this.container.RegisterType<IDetailedTemporaryFileWriter, NullFileWriter>();
                }
            }

            if (!this.container.IsRegistered<ITemporaryFileWriter>())
            {
                if (job.Config.WriteTemporaryFilesToDisk)
                {
                    this.container.RegisterType<ITemporaryFileWriter, FileWriter>();
                }
                else
                {
                    this.container.RegisterType<ITemporaryFileWriter, NullFileWriter>();
                }
            }

            if (!this.container.IsRegistered<IFileWriter>())
            {
                this.container.RegisterType<IFileWriter, FileWriter>();
            }

            if (!this.container.IsRegistered<IEntitySavedToJsonLogger>())
            {
                this.container.RegisterType<IEntitySavedToJsonLogger, NullEntitySavedToJsonLogger>();
            }

            if (!this.container.IsRegistered<IBatchEventsLogger>())
            {
                this.container.RegisterType<IBatchEventsLogger, NullBatchEventsLogger>();
            }

            if (!this.container.IsRegistered<IJobEventsLogger>())
            {
                this.container.RegisterType<IJobEventsLogger, NullJobEventsLogger>();
            }

            if (!this.container.IsRegistered<IQuerySqlLogger>())
            {
                this.container.RegisterType<IQuerySqlLogger, NullQuerySqlLogger>();
            }

            if (!this.container.IsRegistered<IProgressMonitor>())
            {
                var progressMonitor = new ProgressMonitor(new NullProgressLogger());
                this.container.RegisterInstance<IProgressMonitor>(progressMonitor);
            }

            if (!this.container.IsRegistered<ISqlConnectionFactory>())
            {
                this.container.RegisterType<ISqlConnectionFactory, ReliableSqlConnectionFactory>();
            }

            if (!this.container.IsRegistered<IDatabusSqlReader>())
            {
                var sqlConnectionFactory = this.container.Resolve<ISqlConnectionFactory>();
                var sqlGeneratorFactory = this.container.Resolve<ISqlGeneratorFactory>();
                var databusSqlReader = new DatabusSqlReader(
                    job.Config.ConnectionString,
                    job.Config.SqlCommandTimeoutInSeconds,
                    sqlConnectionFactory,
                    sqlGeneratorFactory);
                this.container.RegisterInstance<IDatabusSqlReader>(databusSqlReader);
            }

            if (!this.container.IsRegistered<ISchemaLoader>())
            {
                var sqlConnectionFactory = this.container.Resolve<ISqlConnectionFactory>();
                var sqlGeneratorFactory = this.container.Resolve<ISqlGeneratorFactory>();
                var schemaLoader = new SchemaLoader(
                    job.Config.ConnectionString,
                    job.Data.TopLevelDataSource.Key,
                    sqlConnectionFactory,
                    sqlGeneratorFactory);
                this.container.RegisterInstance<ISchemaLoader>(schemaLoader);
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

            if (!this.container.IsRegistered<IHttpRequestInterceptor>())
            {
                if (!string.IsNullOrWhiteSpace(job.Config.UrlUserName))
                {
                    this.container.RegisterInstance<IHttpRequestInterceptor>(
                        new BasicAuthorizationRequestInterceptor(job.Config.UrlUserName, job.Config.UrlPassword));
                }
                else
                {
                    this.container.RegisterType<IHttpRequestInterceptor, DummyHttpRequestInterceptor>();
                }
            }

            if (!this.container.IsRegistered<IHttpResponseInterceptor>())
            {
                this.container.RegisterType<IHttpResponseInterceptor, DummyHttpResponseInterceptor>();
            }

            if (!this.container.IsRegistered<IHttpRequestLogger>())
            {
                this.container.RegisterInstance<IHttpRequestLogger>(new DatabusHttpRequestLogger(this.container.Resolve<ITemporaryFileWriter>(), job.Config.LocalSaveFolder));
            }

            if (!this.container.IsRegistered<IHttpResponseLogger>())
            {
                this.container.RegisterInstance<IHttpResponseLogger>(new DatabusHttpResponseLogger(this.container.Resolve<ITemporaryFileWriter>(), job.Config.LocalSaveFolder));
            }

            if (job.Config.UseMultipleThreads)
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