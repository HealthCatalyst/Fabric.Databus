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

    /// <summary>
    /// The ImportRunner interface.
    /// </summary>
    public interface IImportRunner
    {
        /// <summary>
        /// The run pipeline.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        void RunPipeline(IJob job);

        /// <summary>
        /// The run pipeline.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <param name="jobStatusTracker">
        /// The job status tracker.
        /// </param>
        void RunPipeline(IJob job, IJobStatusTracker jobStatusTracker);
    }
}
