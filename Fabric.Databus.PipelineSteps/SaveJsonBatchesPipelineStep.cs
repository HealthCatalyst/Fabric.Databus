﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveJsonBatchesPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SaveJsonBatchesPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Json;

    using Newtonsoft.Json;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    public class SaveJsonBatchesPipelineStep : BasePipelineStep<SaveBatchQueueItem, FileUploadQueueItem>
    {
        /// <inheritdoc />
        public SaveJsonBatchesPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken,
            PipelineStepState pipelineStepState) 
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken, pipelineStepState)
        {
        }

        /// <inheritdoc />
        protected override string LoggerName => "SaveJsonBatch";

        /// <inheritdoc />
        protected override string GetId(SaveBatchQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <inheritdoc />
        protected override async System.Threading.Tasks.Task HandleAsync(SaveBatchQueueItem workItem)
        {
            await this.FlushDocumentsToBatchFile(workItem.ItemsToSave);
        }

        /// <summary>
        /// The flush documents to batch file.
        /// </summary>
        /// <param name="documentCacheItems">
        /// The document cache items.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task FlushDocumentsToBatchFile(IEnumerable<IJsonObjectQueueItem> documentCacheItems)
        {
            var stream = new MemoryStream(); // do not use using since we'll pass it to next queue

            using (var textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            using (var writer = new JsonTextWriter(textWriter))
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var documentCacheItem in documentCacheItems)
                {
                    var doc = documentCacheItem.Document;
                    var entityId = documentCacheItem.Id;

                    await writer.WriteStartObjectAsync();
                    using (new JsonPropertyWrapper(writer, "update"))
                    {
                        await writer.WritePropertyNameAsync("_id");
                        await writer.WriteValueAsync(entityId);
                    }
                    await writer.WriteEndObjectAsync();
                    await writer.WriteRawAsync("\n");

                    await writer.WriteStartObjectAsync(); // <update>

                    //-- start writing doc
                    await writer.WritePropertyNameAsync("doc");

                    await doc.WriteToAsync(writer);
                    //// writer.WriteRaw(doc.ToString());

                    // https://www.elastic.co/guide/en/elasticsearch/reference/current/docs-update.html
                    await writer.WritePropertyNameAsync("doc_as_upsert");
                    await writer.WriteValueAsync(true);

                    await writer.WriteEndObjectAsync(); // </update>

                    await writer.WriteRawAsync("\n");
                }
            }

            var batchNumber = this.pipelineStepState.IncrementCurrentBatchFileNumber();

            await this.AddToOutputQueueAsync(new FileUploadQueueItem
                                                 {
                                                     BatchNumber = batchNumber,
                                                     // ReSharper disable once PossibleMultipleEnumeration
                                                     TotalBatches = documentCacheItems.First().TotalBatches,
                                                     Stream = stream
                                                 });

            this.MyLogger.Verbose("Wrote batch: {batchNumber}", batchNumber);
        }
    }
}
