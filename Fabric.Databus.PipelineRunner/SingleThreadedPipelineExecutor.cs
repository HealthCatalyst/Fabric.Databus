// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleThreadedPipelineExecutor.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SingleThreadedPipelineExecutor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineRunner
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Pipeline;
    using Fabric.Databus.Interfaces.Queues;

    using Unity;
    using Unity.Resolution;

    /// <summary>
    /// The single threaded pipeline executor.
    /// </summary>
    public class SingleThreadedPipelineExecutor : PipelineExecutorBase
    {
        /// <inheritdoc />
        public SingleThreadedPipelineExecutor(IUnityContainer container, CancellationTokenSource cancellationTokenSource)
            : base(container, cancellationTokenSource)
        {
        }

        /// <inheritdoc />
        public override Task RunPipelineTasksAsync(
            IQueryConfig config,
            IList<PipelineStepInfo> pipelineSteps,
            int timeoutInMilliseconds)
        {
            foreach (var processor in pipelineSteps)
            {
                this.RunSync(
                    () => (IPipelineStep)this.container.Resolve(
                        processor.Type,
                        new ParameterOverride("cancellationToken", this.cancellationTokenSource.Token)));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// The run async.
        /// </summary>
        /// <param name="functionQueueProcessor">
        /// The function pipeline step.
        /// </param>
        private void RunSync(Func<IPipelineStep> functionQueueProcessor)
        {
            this.stepNumber++;

            var thisStepNumber = this.stepNumber;

            var queueProcessor = functionQueueProcessor();

            var type = queueProcessor.GetType();

            queueProcessor.CreateOutQueue(thisStepNumber);

            queueProcessor.InitializeWithStepNumber(thisStepNumber);

            queueProcessor.MonitorWorkQueueAsync().Wait();

            functionQueueProcessor().MarkOutputQueueAsCompleted(thisStepNumber);
        }
    }
}
