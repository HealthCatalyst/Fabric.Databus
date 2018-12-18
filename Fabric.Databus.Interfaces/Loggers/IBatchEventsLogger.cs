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
    /// <summary>
    /// The BatchCompletedLogger interface.
    /// </summary>
    public interface IBatchEventsLogger
    {
        /// <summary>
        /// The batch completed.
        /// </summary>
        /// <param name="batchNumber">
        /// The batch number.
        /// </param>
        void BatchCompleted(int batchNumber);
    }
}
