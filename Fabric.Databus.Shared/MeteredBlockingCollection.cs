// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeteredBlockingCollection.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the MeteredBlockingCollection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces;

    using Serilog;

    /// <summary>
    /// The metered blocking collection.
    /// </summary>
    /// <typeparam name="T">item type
    /// </typeparam>
    public class MeteredBlockingCollection<T> : IMeteredBlockingCollection<T>
    {
        /// <summary>
        /// The logger.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Logger = new LoggerConfiguration().CreateLogger();

        private readonly BlockingCollection<T> _blockingCollection;

        private readonly int _maxItems;
        private readonly object _locker = new object();
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeteredBlockingCollection{T}"/> class.
        /// </summary>
        /// <param name="concurrentQueue">
        /// The concurrent queue.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public MeteredBlockingCollection(IProducerConsumerCollection<T> concurrentQueue, string name)
        {
            this._blockingCollection = new BlockingCollection<T>(concurrentQueue);
            this._name = name;
        }

        /// <inheritdoc />
        public MeteredBlockingCollection(IProducerConsumerCollection<T> concurrentQueue, string name, int maxItems)
            : this(concurrentQueue, name)
        {
            this._maxItems = maxItems;
        }

        /// <param name="cancellationToken"></param>
        /// <inheritdoc />
        public T Take(CancellationToken cancellationToken)
        {
            var item = this._blockingCollection.Take();
            this.ReleaseLockIfNeeded();

            return item;
        }

        /// <summary>
        /// The release lock if needed.
        /// </summary>
        private void ReleaseLockIfNeeded()
        {
            if (this._maxItems > 0)
            {
                lock (this._locker) // Let's now wake up the thread by
                {
                    if (this.Count <= this._maxItems)
                    {
                        Logger.Verbose($"MeteredQueue.ReleaseAll {this._name}");

                        Monitor.PulseAll(this._locker);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            this.BlockIfNeeded();

            this._blockingCollection.Add(item);
        }

        /// <summary>
        /// The block if needed.
        /// </summary>
        private void BlockIfNeeded()
        {
            if (this._maxItems > 0)
            {
                // if we have enough items in the queue then block
                // http://www.albahari.com/threading/part4.aspx#_Signaling_with_Wait_and_Pulse
                lock (this._locker)
                {
                    bool didLock = false;
                    while (this.Count > this._maxItems)
                    {
                        didLock = true;
                        Logger.Verbose($"MeteredQueue.Block {this._name}, Count={this.Count:N0}");
                        Monitor.Wait(this._locker); // Lock is released while we’re waiting
                    }
                    if (didLock)
                    {
                        Logger.Verbose($"MeteredQueue.Released {this._name}, Count={this.Count:N0}");
                    }
                }
            }
        }

        /// <inheritdoc />
        public bool Any()
        {
            return this._blockingCollection.Any();
        }

        /// <inheritdoc />
        public bool TryTake(out T cacheItem)
        {
            var result = this._blockingCollection.TryTake(out cacheItem);
            this.ReleaseLockIfNeeded();

            return result;
        }

        public int Count => this._blockingCollection.Count;
        public bool IsCompleted => this._blockingCollection.IsCompleted;

        /// <inheritdoc />
        public void CompleteAdding()
        {
            this._blockingCollection.CompleteAdding();
        }

        // ReSharper disable once ConvertToAutoProperty
        public string Name => this._name;
    }
}
