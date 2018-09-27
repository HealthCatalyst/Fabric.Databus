// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueManager.cs" company="">
//   
// </copyright>
// <summary>
//   The queue manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Shared
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using ElasticSearchSqlFeeder.Interfaces;

    /// <summary>
    /// The queue manager.
    /// </summary>
    public class QueueManager : IQueueManager
    {
        private readonly ConcurrentDictionary<string, IMeteredBlockingCollection> _queues = 
            new ConcurrentDictionary<string, IMeteredBlockingCollection>();

        private int _nextId = 0;

        public IMeteredBlockingCollection<T> CreateInputQueue<T>(int stepNumber)
        {
            return CreateQueue<T>(stepNumber);
        }
        public IMeteredBlockingCollection<T> GetInputQueue<T>(int stepNumber)
        {
            return GetQueue<T>(stepNumber);
        }
        public IMeteredBlockingCollection<T> GetOutputQueue<T>(int stepNumber)
        {
            return GetQueue<T>(stepNumber+1);
        }
        public IMeteredBlockingCollection<T> CreateOutputQueue<T>(int stepNumber)
        {
            return CreateQueue<T>(stepNumber + 1);
        }

        private IMeteredBlockingCollection<T> CreateQueue<T>(int id)
        {
            var queueName = GetQueueName<T>(id);
            if (_queues.ContainsKey(queueName)) throw new ArgumentException($"Queue already exists with name: {queueName}");

            var blockingCollection = _queues.GetOrAdd(queueName, (a) => InternalCreateQueue<T>(id));

            return blockingCollection as IMeteredBlockingCollection<T>;
        }

        private IMeteredBlockingCollection<T> GetQueue<T>(int id)
        {
            var queueName = GetQueueName<T>(id);
            if (!_queues.ContainsKey(queueName)) throw new ArgumentException($"No queue found with name: {queueName}");

            var blockingCollection = _queues[queueName];

            return blockingCollection as IMeteredBlockingCollection<T>;
        }

        private string GetQueueName<T>(int id)
        {
            return typeof(T).Name + id;
        }

        private IMeteredBlockingCollection InternalCreateQueue<T>(int id)
        {
            return new SimpleBlockingCollection<T>(
                new ConcurrentQueue<T>(),
                GetQueueName<T>(id),
                1000);
        }

        //public void WaitTillAllQueuesAreCompleted<T>()
        //{
        //    while (GetNonEmptyQueues<T>().Any())
        //    {
        //        var nonEmptyQueues = GetNonEmptyQueues<T>();
        //        Thread.Sleep(1000);
        //    }
        //}

        //private IEnumerable<KeyValuePair<string, IMeteredBlockingCollection>> GetNonEmptyQueues<T>()
        //{
        //    var key = GetQueueName<T>();
        //    return _queues.Where(q => q.Key != key && q.Value.Any());
        //}

        public int GetUniqueId()
        {
            return Interlocked.Increment(ref _nextId);
        }
    }
}
