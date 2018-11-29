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
    using System.Threading;

    using Fabric.Databus.Config;
    using Fabric.Databus.PipelineRunner;

    using Serilog;

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

            pipelineRunner.RunPipeline(config);

            stopwatch.Stop();

            var timeElapsed = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
            var threadText = config.Config.UseMultipleThreads ? "multiple threads" : "single thread";
            container.Resolve<ILogger>().Verbose($"Finished in {timeElapsed} using {threadText}");
        }
    }
}
