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
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Shared;

    using Newtonsoft.Json.Linq;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// Reads a JsonDocumentMergerQueueItem with a list of json objects and merges them into one json object based on property name.  Then stores these in a IJsonObjectQueueItem.
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
        private readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new ConcurrentDictionary<string, SemaphoreSlim>();

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

            this.folder = this.fileWriter.CombinePath(configLocalSaveFolder, $"{this.UniqueId}-JsonDocumentMerge");
        }

        /// <inheritdoc />
        protected override string LoggerName => "JsonDocumentMerger";

        /// <inheritdoc />
        protected override async Task HandleAsync(JsonDocumentMergerQueueItem workItem)
        {
            // if work item batch number is newer than the last one we saw that we can flush anything related to the old batch
            if (currentBatchNumber != workItem.BatchNumber)
            {
                // find all items with old batch numbers and put them in the out queue
                var list = this.documentDictionary.RemoveAllItemsExceptFromBatchNumber(workItem.BatchNumber);
                await this.SendToOutputQueue(list);

                currentBatchNumber = workItem.BatchNumber;
            }

            await this.AddToJsonObjectAsync(workItem.QueryId, workItem.Id, workItem.PropertyName, workItem.NewJObjects, workItem.BatchNumber);
        }

        /// <inheritdoc />
        protected override async Task CompleteAsync(string queryId, bool isLastThreadForThisTask)
        {
            var list = this.documentDictionary.RemoveAll();
            await this.SendToOutputQueue(list);

            await SequenceBarrier.CompleteQuery(queryId);

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
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task AddToJsonObjectAsync(string queryId, string id, string propertyName, JObject[] newJObjects, int batchNumber)
        {
            // BlockIfMaxSizeReached(id);

            // lock on id so multiple threads cannot update the same document at the same time
            var semaphoreSlim = this.locks.GetOrAdd(id, s => new SemaphoreSlim(1, 1));

            // Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            await semaphoreSlim.WaitAsync();
            try
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
                    await this.entityJsonWriter.SetPropertiesByMergeAsync(propertyName, newJObjects, document);
                }
                else
                {
                    document = this.documentDictionary.GetById(id).Document;

                    this.MyLogger.Verbose($"UpdatedJsonObject: id:{id}  _numDocumentsModified={this.numDocumentsModified:N0} _documentDictionary.Count={this.documentDictionary.Count:N0}");
                    await this.entityJsonWriter.SetPropertiesByMergeAsync(propertyName, newJObjects, document);
                }
            }
            finally
            {
                // When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                // This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                semaphoreSlim.Release();
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
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task AddDocumentsToOutQueue(string min)
        {
            var keys = this.documentDictionary.GetKeysLessThan(min);

            await this.AddDocumentsToOutQueueByKeys(keys);
        }

        /// <summary>
        /// The add documents to out queue by keys.
        /// </summary>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task AddDocumentsToOutQueueByKeys(IList<string> keys)
        {
            foreach (var key in keys)
            {
                await this.AddDocumentToOutputQueueByKey(key);
            }
        }

        /// <summary>
        /// The add document to output queue by key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task AddDocumentToOutputQueueByKey(string key)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            // ReSharper disable once InlineOutVariableDeclaration
            IJsonObjectQueueItem item;
#pragma warning restore IDE0018 // Inline variable declaration
            if (this.documentDictionary.TryRemove(key, out item))
            {
                this.MyLogger.Verbose(
                    $"Remove from Dictionary: _documentDictionary.Count={this.documentDictionary.Count:N0}");

                await this.AddToOutputQueueAsync(item);
            }
        }

        /// <summary>
        /// The send to output queue.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task SendToOutputQueue(IList<Tuple<string, IJsonObjectQueueItem>> list)
        {
            var path = this.folder;

            this.fileWriter.CreateDirectory(path);

            foreach (var tuple in list)
            {
                if (!this.Config.KeepTemporaryLookupColumnsInOutput)
                {
                    // remove temporary columns
                    await this.entityJsonWriter.RemoveTemporaryColumns(tuple.Item2.Document, this.Config.TopLevelKeyColumn);
                }

                await this.AddToOutputQueueAsync(tuple.Item2);

                if (this.fileWriter.IsWritingEnabled)
                {
                    await this.fileWriter.WriteToFileAsync(this.fileWriter.CombinePath(path, $"{tuple.Item1}.json"), tuple.Item2.Document.ToString());
                }
            }
        }
    }
}
