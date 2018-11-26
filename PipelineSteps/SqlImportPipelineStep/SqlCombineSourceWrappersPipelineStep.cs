// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlCombineSourceWrappersPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlCombineSourceWrappersPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SqlImportPipelineStep
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Shared;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The sql combine source wrappers pipeline step.
    /// </summary>
    public class SqlCombineSourceWrappersPipelineStep : BasePipelineStep<SqlDataLoadedQueueItem, SourceWrapperCollectionQueueItem>
    {
        /// <summary>
        /// The source wrapper collection.
        /// </summary>
        private readonly SourceWrapperCollection sourceWrapperCollection = new SourceWrapperCollection();

        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        private int batchNumber;

        /// <inheritdoc />
        public SqlCombineSourceWrappersPipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
        }

        /// <inheritdoc />
        protected override string LoggerName => "SqlCombineSourceWrappers";

        /// <inheritdoc />
        protected override Task HandleAsync(SqlDataLoadedQueueItem workItem)
        {
            var keyLevels = workItem.PropertyName.Count(c => c == '.');

            var keys = new List<string>();
            for (int i = 0; i < keyLevels + 1; i++)
            {
                keys.Add($"KeyLevel{i+1}");
            }

            this.sourceWrapperCollection.Add(
                new SourceWrapper(
                    workItem.QueryId,
                    workItem.Columns,
                    workItem.PropertyName,
                    workItem.Rows,
                    keys,
                    workItem.PropertyType != "object",
                    this.Config.KeepTemporaryLookupColumnsInOutput));

            this.batchNumber = workItem.BatchNumber;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        /// <summary>
        /// The complete async.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        /// The is last thread for this task.
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        protected override Task CompleteAsync(string queryId, bool isLastThreadForThisTask)
        {
            this.AddToOutputQueueAsync(new SourceWrapperCollectionQueueItem
                                           {
                                               BatchNumber = this.batchNumber,
                                               SourceWrapperCollection = this.sourceWrapperCollection
                                           });

            return base.CompleteAsync(queryId, isLastThreadForThisTask);
        }

        /// <inheritdoc />
        protected override string GetId(SqlDataLoadedQueueItem workItem)
        {
            return workItem.QueryId;
        }
    }
}
