// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveBatchPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SaveBatchPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SaveBatchPipelineStep
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Json;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    public class SaveBatchPipelineStep : BasePipelineStep<SaveBatchQueueItem, FileUploadQueueItem>
    {
        /// <summary>
        /// The current batch file number.
        /// </summary>
        private static int currentBatchFileNumber = 0;

        /// <inheritdoc />
        public SaveBatchPipelineStep(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken) 
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
        }

        /// <inheritdoc />
        protected override string LoggerName => "SaveBatch";

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
        private async Task FlushDocumentsToBatchFile(List<IJsonObjectQueueItem> documentCacheItems)
        {
            var docs = documentCacheItems.Select(c => c.Document).ToList();

            var stream = new MemoryStream(); // do not use using since we'll pass it to next queue

            using (var textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            using (var writer = new JsonTextWriter(textWriter))
            {
                foreach (var doc in docs)
                {
                    var entityId = doc[this.Config.TopLevelKeyColumn].Value<string>();

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
                    //writer.WriteRaw(doc.ToString());

                    // https://www.elastic.co/guide/en/elasticsearch/reference/current/docs-update.html
                    await writer.WritePropertyNameAsync("doc_as_upsert");
                    await writer.WriteValueAsync(true);

                    await writer.WriteEndObjectAsync(); // </update>

                    await writer.WriteRawAsync("\n");
                }
            }

            var batchNumber = Interlocked.Increment(ref currentBatchFileNumber);

            await this.AddToOutputQueueAsync(new FileUploadQueueItem
                                                 {
                                                     BatchNumber = batchNumber,
                                                     Stream = stream
                                                 });

            this.MyLogger.Verbose($"Wrote batch: {batchNumber}");
        }
    }
}
