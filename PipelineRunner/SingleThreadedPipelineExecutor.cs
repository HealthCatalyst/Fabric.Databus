// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleThreadedPipelineExecutor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SingleThreadedPipelineExecutor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunner
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using ElasticSearchSqlFeeder.Interfaces;

    using Unity;

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
        public override void RunPipelineTasks(IQueryConfig config, IList<QueueProcessorInfo> processors, int timeoutInMilliseconds)
        {
            foreach (var processor in processors)
            {
                this.RunSync(() => (IBaseQueueProcessor)this.container.Resolve(processor.Type));
            }
        }

        /// <summary>
        /// The run async.
        /// </summary>
        /// <param name="functionQueueProcessor">
        /// The fn queue processor.
        /// </param>
        private void RunSync(Func<IBaseQueueProcessor> functionQueueProcessor)
        {
            this.stepNumber++;

            var thisStepNumber = this.stepNumber;

            var queueProcessor = functionQueueProcessor();

            var type = queueProcessor.GetType();

            queueProcessor.CreateOutQueue(thisStepNumber);

            queueProcessor.InitializeWithStepNumber(thisStepNumber);

            queueProcessor.MonitorWorkQueue();

            functionQueueProcessor().MarkOutputQueueAsCompleted(thisStepNumber);
        }
    }
}
