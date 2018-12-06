// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleThreadedQueueFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SingleThreadedQueueFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The queue factory.
    /// </summary>
    public class SingleThreadedQueueFactory : IQueueFactory
    {
        /// <inheritdoc />
        public IQueue Create<T>(string name)
            where T : class, IQueueItem
        {
            return new SingleThreadedQueue<T>(name);
        }
    }
}