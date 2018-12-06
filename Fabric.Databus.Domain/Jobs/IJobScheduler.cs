// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJobScheduler.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IJobScheduler type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Domain.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Shared;

    using Serilog;

    /// <summary>
    /// The JobScheduler interface.
    /// </summary>
    public interface IJobScheduler
    {
        /// <summary>
        /// The schedule job.
        /// </summary>
        /// <param name="queryConfig">
        /// The query config.
        /// </param>
        /// <param name="jobName">
        /// The job name.
        /// </param>
        void ScheduleJob(QueryConfig queryConfig, string jobName);

        /// <summary>
        /// The execute job immediately.
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        Guid ExecuteJobImmediately(Job query);

        /// <summary>
        /// The get job status.
        /// </summary>
        /// <param name="jobGuid">
        /// The job guid.
        /// </param>
        /// <returns>
        /// The <see cref="JobHistoryItem"/>.
        /// </returns>
        JobHistoryItem GetJobStatus(Guid jobGuid);

        /// <summary>
        /// The get job status.
        /// </summary>
        /// <param name="jobName">
        /// The job name.
        /// </param>
        /// <returns>
        /// The <see cref="JobHistoryItem"/>.
        /// </returns>
        JobHistoryItem GetJobStatus(string jobName);

        /// <summary>
        /// The get job history.
        /// </summary>
        /// <param name="jobName">
        /// The job name.
        /// </param>
        /// <returns>
        /// The <see cref="ICollection{T}"/>.
        /// </returns>
        ICollection<JobHistoryItem> GetJobHistory(string jobName);

        /// <summary>
        /// The get most recent jobs.
        /// </summary>
        /// <param name="numberOfJobs">
        /// The number of jobs.
        /// </param>
        /// <returns>
        /// The <see cref="ICollection{T}"/>.
        /// </returns>
        ICollection<JobHistoryItem> GetMostRecentJobs(int numberOfJobs);

        /// <summary>
        /// The validate job.
        /// </summary>
        /// <param name="queryConfig">
        /// The query config.
        /// </param>
        /// <param name="jobName">
        /// The job name.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<ConfigValidationResult> ValidateJobAsync(string queryConfig, string jobName, ILogger logger);
    }
}
