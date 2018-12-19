// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullJobEventsLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NullJobEventsLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Loggers
{
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    public class NullJobEventsLogger : IJobEventsLogger
    {
        /// <inheritdoc />
        public void JobCompleted(IJobCompletedQueueItem jobCompletedQueueItem)
        {
        }
    }
}
