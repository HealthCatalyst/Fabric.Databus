// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryQueueWithoutBlockingFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the InMemoryQueueWithoutBlockingFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    public class InMemoryQueueWithoutBlockingFactory : IQueueFactory
    {
        /// <inheritdoc />
        public IQueue Create<T>(string name)
            where T : class, IQueueItem
        {
            return new InMemoryQueueWithoutBlocking<T>(name);
        }
    }
}
