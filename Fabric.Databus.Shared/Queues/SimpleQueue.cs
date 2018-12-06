// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleQueue.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SimpleQueue type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;

    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The simple blocking collection.
    /// </summary>
    /// <typeparam name="T">type of item
    /// </typeparam>
    public class SimpleQueue<T> : IQueue<T>
        where T : class, IQueueItem
    {
        /// <summary>
        /// The blocking collection.
        /// </summary>
        private readonly BlockingCollection<T> blockingCollection;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleQueue{T}"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public SimpleQueue(string name)
        {
            var concurrentQueue = new ConcurrentQueue<T>();
            this.blockingCollection = new BlockingCollection<T>(concurrentQueue);
            this.name = name;
        }

        /// <inheritdoc />
        public int Count => this.blockingCollection.Count;

        /// <inheritdoc />
        public bool IsCompleted => this.blockingCollection.IsCompleted;

        /// <summary>
        /// The name.
        /// </summary>
        public string Name => this.name;

        /// <inheritdoc />
        /// <summary>
        /// The take.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// The <see cref="!:T" />.
        /// </returns>
        [System.Diagnostics.DebuggerNonUserCode]
        [System.Diagnostics.DebuggerHidden]
        public T Take(CancellationToken cancellationToken)
        {
            try
            {
                return this.blockingCollection.Take(cancellationToken);
            }
            catch (InvalidOperationException)
            {
            }

            return default(T);
        }

        /// <inheritdoc />
        /// <summary>
        /// The take.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// The <see cref="!:T" />.
        /// </returns>
        [System.Diagnostics.DebuggerNonUserCode]
        [System.Diagnostics.DebuggerHidden]
        public IQueueItem TakeGeneric(CancellationToken cancellationToken)
        {
            try
            {
                return this.blockingCollection.Take(cancellationToken);
            }
            catch (InvalidOperationException)
            {
            }

            return default(T);
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            this.blockingCollection.Add(item);
        }

        /// <inheritdoc />
        public void AddBatchCompleted(IQueueItem item)
        {
        }

        /// <inheritdoc />
        public void AddJobCompleted(IQueueItem item)
        {
        }

        /// <inheritdoc />
        public bool Any()
        {
            return this.blockingCollection.Any();
        }

        /// <inheritdoc />
        public bool TryTake(out T cacheItem)
        {
            var result = this.blockingCollection.TryTake(out cacheItem);

            return result;
        }

        /// <inheritdoc />
        public void CompleteAdding()
        {
            this.blockingCollection.CompleteAdding();
        }
    }
}
