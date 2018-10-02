// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleBlockingCollection.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SimpleBlockingCollection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;

    using Fabric.Databus.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The simple blocking collection.
    /// </summary>
    /// <typeparam name="T">type of item
    /// </typeparam>
    public class SimpleBlockingCollection<T> : IMeteredBlockingCollection<T>
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
        /// Initializes a new instance of the <see cref="SimpleBlockingCollection{T}"/> class.
        /// </summary>
        /// <param name="concurrentQueue">
        /// The concurrent queue.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public SimpleBlockingCollection(IProducerConsumerCollection<T> concurrentQueue, string name)
        {
            this.blockingCollection = new BlockingCollection<T>(concurrentQueue);
            this.name = name;
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.Shared.SimpleBlockingCollection`1" /> class.
        /// </summary>
        /// <param name="concurrentQueue">
        /// The concurrent queue.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        public SimpleBlockingCollection(IProducerConsumerCollection<T> concurrentQueue, string name, int maxItems)
            : this(concurrentQueue, name)
        {
        }

        /// <summary>
        /// The count.
        /// </summary>
        public int Count => this.blockingCollection.Count;

        /// <summary>
        /// The is completed.
        /// </summary>
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
        public void Add(T item)
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
            var result = this.blockingCollection.TryTake(out cacheItem);

            return result;
        }

        /// <summary>
        /// The complete adding.
        /// </summary>
        public void CompleteAdding()
        {
            this.blockingCollection.CompleteAdding();
        }
    }
}
