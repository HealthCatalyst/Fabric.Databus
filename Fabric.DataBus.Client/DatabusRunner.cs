// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusRunner.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   The runner.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Client
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.ElasticSearch;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.Http;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.Shared;
    using Fabric.Databus.Shared.Loggers;

    using PipelineRunner;

    using Serilog;

    using Unity;

    /// <summary>
    /// The runner.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class DatabusRunner
    {
        /// <summary>
        /// The run elastic search pipeline.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        public void RunElasticSearchPipeline(IJob config, CancellationToken cancellationToken)
        {
            using (var progressMonitor = new ProgressMonitor(new ConsoleProgressLogger()))
            {
                var container = new UnityContainer();

                ILogger logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo
                    .File(Path.Combine(Path.GetTempPath(), "Databus.out.txt")).CreateLogger();

                container.RegisterInstance<IProgressMonitor>(progressMonitor);

                var databusSqlReader = new DatabusSqlReader(config.Config.ConnectionString, 0);
                container.RegisterInstance<IDatabusSqlReader>(databusSqlReader);
                container.RegisterType<IElasticSearchUploaderFactory, ElasticSearchUploaderFactory>();
                container.RegisterType<IElasticSearchUploader, ElasticSearchUploader>();
                container.RegisterType<IHttpClientFactory, HttpClientFactory>();
                container.RegisterInstance(logger);

                if (config.Config.UseMultipleThreads)
                {
                    container.RegisterType<IPipelineExecutorFactory, MultiThreadedPipelineExecutorFactory>();
                }
                else
                {
                    container.RegisterType<IPipelineExecutorFactory, SingleThreadedPipelineExecutorFactory>();
                }

                this.RunElasticSearchPipeline(container, config, cancellationToken);
            }
        }

        /// <summary>
        /// The run elastic search pipeline.
        /// </summary>
        /// <param name="container">
        ///     The container.
        /// </param>
        /// <param name="config">
        ///     The config.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token
        /// </param>
        public void RunElasticSearchPipeline(
            IUnityContainer container,
            IJob config,
            CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var pipelineRunner = new PipelineRunner(container, cancellationToken);

            pipelineRunner.RunElasticSearchPipeline(config);

            stopwatch.Stop();

            var timeElapsed = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
            var threadText = config.Config.UseMultipleThreads ? "multiple threads" : "single thread";
            container.Resolve<ILogger>().Verbose($"Finished in {timeElapsed} using {threadText}");
        }

        /// <summary>
        /// The run elastic search pipeline.
        /// </summary>
        /// <param name="container">
        ///     The container.
        /// </param>
        /// <param name="config">
        ///     The config.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token
        /// </param>
        public void RunRestApiPipeline(
            IUnityContainer container,
            IJob config,
            CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var pipelineRunner = new PipelineRunner(container, cancellationToken);

            pipelineRunner.RunRestApiPipeline(config);

            stopwatch.Stop();

            var timeElapsed = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
            var threadText = config.Config.UseMultipleThreads ? "multiple threads" : "single thread";
            container.Resolve<ILogger>().Verbose($"Finished in {timeElapsed} using {threadText}");
        }

    }
}
