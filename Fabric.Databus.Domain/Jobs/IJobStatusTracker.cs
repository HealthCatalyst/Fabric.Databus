using System;
using ElasticSearchSqlFeeder.Shared;

namespace Fabric.Databus.Domain.Jobs
{
    public interface IJobStatusTracker
    {
        void TrackStart();

        void TrackError(Exception e);

        void TrackCompletion();

        void UpdateProgress();

    }
}
