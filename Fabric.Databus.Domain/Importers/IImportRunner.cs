// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IImportRunner.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IImportRunner type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Domain.Importers
{
    using System.Threading;

    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.Jobs;

    /// <summary>
    /// The ImportRunner interface.
    /// </summary>
    public interface IImportRunner
    {
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
        /// The cancellation token.
        /// </param>
        void RunPipeline(IJob config, IProgressMonitor progressMonitor, IJobStatusTracker jobStatusTracker, CancellationToken cancellationToken);
    }
}
