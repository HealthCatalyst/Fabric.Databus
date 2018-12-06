// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiThreadedQueueFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MultiThreadedQueueFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The advanced queue factory.
    /// </summary>
    public class MultiThreadedQueueFactory : IQueueFactory
    {
        /// <inheritdoc />
        public IQueue Create<T>(string name)
            where T : class, IQueueItem
        {
            return new MultiThreadedQueue<T>(name);
        }
    }
}
