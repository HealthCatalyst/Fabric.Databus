// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IImportRunner.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IImportRunner type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Domain.Importers
{
    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.Jobs;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Loggers;

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
        void RunPipeline(IJob config, IProgressMonitor progressMonitor, IJobStatusTracker jobStatusTracker);
    }
}
