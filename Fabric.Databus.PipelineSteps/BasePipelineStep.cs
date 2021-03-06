﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Exceptions;
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
        /// The pipeline step state.
        /// </summary>
        protected readonly PipelineStepState pipelineStepState;

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
        /// <param name="pipelineStepState">
        /// pipeline Step State
        /// </param>
        protected BasePipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken,
            PipelineStepState pipelineStepState)
        {
            this.queueManager = queueManager ?? throw new ArgumentNullException(nameof(jobConfig));
            this.progressMonitor = progressMonitor ?? throw new ArgumentNullException(nameof(progressMonitor));
            this.cancellationToken = cancellationToken;
            this.pipelineStepState = pipelineStepState ?? throw new ArgumentNullException(nameof(pipelineStepState));

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
            var currentProcessorCount = this.pipelineStepState.IncrementCurrentInstancesOfStep();
            this.pipelineStepState.IncrementMaximumInstancesOfStep();

            var isFirstThreadForThisTask = currentProcessorCount < 2;
            await this.BeginAsync(isFirstThreadForThisTask);

            this.LogToConsole(null, null, PipelineStepStatus.Starting, 0, 0);

            this.workItemQueryId = null;
            var stopWatch = new Stopwatch();

            while (true)
            {
                this.cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (await this.ProcessWorkItemAsync(stopWatch) == false)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    this.MyLogger.Verbose("{@Exception}", e);
                    this.pipelineStepState.ErrorText = e.ToString();
                    throw new DatabusPipelineStepException(this.LoggerName, e);
                }
            }

            this.MyLogger.Verbose("Completed {QueryId}: queue: {InQueueCount}", this.workItemQueryId, this.InQueue.Count);

            this.LogToConsole(null, null, PipelineStepStatus.Completed, 0, 0);
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
        /// <param name="batchNumber">
        /// The batch Number.
        /// </param>
        /// <param name="batchCompletedQueueItem">
        /// The batch Completed Queue Item.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task CompleteBatchForTestingAsync(string queryId, bool isLastThreadForThisTask, int batchNumber, BatchCompletedQueueItem batchCompletedQueueItem)
        {
            await this.CompleteBatchAsync(queryId, isLastThreadForThisTask, batchNumber, batchCompletedQueueItem);
        }

        /// <summary>
        /// The internal handle.
        /// </summary>
        /// <param name="workItem">
        /// The workItem.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task InternalHandleAsync(TQueueInItem workItem)
        {
            await this.HandleAsync(workItem);
        }

        /// <summary>
        /// The log item to console.
        /// </summary>
        /// <param name="wt">
        /// The workItem.
        /// </param>
        /// <param name="pipelineStepStatus">
        /// The pipeline Step Status.
        /// </param>
        protected virtual void LogItemToConsole(TQueueInItem wt, PipelineStepStatus pipelineStepStatus)
        {
            var myId = this.GetId(wt);

            this.LogToConsole(myId, wt.QueryId, pipelineStepStatus, wt.BatchNumber, wt.TotalBatches);
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
            this.pipelineStepState.IncrementTotalItemsAddedToOutputQueue();

            if (this.workItemQueryId != null)
            {
                if (this.Config.TrackPerformance)
                {
                    this.pipelineStepState.TotalItemsAddedToOutputQueueByQueryId.AddOrUpdate(this.workItemQueryId, 1, (key, currentValue) => currentValue + 1);
                }
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
        /// The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        /// The is last thread for this task.
        /// </param>
        /// <param name="batchNumber">
        /// The batch Number.
        /// </param>
        /// <param name="batchCompletedQueueItem">
        /// The batch Completed Queue Item.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected virtual async Task CompleteBatchAsync(string queryId, bool isLastThreadForThisTask, int batchNumber, IBatchCompletedQueueItem batchCompletedQueueItem)
        {
            await this.AddBatchCompletionMessageToOutputQueueAsync(batchCompletedQueueItem);
        }

        /// <summary>
        /// The complete job async.
        /// </summary>
        /// <param name="queryId">
        ///     The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        ///     The is last thread for this task.
        /// </param>
        /// <param name="jobCompletedQueueItem">
        /// job completed queue item
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected virtual async Task CompleteJobAsync(
            string queryId,
            bool isLastThreadForThisTask,
            IJobCompletedQueueItem jobCompletedQueueItem)
        {
            await this.AddJobCompletionMessageToOutputQueueAsync(jobCompletedQueueItem);
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
        /// <param name="batchCompletedQueueItem">
        /// batch completed queue item
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected async Task AddBatchCompletionMessageToOutputQueueAsync(IBatchCompletedQueueItem batchCompletedQueueItem)
        {
            this.outQueue.AddBatchCompleted(batchCompletedQueueItem);

            await Task.CompletedTask;
        }

        /// <summary>
        /// The add batch completion message to output queue async.
        /// </summary>
        /// <param name="jobCompletedQueueItem">
        /// job completed queue item
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected async Task AddJobCompletionMessageToOutputQueueAsync(IJobCompletedQueueItem jobCompletedQueueItem)
        {
            this.outQueue.AddJobCompleted(jobCompletedQueueItem);

            await Task.CompletedTask;
        }

        /// <summary>
        /// The wait till output queue is empty async.
        /// </summary>
        /// <param name="batchNumber">
        /// The batch Number.
        /// </param>
        /// <param name="totalBatches">
        /// The total Batches.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected async Task WaitTillOutputQueueIsEmptyAsync(int batchNumber, int totalBatches)
        {
            this.LogToConsole(null, null, PipelineStepStatus.Waiting, batchNumber, totalBatches);

            await this.outQueue.WaitTillEmptyAsync(this.cancellationToken);
        }

        /// <summary>
        /// Separate this out to ensure the GC cleans up
        /// </summary>
        /// <param name="stopWatch">
        /// stop watch
        /// </param>
        /// <returns>
        /// result of processing
        /// </returns>
        private async Task<bool> ProcessWorkItemAsync(Stopwatch stopWatch)
        {
            stopWatch.Restart();

            IQueueItem wt1 = this.InQueue.TakeGeneric(this.cancellationToken);

            if (wt1 == null)
            {
                return false;
            }

            if (wt1 is JobCompletedQueueItem jobCompletedQueueItem)
            {
                this.MyLogger.Verbose(
                    "{Name} Job Completed. Length: {QueueLength} {totalItemsProcessed},  {totalItemsProcessedByThisProcessor0}",
                    this.LoggerName,
                    this.outQueue.Count,
                    this.pipelineStepState.TotalItemsProcessed,
                    this.totalItemsProcessedByThisProcessor);

                await this.CompleteJobAsync(null, true, jobCompletedQueueItem);
                return false;
            }

            if (wt1 is BatchCompletedQueueItem batchCompletedQueueItem)
            {
                this.MyLogger.Verbose(
                    "{Name} Batch Completed (batch). Length: {QueueLength} {totalItemsProcessed},  {totalItemsProcessedByThisProcessor0}",
                    this.LoggerName,
                    wt1.BatchNumber,
                    this.outQueue.Count,
                    this.pipelineStepState.TotalItemsProcessed,
                    this.totalItemsProcessedByThisProcessor);

                await this.CompleteBatchAsync(null, true, batchCompletedQueueItem.BatchNumber, batchCompletedQueueItem);
                return true;
            }

            var wt = wt1 as TQueueInItem ?? throw new Exception("wt1 is not TQueueItem");

            this.pipelineStepState.BlockedTime = this.pipelineStepState.BlockedTime.Add(stopWatch.Elapsed);

            this.workItemQueryId = wt.QueryId;

            this.LogItemToConsole(wt, PipelineStepStatus.Processing);

            stopWatch.Restart();

            try
            {
                // if we use await here then we go through all the items and nothing gets processed
                this.InternalHandleAsync(wt).Wait(this.cancellationToken);
            }
            catch (Exception e)
            {
                throw new DatabusPipelineStepWorkItemException(wt1, e);
            }

            this.pipelineStepState.ProcessingTime = this.pipelineStepState.ProcessingTime.Add(stopWatch.Elapsed);

            var uniqueWorkItemId = this.GetId(wt);
            if (uniqueWorkItemId != null)
            {
                if (this.Config.TrackPerformance)
                {
                    this.pipelineStepState.ProcessingTimeByQueryId.AddOrUpdate(
                    uniqueWorkItemId,
                    stopWatch.Elapsed,
                    (myQueryId, previousElapsed) => previousElapsed.Add(stopWatch.Elapsed));
                }
            }

            stopWatch.Stop();

            this.totalItemsProcessedByThisProcessor++;

            this.pipelineStepState.IncrementTotalItemsProcessed();

            this.LogItemToConsole(wt, PipelineStepStatus.Processed);

            this.MyLogger.Debug(
                "{Name}({queryId}) Processed: {id} Queue Length: {QueueLength} {totalItemsProcessed},  {totalItemsProcessedByThisProcessor0}",
                this.LoggerName,
                this.workItemQueryId,
                this.GetId(wt),
                this.outQueue.Count,
                this.pipelineStepState.TotalItemsProcessed,
                this.totalItemsProcessedByThisProcessor);

            // try to explicitly remove reference
            // ReSharper disable once RedundantAssignment
            wt = null;
            // ReSharper disable once RedundantAssignment
            wt1 = null;

            return true;
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
        /// <param name="pipelineStepStatus">
        ///     The pipeline Step Status.
        /// </param>
        /// <param name="batchNumber">
        ///     batch number
        /// </param>
        /// <param name="totalBatches">
        /// total batches
        /// </param>
        private void LogToConsole(
            string id1,
            string queryId,
            PipelineStepStatus pipelineStepStatus,
            int batchNumber,
            int totalBatches)
        {
            var timeElapsedProcessing = queryId != null && this.pipelineStepState.ProcessingTimeByQueryId.ContainsKey(queryId)
                                            ? this.pipelineStepState.ProcessingTimeByQueryId[queryId]
                                            : this.pipelineStepState.ProcessingTime;

            var itemsAddedToOutputQueue = this.workItemQueryId != null && this.pipelineStepState.TotalItemsAddedToOutputQueueByQueryId.ContainsKey(this.workItemQueryId)
                                              ? this.pipelineStepState.TotalItemsAddedToOutputQueueByQueryId[this.workItemQueryId]
                                              : this.pipelineStepState.TotalItemsAddedToOutputQueue;

            var progressMonitorItem = new ProgressMonitorItem
            {
                StepNumber = this.stepNumber,
                BatchNumber = batchNumber,
                TotalBatches = totalBatches,
                QueryId = queryId,
                LoggerName = this.LoggerName,
                Id = id1,
                UniqueStepId = this.UniqueId,
                InQueueCount = this.InQueue.Count,
                InQueueName = this.InQueue.Name,
                Status = pipelineStepStatus,
                TimeElapsedProcessing = timeElapsedProcessing,
                TimeElapsedBlocked = this.pipelineStepState.BlockedTime,
                TotalItemsProcessed = this.pipelineStepState.TotalItemsProcessed,
                TotalItemsAddedToOutputQueue = itemsAddedToOutputQueue,
                OutQueueName = this.outQueue.Name,
                QueueProcessorCount = this.pipelineStepState.CurrentInstancesOfStep,
                MaxQueueProcessorCount = this.pipelineStepState.MaximumInstancesOfStep,
                ErrorText = this.pipelineStepState.ErrorText,
            };

            this.progressMonitor.SetProgressItem(progressMonitorItem);

            if (pipelineStepStatus != PipelineStepStatus.Starting && pipelineStepStatus != PipelineStepStatus.Completed)
            {
                this.MyLogger.Verbose("{PipelineStep} [{BatchNumber}] ({Status}): {@ProgressMonitorItem}", this.LoggerName, batchNumber, pipelineStepStatus, progressMonitorItem);
            }
            else
            {
                this.MyLogger.Information("{PipelineStep} [{BatchNumber}] ({Status}): {@ProgressMonitorItem}", this.LoggerName, batchNumber, pipelineStepStatus, progressMonitorItem);
            }

            if (pipelineStepStatus == PipelineStepStatus.Completed)
            {
                this.progressMonitor.CompleteProgressItemsWithUniqueId(this.UniqueId);
            }
        }
    }
}
