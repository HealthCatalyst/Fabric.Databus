// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJobEventsLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IJobEventsLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Loggers
{
    using Fabric.Databus.Interfaces.Queues;

    /// <summary>
    /// The JobEventsLogger interface.
    /// </summary>
    public interface IJobEventsLogger
    {
        /// <summary>
        /// The job completed.
        /// </summary>
        /// <param name="jobCompletedQueueItem">
        /// The job completed queue item.
        /// </param>
        void JobCompleted(IJobCompletedQueueItem jobCompletedQueueItem);
    }
}
