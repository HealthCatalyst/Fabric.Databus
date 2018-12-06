// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueue.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IQueue type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Queues
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The MeteredBlockingCollection interface.
    /// </summary>
    public interface IQueue
    {
        /// <summary>
        /// Gets the count.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a value indicating whether is completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// The any.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool Any();

        /// <summary>
        /// The complete adding.
        /// </summary>
        void CompleteAdding();
    }

    /// <inheritdoc />
    /// <summary>
    /// The MeteredBlockingCollection interface.
    /// </summary>
    /// <typeparam name="T">type of item
    /// </typeparam>
    public interface IQueue<T> : IQueue
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The take.
        /// </summary>
        /// <param name="cancellationToken">
        ///     The cancellation token
        /// </param>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        T Take(CancellationToken cancellationToken);

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        void Add(T item);

        /// <summary>
        /// The add batch completed.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        void AddBatchCompleted(IQueueItem item);

        /// <summary>
        /// The add job completed.
        /// </summary>
        /// <param name="item">
        /// The job completed queue item.
        /// </param>
        void AddJobCompleted(IQueueItem item);

        /// <summary>
        /// The try take.
        /// </summary>
        /// <param name="cacheItem">
        /// The cache item.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool TryTake(out T cacheItem);

        /// <summary>
        /// The take.
        /// </summary>
        /// <param name="cancellationToken">
        /// cancellation token
        /// </param>
        /// <returns>
        /// The <see cref="!:T" />.
        /// </returns>
        IQueueItem TakeGeneric(CancellationToken cancellationToken);

        /// <summary>
        /// The wait till empty async.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task WaitTillEmptyAsync(CancellationToken cancellationToken);
    }
}
