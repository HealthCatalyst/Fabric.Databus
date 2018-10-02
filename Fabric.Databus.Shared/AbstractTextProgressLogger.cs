// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractTextProgressLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the AbstractTextProgressLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using Fabric.Databus.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The abstract text progress logger.
    /// </summary>
    public abstract class AbstractTextProgressLogger : IProgressLogger
    {
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

        /// <summary>
        /// The get log.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public abstract string GetLog();

        /// <inheritdoc />
        public void LogProgressMonitorItem(int key, ProgressMonitorItem progressMonitorItem)
        {
            var isQueueCompleted = progressMonitorItem.IsInQueueCompleted ? " [C]" : string.Empty;

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
                $"{key,4}-{loggerName,25}({queueProcessorCount,3}/{maxQueueProcessorCount,3}) {string.Empty,4} {inQueueCount,7} {processed,10} {@out,7} {time,15} {dictionary,7} {isQueueCompleted,3}";

            this.AppendLine(text);
        }

        /// <inheritdoc />
        public void LogHeader()
        {
            var key = "key";
            var loggerName = "Step";
            var queueProcessorCount = "current";
            var maxQueueProcessorCount = "max";
            var inQueueCount = "In";
            var minimum = "minimum";
            var time = "Time";
            var dictionary = "dictionary";
            var @out = "Out";
            var processed = "Processed";

            var text =
                $"{key,4} {loggerName,25}({queueProcessorCount,3}/{maxQueueProcessorCount,3}) {inQueueCount,7} {processed,10} {@out,7} {time, 15} {dictionary, 7} C";

            this.AppendLine(text);
        }
    }
}