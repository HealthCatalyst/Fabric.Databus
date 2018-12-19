// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProgressMonitorItem.cs" company="">
//   
// </copyright>
// <summary>
//   The progress monitor item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Loggers
{
    using System;
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Pipeline;

    /// <summary>
    /// The progress monitor item.
    /// </summary>
    public class ProgressMonitorItem
    {
        /// <summary>
        /// Gets or sets the query id.
        /// </summary>
        public string QueryId { get; set; }

        /// <summary>
        /// Gets or sets the logger name.
        /// </summary>
        public string LoggerName { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the in queue count.
        /// </summary>
        public int InQueueCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        public string Minimum { get; set; }

        /// <summary>
        /// Gets or sets the time elapsed processing.
        /// </summary>
        public TimeSpan TimeElapsedProcessing { get; set; }

        /// <summary>
        /// Gets or sets the time elapsed blocked.
        /// </summary>
        public TimeSpan TimeElapsedBlocked { get; set; }

        /// <summary>
        /// The time elapsed processing as text.
        /// </summary>
        public string TimeElapsedProcessingAsText => this.TimeElapsedProcessing.ToString("g");

        /// <summary>
        /// The time elapsed blocked as text.
        /// </summary>
        public string TimeElapsedBlockedAsText => this.TimeElapsedBlocked.ToString("g");

        /// <summary>
        /// Gets or sets the last completed entity id for each query.
        /// </summary>
        public List<KeyValuePair<string, string>> LastCompletedEntityIdForEachQuery { get; set; }

        /// <summary>
        /// Gets or sets the document dictionary count.
        /// </summary>
        public int DocumentDictionaryCount { get; set; }

        /// <summary>
        /// Gets or sets the total items processed.
        /// </summary>
        public int TotalItemsProcessed { get; set; }

        /// <summary>
        /// Gets or sets the total items added to output queue.
        /// </summary>
        public int TotalItemsAddedToOutputQueue { get; set; }

        /// <summary>
        /// Gets or sets the step number.
        /// </summary>
        public int StepNumber { get; set; }

        /// <summary>
        /// Gets or sets the queue processor count.
        /// </summary>
        public int QueueProcessorCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is in queue completed.
        /// </summary>
        public PipelineStepState State { get; set; }

        /// <summary>
        /// Gets or sets the in queue name.
        /// </summary>
        public string InQueueName { get; set; }

        /// <summary>
        /// Gets or sets the out queue name.
        /// </summary>
        public string OutQueueName { get; set; }

        /// <summary>
        /// Gets or sets the error text.
        /// </summary>
        public string ErrorText { get; set; }

        /// <summary>
        /// Gets or sets the max queue processor count.
        /// </summary>
        public int MaxQueueProcessorCount { get; set; }

        /// <summary>
        /// Gets or sets the unique step id.
        /// </summary>
        public int UniqueStepId { get; set; }

        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        public int BatchNumber { get; set; }

        /// <summary>
        /// Gets or sets the total batches.
        /// </summary>
        public int TotalBatches { get; set; }
    }
}