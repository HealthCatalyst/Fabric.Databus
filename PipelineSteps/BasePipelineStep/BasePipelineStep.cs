// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasePipelineStep.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   Defines the BasePipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace BasePipelineStep
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.ProgressMonitor;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The base queue processor.
    /// </summary>
    /// <typeparam name="TQueueInItem">
    /// in item
    /// </typeparam>
    /// <typeparam name="TQueueOutItem">
    /// out item
    /// </typeparam>
    public abstract class BasePipelineStep<TQueueInItem, TQueueOutItem> : IPipelineStep
        where TQueueInItem : IQueueItem
        where TQueueOutItem : IQueueItem
    {
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
        /// The queue manager.
        /// </summary>
        private readonly IQueueManager queueManager;

        /// <summary>
        /// The progress monitor.
        /// </summary>
        private readonly IProgressMonitor progressMonitor;

        /// <summary>
        /// The cancellation token.
        /// </summary>
        private readonly CancellationToken cancellationToken;

        /// <summary>
        /// The id.
        /// </summary>
        private readonly int id = 0;

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
        /// Initializes a new instance of the <see cref="BasePipelineStep{TQueueInItem,TQueueOutItem}"/> class.
        /// </summary>
        /// <param name="jobConfig">
        ///     The queue context.
        /// </param>
        /// <param name="logger">
        ///     The logger
        /// </param>
        /// <param name="queueManager">
        ///     The queue manager
        /// </param>
        /// <param name="progressMonitor">
        ///     The progress Monitor.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation Token.
        /// </param>
        protected BasePipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken)
        {
            this.queueManager = queueManager ?? throw new ArgumentNullException(nameof(jobConfig));
            this.progressMonitor = progressMonitor ?? throw new ArgumentNullException(nameof(progressMonitor));
            this.cancellationToken = cancellationToken;

            this.Config = jobConfig;
            if (this.Config == null)
            {
                throw new ArgumentNullException(nameof(this.Config));
            }

            this.id = queueManager.GetUniqueId();

            this.MyLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the my logger.
        /// </summary>
        protected ILogger MyLogger { get; }

        /// <summary>
        /// Gets or sets the in queue.
        /// </summary>
        protected IMeteredBlockingCollection<TQueueInItem> InQueue { get; set; }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        public IJobConfig Config { get; set; }

        /// <summary>
        /// Gets the logger name.
        /// </summary>
        protected abstract string LoggerName { get; }

        /// <summary>
        /// The unique id.
        /// </summary>
        protected int UniqueId => this.id;

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
                this.cancellationToken.ThrowIfCancellationRequested();

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

                    this.MyLogger.Verbose(
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
                    this.MyLogger.Verbose("{@Exception}", e);
                    errorText = e.ToString();
                    throw;
                }
            }

            currentProcessorCount = Interlocked.Decrement(ref processorCount);
            var isLastThreadForThisTask = currentProcessorCount < 1;
            this.Complete(queryId, isLastThreadForThisTask);

            this.MyLogger.Verbose($"Completed {queryId}: queue: {this.InQueue.Count}");

            this.LogToConsole(null);
        }

        /// <summary>
        /// The mark output queue as completed.
        /// </summary>
        /// <param name="stepNumber1">
        /// The step number.
        /// </param>
        public void MarkOutputQueueAsCompleted(int stepNumber1)
        {
            this.queueManager.GetOutputQueue<TQueueOutItem>(stepNumber1).CompleteAdding();
        }

        /// <summary>
        /// The initialize with step number.
        /// </summary>
        /// <param name="stepNumber1">
        /// The step number.
        /// </param>
        public void InitializeWithStepNumber(int stepNumber1)
        {
            this.stepNumber = stepNumber1;
            this.InQueue = this.queueManager.GetInputQueue<TQueueInItem>(stepNumber1);
            this.outQueue = this.queueManager.GetOutputQueue<TQueueOutItem>(stepNumber1);
        }

        /// <summary>
        /// The create out queue.
        /// </summary>
        /// <param name="stepNumber1">
        /// The step number.
        /// </param>
        public void CreateOutQueue(int stepNumber1)
        {
            this.queueManager.CreateOutputQueue<TQueueOutItem>(stepNumber1);
        }

        /// <summary>
        /// The test complete.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        /// The is last thread for this task.
        /// </param>
        public void TestComplete(string queryId, bool isLastThreadForThisTask)
        {
            this.Complete(queryId, isLastThreadForThisTask);
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
            // Logger.Verbose($"AddToOutputQueue {item.ToJson()}");
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
        /// The log to console.
        /// </summary>
        /// <param name="id1">
        /// The id.
        /// </param>
        private void LogToConsole(string id1)
        {
            this.progressMonitor.SetProgressItem(new ProgressMonitorItem
            {
                StepNumber = this.stepNumber,
                LoggerName = this.LoggerName,
                Id = id1,
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
