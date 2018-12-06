// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdvancedQueueFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the AdvancedQueueFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The advanced queue factory.
    /// </summary>
    public class AdvancedQueueFactory : IQueueFactory
    {
        /// <inheritdoc />
        public IQueue Create<T>(string name)
            where T : class, IQueueItem
        {
            return new AdvancedQueue<T>(name);
        }
    }
}
