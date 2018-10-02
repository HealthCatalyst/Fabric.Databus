// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPipelineStep.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// The PipelineStep interface.
    /// </summary>
    public interface IPipelineStep
    {
        /// <summary>
        /// The monitor work queue.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task MonitorWorkQueueAsync();

        /// <summary>
        /// The mark output queue as completed.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        void MarkOutputQueueAsCompleted(int stepNumber);

        /// <summary>
        /// The initialize with step number.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        void InitializeWithStepNumber(int stepNumber);

        /// <summary>
        /// The create out queue.
        /// </summary>
        /// <param name="stepNumber">
        /// The step number.
        /// </param>
        void CreateOutQueue(int stepNumber);
    }
}