using System;
using System.Collections.Generic;

namespace Fabric.Databus.Domain.Jobs
{
    using Fabric.Databus.Shared;

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
