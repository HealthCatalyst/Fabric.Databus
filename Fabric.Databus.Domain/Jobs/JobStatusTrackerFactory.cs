using System;
using System.Collections.Generic;
using System.Text;
using ElasticSearchSqlFeeder.Shared;

namespace Fabric.Databus.Domain.Jobs
{
    public class JobStatusTrackerFactory : IJobStatusTrackerFactory
    {
        public IJobStatusTracker GetTracker(IJobHistoryStore jobHistoryStore, JobHistoryItem jobHistoryItem)
        {
            return new JobStatusTracker(jobHistoryStore, jobHistoryItem);
        }
    }
}
