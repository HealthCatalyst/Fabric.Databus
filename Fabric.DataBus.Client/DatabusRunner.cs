// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusRunner.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   The runner.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.DataBus.Client
{
    using System.Diagnostics;
    using System.Threading;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;

    using PipelineRunner;

    using Unity;

    /// <summary>
    /// The runner.
    /// </summary>
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (ProgressMonitor progressMonitor = new ProgressMonitor(new ConsoleProgressLogger()))
            {
                var container = new UnityContainer();
                container.RegisterType<IDatabusSqlReader, DatabusSqlReader>();

                var pipelineRunner = new PipelineRunner(container, cancellationToken);

                pipelineRunner.RunPipeline(config, progressMonitor);
            }

            stopwatch.Stop();
        }
    }
}
