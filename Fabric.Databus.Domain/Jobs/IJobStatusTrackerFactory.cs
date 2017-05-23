using ElasticSearchSqlFeeder.Shared;

namespace Fabric.Databus.Domain.Jobs
{
    public interface IJobStatusTrackerFactory
    {
        IJobStatusTracker GetTracker(IJobHistoryStore jobHistoryStore, JobHistoryItem jobHistoryItem);
    }
}