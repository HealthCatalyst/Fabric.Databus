// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryQueue.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the InMemoryQueue type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The advanced queue.
    /// </summary>
    /// <typeparam name="T">
    /// type of queue
    /// </typeparam>
    public class InMemoryQueue<T> : IQueue<T>
        where T : class, IQueueItem
    {
        /// <summary>
        /// The blocking collection.
        /// </summary>
        private readonly BlockingCollection<IQueueItem> blockingCollection;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryQueue{T}"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public InMemoryQueue(string name)
        {
            var concurrentQueue = new ConcurrentQueue<IQueueItem>();
            this.blockingCollection = new BlockingCollection<IQueueItem>(concurrentQueue);
            this.name = name;
        }

        /// <inheritdoc />
        public int Count => this.blockingCollection.Count(c => c is T);

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
        public virtual async Task WaitTillEmptyAsync(CancellationToken cancellationToken)
        {
            while (this.Any())
            {
                await Task.Delay(1000, cancellationToken);
            }
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            this.blockingCollection.Add(item);
        }

        /// <inheritdoc />
        public void AddBatchCompleted(IQueueItem item)
        {
            this.blockingCollection.Add(item);
        }

        /// <inheritdoc />
        public void AddJobCompleted(IQueueItem item)
        {
            this.blockingCollection.Add(item);
        }

        /// <inheritdoc />
        public bool Any()
        {
            return this.blockingCollection.Any();
        }

        /// <inheritdoc />
        public bool TryTake(out T cacheItem)
        {
            var result = this.blockingCollection.TryTake(out var cacheItem1);

            cacheItem = cacheItem1 as T;
            return result;
        }

        /// <inheritdoc />
        public void CompleteAdding()
        {
            // this.blockingCollection.CompleteAdding();
        }
    }
}
