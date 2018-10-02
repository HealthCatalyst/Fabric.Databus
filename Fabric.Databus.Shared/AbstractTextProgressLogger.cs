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
    using System;

    using Fabric.Databus.Interfaces;

    /// <summary>
    /// The abstract text progress logger.
    /// </summary>
    public abstract class AbstractTextProgressLogger : IProgressLogger
    {
        /// <summary>
        /// The reset.
        /// </summary>
        public abstract void Reset();
        public abstract void AppendLine(string formattableString);
        public abstract string GetLog();

        public void LogProgressMonitorItem(int key, ProgressMonitorItem progressMonitorItem)
        {
            var isQueueCompleted = progressMonitorItem.IsInQueueCompleted ? " [C]" : String.Empty;

            var formattableString =
                $"{key,4}-{progressMonitorItem.LoggerName,25}({progressMonitorItem.QueueProcessorCount,3}/{progressMonitorItem.MaxQueueProcessorCount,3}) InQueue:{progressMonitorItem.InQueueCount,10:N0} Minimum:{progressMonitorItem.Minimum,10}";

            //if (progressMonitorItem.Value.LastCompletedEntityIdForEachQuery != null)
            //{
            //    foreach (var keyValuePair in progressMonitorItem.Value.LastCompletedEntityIdForEachQuery.OrderBy(a => a.Key))
            //    {
            //        formattableString += $" {keyValuePair.Key}: {keyValuePair.Value}";
            //    }
            //}

            if (progressMonitorItem.TimeElapsedProcessing != TimeSpan.Zero)
            {
                // ReSharper disable once UseFormatSpecifierInInterpolation
                formattableString +=
                    $" Time:{progressMonitorItem.TimeElapsedProcessing.ToString(@"hh\:mm\:ss"),10}";
            }

            if (progressMonitorItem.DocumentDictionaryCount > 0)
            {
                formattableString += $" Dictionary:{progressMonitorItem.DocumentDictionaryCount,10}";
            }

            formattableString += $" Processed: {progressMonitorItem.TotalItemsProcessed,10:N0}";
            formattableString += $" Out: {progressMonitorItem.TotalItemsAddedToOutputQueue,10:N0}";
            formattableString += $"{isQueueCompleted,3}";

            this.AppendLine(formattableString);
        }

    }
}