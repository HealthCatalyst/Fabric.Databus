namespace Fabric.Databus.Domain.Jobs
{
    using Fabric.Databus.Shared;

    public interface IJobStatusTrackerFactory
    {
        IJobStatusTracker GetTracker(IJobHistoryStore jobHistoryStore, JobHistoryItem jobHistoryItem);
    }
}