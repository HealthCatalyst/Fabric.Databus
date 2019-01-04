// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineExecutorBase.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the PipelineExecutorBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineRunner
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Pipeline;

    using Unity;

    /// <summary>
    /// The pipeline executor base.
    /// </summary>
    public abstract class PipelineExecutorBase : IPipelineExecutor
    {
        /// <summary>
        /// The cancellation token source.
        /// </summary>
        protected CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The container.
        /// </summary>
        protected IUnityContainer container;

        /// <summary>
        /// The step number.
        /// </summary>
        protected int stepNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineExecutorBase"/> class. 
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="cancellationTokenSource">
        /// The cancellation token source.
        /// </param>
        protected PipelineExecutorBase(IUnityContainer container, CancellationTokenSource cancellationTokenSource)
        {
            this.container = container;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        /// <inheritdoc />
        public abstract Task RunPipelineTasksAsync(
            IQueryConfig config,
            IList<PipelineStepInfo> pipelineSteps,
            int timeoutInMilliseconds);
    }
}