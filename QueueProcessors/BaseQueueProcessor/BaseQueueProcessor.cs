// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseQueueProcessor.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   Defines the BaseQueueProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace BaseQueueProcessor
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.ProgressMonitor;

    using NLog;

    using QueueItems;

    /// <summary>
    /// The base queue processor.
    /// </summary>
    /// <typeparam name="TQueueInItem">
    /// in item
    /// </typeparam>
    /// <typeparam name="TQueueOutItem">
    /// out item
    /// </typeparam>
    public abstract class BaseQueueProcessor<TQueueInItem, TQueueOutItem> : IBaseQueueProcessor
        where TQueueInItem : IQueueItem
        where TQueueOutItem : IQueueItem
    {
        // intentionally want a different id for each derived class
        // ReSharper disable once StaticMemberInGenericType
        private readonly int id = 0;

        /// <summary>
        /// The queue context.
        /// </summary>
        protected readonly IQueueContext QueueContext;

        /// <summary>
        /// The my logger.
        /// </summary>
        protected Logger MyLogger;

        /// <summary>
        /// The _in queue.
        /// </summary>
        protected IMeteredBlockingCollection<TQueueInItem> InQueue;

        /// <summary>
        /// The _total items processed.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static int totalItemsProcessed;

        /// <summary>
        /// The _total items added to output queue.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static int totalItemsAddedToOutputQueue;

        /// <summary>
        /// The _processing time.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static TimeSpan processingTime = TimeSpan.Zero;

        /// <summary>
        /// The _blocked time.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static TimeSpan blockedTime = TimeSpan.Zero;

        /// <summary>
        /// The processor count.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static int processorCount;

        /// <summary>
        /// The max processor count.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static int maxProcessorCount;

        /// <summary>
        /// The error text.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static string errorText;

        /// <summary>
        /// The _step number.
        /// </summary>
        private int stepNumber;

        /// <summary>
        /// The _out queue.
        /// </summary>
        private IMeteredBlockingCollection<TQueueOutItem> outQueue;

        /// <summary>
        /// The _total items processed by this processor.
        /// </summary>
        private int totalItemsProcessedByThisProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseQueueProcessor{TQueueInItem,TQueueOutItem}"/> class.
        /// </summary>
        /// <param name="queueContext">
        /// The queue context.
        /// </param>
        protected BaseQueueProcessor(IQueueContext queueContext)
        {
            this.QueueContext = queueContext;

            this.Config = queueContext.Config;

            this.id = queueContext.QueueManager.GetUniqueId();

            // ReSharper disable once VirtualMemberCallInConstructor
            this.MyLogger = LogManager.GetLogger(this.LoggerName + "_" + this.id);
        }

        /// <summary>
        /// The unique id.
        /// </summary>
        protected int UniqueId => this.id;

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        public IQueryConfig Config { get; set; }

        /// <summary>
        /// The monitor work queue.
        /// </summary>
        public void MonitorWorkQueue()
        {
            var currentProcessorCount = Interlocked.Increment(ref processorCount);
            Interlocked.Increment(ref maxProcessorCount);

            var isFirstThreadForThisTask = currentProcessorCount < 2;
            this.Begin(isFirstThreadForThisTask);

            this.LogToConsole(null);

            string queryId = null;
            var stopWatch = new Stopwatch();

            while (true)
            {
                try
                {
                    stopWatch.Restart();

                    var wt = this.InQueue.Take();

                    blockedTime = blockedTime.Add(stopWatch.Elapsed);

                    queryId = wt.QueryId;

                    this.LogItemToConsole(wt);

                    stopWatch.Restart();

                    this.InternalHandle(wt);

                    processingTime = processingTime.Add(stopWatch.Elapsed);
                    stopWatch.Stop();

                    this.totalItemsProcessedByThisProcessor++;

                    Interlocked.Increment(ref totalItemsProcessed);

                    this.LogItemToConsole(wt);

                    this.MyLogger.Trace(
                        $"Processing: {wt.QueryId} {this.GetId(wt)}, Queue Length: {this.outQueue.Count:N0}, Processed: {totalItemsProcessed:N0}, ByThisProcessor: {this.totalItemsProcessedByThisProcessor:N0}");

                }
                catch (InvalidOperationException e)
                {
                    if (e.Source == "System.Collections.Concurrent"
                        || e.Message?.Contains("The collection argument is empty") == true)
                    {
                        break;
                    }

                    throw;
                }
                catch (Exception e)
                {
                    this.MyLogger.Trace(e);
                    errorText = e.ToString();
                    throw;
                }
            }

            currentProcessorCount = Interlocked.Decrement(ref processorCount);
            var isLastThreadForThisTask = currentProcessorCount < 1;
            this.Complete(queryId, isLastThreadForThisTask);

            this.MyLogger.Trace($"Completed {queryId}: queue: {this.InQueue.Count}");

            this.LogToConsole(null);
        }

        /// <summary>
        /// The mark output queue as completed.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        public void MarkOutputQueueAsCompleted(int stepNumber)
        {
            this.QueueContext.QueueManager.GetOutputQueue<TQueueOutItem>(stepNumber).CompleteAdding();
        }

        /// <summary>
        /// The initialize with step number.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        public void InitializeWithStepNumber(int stepNumber)
        {
            this.stepNumber = stepNumber;
            this.InQueue = this.QueueContext.QueueManager.GetInputQueue<TQueueInItem>(stepNumber);
            this.outQueue = this.QueueContext.QueueManager.GetOutputQueue<TQueueOutItem>(stepNumber);
        }

        /// <summary>
        /// The create out queue.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        public void CreateOutQueue(int stepNumber)
        {
            this.QueueContext.QueueManager.CreateOutputQueue<TQueueOutItem>(stepNumber);
        }

        /// <summary>
        /// The internal handle.
        /// </summary>
        /// <param name="wt">
        /// The wt.
        /// </param>
        internal void InternalHandle(TQueueInItem wt)
        {
            this.Handle(wt);
        }

        /// <summary>
        /// The log item to console.
        /// </summary>
        /// <param name="wt">
        /// The wt.
        /// </param>
        protected virtual void LogItemToConsole(TQueueInItem wt)
        {
            var id = this.GetId(wt);

            this.LogToConsole(id);
        }

        /// <summary>
        /// The add to output queue.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        protected void AddToOutputQueue(TQueueOutItem item)
        {
            // Logger.Trace($"AddToOutputQueue {item.ToJson()}");
            this.outQueue.Add(item);
            Interlocked.Increment(ref totalItemsAddedToOutputQueue);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="workItem">
        /// The workItem.
        /// </param>
        protected abstract void Handle(TQueueInItem workItem);

        /// <summary>
        /// The begin.
        /// </summary>
        /// <param name="isFirstThreadForThisTask">
        /// The is first thread for this task.
        /// </param>
        protected abstract void Begin(bool isFirstThreadForThisTask);

        /// <summary>
        /// The complete.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        /// The is last thread for this task.
        /// </param>
        protected abstract void Complete(string queryId, bool isLastThreadForThisTask);

        /// <summary>
        /// The get id.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected abstract string GetId(TQueueInItem workItem);

        /// <summary>
        /// Gets the logger name.
        /// </summary>
        protected abstract string LoggerName { get; }

        /// <summary>
        /// The log to console.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        private void LogToConsole(string id)
        {
            this.QueueContext.ProgressMonitor.SetProgressItem(new ProgressMonitorItem
            {
                StepNumber = this.stepNumber,
                LoggerName = this.LoggerName,
                Id = id,
                InQueueCount = this.InQueue.Count,
                InQueueName = this.InQueue.Name,
                IsInQueueCompleted = this.InQueue.IsCompleted,
                TimeElapsedProcessing = processingTime,
                TimeElapsedBlocked = blockedTime,
                TotalItemsProcessed = totalItemsProcessed,
                TotalItemsAddedToOutputQueue = totalItemsAddedToOutputQueue,
                OutQueueName = this.outQueue.Name,
                QueueProcessorCount = processorCount,
                MaxQueueProcessorCount = maxProcessorCount,
                ErrorText = errorText,
            });
        }
    }
}
