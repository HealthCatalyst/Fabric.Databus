// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleThreadedPipelineExecutorFactory.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SingleThreadedPipelineExecutorFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineRunner
{
    using System.Threading;

    using Unity;

    /// <inheritdoc />
    /// <summary>
    /// The single threaded pipeline executor factory.
    /// </summary>
    public class SingleThreadedPipelineExecutorFactory : IPipelineExecutorFactory
    {
        /// <inheritdoc />
        public IPipelineExecutor Create(IUnityContainer container, CancellationTokenSource cancellationTokenSource)
        {
            return new SingleThreadedPipelineExecutor(container, cancellationTokenSource);
        }
    }
}