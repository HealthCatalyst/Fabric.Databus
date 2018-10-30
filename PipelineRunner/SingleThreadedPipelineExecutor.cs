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

    using Fabric.Databus.Interfaces.Config;
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
        public override void RunPipelineTasks(IQueryConfig config, IList<PipelineStepInfo> processors, int timeoutInMilliseconds)
        {
            foreach (var processor in processors)
            {
                this.RunSync(
                    () => (IPipelineStep)this.container.Resolve(
                        processor.Type,
                        new ParameterOverride("cancellationToken", this.cancellationTokenSource.Token)));
            }
        }

        /// <summary>
        /// The run async.
        /// </summary>
        /// <param name="functionQueueProcessor">
        /// The fn queue processor.
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
