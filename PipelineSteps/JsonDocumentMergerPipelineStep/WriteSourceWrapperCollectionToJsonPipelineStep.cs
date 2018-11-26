// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WriteSourceWrapperCollectionToJsonPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the WriteSourceWrapperCollectionToJsonPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JsonDocumentMergerPipelineStep
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The write source wrapper collection to json pipeline step.
    /// </summary>
    public class WriteSourceWrapperCollectionToJsonPipelineStep : BasePipelineStep<SourceWrapperCollectionQueueItem, IJsonObjectQueueItem>
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:JsonDocumentMergerPipelineStep.WriteSourceWrapperCollectionToJsonPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The job config.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager">
        /// The queue manager.
        /// </param>
        /// <param name="progressMonitor">
        /// The progress monitor.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        public WriteSourceWrapperCollectionToJsonPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
        }

        /// <inheritdoc />
        protected override string LoggerName => "WriteSourceWrapperCollectionToJson";

        /// <inheritdoc />
        protected override Task HandleAsync(SourceWrapperCollectionQueueItem workItem)
        {
            using (var textWriter = new StringWriter())
            {
                using (var writer = new JsonTextWriter(textWriter))
                {
                    workItem.SourceWrapperCollection.WriteToJson(writer);
                }

                var result = textWriter.ToString();
                var actualJson = JArray.Parse(result);
                var container = new JObject { { "result", actualJson } };
                this.AddToOutputQueueAsync(new JsonObjectQueueItem { Document = container });
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override string GetId(SourceWrapperCollectionQueueItem workItem)
        {
            return workItem.QueryId;
        }
    }
}
