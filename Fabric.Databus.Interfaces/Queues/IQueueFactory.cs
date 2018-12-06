// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueueFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The QueueFactory interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Queues
{
    /// <summary>
    /// The QueueFactory interface.
    /// </summary>
    public interface IQueueFactory
    {
        /// <summary>
        /// The internal create queue.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <typeparam name="T">
        /// type of queue
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        IQueue Create<T>(string name) where T : class, IQueueItem;
    }
}