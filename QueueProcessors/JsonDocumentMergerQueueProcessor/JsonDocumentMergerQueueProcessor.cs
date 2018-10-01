// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonDocumentMergerQueueProcessor.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   Defines the JsonDocumentMergerQueueProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JsonDocumentMergerQueueProcessor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Shared;

    using Newtonsoft.Json.Linq;

    using QueueItems;

    using Serilog;

    /// <summary>
    /// The json document merger queue processor.
    /// </summary>
    public class JsonDocumentMergerQueueProcessor : BaseQueueProcessor<JsonDocumentMergerQueueItem, IJsonObjectQueueItem>
    {
        /// <summary>
        /// The sequence barrier.
        /// </summary>
        private static readonly SequenceBarrier SequenceBarrier = new SequenceBarrier();

        /// <summary>
        /// The current batch number.
        /// </summary>
        private static int currentBatchNumber = 0;

        /// <summary>
        /// The _locks.
        /// </summary>
        private readonly ConcurrentDictionary<string, object> locks = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <summary>
        /// The num documents modified.
        /// </summary>
        private int numDocumentsModified;

        /// <inheritdoc />
        public JsonDocumentMergerQueueProcessor(IQueueContext queueContext, ILogger logger, IQueueManager queueManager, IProgressMonitor progressMonitor)
            : base(queueContext, logger, queueManager, progressMonitor)
        {
            var configLocalSaveFolder = this.Config.LocalSaveFolder;
            if (configLocalSaveFolder == null)
            {
                throw new ArgumentNullException(nameof(this.Config.LocalSaveFolder));
            }

            this.folder = Path.Combine(configLocalSaveFolder, $"{UniqueId}-JsonDocumentMerge");
        }

        /// <summary>
        /// The logger name.
        /// </summary>
        protected override string LoggerName => "JsonDocumentMerger";

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="workItem">
        /// The workItem.
        /// </param>
        protected override void Handle(JsonDocumentMergerQueueItem workItem)
        {
            // if work item batch number is newer than the last one we saw that we can flush anything related to the old batch
            if (currentBatchNumber != workItem.BatchNumber)
            {
                // find all items with old batch numbers and put them in the out queue
                var list = this.QueueContext.DocumentDictionary.RemoveItems(item => item.Value.BatchNumber != workItem.BatchNumber);
                this.SendToOutputQueue(list);

                currentBatchNumber = workItem.BatchNumber;
            }

            this.AddToJsonObject(workItem.QueryId, workItem.Id, workItem.PropertyName, workItem.NewJObjects, workItem.BatchNumber);
        }

        /// <summary>
        /// The begin.
        /// </summary>
        /// <param name="isFirstThreadForThisTask">
        /// The is first thread for this task.
        /// </param>
        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        /// <summary>
        /// The complete.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        /// The is last thread for this task.
        /// </param>
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            var list = this.QueueContext.DocumentDictionary.RemoveAll();
            this.SendToOutputQueue(list);

            SequenceBarrier.CompleteQuery(queryId);

            this.MyLogger.Verbose("Finished");
        }

        /// <summary>
        /// The get id.
        /// </summary>
        /// <param name="workItem">
        /// The workItem.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected override string GetId(JsonDocumentMergerQueueItem workItem)
        {
            return null;
        }

        // protected override void LogItemToConsole(JsonDocumentMergerQueueItem workItem)
        // {
        //    // don't log here
        // }

        /// <summary>
        /// The walk node.
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        private static void WalkNode(JToken node, Action<JObject> action)
        {
            if (node.Type == JTokenType.Object)
            {
                action((JObject)node);

                foreach (JProperty child in node.Children<JProperty>())
                {
                    WalkNode(child.Value, action);
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                foreach (JToken child in node.Children())
                {
                    WalkNode(child, action);
                }
            }
        }

        /// <summary>
        /// The add to json object.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="newJObjects">
        /// The new j objects.
        /// </param>
        /// <param name="batchNumber">
        /// The batch number.
        /// </param>
        private void AddToJsonObject(string queryId, string id, string propertyName, JObject[] newJObjects, int batchNumber)
        {
            // BlockIfMaxSizeReached(id);

            // lock on id so multiple threads cannot update the same document at the same time
            lock (this.locks.GetOrAdd(id, s => new object()))
            {
                JObject document;
                if (!this.QueueContext.DocumentDictionary.ContainsKey(id))
                {
                    document = new JObject { { this.Config.TopLevelKeyColumn, id } };
                    var jsonObjectCacheItem = new JsonObjectQueueItem
                    {
                        BatchNumber = batchNumber,
                        Id = id,
                        Document = document
                    };


                    this.QueueContext.DocumentDictionary.Add(id, jsonObjectCacheItem);

                    Interlocked.Increment(ref this.numDocumentsModified);

                    this.MyLogger.Verbose($"AddToJsonObject: id:{id} _numDocumentsModified={this.numDocumentsModified:N0} _documentDictionary.Count={this.QueueContext.DocumentDictionary.Count:N0}");
                    JsonHelper.SetPropertiesByMerge(propertyName, newJObjects, document);
                }
                else
                {
                    document = this.QueueContext.DocumentDictionary[id].Document;

                    this.MyLogger.Verbose($"UpdatedJsonObject: id:{id}  _numDocumentsModified={this.numDocumentsModified:N0} _documentDictionary.Count={this.QueueContext.DocumentDictionary.Count:N0}");
                    JsonHelper.SetPropertiesByMerge(propertyName, newJObjects, document);
                }

            }

            var minimum = SequenceBarrier.UpdateMinimumEntityIdProcessed(queryId, id);

            // QueueContext.
            //    ProgressMonitor.SetProgressItem(new ProgressMonitorItem
            //    {
            //        LoggerName = LoggerName,
            //        Id = id,
            //        StepNumber = _stepNumber,
            //        //QueryId = queryId,
            //        InQueueCount = _inQueue.Count,
            //        Minimum = minimum,
            //        TimeElapsed = TimeSpan.Zero,
            //        LastCompletedEntityIdForEachQuery = SequenceBarrier.LastCompletedEntityIdForEachQuery.ToList(),
            //        DocumentDictionaryCount = _documentDictionary.Count,
            //        TotalItemsProcessed = _totalItemsProcessed,
            //        TotalItemsAddedToOutputQueue = _totalItemsAddedToOutputQueue,
            //    });

            // Console.Write($"\r{LoggerName} Id:{id} Remaining: {_inQueue.Count:N0} queryId:{queryId} Minimum id:{minimum}");
            this.MyLogger.Verbose($"Processed id: {id} for queryId:{queryId} Minimum id:{minimum}");

            // AddDocumentsToOutQueue(minimum);

            // AddDocumentToOutputQueueByKey(id);
        }

        /// <summary>
        /// The add documents to out queue.
        /// </summary>
        /// <param name="min">
        /// The min.
        /// </param>
        private void AddDocumentsToOutQueue(string min)
        {
            var keys = this.QueueContext.DocumentDictionary.GetKeysLessThan(min);

            this.AddDocumentsToOutQueueByKeys(keys);
        }

        /// <summary>
        /// The add documents to out queue by keys.
        /// </summary>
        /// <param name="keys">
        /// The keys.
        /// </param>
        private void AddDocumentsToOutQueueByKeys(IList<string> keys)
        {
            foreach (var key in keys)
            {
                this.AddDocumentToOutputQueueByKey(key);
            }
        }

        /// <summary>
        /// The add document to output queue by key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        private void AddDocumentToOutputQueueByKey(string key)
        {
            IJsonObjectQueueItem item;
            if (this.QueueContext.DocumentDictionary.TryRemove(key, out item))
            {
                this.MyLogger.Verbose(
                    $"Remove from Dictionary: _documentDictionary.Count={this.QueueContext.DocumentDictionary.Count:N0}");

                this.AddToOutputQueue(item);
            }
        }

        /// <summary>
        /// The send to output queue.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        private void SendToOutputQueue(IList<Tuple<string, IJsonObjectQueueItem>> list)
        {
            var path = this.folder;
            if (this.Config.WriteDetailedTemporaryFilesToDisk)
            {
                Directory.CreateDirectory(path);
            }

            foreach (var tuple in list)
            {
                // remove temporary columns
                this.RemoveTemporaryColumns(tuple.Item2.Document);

                this.AddToOutputQueue(tuple.Item2);

                if (this.Config.WriteDetailedTemporaryFilesToDisk)
                {
                    File.AppendAllText(Path.Combine(path, $"{tuple.Item1}.json"), tuple.Item2.Document.ToString());
                }
            }
        }

        /// <summary>
        /// The remove temporary columns.
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        private void RemoveTemporaryColumns(JObject node)
        {
            if (!this.Config.KeepTemporaryLookupColumnsInOutput)
            {
                WalkNode(
                    node,
                    n =>
                        {
                            var properties = n.Properties().Select(p => p.Name).ToList();
                    var propertiesToRemove = properties
                        .Where(p => p.StartsWith("KeyLevel", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var property in propertiesToRemove)
                    {
                        n.Remove(property);
                    }

                    if (n.Parent != null)
                    {
                        propertiesToRemove =
                            properties.Where(
                                    p => p.StartsWith(Config.TopLevelKeyColumn, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                        foreach (var property in propertiesToRemove)
                        {
                            n.Remove(property);
                        }
                    }
                });
            }
        }
    }
}
