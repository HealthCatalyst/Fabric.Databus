// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestBatchEventsLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TestBatchEventsLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests.Helpers
{
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The test batch events logger.
    /// </summary>
    public class TestBatchEventsLogger : IBatchEventsLogger
    {
        /// <summary>
        /// Gets the batch completed queue item.
        /// </summary>
        public List<IBatchCompletedQueueItem> BatchCompletedQueueItems { get; } = new List<IBatchCompletedQueueItem>();

        /// <inheritdoc />
        public void BatchCompleted(IBatchCompletedQueueItem batchCompletedQueueItem1)
        {
            this.BatchCompletedQueueItems.Add(batchCompletedQueueItem1);
        }
    }
}
