// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonDocumentMergerPipelineStep.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   Defines the JsonDocumentMergerPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JsonDocumentMergerPipelineStep
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Shared;

    using Newtonsoft.Json.Linq;

    using QueueItems;

    using Serilog;

    /// <summary>
    /// The json document merger queue processor.
    /// </summary>
    public class JsonDocumentMergerPipelineStep : BasePipelineStep<JsonDocumentMergerQueueItem, IJsonObjectQueueItem>
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
        /// The document dictionary.
        /// </summary>
        private readonly IDocumentDictionary documentDictionary;

        /// <summary>
        /// The entity json writer.
        /// </summary>
        private readonly IEntityJsonWriter entityJsonWriter;

        /// <summary>
        /// The file writer.
        /// </summary>
        private readonly IDetailedTemporaryFileWriter fileWriter;

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
        public JsonDocumentMergerPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            IDocumentDictionary documentDictionary,
            IEntityJsonWriter entityJsonWriter,
            IDetailedTemporaryFileWriter fileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.documentDictionary = documentDictionary ?? throw new ArgumentNullException(nameof(documentDictionary));
            this.entityJsonWriter = entityJsonWriter ?? throw new ArgumentNullException(nameof(entityJsonWriter));
            this.fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            var configLocalSaveFolder = this.Config.LocalSaveFolder;
            if (configLocalSaveFolder == null)
            {
                throw new ArgumentNullException(nameof(this.Config.LocalSaveFolder));
            }

            this.folder = Path.Combine(configLocalSaveFolder, $"{this.UniqueId}-JsonDocumentMerge");
        }

        /// <inheritdoc />
        protected override string LoggerName => "JsonDocumentMerger";

        /// <inheritdoc />
        protected override void Handle(JsonDocumentMergerQueueItem workItem)
        {
            // if work item batch number is newer than the last one we saw that we can flush anything related to the old batch
            if (currentBatchNumber != workItem.BatchNumber)
            {
                // find all items with old batch numbers and put them in the out queue
                var list = this.documentDictionary.RemoveAllItemsExceptFromBatchNumber(workItem.BatchNumber);
                this.SendToOutputQueue(list);

                currentBatchNumber = workItem.BatchNumber;
            }

            this.AddToJsonObject(workItem.QueryId, workItem.Id, workItem.PropertyName, workItem.NewJObjects, workItem.BatchNumber);
        }

        /// <inheritdoc />
        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        /// <inheritdoc />
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            var list = this.documentDictionary.RemoveAll();
            this.SendToOutputQueue(list);

            SequenceBarrier.CompleteQuery(queryId);

            this.MyLogger.Verbose("Finished");
        }

        /// <inheritdoc />
        protected override string GetId(JsonDocumentMergerQueueItem workItem)
        {
            return null;
        }

        // protected override void LogItemToConsole(JsonDocumentMergerQueueItem workItem)
        // {
        //    // don't log here
        // }

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
                if (!this.documentDictionary.ContainsKey(id))
                {
                    document = new JObject { { this.Config.TopLevelKeyColumn, id } };
                    var jsonObjectCacheItem = new JsonObjectQueueItem
                    {
                        BatchNumber = batchNumber,
                        Id = id,
                        Document = document
                    };


                    this.documentDictionary.Add(id, jsonObjectCacheItem);

                    Interlocked.Increment(ref this.numDocumentsModified);

                    this.MyLogger.Verbose($"AddToJsonObject: id:{id} _numDocumentsModified={this.numDocumentsModified:N0} _documentDictionary.Count={this.documentDictionary.Count:N0}");
                    this.entityJsonWriter.SetPropertiesByMerge(propertyName, newJObjects, document);
                }
                else
                {
                    document = this.documentDictionary.GetById(id).Document;

                    this.MyLogger.Verbose($"UpdatedJsonObject: id:{id}  _numDocumentsModified={this.numDocumentsModified:N0} _documentDictionary.Count={this.documentDictionary.Count:N0}");
                    this.entityJsonWriter.SetPropertiesByMerge(propertyName, newJObjects, document);
                }

            }

            var minimum = SequenceBarrier.UpdateMinimumEntityIdProcessed(queryId, id);

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
            var keys = this.documentDictionary.GetKeysLessThan(min);

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
            if (this.documentDictionary.TryRemove(key, out item))
            {
                this.MyLogger.Verbose(
                    $"Remove from Dictionary: _documentDictionary.Count={this.documentDictionary.Count:N0}");

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

            this.fileWriter.CreateDirectory(path);

            foreach (var tuple in list)
            {
                if (!this.Config.KeepTemporaryLookupColumnsInOutput)
                {
                    // remove temporary columns
                    this.entityJsonWriter.RemoveTemporaryColumns(tuple.Item2.Document, this.Config.TopLevelKeyColumn);
                }

                this.AddToOutputQueue(tuple.Item2);

                this.fileWriter.WriteToFile(Path.Combine(path, $"{tuple.Item1}.json"), tuple.Item2.Document.ToString());
            }
        }
    }
}
