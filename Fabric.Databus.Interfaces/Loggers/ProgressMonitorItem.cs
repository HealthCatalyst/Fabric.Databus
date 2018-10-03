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

    /// <summary>
    /// The progress monitor item.
    /// </summary>
    public class ProgressMonitorItem
    {
        public string QueryId { get; set; }
        public string LoggerName { get; set; }
        public string Id { get; set; }
        public int InQueueCount { get; set; }
        public string Minimum { get; set; }
        public TimeSpan TimeElapsedProcessing { get; set; }
        public TimeSpan TimeElapsedBlocked { get; set; }
        public string TimeElapsedProcessingAsText => this.TimeElapsedProcessing.ToString("g");
        public string TimeElapsedBlockedAsText => this.TimeElapsedBlocked.ToString("g");

        public List<KeyValuePair<string, string>> LastCompletedEntityIdForEachQuery { get; set; }
        public int DocumentDictionaryCount { get; set; }
        public int TotalItemsProcessed { get; set; }
        public int TotalItemsAddedToOutputQueue { get; set; }
        public int StepNumber { get; set; }
        public int QueueProcessorCount { get; set; }
        public bool IsInQueueCompleted { get; set; }
        public string InQueueName { get; set; }
        public string OutQueueName { get; set; }
        public string ErrorText { get; set; }
        public int MaxQueueProcessorCount { get; set; }
    }
}