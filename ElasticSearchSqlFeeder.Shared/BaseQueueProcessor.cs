using ElasticSearchSqlFeeder.Interfaces;
using System;
using System.Diagnostics;
using System.Threading;
using ElasticSearchSqlFeeder.ProgressMonitor;
using Fabric.Databus.Config;
using NLog;

namespace ElasticSearchSqlFeeder.Shared
{
    public abstract class BaseQueueProcessor<TQueueInItem, TQueueOutItem> : IBaseQueueProcessor
        where TQueueInItem : IQueueItem
        where TQueueOutItem : IQueueItem
    {
        // intentionally want a different id for each derived class
        // ReSharper disable once StaticMemberInGenericType
        private int _id = 0;

        protected Logger MyLogger;

        protected IMeteredBlockingCollection<TQueueInItem> _inQueue;
        private IMeteredBlockingCollection<TQueueOutItem> _outQueue;

        private int _totalItemsProcessedByThisProcessor;

        // ReSharper disable once StaticMemberInGenericType
        private static int _totalItemsProcessed;

        // ReSharper disable once StaticMemberInGenericType
        private static int _totalItemsAddedToOutputQueue;

        // ReSharper disable once StaticMemberInGenericType
        private static TimeSpan _processingTime = TimeSpan.Zero;
        // ReSharper disable once StaticMemberInGenericType
        private static TimeSpan _blockedTime = TimeSpan.Zero;

        // ReSharper disable once StaticMemberInGenericType
        private static int _processorCount;
        private static int _maxProcessorCount;

        protected readonly QueueContext QueueContext;
        private int _stepNumber;
        private static string _errorText;

        protected BaseQueueProcessor(QueueContext queueContext)
        {
            QueueContext = queueContext;

            Config = queueContext.Config;

            _id = queueContext.QueueManager.GetUniqueId();

            MyLogger = LogManager.GetLogger(LoggerName + "_" + _id);

            _totalItemsProcessed = 0;
            _totalItemsAddedToOutputQueue = 0;
        }

        protected int UniqueId => _id;

        public QueryConfig Config { get; set; }

        public void MonitorWorkQueue()
        {
            var currentProcessorCount = Interlocked.Increment(ref _processorCount);
            Interlocked.Increment(ref _maxProcessorCount);

            var isFirstThreadForThisTask = currentProcessorCount < 2;
            Begin(isFirstThreadForThisTask);

            LogToConsole(null);

            string queryId = null;
            var stopWatch = new Stopwatch();

            while (true)
            {
                try
                {
                    stopWatch.Restart();

                    var wt = _inQueue.Take();

                    _blockedTime = _blockedTime.Add(stopWatch.Elapsed);

                    queryId = wt.QueryId;

                    LogItemToConsole(wt);

                    stopWatch.Restart();

                    Handle(wt);

                    _processingTime = _processingTime.Add(stopWatch.Elapsed);
                    stopWatch.Stop();

                    _totalItemsProcessedByThisProcessor++;

                    Interlocked.Increment(ref _totalItemsProcessed);

                    LogItemToConsole(wt);

                    MyLogger.Trace(
                        $"Processing: {wt.QueryId} {GetId(wt)}, Queue Length: {_outQueue.Count:N0}, Processed: {_totalItemsProcessed:N0}, ByThisProcessor: {_totalItemsProcessedByThisProcessor:N0}");

                }
                catch (InvalidOperationException e)
                {
                    if (e.Source == "System.Collections.Concurrent"
                        || e.Message?.Contains("The collection argument is empty")==true
                        ) break;
                    throw;
                }
                catch (Exception e)
                {
                    MyLogger.Trace(e);
                    _errorText = e.ToString();
                    throw;
                }
            }

            currentProcessorCount = Interlocked.Decrement(ref _processorCount);
            var isLastThreadForThisTask = currentProcessorCount < 1;
            Complete(queryId, isLastThreadForThisTask);

            MyLogger.Trace($"Completed {queryId}: queue: {_inQueue.Count}");
            
            LogToConsole(null);
        }

        protected virtual void LogItemToConsole(TQueueInItem wt)
        {
            var id = GetId(wt);

            LogToConsole(id);
        }

        private void LogToConsole(string id)
        {
            QueueContext.ProgressMonitor.SetProgressItem(new ProgressMonitorItem
            {
                StepNumber = _stepNumber,
                LoggerName = LoggerName,
                Id = id,
                InQueueCount = _inQueue.Count,
                InQueueName = _inQueue.Name,
                IsInQueueCompleted = _inQueue.IsCompleted,
                TimeElapsedProcessing = _processingTime,
                TimeElapsedBlocked = _blockedTime,
                TotalItemsProcessed = _totalItemsProcessed,
                TotalItemsAddedToOutputQueue = _totalItemsAddedToOutputQueue,
                OutQueueName = _outQueue.Name,
                QueueProcessorCount = _processorCount,
                MaxQueueProcessorCount = _maxProcessorCount,
                ErrorText = _errorText,
            });
        }

        protected void AddToOutputQueue(TQueueOutItem item)
        {
            //Logger.Trace($"AddToOutputQueue {item.ToJson()}");
            _outQueue.Add(item);
            Interlocked.Increment(ref _totalItemsAddedToOutputQueue);
        }

        protected abstract void Handle(TQueueInItem workitem);

        protected abstract void Begin(bool isFirstThreadForThisTask);

        protected abstract void Complete(string queryId, bool isLastThreadForThisTask);

        protected abstract string GetId(TQueueInItem workitem);

        protected abstract string LoggerName { get; }

        public void MarkOutputQueueAsCompleted(int stepNumber)
        {
            QueueContext.QueueManager.GetOutputQueue<TQueueOutItem>(stepNumber).CompleteAdding();
        }

        public void InitializeWithStepNumber(int stepNumber)
        {
            _stepNumber = stepNumber;
            _inQueue = QueueContext.QueueManager.GetInputQueue<TQueueInItem>(stepNumber);
            _outQueue = QueueContext.QueueManager.GetOutputQueue<TQueueOutItem>(stepNumber);
        }

        public void CreateOutQueue(int stepNumber)
        {
            QueueContext.QueueManager.CreateOutputQueue<TQueueOutItem>(stepNumber);
        }
    }

}
