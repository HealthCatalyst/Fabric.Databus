// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMeteredConcurrentDictionary.cs" company="">
//   
// </copyright>
// <summary>
//   The MeteredConcurrentDictionary interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The MeteredConcurrentDictionary interface.
    /// </summary>
    /// <typeparam name="TKey">
    /// </typeparam>
    /// <typeparam name="TValue">
    /// </typeparam>
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
        IList<Tuple<TKey, TValue>> RemoveItems(Func<KeyValuePair<TKey, TValue>, bool> fnKey);
    }
}