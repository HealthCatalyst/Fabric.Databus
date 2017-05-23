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
                $"{key}-{progressMonitorItem.LoggerName}({progressMonitorItem.QueueProcessorCount}) Id:{progressMonitorItem.Id,10} InQueue:{progressMonitorItem.InQueueCount:N0}{isQueueCompleted} Minimum:{progressMonitorItem.Minimum,10}";

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
                    $" Time:{progressMonitorItem.TimeElapsedProcessing.ToString(@"hh\:mm\:ss")}";
            }

            if (progressMonitorItem.DocumentDictionaryCount > 0)
            {
                formattableString += $" Dictionary:{progressMonitorItem.DocumentDictionaryCount}";
            }

            formattableString += $" Processed: {progressMonitorItem.TotalItemsProcessed:N0}";
            formattableString += $" Out: {progressMonitorItem.TotalItemsAddedToOutputQueue:N0}";

            AppendLine(formattableString);

        }

    }
}