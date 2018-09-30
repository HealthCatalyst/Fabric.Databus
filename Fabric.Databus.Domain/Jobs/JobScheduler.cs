namespace Fabric.Databus.Domain.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ConfigValidators;
    using Fabric.Databus.Domain.Importers;
    using Fabric.Databus.Domain.ProgressMonitors;

    using Serilog;

    public class JobScheduler : IJobScheduler
    {
        private readonly ILogger _logger;
        private readonly IJobHistoryStore _jobHistoryStore;
        private readonly IImportRunner _importRunner;
        private readonly IConfigValidator _configValidator;
        private readonly IJobStatusTrackerFactory _jobStatusTrackerFactory;

        public JobScheduler(ILogger logger, 
            IJobHistoryStore jobHistoryStore, 
            IJobStatusTrackerFactory jobStatusTrackerFactory,
            IImportRunner importRunner,
            IConfigValidator configValidator)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (jobHistoryStore == null)
            {
                throw new ArgumentNullException(nameof(jobHistoryStore));
            }
            if (importRunner == null)
            {
                throw new ArgumentNullException(nameof(importRunner));
            }
            if (configValidator == null)
            {
                throw new ArgumentNullException(nameof(configValidator));
            }
            if (jobStatusTrackerFactory == null)
            {
                throw new ArgumentNullException(nameof(jobStatusTrackerFactory));
            }
            _logger = logger;
            _jobHistoryStore = jobHistoryStore;
            _importRunner = importRunner;
            _configValidator = configValidator;
            _jobStatusTrackerFactory = jobStatusTrackerFactory;
        }

        public void ScheduleJob(QueryConfig queryConfig, string jobName)
        {
            throw new NotImplementedException();
        }

        public Guid ExecuteJobImmediately(Job query)
        {
            var jobHistoryItem = CreateJobHistoryItem(query);
            _jobHistoryStore.AddJobHistoryItem(jobHistoryItem);
            var jobStatusTracker = _jobStatusTrackerFactory.GetTracker(_jobHistoryStore, jobHistoryItem);
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                Task.Run(() => _importRunner.RunPipeline(query, jobHistoryItem.ProgressMonitor, jobStatusTracker));
            }
            return jobHistoryItem.Id;
        }

        public async Task<ConfigValidationResult> ValidateJob(string queryConfig, string jobName)
        {
            var result = await _configValidator.ValidateFromText(queryConfig);

            return result;
        }

        public JobHistoryItem GetJobStatus(Guid jobGuid)
        {
            return _jobHistoryStore.GetJobHistoryItem(jobGuid);
        }

        public JobHistoryItem GetJobStatus(string jobName)
        {
            return _jobHistoryStore.GetLatestJobHistoryItem(jobName);
        }

        public ICollection<JobHistoryItem> GetJobHistory(string jobName)
        {
            return _jobHistoryStore.GetJobHistory(jobName);
        }

        public ICollection<JobHistoryItem> GetMostRecentJobs(int numberofJobs)
        {
            return _jobHistoryStore.GetMostRecentJobs(numberofJobs);
        }
        

        private JobHistoryItem CreateJobHistoryItem(Job query)
        {
            var jobHistoryItem = new JobHistoryItem
            {
                Id = Guid.NewGuid(),
                ExecutedQuery = query,
                Name = query.Config.Name,
                Status = JobHistoryItem.ScheduledStatus,
            };

            jobHistoryItem.ProgressMonitor = new ProgressMonitor(null);

            return jobHistoryItem;
        }
        
    }
}
