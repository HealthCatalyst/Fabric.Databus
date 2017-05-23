using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Config;

namespace Fabric.Databus.Domain.Jobs
{
    public interface IJobScheduler
    {
        void ScheduleJob(QueryConfig queryConfig, string jobName);

        Guid ExecuteJobImmediately(Job query);

        JobHistoryItem GetJobStatus(Guid jobGuid);

        JobHistoryItem GetJobStatus(string jobName);

        ICollection<JobHistoryItem> GetJobHistory(string jobName);

        ICollection<JobHistoryItem> GetMostRecentJobs(int numberofJobs);

        Task<ConfigValidationResult> ValidateJob(string queryConfig, string jobName);
    }
}
