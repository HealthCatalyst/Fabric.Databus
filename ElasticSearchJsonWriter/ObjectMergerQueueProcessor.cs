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

    public class ObjectMergerQueueProcessor : BaseQueueProcessor<ConvertDatabaseToJsonQueueItem, JsonObjectQueueItem>
    {
        private readonly IMeteredConcurrentDictionary<string, JsonObjectQueueItem> _documentDictionary;

        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        private static readonly SequenceBarrier SequenceBarrier = new SequenceBarrier();

        private int _numDocumentsModified;

        private static int _currentBatchNumber = 0;

        private readonly string _folder;
        
        public ObjectMergerQueueProcessor(IMeteredConcurrentDictionary<string, JsonObjectQueueItem> documentDictionary, QueueContext queueContext)
            : base(queueContext)
        {
            _documentDictionary = documentDictionary;

            _folder = Path.Combine(Config.LocalSaveFolder, $"{UniqueId}-ObjectMerge");

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


        protected override void Handle(ConvertDatabaseToJsonQueueItem wt)
        {
            // if work item batch number is newer than the last one we saw that we can flush anything related to the old batch
            if(_currentBatchNumber != wt.BatchNumber)
            {
                //find all items with old batch numbers and put them in the out queue
                var list = _documentDictionary.RemoveItems(item => item.Value.BatchNumber != wt.BatchNumber);
                SendToOutputQueue(list);

                _currentBatchNumber = wt.BatchNumber;
            }

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
            var path = _folder;
            if (Config.WriteDetailedTemporaryFilesToDisk)
            {
                Directory.CreateDirectory(path);
            }

            foreach (var tuple in list)
            {
                //remove temporary columns
                RemoveTemporaryColumns(tuple.Item2.Document);

                AddToOutputQueue(tuple.Item2);

                if (Config.WriteDetailedTemporaryFilesToDisk)
                {
                    File.AppendAllText(Path.Combine(path, $"{tuple.Item1}.json"), tuple.Item2.Document.ToString());
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

        protected override string GetId(ConvertDatabaseToJsonQueueItem workitem)
        {
            return null;
        }

        //protected override void LogItemToConsole(JsonDocumentMergerQueueItem wt)
        //{
        //    // don't log here
        //}

        protected override string LoggerName => "JsonDocumentMerger";

    }

    public class DocumentMerger
    {

    }


}
