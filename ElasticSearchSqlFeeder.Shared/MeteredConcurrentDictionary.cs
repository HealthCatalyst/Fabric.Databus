using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElasticSearchSqlFeeder.Interfaces;

namespace ElasticSearchSqlFeeder.Shared
{
    using Serilog;
    using Serilog.Core;

    public class MeteredConcurrentDictionary<TKey, TValue> : IMeteredConcurrentDictionary<TKey, TValue> where TKey : IComparable
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger Logger = new LoggerConfiguration().CreateLogger();

        //private readonly ConcurrentDictionary<TKey, TValue> _concurrentDictionary = 
        //    new ConcurrentDictionary<TKey, TValue>();

        private static readonly SortedList<TKey, TValue> KeyHash = new SortedList<TKey, TValue>();

        private int _maxItems;
        private readonly object _locker = new object();

        public MeteredConcurrentDictionary(int maxItems)
        {
            _maxItems = maxItems;
        }
        public bool ContainsKey(TKey id)
        {
            lock (((ICollection)KeyHash).SyncRoot)
            {
                return KeyHash.ContainsKey(id);
            }

        }

        public void Add(TKey id, TValue jsonObjectCacheItem)
        {
            //BlockIfNeeded(id);
            lock (((ICollection)KeyHash).SyncRoot)
            {
                if (KeyHash.ContainsKey(id))
                {
                    KeyHash[id] = jsonObjectCacheItem;
                }
                else
                {
                    KeyHash.Add(id, jsonObjectCacheItem);

                }
            }
            //_concurrentDictionary.GetOrAdd(id, jsonObjectCacheItem);
        }

        private void BlockIfNeeded(TKey id)
        {
            if (_maxItems > 0)
            {
                // if we have enough items in the queue then block
                // http://www.albahari.com/threading/part4.aspx#_Signaling_with_Wait_and_Pulse
                lock (_locker)
                {
                    while (Count > _maxItems)
                    {
                        Logger.Verbose($"MeteredDictionary.Block id={id} Count={Count:N0}");
                        Monitor.Wait(_locker); // Lock is released while we’re waiting

                    }
                    Logger.Verbose($"MeteredDictionary.Released id={id} Count={Count:N0}");
                }
            }
        }

        public TValue this[TKey id]
        {
            get
            {
                lock (((ICollection)KeyHash).SyncRoot)
                {
                    return KeyHash[id];
                }
            }
        }


        public bool TryRemove(TKey id, out TValue item)
        {
            lock (((ICollection)KeyHash).SyncRoot)
            {
                if (KeyHash.ContainsKey(id) == false)
                {
                    item = default(TValue);
                    return false;
                }

                item = KeyHash[id];

                var result = KeyHash.Remove(id);

                if (result)
                {
                    if (_maxItems > 0)
                    {
                        lock (_locker) // Let's now wake up the thread by
                        {
                            if (Count <= _maxItems)
                            {
                                //Console.WriteLine($"MeteredDictionary.ReleaseAll id={id}");

                                //Monitor.PulseAll(_locker);
                            }
                        }
                    }
                }
                return result;
            }
        }

        public int Count
        {
            get
            {
                lock (((ICollection)KeyHash).SyncRoot)
                {
                    return KeyHash.Count;
                }
            }
        }

        public IList<TKey> GetKeys()
        {
            lock (((ICollection)KeyHash).SyncRoot)
            {
                return KeyHash.Keys.ToList();
            }
        }

        public IList<TKey> GetKeysLessThan(TKey min)
        {
            lock (((ICollection)KeyHash).SyncRoot)
            {
                var keysLessThan = KeyHash.Where(k => k.Key.CompareTo(min) < 0).Select(k => k.Key).ToList();

                if (keysLessThan.Any())
                {
                    Logger.Verbose($"Removing keys from dictionary: {keysLessThan.FirstOrDefault()} to {keysLessThan.LastOrDefault()}");
                }
                return keysLessThan;
            }
        }

        public IList<Tuple<TKey, TValue>> RemoveAll()
        {
            lock (((ICollection) KeyHash).SyncRoot)
            {
                var tuples = KeyHash.Select(k => new Tuple<TKey, TValue>(k.Key, k.Value)).ToList();
                KeyHash.Clear();
                return tuples;
            }

        }

        public IList<Tuple<TKey, TValue>> RemoveItems(Func<KeyValuePair<TKey, TValue>, bool> fnKey)
        {
            lock (((ICollection)KeyHash).SyncRoot)
            {
                var itemsToRemove = KeyHash.Where(fnKey).Select(k => new Tuple<TKey, TValue>(k.Key, k.Value)).ToList();

                if (itemsToRemove.Any())
                {
                    var firstOrDefault = itemsToRemove.FirstOrDefault();
                    var lastOrDefault = itemsToRemove.LastOrDefault();
                    if (firstOrDefault != null && lastOrDefault != null)
                    {
                        Logger.Verbose($"Removing keys from dictionary: {firstOrDefault.Item1} to {lastOrDefault.Item1}");
                    }

                    var itemsToKeep = KeyHash.Where(a => !fnKey(a)).ToList();
                    KeyHash.Clear();
                    itemsToKeep.ForEach(k => KeyHash.Add(k.Key, k.Value));
                }

                return itemsToRemove;
            }
        }

    }
}
