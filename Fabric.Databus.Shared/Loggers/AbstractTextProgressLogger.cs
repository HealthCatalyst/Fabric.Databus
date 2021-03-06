// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractTextProgressLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the AbstractTextProgressLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Loggers
{
    using System;

    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Shared;

    /// <inheritdoc />
    /// <summary>
    /// The abstract text progress logger.
    /// </summary>
    public abstract class AbstractTextProgressLogger : IProgressLogger
    {
        /// <inheritdoc />
        /// <summary>
        /// The reset.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// The append line.
        /// </summary>
        /// <param name="text">
        /// The text string.
        /// </param>
        public abstract void AppendLine(string text);

        /// <inheritdoc />
        /// <summary>
        /// The get log.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.String" />.
        /// </returns>
        public abstract string GetLog();

        /// <inheritdoc />
        public void LogProgressMonitorItem(string key, ProgressMonitorItem progressMonitorItem)
        {
            var loggerName = progressMonitorItem.LoggerName;
            var queueProcessorCount = progressMonitorItem.QueueProcessorCount;
            var maxQueueProcessorCount = progressMonitorItem.MaxQueueProcessorCount;
            var inQueueCount = progressMonitorItem.InQueueCount;
            var minimum = progressMonitorItem.Minimum;
            var time = progressMonitorItem.TimeElapsedProcessing.ToString(@"hh\:mm\:ss");
            var dictionary = progressMonitorItem.DocumentDictionaryCount;
            var @out = progressMonitorItem.TotalItemsAddedToOutputQueue;
            var processed = progressMonitorItem.TotalItemsProcessed;

            var text =
                $"{key,30} {loggerName,25}({queueProcessorCount,3}/{maxQueueProcessorCount,3}) {string.Empty,4} {inQueueCount,7} {processed,10} {@out,7} {time,15} {progressMonitorItem.Status,10} {progressMonitorItem.BatchNumber, 5}/{progressMonitorItem.TotalBatches,5}";

            this.AppendLine(text);
        }

        /// <inheritdoc />
        public void LogHeader()
        {
            const string Key = "key";
            const string LoggerName = "Step";
            const string QueueProcessorCount = "current";
            const string MaxQueueProcessorCount = "max";
            const string InQueueCount = "In";
            const string Time = "Time";
            const string Out = "Out";
            const string Processed = "Processed";

            const string State = "Status";
            const string Batch = "Batch";
            const string TotalBatches = "Total";

            var text =
                $"{Key,30} {LoggerName,25}({QueueProcessorCount,3}/{MaxQueueProcessorCount,3}) {InQueueCount,7} {Processed,10} {Out,7} {Time,15} {State, 10} {Batch, 5}/{TotalBatches, 5}";

            this.AppendLine(text);
        }

        /// <inheritdoc />
        public void LogFooter()
        {
            var memoryInBytes = GC.GetTotalMemory(false);
            this.AppendLine($"Memory Working set: {memoryInBytes.ToDisplayString()}");
        }
    }
}