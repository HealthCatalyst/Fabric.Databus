// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPipelineExecutorFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IPipelineExecutorFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunner
{
    using System.Threading;

    using Unity;

    /// <summary>
    /// The PipelineExecutorFactory interface.
    /// </summary>
    public interface IPipelineExecutorFactory
    {
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="cancellationTokenSource">
        /// The cancellation token source.
        /// </param>
        /// <returns>
        /// The <see cref="IPipelineExecutor"/>.
        /// </returns>
        IPipelineExecutor Create(IUnityContainer container, CancellationTokenSource cancellationTokenSource);
    }
}