// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestJobEventsLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TestJobEventsLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests.Helpers
{
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The test job events logger.
    /// </summary>
    public class TestJobEventsLogger : IJobEventsLogger
    {
        /// <summary>
        /// Gets the job completed queue items.
        /// </summary>
        public List<IJobCompletedQueueItem> JobCompletedQueueItems { get; } = new List<IJobCompletedQueueItem>();

        /// <inheritdoc />
        public void JobCompleted(IJobCompletedQueueItem jobCompletedQueueItem)
        {
            this.JobCompletedQueueItems.Add(jobCompletedQueueItem);
        }
    }
}
