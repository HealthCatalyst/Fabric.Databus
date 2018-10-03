// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueueManager.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IQueueManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Queues
{
    /// <summary>
    /// The QueueManager interface.
    /// </summary>
    public interface IQueueManager
    {
        //void CompleteAdding<T>();
        //void WaitTillAllQueuesAreCompleted<T>();
        /// <summary>
        /// The get unique id.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        int GetUniqueId();

        /// <summary>
        /// The get input queue.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        /// <typeparam name="T">type of item
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        IQueue<T> GetInputQueue<T>(int stepNumber);

        /// <summary>
        /// The get output queue.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        /// <typeparam name="T">type of item
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        IQueue<T> GetOutputQueue<T>(int stepNumber);

        /// <summary>
        /// The create output queue.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        /// <typeparam name="T">type of item
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        IQueue<T> CreateOutputQueue<T>(int stepNumber);

        /// <summary>
        /// The create input queue.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        /// <typeparam name="T">type of item
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        IQueue<T> CreateInputQueue<T>(int stepNumber);
    }
}
