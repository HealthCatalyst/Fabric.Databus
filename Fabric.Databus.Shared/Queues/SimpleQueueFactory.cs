// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleQueueFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SimpleQueueFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using System.Collections.Concurrent;

    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The queue factory.
    /// </summary>
    public class SimpleQueueFactory : IQueueFactory
    {
        /// <inheritdoc />
        public IQueue Create<T>(string name)
        {
            return new SimpleQueue<T>(new ConcurrentQueue<T>(), name);
        }
    }
}