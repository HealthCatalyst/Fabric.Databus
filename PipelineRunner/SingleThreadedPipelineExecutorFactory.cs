// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleThreadedPipelineExecutorFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SingleThreadedPipelineExecutorFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunner
{
    using System.Threading;

    using Unity;

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