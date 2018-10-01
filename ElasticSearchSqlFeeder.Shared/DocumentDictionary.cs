// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentDictionary.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the DocumentDictionary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Shared
{
    using System;
    using System.Collections.Generic;

    using ElasticSearchSqlFeeder.Interfaces;

    using QueueItems;

    /// <inheritdoc />
    /// <summary>
    /// The document dictionary.
    /// </summary>
    public class DocumentDictionary : IDocumentDictionary
    {
        /// <summary>
        /// The metered concurrent dictionary.
        /// </summary>
        private readonly MeteredConcurrentDictionary<string, IJsonObjectQueueItem> meteredConcurrentDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDictionary"/> class.
        /// </summary>
        /// <param name="maximumDocumentsInQueue">
        /// The maximum documents in queue.
        /// </param>
        public DocumentDictionary(int maximumDocumentsInQueue)
        {
            this.meteredConcurrentDictionary = new MeteredConcurrentDictionary<string, IJsonObjectQueueItem>(maximumDocumentsInQueue);
        }

        /// <inheritdoc />
        public int Count => this.meteredConcurrentDictionary.Count;

        /// <inheritdoc />
        public IList<Tuple<string, IJsonObjectQueueItem>> RemoveAllItemsExceptFromBatchNumber(int batchNumber)
        {
            return this.meteredConcurrentDictionary.RemoveItems(item => item.Value.BatchNumber != batchNumber);
        }

        /// <inheritdoc />
        public IList<Tuple<string, IJsonObjectQueueItem>> RemoveAll()
        {
            return this.meteredConcurrentDictionary.RemoveAll();
        }

        /// <inheritdoc />
        public bool ContainsKey(string id)
        {
            return this.meteredConcurrentDictionary.ContainsKey(id);
        }

        /// <inheritdoc />
        public void Add(string id, IJsonObjectQueueItem jsonObjectCacheItem)
        {
            this.meteredConcurrentDictionary.Add(id, jsonObjectCacheItem);
        }

        /// <inheritdoc />
        public IList<string> GetKeysLessThan(string min)
        {
            return this.meteredConcurrentDictionary.GetKeysLessThan(min);
        }

        /// <inheritdoc />
        public bool TryRemove(string key, out IJsonObjectQueueItem item)
        {
            return this.meteredConcurrentDictionary.TryRemove(key, out item);
        }

        /// <inheritdoc />
        public IJsonObjectQueueItem GetById(string id)
        {
            return this.meteredConcurrentDictionary[id];
        }
    }
}