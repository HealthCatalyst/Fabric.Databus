// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMeteredBlockingCollection.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IMeteredBlockingCollection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The MeteredBlockingCollection interface.
    /// </summary>
    public interface IMeteredBlockingCollection
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
    public interface IMeteredBlockingCollection<T> : IMeteredBlockingCollection
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The take.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token
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
        /// The try take.
        /// </summary>
        /// <param name="cacheItem">
        /// The cache item.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool TryTake(out T cacheItem);
    }
}
