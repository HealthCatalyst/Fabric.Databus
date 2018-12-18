// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBatchEventsLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IBatchEventsLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Loggers
{
    using Fabric.Databus.Interfaces.Queues;

    /// <summary>
    /// The BatchCompletedLogger interface.
    /// </summary>
    public interface IBatchEventsLogger
    {
        /// <summary>
        /// The batch completed.
        /// </summary>
        /// <param name="batchCompletedQueueItem">
        /// The batch Completed Queue Item.
        /// </param>
        void BatchCompleted(IBatchCompletedQueueItem batchCompletedQueueItem);
    }
}
