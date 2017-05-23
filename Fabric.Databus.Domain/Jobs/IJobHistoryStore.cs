using System;
using System.Collections.Generic;
using ElasticSearchSqlFeeder.Shared;

namespace Fabric.Databus.Domain.Jobs
{
    public interface IJobHistoryStore
    {
        IList<JobHistoryItem> GetJobHistory(string jobName);

        IList<JobHistoryItem> GetMostRecentJobs(int numberOfJobs);

        JobHistoryItem GetLatestJobHistoryItem(string jobName);

        JobHistoryItem GetJobHistoryItem(Guid executionId);

        void AddJobHistoryItem(JobHistoryItem item);

        void UpdateJobHistoryItem(JobHistoryItem item);
    }
}
