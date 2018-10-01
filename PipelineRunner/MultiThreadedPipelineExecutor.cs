// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiThreadedPipelineExecutor.cs" company="Health Catalyst">
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

    using ElasticSearchSqlFeeder.Interfaces;

    using FileSavePipelineStep;

    using FileUploadPipelineStep;

    using SaveBatchPipelineStep;

    using SqlBatchPipelineStep;

    using Unity;

    /// <summary>
    /// The multi threader pipeline executor.
    /// </summary>
    public class MultiThreadedPipelineExecutor : PipelineExecutorBase
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:PipelineRunner.MultiThreadedPipelineExecutor" /> class.
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

        /// <inheritdoc />
        public override void RunPipelineTasks(
            IQueryConfig config,
            IList<PipelineStepInfo> processors,
            int timeoutInMilliseconds)
        {
            var tasks = processors
                .Select(processor => this.RunAsync(() => (IPipelineStep)this.container.Resolve(processor.Type), processor.Count))
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
        /// <param name="functionPipelineStep">
        /// The fn queue processor.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        private IList<Task> RunAsync(Func<IPipelineStep> functionPipelineStep, int count)
        {
            IList<Task> tasks = new List<Task>();

            this.stepNumber++;

            var thisStepNumber = this.stepNumber;

            bool isFirst = true;

            for (var i = 0; i < count; i++)
            {
                var PipelineStep = functionPipelineStep();

                if (isFirst)
                {
                    PipelineStep.CreateOutQueue(thisStepNumber);
                    isFirst = false;
                }

                PipelineStep.InitializeWithStepNumber(thisStepNumber);

                var task = Task.Factory.StartNew(
                    (o) =>
                    {
                        PipelineStep.MonitorWorkQueue();
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
                        functionPipelineStep().MarkOutputQueueAsCompleted(thisStepNumber);
                    }
                });

            return tasks;
        }
    }
}
