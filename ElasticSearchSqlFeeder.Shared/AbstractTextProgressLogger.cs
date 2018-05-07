using System;
using System.Collections.Generic;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.ProgressMonitor;

namespace ElasticSearchSqlFeeder.Shared
{
    public abstract class AbstractTextProgressLogger : IProgressLogger
    {
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

            AppendLine(formattableString);
        }

    }
}