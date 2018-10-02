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

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Json;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    public class SaveBatchPipelineStep : BasePipelineStep<SaveBatchQueueItem, FileUploadQueueItem>
    {
        private static int _currentBatchFileNumber = 0;

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


        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
        }

        protected override string GetId(SaveBatchQueueItem workItem)
        {
            return workItem.QueryId;
        }

        protected override void Handle(SaveBatchQueueItem workItem)
        {
            this.FlushDocumentsToBatchFile(workItem.ItemsToSave);
        }

        private void FlushDocumentsToBatchFile(List<IJsonObjectQueueItem> documentCacheItems)
        {
            var docs = documentCacheItems.Select(c => c.Document).ToList();

            var stream = new MemoryStream(); // do not use using since we'll pass it to next queue

            using (var textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            using (var writer = new JsonTextWriter(textWriter))
            {
                foreach (var doc in docs)
                {
                    var entityId = doc[this.Config.TopLevelKeyColumn].Value<string>();

                    writer.WriteStartObject();
                    using (new JsonPropertyWrapper(writer, "update"))
                    {
                        writer.WritePropertyName("_id");
                        writer.WriteValue(entityId);
                    }
                    writer.WriteEndObject();
                    writer.WriteRaw("\n");

                    writer.WriteStartObject(); // <update>

                    //-- start writing doc
                    writer.WritePropertyName("doc");

                    doc.WriteTo(writer);
                    //writer.WriteRaw(doc.ToString());

                    // https://www.elastic.co/guide/en/elasticsearch/reference/current/docs-update.html
                    writer.WritePropertyName("doc_as_upsert");
                    writer.WriteValue(true);

                    writer.WriteEndObject(); // </update>

                    writer.WriteRaw("\n");
                }

            }

            var batchNumber = Interlocked.Increment(ref _currentBatchFileNumber);

            this.AddToOutputQueue(new FileUploadQueueItem
            {
                BatchNumber = batchNumber,
                Stream = stream
            });

            this.MyLogger.Verbose($"Wrote batch: {batchNumber}");
        }

        protected override string LoggerName => "SaveBatch";


    }
}
