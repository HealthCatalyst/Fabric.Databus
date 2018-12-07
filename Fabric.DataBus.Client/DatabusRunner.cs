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
    using Fabric.Databus.PipelineRunner;
    using Fabric.Databus.Shared.Loggers;

    using Microsoft.Extensions.Configuration;

    using Serilog;
    using Serilog.Core;

    using Unity;

    /// <summary>
    /// The runner.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class DatabusRunner
    {
        /// <summary>
        /// The run pipeline.
        /// </summary>
        /// <param name="container">
        ///     The container.
        /// </param>
        /// <param name="job">
        ///     The job.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token
        /// </param>
        public void RunRestApiPipeline(
            IUnityContainer container,
            IJob job,
            CancellationToken cancellationToken)
        {
            if (!container.IsRegistered<ILogger>())
            {
                if (job.Config.LogToSeq || !string.IsNullOrWhiteSpace(job.Config.LogFile))
                {
                    var loggerConfiguration = new LoggerConfiguration().Enrich.With(new ThreadIdEnricher());

                    loggerConfiguration = job.Config.LogVerbose
                                              ? loggerConfiguration.MinimumLevel.Verbose()
                                              : loggerConfiguration.MinimumLevel.Information();

                    if (!string.IsNullOrWhiteSpace(job.Config.LogFile))
                    {
                        loggerConfiguration =
                            loggerConfiguration.WriteTo.File(job.Config.LogFile, rollingInterval: RollingInterval.Day);
                    }

                    if (job.Config.LogToSeq)
                    {
                        loggerConfiguration = loggerConfiguration.WriteTo
                            .Seq("http://localhost:5341");
                    }

                    ILogger logger = loggerConfiguration.CreateLogger();
                    container.RegisterInstance(logger);


                    logger.Information("DatabusRunner start with job config");
                }
                else
                {
                    if (File.Exists(Path.Combine(System.AppContext.BaseDirectory, "serilog-config.json")))
                    {
                        var configuration = new ConfigurationBuilder()
                            .SetBasePath(System.AppContext.BaseDirectory)
                            // ReSharper disable once StringLiteralTypo
                            .AddJsonFile("serilog-config.json")
                            .Build();

                        ILogger logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(configuration)
                            .CreateLogger();
                        container.RegisterInstance(logger);


                        logger.Information("DatabusRunner start from serilog-config.json");
                    }
                }
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var pipelineRunner = new PipelineRunner(container, cancellationToken);

            pipelineRunner.RunPipeline(job);

            stopwatch.Stop();

            var timeElapsed = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
            var threadText = job.Config.UseMultipleThreads ? "multiple threads" : "single thread";
            container.Resolve<ILogger>().Verbose("Finished in {timeElapsed} using {threadText}", timeElapsed, threadText);
        }
    }
}
