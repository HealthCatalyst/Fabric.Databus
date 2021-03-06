﻿// --------------------------------------------------------------------------------------------------------------------
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
    using Fabric.Databus.Interfaces.Pipeline;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.PipelineSteps;

    using Unity;
    using Unity.Resolution;

    /// <inheritdoc />
    /// <summary>
    /// The multi thread pipeline executor.
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
        public override async Task RunPipelineTasksAsync(
            IQueryConfig config,
            IList<PipelineStepInfo> pipelineSteps,
            int timeoutInMilliseconds)
        {
            List<Task> tasks = new List<Task>();

            foreach (var pipelineStep in pipelineSteps)
            {
                var pipelineStepState = new PipelineStepState();

                // ReSharper disable once ConvertToLocalFunction
#pragma warning disable IDE0039 // Use local function
                Func<IPipelineStep> functionPipelineStep =
#pragma warning restore IDE0039 // Use local function
                    () => (IPipelineStep)this.container.Resolve(
                        pipelineStep.Type,
                        new ParameterOverride("cancellationToken", this.cancellationTokenSource.Token),
                        new ParameterOverride("pipelineStepState", pipelineStepState));

                var tasksForPipelineStep = this.CreateTasks(functionPipelineStep, pipelineStep.Count);

                tasks.AddRange(tasksForPipelineStep);

                // create a local variable so it can captured in the closure
                var thisStepNumber = this.stepNumber;

                // mark a queue as done when all the tasks for that queue are done
#pragma warning disable 4014
                Task.WhenAll(tasksForPipelineStep).ContinueWith(
#pragma warning restore 4014
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
#pragma warning disable 4014
                    task.ContinueWith(
#pragma warning restore 4014
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
                await Task.WhenAll(tasks.ToArray());
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
        /// The function that returns a pipeline step.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <returns>
        /// The <see cref="IList{T}"/>.
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
