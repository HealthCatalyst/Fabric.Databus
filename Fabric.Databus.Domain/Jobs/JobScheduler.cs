// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobScheduler.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JobScheduler type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Domain.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ConfigValidators;
    using Fabric.Databus.Domain.Importers;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.Shared;

    using Serilog;

    /// <summary>
    /// The job scheduler.
    /// </summary>
    public class JobScheduler : IJobScheduler
    {
        /// <summary>
        /// The _logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The _job history store.
        /// </summary>
        private readonly IJobHistoryStore jobHistoryStore;

        /// <summary>
        /// The import runner.
        /// </summary>
        private readonly IImportRunner importRunner;

        /// <summary>
        /// The _config validator.
        /// </summary>
        private readonly IConfigValidator configValidator;

        /// <summary>
        /// The job status tracker factory.
        /// </summary>
        private readonly IJobStatusTrackerFactory jobStatusTrackerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobScheduler"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="jobHistoryStore">
        /// The job history store.
        /// </param>
        /// <param name="jobStatusTrackerFactory">
        /// The job status tracker factory.
        /// </param>
        /// <param name="importRunner">
        /// The import runner.
        /// </param>
        /// <param name="configValidator">
        /// The config validator.
        /// </param>
        /// <exception cref="ArgumentNullException">argument exception
        /// </exception>
        public JobScheduler(
            ILogger logger,
            IJobHistoryStore jobHistoryStore,
            IJobStatusTrackerFactory jobStatusTrackerFactory,
            IImportRunner importRunner,
            IConfigValidator configValidator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.jobHistoryStore = jobHistoryStore ?? throw new ArgumentNullException(nameof(jobHistoryStore));
            this.importRunner = importRunner ?? throw new ArgumentNullException(nameof(importRunner));
            this.configValidator = configValidator ?? throw new ArgumentNullException(nameof(configValidator));
            this.jobStatusTrackerFactory = jobStatusTrackerFactory ?? throw new ArgumentNullException(nameof(jobStatusTrackerFactory));
        }

        /// <summary>
        /// The schedule job.
        /// </summary>
        /// <param name="queryConfig">
        /// The query config.
        /// </param>
        /// <param name="jobName">
        /// The job name.
        /// </param>
        /// <exception cref="NotImplementedException">not implemented exception
        /// </exception>
        public void ScheduleJob(QueryConfig queryConfig, string jobName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The execute job immediately.
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        public Guid ExecuteJobImmediately(Job query)
        {
            var jobHistoryItem = this.CreateJobHistoryItem(query);
            this.jobHistoryStore.AddJobHistoryItem(jobHistoryItem);
            var jobStatusTracker = this.jobStatusTrackerFactory.GetTracker(this.jobHistoryStore, jobHistoryItem);
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                Task.Run(() => this.importRunner.RunPipeline(query, jobStatusTracker), cancellationTokenSource.Token);
            }
            return jobHistoryItem.Id;
        }

        /// <summary>
        /// The validate job.
        /// </summary>
        /// <param name="queryConfig">
        /// The query config.
        /// </param>
        /// <param name="jobName">
        /// The job name.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ConfigValidationResult> ValidateJob(string queryConfig, string jobName)
        {
            var result = await this.configValidator.ValidateFromTextAsync(queryConfig);

            return result;
        }

        /// <summary>
        /// The get job status.
        /// </summary>
        /// <param name="jobGuid">
        /// The job guid.
        /// </param>
        /// <returns>
        /// The <see cref="JobHistoryItem"/>.
        /// </returns>
        public JobHistoryItem GetJobStatus(Guid jobGuid)
        {
            return this.jobHistoryStore.GetJobHistoryItem(jobGuid);
        }

        /// <summary>
        /// The get job status.
        /// </summary>
        /// <param name="jobName">
        /// The job name.
        /// </param>
        /// <returns>
        /// The <see cref="JobHistoryItem"/>.
        /// </returns>
        public JobHistoryItem GetJobStatus(string jobName)
        {
            return this.jobHistoryStore.GetLatestJobHistoryItem(jobName);
        }

        /// <summary>
        /// The get job history.
        /// </summary>
        /// <param name="jobName">
        /// The job name.
        /// </param>
        /// <returns>
        /// The <see cref="ICollection{T}"/>.
        /// </returns>
        public ICollection<JobHistoryItem> GetJobHistory(string jobName)
        {
            return this.jobHistoryStore.GetJobHistory(jobName);
        }

        /// <summary>
        /// The get most recent jobs.
        /// </summary>
        /// <param name="numberOfJobs">
        /// The number of jobs.
        /// </param>
        /// <returns>
        /// The <see cref="ICollection{T}"/>.
        /// </returns>
        public ICollection<JobHistoryItem> GetMostRecentJobs(int numberOfJobs)
        {
            return this.jobHistoryStore.GetMostRecentJobs(numberOfJobs);
        }

        /// <summary>
        /// The create job history item.
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <returns>
        /// The <see cref="JobHistoryItem"/>.
        /// </returns>
        private JobHistoryItem CreateJobHistoryItem(Job query)
        {
            var jobHistoryItem = new JobHistoryItem
                                     {
                                         Id = Guid.NewGuid(),
                                         ExecutedQuery = query,
                                         Name = query.Config.Name,
                                         Status = JobHistoryItem.ScheduledStatus,
                                         ProgressMonitor = new ProgressMonitor(null)
                                     };

            return jobHistoryItem;
        }
    }
}