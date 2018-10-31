// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiThreadedPipelineExecutor.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the MultiThreadedPipelineExecutor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Queues;

    using Unity;
    using Unity.Resolution;

    /// <summary>
    /// The multi threader pipeline executor.
    /// </summary>
    public class MultiThreadedPipelineExecutor : PipelineExecutorBase
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.PipelineRunner.MultiThreadedPipelineExecutor" /> class.
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
            IList<PipelineStepInfo> pipelineSteps,
            int timeoutInMilliseconds)
        {
            List<Task> tasks = new List<Task>();

            foreach (var pipelineStep in pipelineSteps)
            {
                // ReSharper disable once ConvertToLocalFunction
                Func<IPipelineStep> functionPipelineStep =
                    () => (IPipelineStep)this.container.Resolve(
                    pipelineStep.Type,
                    new ParameterOverride("cancellationToken", this.cancellationTokenSource.Token));

                var tasksForPipelineStep = this.CreateTasks(functionPipelineStep, pipelineStep.Count);

                tasks.AddRange(tasksForPipelineStep);

                // create a local variable so it can captured in the closure
                var thisStepNumber = this.stepNumber;

                // mark a queue as done when all the tasks for that queue are done
                Task.WhenAll(tasksForPipelineStep).ContinueWith(
                    t =>
                        {
                            functionPipelineStep().MarkOutputQueueAsCompleted(thisStepNumber);
                        });
            }

            try
            {
                foreach (var task in tasks)
                {
                    // do it here so we don't have the original tasks in the list not the continuation ones
                    task.ContinueWith(
                        t =>
                            {
                                if (!this.cancellationTokenSource.IsCancellationRequested)
                                {
                                    // set the cancellationToken so other tasks are canceled
                                    this.cancellationTokenSource.Cancel();
                                }
                            },
                        this.cancellationTokenSource.Token,
                        TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                        TaskScheduler.Current);
                }

                tasks.ForEach(t => t.Start());
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                var realExceptions = ex.Flatten().InnerExceptions.Where(e => !(e is TaskCanceledException)).ToList();
                throw new AggregateException(realExceptions);
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
        [Pure]
        private IList<Task> CreateTasks(Func<IPipelineStep> functionPipelineStep, int count)
        {
            IList<Task> tasks = new List<Task>();

            this.stepNumber++;

            // create a local variable so it can captured in the closure
            var thisStepNumber = this.stepNumber;

            bool isFirst = true;

            for (var i = 0; i < count; i++)
            {
                var pipelineStep = functionPipelineStep();

                if (isFirst)
                {
                    pipelineStep.CreateOutQueue(thisStepNumber);
                    isFirst = false;
                }

                pipelineStep.InitializeWithStepNumber(thisStepNumber);

                var task = new Task<int>(
                    (o) =>
                    {
                        pipelineStep.MonitorWorkQueueAsync().Wait();
                        return 1;
                    },
                    thisStepNumber,
                    this.cancellationTokenSource.Token);

                tasks.Add(task);
            }

            return tasks;
        }
    }
}
