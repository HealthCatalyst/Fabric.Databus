// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineExecutorFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the PipelineExecutorFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunner
{
    using System.Threading;

    using Unity;

    /// <summary>
    /// The pipeline executor factory.
    /// </summary>
    public class MultiThreadedPipelineExecutorFactory : IPipelineExecutorFactory
    {
        /// <inheritdoc />
        public IPipelineExecutor Create(IUnityContainer container, CancellationTokenSource cancellationTokenSource)
        {
            return new MultiThreaderPipelineExecutor(container, cancellationTokenSource);
        }
    }
}
