// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeteredQueue.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the MeteredQueue type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Queues;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The metered blocking collection.
    /// </summary>
    /// <typeparam name="T">item type
    /// </typeparam>
    public class MeteredQueue<T> : IQueue<T>
        where T : class, IQueueItem
    {
        /// <summary>
        /// The logger.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Logger = new LoggerConfiguration().CreateLogger();

        /// <summary>
        /// The blocking collection.
        /// </summary>
        private readonly BlockingCollection<T> blockingCollection;

        /// <summary>
        /// The _max items.
        /// </summary>
        private readonly int _maxItems;

        private readonly object _locker = new object();

        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeteredQueue{T}"/> class.
        /// </summary>
        /// <param name="concurrentQueue">
        /// The concurrent queue.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public MeteredQueue(IProducerConsumerCollection<T> concurrentQueue, string name)
        {
            this.blockingCollection = new BlockingCollection<T>(concurrentQueue);
            this._name = name;
        }

        /// <inheritdoc />
        public MeteredQueue(IProducerConsumerCollection<T> concurrentQueue, string name, int maxItems)
            : this(concurrentQueue, name)
        {
            this._maxItems = maxItems;
        }

        /// <inheritdoc />
        public int Count => this.blockingCollection.Count;

        /// <inheritdoc />
        public bool IsCompleted => this.blockingCollection.IsCompleted;

        /// <summary>
        /// The name.
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public string Name => this._name;

        /// <param name="cancellationToken"></param>
        /// <inheritdoc />
        public T Take(CancellationToken cancellationToken)
        {
            var item = this.blockingCollection.Take();
            this.ReleaseLockIfNeeded();

            return item;
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
            this.BlockIfNeeded();

            this.blockingCollection.Add(item);
        }

        /// <inheritdoc />
        public void AddBatchCompleted(IQueueItem item)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void AddJobCompleted(IQueueItem item)
        {
            throw new NotImplementedException();
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
            this.ReleaseLockIfNeeded();

            return result;
        }

        /// <inheritdoc />
        public void CompleteAdding()
        {
            this.blockingCollection.CompleteAdding();
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

        /// <summary>
        /// The release lock if needed.
        /// </summary>
        private void ReleaseLockIfNeeded()
        {
            if (this._maxItems > 0)
            {
                lock (this._locker)
                {
                    // Let's now wake up the thread by
                    if (this.Count <= this._maxItems)
                    {
                        Logger.Verbose($"MeteredQueue.ReleaseAll {this._name}");

                        Monitor.PulseAll(this._locker);
                    }
                }
            }
        }
    }
}
