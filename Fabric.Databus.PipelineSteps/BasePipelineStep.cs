// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasePipelineStep.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   Defines the BasePipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Pipeline;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.QueueItems;

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
        where TQueueInItem : class, IQueueItem
        where TQueueOutItem : class, IQueueItem
    {
        /// <summary>
        /// The processing time by query id.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<string, TimeSpan> ProcessingTimeByQueryId = new ConcurrentDictionary<string, TimeSpan>();

        /// <summary>
        /// The total items added to output queue by query id.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<string, int> TotalItemsAddedToOutputQueueByQueryId = new ConcurrentDictionary<string, int>();

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
        private readonly int id;

        /// <summary>
        /// The _step number.
        /// </summary>
        private int stepNumber;

        /// <summary>
        /// The _out queue.
        /// </summary>
        private IQueue<TQueueOutItem> outQueue;

        /// <summary>
        /// The _total items processed by this processor.
        /// </summary>
        private int totalItemsProcessedByThisProcessor;

        /// <summary>
        /// The work item query id.
        /// </summary>
        private string workItemQueryId;

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
        protected IQueue<TQueueInItem> InQueue { get; set; }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        protected IJobConfig Config { get; set; }

        /// <summary>
        /// Gets the logger name.
        /// </summary>
        protected abstract string LoggerName { get; }


        /// <summary>
        /// The unique id.
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        protected int UniqueId => this.id;

        /// <inheritdoc />
        public async Task MonitorWorkQueueAsync()
        {
            var currentProcessorCount = Interlocked.Increment(ref processorCount);
            Interlocked.Increment(ref maxProcessorCount);

            var isFirstThreadForThisTask = currentProcessorCount < 2;
            await this.BeginAsync(isFirstThreadForThisTask);

            this.LogToConsole(null, null, PipelineStepState.Starting, 0);

            this.workItemQueryId = null;
            var stopWatch = new Stopwatch();

            while (true)
            {
                this.cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    stopWatch.Restart();

                    IQueueItem wt1 = this.InQueue.TakeGeneric(this.cancellationToken);

                    if (wt1 == null)
                    {
                        break;
                    }

                    if (wt1 is JobCompletedQueueItem)
                    {
                        await this.AddJobCompletionMessageToOutputQueueAsync();
                        break;
                    }

                    if (wt1 is BatchCompletedQueueItem batchCompletedQueueItem)
                    {
                        await this.CompleteAsync(null, true);
                        await this.AddBatchCompletionMessageToOutputQueueAsync(batchCompletedQueueItem.BatchNumber);
                        continue;
                    }

                    var wt = wt1 as TQueueInItem ?? throw new Exception("wt1 is not TQueueItem");

                    blockedTime = blockedTime.Add(stopWatch.Elapsed);

                    this.workItemQueryId = wt.QueryId;

                    this.LogItemToConsole(wt, PipelineStepState.Processing);

                    stopWatch.Restart();

                    // if we use await here then we go through all the items and nothing gets processed
                    this.InternalHandleAsync(wt).Wait(this.cancellationToken);

                    processingTime = processingTime.Add(stopWatch.Elapsed);

                    var uniqueWorkItemId = this.GetId(wt);
                    if (uniqueWorkItemId != null)
                    {
                        ProcessingTimeByQueryId.AddOrUpdate(
                            uniqueWorkItemId,
                            stopWatch.Elapsed,
                            (myQueryId, previousElapsed) => previousElapsed.Add(stopWatch.Elapsed));
                    }

                    stopWatch.Stop();

                    this.totalItemsProcessedByThisProcessor++;

                    Interlocked.Increment(ref totalItemsProcessed);

                    this.LogItemToConsole(wt, PipelineStepState.Processed);

                    this.MyLogger.Verbose(
                        $"Processing: {wt.QueryId} {this.GetId(wt)}, Queue Length: {this.outQueue.Count:N0}, Processed: {totalItemsProcessed:N0}, ByThisProcessor: {this.totalItemsProcessedByThisProcessor:N0}");
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
            await this.CompleteAsync(this.workItemQueryId, isLastThreadForThisTask);

            this.MyLogger.Verbose($"Completed {this.workItemQueryId}: queue: {this.InQueue.Count}");

            this.LogToConsole(null, null, PipelineStepState.Completed, 0);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
            this.CompleteAsync(queryId, isLastThreadForThisTask);
        }

        /// <summary>
        /// The internal handle.
        /// </summary>
        /// <param name="wt">
        /// The wt.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task InternalHandleAsync(TQueueInItem wt)
        {
            await this.HandleAsync(wt);
        }

        /// <summary>
        /// The log item to console.
        /// </summary>
        /// <param name="wt">
        /// The wt.
        /// </param>
        /// <param name="pipelineStepState">
        /// The pipeline Step State.
        /// </param>
        protected virtual void LogItemToConsole(TQueueInItem wt, PipelineStepState pipelineStepState)
        {
            var myId = this.GetId(wt);

            this.LogToConsole(myId, wt.QueryId, pipelineStepState, wt.BatchNumber);
        }

        /// <summary>
        /// The add to output queue.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected Task AddToOutputQueueAsync(TQueueOutItem item)
        {
            // Logger.Verbose($"AddToOutputQueueAsync {item.ToJson()}");
            this.outQueue.Add(item);
            Interlocked.Increment(ref totalItemsAddedToOutputQueue);

            if (this.workItemQueryId != null)
            {
                TotalItemsAddedToOutputQueueByQueryId.AddOrUpdate(this.workItemQueryId, 1, (key, currentValue) => currentValue + 1);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="workItem">
        /// The workItem.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected abstract Task HandleAsync(TQueueInItem workItem);

        /// <summary>
        /// The begin.
        /// </summary>
        /// <param name="isFirstThreadForThisTask">
        /// The is first thread for this task.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected virtual Task BeginAsync(bool isFirstThreadForThisTask)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// The complete.
        /// </summary>
        /// <param name="queryId">
        ///     The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        ///     The is last thread for this task.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected virtual Task CompleteAsync(string queryId, bool isLastThreadForThisTask)
        {
            return Task.CompletedTask;
        }

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
        /// The add batch completion message to output queue async.
        /// </summary>
        /// <param name="batchNumber">
        /// The batch number.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected async Task AddBatchCompletionMessageToOutputQueueAsync(int batchNumber)
        {
            this.outQueue.AddBatchCompleted(new BatchCompletedQueueItem { BatchNumber = batchNumber });

            await Task.CompletedTask;
        }

        /// <summary>
        /// The add batch completion message to output queue async.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected async Task AddJobCompletionMessageToOutputQueueAsync()
        {
            this.outQueue.AddJobCompleted(new JobCompletedQueueItem());

            await Task.CompletedTask;
        }

        /// <summary>
        /// The wait till output queue is empty async.
        /// </summary>
        /// <param name="batchNumber">
        /// The batch Number.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected async Task WaitTillOutputQueueIsEmptyAsync(int batchNumber)
        {
            this.LogToConsole(null, null, PipelineStepState.Waiting, batchNumber);

            await this.outQueue.WaitTillEmptyAsync(this.cancellationToken);
        }

        /// <summary>
        /// The log to console.
        /// </summary>
        /// <param name="id1">
        ///     The id.
        /// </param>
        /// <param name="queryId">
        ///     The query Id.
        /// </param>
        /// <param name="pipelineStepState">
        ///     The pipo Step State.
        /// </param>
        /// <param name="batchNumber"></param>
        private void LogToConsole(string id1, string queryId, PipelineStepState pipelineStepState, int batchNumber)
        {
            var timeElapsedProcessing = queryId != null && ProcessingTimeByQueryId.ContainsKey(queryId)
                                            ? ProcessingTimeByQueryId[queryId]
                                            : processingTime;

            var itemsAddedToOutputQueue = this.workItemQueryId != null && TotalItemsAddedToOutputQueueByQueryId.ContainsKey(this.workItemQueryId)
                                              ? TotalItemsAddedToOutputQueueByQueryId[this.workItemQueryId]
                                              : totalItemsAddedToOutputQueue;

            this.progressMonitor.SetProgressItem(new ProgressMonitorItem
            {
                StepNumber = this.stepNumber,
                BatchNumber = batchNumber,
                QueryId = queryId,
                LoggerName = this.LoggerName,
                Id = id1,
                UniqueStepId = this.UniqueId,
                InQueueCount = this.InQueue.Count,
                InQueueName = this.InQueue.Name,
                State = pipelineStepState,
                TimeElapsedProcessing = timeElapsedProcessing,
                TimeElapsedBlocked = blockedTime,
                TotalItemsProcessed = totalItemsProcessed,
                TotalItemsAddedToOutputQueue = itemsAddedToOutputQueue,
                OutQueueName = this.outQueue.Name,
                QueueProcessorCount = processorCount,
                MaxQueueProcessorCount = maxProcessorCount,
                ErrorText = errorText,
            });

            if (pipelineStepState == PipelineStepState.Completed)
            {
                this.progressMonitor.CompleteProgressItemsWithUniqueId(this.UniqueId);
            }
        }
    }
}
