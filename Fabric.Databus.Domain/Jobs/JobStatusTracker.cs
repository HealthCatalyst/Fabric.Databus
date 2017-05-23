using System;
using System.Collections.Concurrent;
using ElasticSearchSqlFeeder.Shared;

namespace Fabric.Databus.Domain.Jobs
{
    public class JobStatusTracker : IJobStatusTracker
    {
        private readonly ConcurrentDictionary<Guid, int> _jobDictionary = new ConcurrentDictionary<Guid, int>();
        private readonly IJobHistoryStore _jobHistoryStore;
        private readonly JobHistoryItem _jobHistoryItem;

        public JobStatusTracker(IJobHistoryStore jobHistoryStore, JobHistoryItem jobHistoryItem)
        {
            if (jobHistoryStore == null)
            {
                throw new ArgumentNullException(nameof(jobHistoryStore));
            }
            if (jobHistoryItem == null)
            {
                throw new ArgumentNullException(nameof(jobHistoryItem));
            }
            _jobHistoryStore = jobHistoryStore;
            _jobHistoryItem = jobHistoryItem;
        }

        public void TrackStart()
        {
            _jobDictionary.AddOrUpdate(_jobHistoryItem.Id, 0, (guid, i) => 0);
            _jobHistoryItem.Status = JobHistoryItem.ExecutingStatus;
            _jobHistoryItem.StartDateTimeUtc = DateTime.UtcNow;
            _jobHistoryItem.ProgressMonitor.JobHistoryUpdateAction = UpdateProgress;
            _jobHistoryStore.UpdateJobHistoryItem(_jobHistoryItem);
        }

        public void TrackError(Exception e)
        {
            _jobHistoryItem.ErrorText = e.ToString();
            _jobHistoryItem.Status = JobHistoryItem.ErrorStatus;
            UpdateLogsAndSave(_jobHistoryItem);
        }

        public void TrackCompletion()
        {
            _jobHistoryItem.EndDateTimeUtc = DateTime.UtcNow;
            _jobHistoryItem.Status = JobHistoryItem.CompletedStatus;
            var elapsedTime = _jobHistoryItem.EndDateTimeUtc - _jobHistoryItem.StartDateTimeUtc;
            _jobHistoryItem.ExecutionTime = elapsedTime.TotalMilliseconds;
            UpdateLogsAndSave(_jobHistoryItem);
        }

        private void UpdateLogsAndSave(JobHistoryItem jobHistoryItem)
        {
            jobHistoryItem.ProgressLogItems = jobHistoryItem.ProgressMonitor.GetSnapshotOfProgressItems();
            _jobHistoryStore.UpdateJobHistoryItem(jobHistoryItem);
            _jobDictionary.TryRemove(jobHistoryItem.Id, out var count);
        }

        public void UpdateProgress()
        {
            if (!_jobDictionary.ContainsKey(_jobHistoryItem.Id)) return;

            var count = _jobDictionary[_jobHistoryItem.Id];
            if (count >= 500)
            {
                _jobHistoryItem.ProgressLogItems = _jobHistoryItem.ProgressMonitor.GetSnapshotOfProgressItems();
                _jobHistoryStore.UpdateJobHistoryItem(_jobHistoryItem);
                _jobDictionary[_jobHistoryItem.Id] = 0;
            }
            else
            {
                _jobDictionary[_jobHistoryItem.Id] = count + 1;
            }
        }
    }
}
