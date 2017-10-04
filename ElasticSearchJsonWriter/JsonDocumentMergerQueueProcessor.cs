using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.ProgressMonitor;
using ElasticSearchSqlFeeder.Shared;
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
                    SetPropertiesByMerge(propertyName, newJObjects, document);
                }
                else
                {
                    document = _documentDictionary[id].Document;

                    MyLogger.Trace($"UpdatedJsonObject: id:{id}  _numDocumentsModified={_numDocumentsModified:N0} _documentDictionary.Count={_documentDictionary.Count:N0}");
                    SetPropertiesByMerge(propertyName, newJObjects, document);
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

        private void SetProperties(string propertyName, JObject[] newJObjects, JObject document)
        {
            foreach (var newJObject in newJObjects)
            {
                SetProperties(propertyName, newJObject, document);//TODO: optimize this
            }
        }

        private void SetPropertiesByMerge(string propertyName, JObject[] newJObjects, JObject document)
        {
            foreach (var newJObject in newJObjects) //TODO: optimize this to avoid a loop
            {
                //MergeWithDocument(document, newJObject);
                MergeWithDocumentFast(document, newJObject, 0);
            }
        }

        private void MergeWithDocument(JObject document, JObject newJObject)
        {
            document.Merge(newJObject, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union,
                MergeNullValueHandling = MergeNullValueHandling.Ignore
            });
        }

        private void MergeWithDocumentFast(JObject originalJObject, JObject newJObject, int level)
        {
            level++;

            //check if the keys match then this is the same object so merge else add to list

            // iterate through all the arrays in newJObject
            //var jTokenType = newJObject.Type;
            //var jEnumerable = newJObject.Children();
            //var count = jEnumerable.Count();
            //var firstOrDefault = jEnumerable.FirstOrDefault();

            CopyAllProperties(originalJObject, newJObject);

            foreach (KeyValuePair<string, JToken> property in newJObject)
            {
                if (property.Value.Type == JTokenType.Array)
                {
                    var array = property.Value as JArray;
                    var selectProperty = originalJObject.SelectToken(property.Key) as JArray;
                    if (selectProperty == null)
                    {
                        // create array if it does not exist
                        // else create a new array
                        selectProperty = new JArray { };
                        originalJObject.Add(property.Key, selectProperty);
                    }
                    MergeArrayFast(selectProperty, array, level);
                }

            }
        }

        private void MergeArrayFast(JArray originalArray, JArray newArray, int level)
        {
            var jEnumerable = newArray.Children();
            foreach (var child in jEnumerable)
            {
                if (child.Type == JTokenType.Object)
                {
                    var key = "KeyLevel" + level;

                    // try to match on key to see if originalArray also has this item
                    var newJObject = (child as JObject);
                    var newKeyValue = newJObject.Property(key).Value;

                    bool found = false;
                    foreach (var originalChild in originalArray.Children())
                    {
                        var originalJObject = (originalChild as JObject);
                        var originalKeyValue = originalJObject.Property(key).Value;
                        if (originalKeyValue.Value<string>() == newKeyValue.Value<string>())
                        {
                            found = true;
                            // found a match on key so try to merge
                            MergeWithDocumentFast(originalJObject, newJObject, level);
                            break;
                        }
                    }

                    if (!found)
                    {
                        AddObjectAtEndOfArray(originalArray, newJObject);
                    }
                }
            }
        }

        private static void AddObjectAtEndOfArray(JArray originalArray, JObject newJObject)
        {
            originalArray.Add(newJObject);
//            // not found so add at the end
//            if (originalArray.Last == null)
//            {
//                if (originalArray.First != null)
//                {
//// if property exists then add to the current array
//                    originalArray.First.AddAfterSelf(newJObject);
//                }
//                else
//                {
//                    originalArray.Children().
//                }
//            }
//            else
//            {
//                // if property exists then add to the current array
//                originalArray.Last.AddAfterSelf(newJObject);
//            }
        }

        private void SetProperties(string propertyName, JObject newJObject, JObject document)
        {
            // if propertyName is not set then copy all the properties into this new object
            if (propertyName == null)
            {
                CopyAllProperties(document, newJObject);
            }
            else
            {
                // else see if the property already exists
                var selectProperty = document.SelectToken(propertyName);
                if (selectProperty != null)
                {
                    if (selectProperty.Last == null)
                    {
                        // if property exists then add to the current array
                        selectProperty.First.AddAfterSelf(newJObject);
                    }
                    else
                    {
                        // if property exists then add to the current array
                        selectProperty.Last.AddAfterSelf(newJObject);
                    }

                }
                else
                {
                    // else create a new array
                    var jArray = new JArray { newJObject };
                    document[propertyName] = jArray;
                }
            }
        }

        private static void CopyAllProperties(JObject originalJObject, JObject newJObject)
        {
            foreach (var property in newJObject.Properties())
            {
                if (property.Value.Type != JTokenType.Array 
                    //&& property.Value.Type != JTokenType.Object
                    && !property.Name.StartsWith("KeyLevel",StringComparison.OrdinalIgnoreCase))
                {
                    originalJObject[property.Name] = property.Value;
                }
            }
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
