// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryQueueFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the InMemoryQueueFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The advanced queue factory.
    /// </summary>
    public class InMemoryQueueFactory : IQueueFactory
    {
        /// <inheritdoc />
        public IQueue Create<T>(string name)
            where T : class, IQueueItem
        {
            return new InMemoryQueue<T>(name);
        }
    }
}
