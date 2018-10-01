// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDocumentDictionary.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IDocumentDictionary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System;
    using System.Collections.Generic;

    using QueueItems;

    /// <summary>
    /// The DocumentDictionary interface.
    /// </summary>
    public interface IDocumentDictionary
    {
        /// <summary>
        /// Gets the count.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The remove all items except from batch number.
        /// </summary>
        /// <param name="batchNumber">
        /// The work item batch number.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        IList<Tuple<string, IJsonObjectQueueItem>> RemoveAllItemsExceptFromBatchNumber(int batchNumber);

        /// <summary>
        /// The remove all.
        /// </summary>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        IList<Tuple<string, IJsonObjectQueueItem>> RemoveAll();

        /// <summary>
        /// The contains key.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool ContainsKey(string id);

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="jsonObjectCacheItem">
        /// The json object cache item.
        /// </param>
        void Add(string id, IJsonObjectQueueItem jsonObjectCacheItem);

        /// <summary>
        /// The get keys less than.
        /// </summary>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        IList<string> GetKeysLessThan(string min);

        /// <summary>
        /// The try remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool TryRemove(string key, out IJsonObjectQueueItem item);

        /// <summary>
        /// The get by id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="IJsonObjectQueueItem"/>.
        /// </returns>
        IJsonObjectQueueItem GetById(string id);
    }
}