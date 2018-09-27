// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiThreadedPipelineExecutor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MultiThreadedPipelineExecutor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ConvertDatabaseRowToJsonQueueProcessor;

    using CreateBatchItemsQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;

    using FileSaveQueueProcessor;

    using FileUploadQueueProcessor;

    using JsonDocumentMergerQueueProcessor;

    using SaveBatchQueueProcessor;

    using SqlBatchQueueProcessor;

    using SqlImportQueueProcessor;

    using Unity;

    /// <summary>
    /// The multi threader pipeline executor.
    /// </summary>
    public class MultiThreadedPipelineExecutor : PipelineExecutorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiThreadedPipelineExecutor"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="cancellationTokenSource">
        /// The cancellation token source.
        /// </param>
        public MultiThreadedPipelineExecutor(IUnityContainer container, CancellationTokenSource cancellationTokenSource)
            : base(container, cancellationTokenSource)
        {
        }

        /// <summary>
        /// The run pipeline tasks.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="processors">processors to run</param>
        /// <param name="timeoutInMilliseconds">
        /// The timeout in milliseconds.
        /// </param>
        /// <exception cref="AggregateException">exception thrown
        /// </exception>
        public override void RunPipelineTasks(
            IQueryConfig config,
            IList<QueueProcessorInfo> processors,
            int timeoutInMilliseconds)
        {
            var tasks = processors
                .Select(processor => this.RunAsync(() => (IBaseQueueProcessor)this.container.Resolve(processor.Type), processor.Count))
                .SelectMany(task => task)
                .ToList();

            try
            {
                Task.WaitAll(tasks.ToArray(), timeoutInMilliseconds, this.cancellationTokenSource.Token);
            }
            catch (Exception)
            {
                var exceptions = tasks.Where(task => task.Exception != null).Select(task => task.Exception.Flatten()).ToList();
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// The run async.
        /// </summary>
        /// <param name="functionQueueProcessor">
        /// The fn queue processor.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        private IList<Task> RunAsync(Func<IBaseQueueProcessor> functionQueueProcessor, int count)
        {
            IList<Task> tasks = new List<Task>();

            this.stepNumber++;

            var thisStepNumber = this.stepNumber;

            bool isFirst = true;

            for (var i = 0; i < count; i++)
            {
                var queueProcessor = functionQueueProcessor();

                if (isFirst)
                {
                    queueProcessor.CreateOutQueue(thisStepNumber);
                    isFirst = false;
                }

                queueProcessor.InitializeWithStepNumber(thisStepNumber);

                var task = Task.Factory.StartNew(
                    (o) =>
                    {
                        queueProcessor.MonitorWorkQueue();
                        return 1;
                    },
                    thisStepNumber,
                    this.cancellationTokenSource.Token);

                tasks.Add(task);
            }

            var taskArray = tasks.ToArray();

            Task.WhenAll(taskArray).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.cancellationTokenSource.Cancel();
                    }
                },
                this.cancellationTokenSource.Token,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current).ContinueWith(
                task =>
                {
                    if (!task.IsFaulted)
                    {
                        functionQueueProcessor().MarkOutputQueueAsCompleted(thisStepNumber);
                    }
                });

            return tasks;
        }
    }
}
