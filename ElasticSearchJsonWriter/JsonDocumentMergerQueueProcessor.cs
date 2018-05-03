using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.ProgressMonitor;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElasticSearchJsonWriter
{

    public class JsonDocumentMergerQueueProcessor : BaseQueueProcessor<JsonDocumentMergerQueueItem, JsonObjectQueueItem>
    {
        private readonly IMeteredConcurrentDictionary<string, JsonObjectQueueItem> _documentDictionary;

        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        private static readonly SequenceBarrier SequenceBarrier = new SequenceBarrier();

        private int _numDocumentsModified;

        private static int _currentBatchNumber = 0;

        public JsonDocumentMergerQueueProcessor(IMeteredConcurrentDictionary<string, JsonObjectQueueItem> documentDictionary, QueueContext queueContext)
            : base(queueContext)
        {
            _documentDictionary = documentDictionary;
        }


        private void AddToJsonObject(string queryId, string id, string propertyName, JObject[] newJObjects, int batchNumber)
        {
            //BlockIfMaxSizeReached(id);

            // lock on id so multiple threads cannot update the same document at the same time
            lock (_locks.GetOrAdd(id, s => new object()))
            {
                JObject document;
                if (!_documentDictionary.ContainsKey(id))
                {
                    document = new JObject { { Config.TopLevelKeyColumn, id } };
                    var jsonObjectCacheItem = new JsonObjectQueueItem
                    {
                        BatchNumber = batchNumber,
                        Id = id,
                        Document = document
                    };


                    _documentDictionary.Add(id, jsonObjectCacheItem);

                    Interlocked.Increment(ref _numDocumentsModified);

                    MyLogger.Trace($"AddToJsonObject: id:{id} _numDocumentsModified={_numDocumentsModified:N0} _documentDictionary.Count={_documentDictionary.Count:N0}");
                    JsonHelper.SetPropertiesByMerge(propertyName, newJObjects, document);
                }
                else
                {
                    document = _documentDictionary[id].Document;

                    MyLogger.Trace($"UpdatedJsonObject: id:{id}  _numDocumentsModified={_numDocumentsModified:N0} _documentDictionary.Count={_documentDictionary.Count:N0}");
                    JsonHelper.SetPropertiesByMerge(propertyName, newJObjects, document);
                }

            }

            var minimum = SequenceBarrier.UpdateMinimumEntityIdProcessed(queryId, id);

            //QueueContext.
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

            //Console.Write($"\r{LoggerName} Id:{id} Remaining: {_inQueue.Count:N0} queryId:{queryId} Minimum id:{minimum}");

            MyLogger.Trace($"Processed id: {id} for queryId:{queryId} Minimum id:{minimum}");

            // AddDocumentsToOutQueue(minimum);

            //AddDocumentToOutputQueueByKey(id);

        }


        private void AddDocumentsToOutQueue(string min)
        {
            var keys = _documentDictionary.GetKeysLessThan(min);

            AddDocumentsToOutQueueByKeys(keys);
        }

        private void AddDocumentsToOutQueueByKeys(IList<string> keys)
        {
            foreach (var key in keys)
            {
                AddDocumentToOutputQueueByKey(key);
            }
        }

        private void AddDocumentToOutputQueueByKey(string key)
        {
            JsonObjectQueueItem item;
            if (_documentDictionary.TryRemove(key, out item))
            {
                MyLogger.Trace(
                    $"Remove from Dictionary: _documentDictionary.Count={_documentDictionary.Count:N0}");


                AddToOutputQueue(item);
            }
        }


        protected override void Handle(JsonDocumentMergerQueueItem wt)
        {
            // if work item batch number is newer than the last one we saw that we can flush anything related to the old batch
            if(_currentBatchNumber != wt.BatchNumber)
            {
                //find all items with old batch numbers and put them in the out queue
                var list = _documentDictionary.RemoveItems(item => item.Value.BatchNumber != wt.BatchNumber);
                SendToOutputQueue(list);

                _currentBatchNumber = wt.BatchNumber;
            }

            AddToJsonObject(wt.QueryId, wt.Id, wt.PropertyName, wt.NewJObjects, wt.BatchNumber);
        }

        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            var list = _documentDictionary.RemoveAll();
            SendToOutputQueue(list);

            SequenceBarrier.CompleteQuery(queryId);

            MyLogger.Trace("Finished");
        }

        private void SendToOutputQueue(IList<Tuple<string, JsonObjectQueueItem>> list)
        {
            foreach (var tuple in list)
            {
                //remove temporary columns
                RemoveTemporaryColumns(tuple.Item2.Document);

                AddToOutputQueue(tuple.Item2);

                if (Config.WriteDetailedTemporaryFilesToDisk)
                {
                    var path = Path.Combine(Config.LocalSaveFolder, "jsondocs");
                    Directory.CreateDirectory(path);

                    File.WriteAllText(Path.Combine(path, tuple.Item1), tuple.Item2.Document.ToString());
                }
            }
        }

        static void WalkNode(JToken node, Action<JObject> action)
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
        private void RemoveTemporaryColumns(JObject node)
        {
            if (!Config.KeepTemporaryLookupColumnsInOutput)
            {
                WalkNode(node, n =>
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

        protected override string GetId(JsonDocumentMergerQueueItem workitem)
        {
            return null;
        }

        //protected override void LogItemToConsole(JsonDocumentMergerQueueItem wt)
        //{
        //    // don't log here
        //}

        protected override string LoggerName => "JsonDocumentMerger";

    }

    public class JsonObjectQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }
        public string Id { get; set; }
        public JObject Document { get; set; }
        public int BatchNumber { get; set; }
    }
    public class JsonDocumentMergerQueueItem : IQueueItem
    {
        public string QueryId { get; set; }
        public string PropertyName { get; set; }

        public string Id { get; set; }
        public JObject[] NewJObjects { get; set; }

        public int BatchNumber { get; set; }

        //public string JoinColumnValue { get; set; }
    }

    public class CurrentProcessingEntity
    {
        public int Id { get; set; }
        public CurrentProcessingEntityLockType ProcessingType { get; set; }
    }

    public enum CurrentProcessingEntityLockType
    {
        None,
        Add,
        Update
    }
}
