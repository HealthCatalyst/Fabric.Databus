using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ElasticSearchSqlFeeder.Shared;

namespace Fabric.Databus.Domain.Jobs
{

    public class InMemoryJobHistoryStore : IJobHistoryStore
    {
        private static readonly ConcurrentBag<JobHistoryItem> Items = new ConcurrentBag<JobHistoryItem>();

        public IList<JobHistoryItem> GetJobHistory(string jobName)
        {
            return Items.Where(j => j.Name == jobName).ToList();
        }

        public IList<JobHistoryItem> GetMostRecentJobs(int numberOfJobs)
        {
            return Items
                .OrderByDescending(jobHistory => jobHistory.StartDateTimeUtc)
                .Take(numberOfJobs)
                .ToList();
        }

        public JobHistoryItem GetLatestJobHistoryItem(string jobName)
        {
            var latestJobHistoryItem = Items.Where(j => j.Name == jobName)
                .OrderByDescending(jobHistory => jobHistory.StartDateTimeUtc)
                .FirstOrDefault();

            AddCurrentLogsToJobHistoryItem(latestJobHistoryItem);

            return latestJobHistoryItem;
        }

        public void AddJobHistoryItem(JobHistoryItem item)
        {
            Items.Add(item);
        }

        public void UpdateJobHistoryItem(JobHistoryItem item)
        {
            //do nothing since this is an in memory store
        }

        public JobHistoryItem GetJobHistoryItem(Guid executionId)
        {
            var item = Items.FirstOrDefault(jh => jh.Id == executionId);

            if (item != null)
            {
                AddCurrentLogsToJobHistoryItem(item);
            }
            return item;
        }

        private void AddCurrentLogsToJobHistoryItem(JobHistoryItem item)
        {
            item.ProgressLogItems = item.ProgressMonitor.GetSnapshotOfProgressItems();
        }
    }
}
