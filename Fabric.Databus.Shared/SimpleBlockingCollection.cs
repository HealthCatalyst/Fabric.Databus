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

    using Fabric.Databus.Interfaces;

    using Serilog;
    using Serilog.Core;

    /// <summary>
    /// The simple blocking collection.
    /// </summary>
    /// <typeparam name="T">type of item
    /// </typeparam>
    public class SimpleBlockingCollection<T> : IMeteredBlockingCollection<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger Logger = new LoggerConfiguration().CreateLogger();

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

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleBlockingCollection{T}"/> class.
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
        /// The take.
        /// </summary>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [System.Diagnostics.DebuggerNonUserCode]
        [System.Diagnostics.DebuggerHidden]
        public T Take()
        {
            try
            {
                var item = this.blockingCollection.Take();

                return item;
            }
            catch (InvalidOperationException)
            {
                // this is thrown when the collection is marked as completed
            }

            return default(T);
        }

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public void Add(T item)
        {
            this.blockingCollection.Add(item);
        }

        /// <summary>
        /// The any.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Any()
        {
            return this.blockingCollection.Any();
        }

        /// <summary>
        /// The try take.
        /// </summary>
        /// <param name="cacheItem">
        /// The cache item.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool TryTake(out T cacheItem)
        {
            var result = this.blockingCollection.TryTake(out cacheItem);

            return result;
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
        /// The complete adding.
        /// </summary>
        public void CompleteAdding()
        {
            this.blockingCollection.CompleteAdding();
        }

        /// <summary>
        /// The name.
        /// </summary>
        public string Name => this.name;
    }
}
