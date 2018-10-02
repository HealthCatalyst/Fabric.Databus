using System;
using System.Collections.Generic;
using System.Text;

namespace Fabric.Databus.Domain.Jobs
{
    using Fabric.Databus.Shared;

    public class JobStatusTrackerFactory : IJobStatusTrackerFactory
    {
        public IJobStatusTracker GetTracker(IJobHistoryStore jobHistoryStore, JobHistoryItem jobHistoryItem)
        {
            return new JobStatusTracker(jobHistoryStore, jobHistoryItem);
        }
    }
}
