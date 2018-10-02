// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJobStatusTracker.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IJobStatusTracker type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



namespace Fabric.Databus.Domain.Jobs
{
    using System;

    public interface IJobStatusTracker
    {
        void TrackStart();

        void TrackError(Exception e);

        void TrackCompletion();

        void UpdateProgress();

    }
}
