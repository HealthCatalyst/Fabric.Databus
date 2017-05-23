using System;
using System.Collections.Generic;

namespace ElasticSearchSqlFeeder.Interfaces
{
    public interface IMeteredConcurrentDictionary<TKey, TValue>
    {
        bool ContainsKey(TKey id);
        void Add(TKey id, TValue jsonObjectCacheItem);
        TValue this[TKey id] { get; }
        bool TryRemove(TKey id, out TValue item);
        int Count { get; }

        IList<TKey> GetKeys();
        IList<TKey> GetKeysLessThan(TKey min);
        IList<Tuple<TKey, TValue>> RemoveAll();
    }
}