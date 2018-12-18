// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullBatchEventsLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NullBatchEventsLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Loggers
{
    using Fabric.Databus.Interfaces.Loggers;

    /// <inheritdoc />
    public class NullBatchEventsLogger : IBatchEventsLogger
    {
        /// <inheritdoc />
        public void BatchCompleted(int batchNumber)
        {
        }
    }
}
