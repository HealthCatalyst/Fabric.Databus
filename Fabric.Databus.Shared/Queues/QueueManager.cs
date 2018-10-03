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
    using System.Threading;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Queues;

    /// <summary>
    /// The queue manager.
    /// </summary>
    public class QueueManager : IQueueManager
    {
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
        /// The queues.
        /// </summary>
        public IDictionary<string, IQueue> Queues => this.queues;

        /// <summary>
        /// The create input queue.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        /// <typeparam name="T">type of queue
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        public IQueue<T> CreateInputQueue<T>(int stepNumber)
        {
            return this.CreateQueue<T>(stepNumber);
        }

        /// <summary>
        /// The get input queue.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        /// <typeparam name="T">type of queue
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        public IQueue<T> GetInputQueue<T>(int stepNumber)
        {
            return this.GetQueue<T>(stepNumber);
        }

        /// <summary>
        /// The get output queue.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        /// <typeparam name="T">type of queue
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        public IQueue<T> GetOutputQueue<T>(int stepNumber)
        {
            return this.GetQueue<T>(stepNumber + 1);
        }

        /// <summary>
        /// The create output queue.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        /// <typeparam name="T">type of queue
        /// </typeparam>
        /// <returns>
        /// The <see cref="IQueue"/>.
        /// </returns>
        public IQueue<T> CreateOutputQueue<T>(int stepNumber)
        {
            return this.CreateQueue<T>(stepNumber + 1);
        }

        /// <summary>
        /// The get unique id.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
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
                throw new ArgumentException($"No queue found with name: {queueName}");
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
        {
            return new SimpleQueue<T>(
                new ConcurrentQueue<T>(),
                this.GetQueueName<T>(id),
                1000);
        }
    }
}
