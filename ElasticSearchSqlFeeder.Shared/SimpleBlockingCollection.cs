using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using ElasticSearchSqlFeeder.Interfaces;
using NLog;

namespace ElasticSearchSqlFeeder.Shared
{
    public class SimpleBlockingCollection<T> : IMeteredBlockingCollection<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly BlockingCollection<T> _blockingCollection;

        private readonly string _name;

        public SimpleBlockingCollection(IProducerConsumerCollection<T> concurrentQueue, string name)
        {
            _blockingCollection = new BlockingCollection<T>(concurrentQueue);
            _name = name;
        }

        public SimpleBlockingCollection(IProducerConsumerCollection<T> concurrentQueue, string name, int maxItems)
            : this(concurrentQueue, name)
        {
        }

        public T Take()
        {
            var item = _blockingCollection.Take();

            return item;
        }

        public void Add(T item)
        {
            _blockingCollection.Add(item);
        }


        public bool Any()
        {
            return _blockingCollection.Any();
        }

        public bool TryTake(out T cacheItem)
        {
            var result = _blockingCollection.TryTake(out cacheItem);

            return result;
        }

        public int Count => _blockingCollection.Count;
        public bool IsCompleted => _blockingCollection.IsCompleted;

        public void CompleteAdding()
        {
            _blockingCollection.CompleteAdding();
        }

        // ReSharper disable once ConvertToAutoProperty
        public string Name => _name;
    }
}
