// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveSchemaQueueProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SaveSchemaQueueProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SaveSchemaQueueProcessor
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using BaseQueueProcessor;

    using ElasticSearchJsonWriter;

    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    using QueueItems;

    using Serilog;

    /// <summary>
    /// The save schema queue processor.
    /// </summary>
    public class SaveSchemaQueueProcessor : BaseQueueProcessor<SaveSchemaQueueItem, MappingUploadQueueItem>
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:SaveSchemaQueueProcessor.SaveSchemaQueueProcessor" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager">
        /// The queue Manager.
        /// </param>
        /// <param name="progressMonitor"></param>
        /// <param name="cancellationToken"></param>
        public SaveSchemaQueueProcessor(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken) 
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
        }

        /// <inheritdoc />
        protected override string LoggerName => "SaveSchema";

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        protected override void Handle(SaveSchemaQueueItem workItem)
        {
            foreach (var mapping in workItem.Mappings.OrderBy(m => m.SequenceNumber).ToList())
            {
                this.SendMapping(mapping, workItem.Job);
            }
        }

        /// <inheritdoc />
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
        }

        /// <summary>
        /// The get id.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected override string GetId(SaveSchemaQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The send mapping.
        /// </summary>
        /// <param name="mapping">
        /// The mapping.
        /// </param>
        /// <param name="workItemJob">
        /// The work item job.
        /// </param>
        private void SendMapping(MappingItem mapping, IJob workItemJob)
        {
            var stream = new MemoryStream(); // do not use using since we'll pass it to next queue

            var propertyPath = mapping.PropertyPath == string.Empty ? null : mapping.PropertyPath;

            using (var textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                // TOD: Imran
                // EsJsonWriter.WriteMappingToStream(mapping.Columns, propertyPath, textWriter, mapping.PropertyType, this.Config.EntityType);
            }

            this.AddToOutputQueue(new MappingUploadQueueItem
                                      {
                                          PropertyName = mapping.PropertyPath,
                                          SequenceNumber = mapping.SequenceNumber,
                                          Stream = stream,
                                          Job = workItemJob
                                      });

            if (this.Config.WriteTemporaryFilesToDisk)
            {
                string path = Path.Combine(this.Config.LocalSaveFolder, propertyPath != null ? $@"mapping-{mapping.SequenceNumber}-{propertyPath}.json" : "mainmapping.json");
                using (var fileStream = File.Create(path))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);

                    fileStream.Flush();
                }
            }
        }
    }
}