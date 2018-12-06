// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueManager.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   The queue manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Shared;

    /// <inheritdoc />
    /// <summary>
    /// The queue manager.
    /// </summary>
    public class QueueManager : IQueueManager
    {
        /// <summary>
        /// The queue factory.
        /// </summary>
        private readonly IQueueFactory queueFactory;

        /// <summary>
        /// The queues.
        /// </summary>
        private readonly ConcurrentDictionary<string, IQueue> queues = 
            new ConcurrentDictionary<string, IQueue>();

        /// <summary>
        /// The next id.
        /// </summary>
        private int nextId;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueManager"/> class.
        /// </summary>
        /// <param name="queueFactory">
        /// The queue factory.
        /// </param>
        public QueueManager(IQueueFactory queueFactory)
        {
            this.queueFactory = queueFactory ?? throw new ArgumentNullException(nameof(queueFactory));
        }

        /// <summary>
        /// The queues.
        /// </summary>
        public IDictionary<string, IQueue> Queues => this.queues;

        /// <inheritdoc />
        public IQueue<T> CreateInputQueue<T>(int stepNumber)
            where T : class, IQueueItem
        {
            return this.CreateQueue<T>(stepNumber);
        }

        /// <inheritdoc />
        public IQueue<T> GetInputQueue<T>(int stepNumber)
        {
            return this.GetQueue<T>(stepNumber);
        }

        /// <inheritdoc />
        public IQueue<T> GetOutputQueue<T>(int stepNumber)
        {
            return this.GetQueue<T>(stepNumber + 1);
        }

        /// <inheritdoc />
        public IQueue<T> CreateOutputQueue<T>(int stepNumber)
            where T : class, IQueueItem
        {
            return this.CreateQueue<T>(stepNumber + 1);
        }

        /// <inheritdoc />
        public int GetUniqueId()
        {
            return Interlocked.Increment(ref this.nextId);
        }

        /// <summary>
        /// The create queue.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <typeparam name="T">type of queue
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        /// <exception cref="ArgumentException">exception thrown
        /// </exception>
        private IQueue<T> CreateQueue<T>(int id)
            where T : class, IQueueItem
        {
            var queueName = this.GetQueueName<T>(id);
            if (this.queues.ContainsKey(queueName))
            {
                throw new ArgumentException($"Queue already exists with name: {queueName}");
            }

            var blockingCollection = this.queues.GetOrAdd(queueName, (a) => this.InternalCreateQueue<T>(id));

            return blockingCollection as IQueue<T>;
        }

        /// <summary>
        /// The get queue.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <typeparam name="T">type of queue
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        /// <exception cref="ArgumentException">exception thrown
        /// </exception>
        private IQueue<T> GetQueue<T>(int id)
        {
            var queueName = this.GetQueueName<T>(id);
            if (!this.queues.ContainsKey(queueName))
            {
                throw new ArgumentException($"No queue found with name: {queueName}.  Current queues: {this.queues.OrderBy(q=>q.Key).Select(q=>q.Key).ToCsv()}");
            }

            var blockingCollection = this.queues[queueName];

            return blockingCollection as IQueue<T>;
        }

        /// <summary>
        /// The get queue name.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <typeparam name="T">type of queue
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetQueueName<T>(int id)
        {
            return typeof(T).Name + id;
        }

        /// <summary>
        /// The internal create queue.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <typeparam name="T">type of queue
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        private IQueue InternalCreateQueue<T>(int id)
            where T : class, IQueueItem
        {
            return this.queueFactory.Create<T>(this.GetQueueName<T>(id));
        }
    }
}
