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
    using Fabric.Databus.Shared;

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
            var container = new UnityContainer();

            ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(Path.Combine(Path.GetTempPath(), "Databus.out.txt"))
                .CreateLogger();

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

            this.RunElasticSearchPipeline(container, config, logger);
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
        /// <param name="logger">
        ///     The logger.
        /// </param>
        public void RunElasticSearchPipeline(IUnityContainer container, IJob config, ILogger logger)
        {
            using (var progressMonitor = new ProgressMonitor(new ConsoleProgressLogger()))
            {
                using (var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource())
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);

                    pipelineRunner.RunPipeline(config, progressMonitor);

                    stopwatch.Stop();

                    var timeElapsed = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                    var threadText = config.Config.UseMultipleThreads ? "multiple threads" : "single thread";
                    logger.Verbose($"Finished in {timeElapsed} using {threadText}");
                }
            }
        }
    }
}
