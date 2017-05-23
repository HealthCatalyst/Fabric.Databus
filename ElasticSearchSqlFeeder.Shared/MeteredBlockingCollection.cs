using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using ElasticSearchSqlFeeder.Interfaces;
using NLog;

namespace ElasticSearchSqlFeeder.Shared
{
    public class MeteredBlockingCollection<T> : IMeteredBlockingCollection<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly BlockingCollection<T> _blockingCollection;

        private readonly int _maxItems;
        private readonly object _locker = new object();
        private readonly string _name;

        public MeteredBlockingCollection(IProducerConsumerCollection<T> concurrentQueue, string name)
        {
            _blockingCollection = new BlockingCollection<T>(concurrentQueue);
            _name = name;
        }

        public MeteredBlockingCollection(IProducerConsumerCollection<T> concurrentQueue, string name, int maxItems)
            : this(concurrentQueue, name)
        {
            _maxItems = maxItems;
        }

        public T Take()
        {
            var item = _blockingCollection.Take();
            ReleaseLockIfNeeded();

            return item;
        }

        private void ReleaseLockIfNeeded()
        {
            if (_maxItems > 0)
            {
                lock (_locker) // Let's now wake up the thread by
                {
                    if (Count <= _maxItems)
                    {
                        Logger.Trace($"MeteredQueue.ReleaseAll {_name}");

                        Monitor.PulseAll(_locker);
                    }
                }
            }
        }

        public void Add(T item)
        {
            BlockIfNeeded();

            _blockingCollection.Add(item);
        }

        private void BlockIfNeeded()
        {
            if (_maxItems > 0)
            {
                // if we have enough items in the queue then block
                // http://www.albahari.com/threading/part4.aspx#_Signaling_with_Wait_and_Pulse
                lock (_locker)
                {
                    bool didLock = false;
                    while (Count > _maxItems)
                    {
                        didLock = true;
                        Logger.Trace($"MeteredQueue.Block {_name}, Count={Count:N0}");
                        Monitor.Wait(_locker); // Lock is released while we’re waiting
                    }
                    if (didLock)
                    {
                        Logger.Trace($"MeteredQueue.Released {_name}, Count={Count:N0}");
                    }
                }
            }
        }

        public bool Any()
        {
            return _blockingCollection.Any();
        }

        public bool TryTake(out T cacheItem)
        {
            var result = _blockingCollection.TryTake(out cacheItem);
            ReleaseLockIfNeeded();

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
