// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineStepState.cs" company="">
//   
// </copyright>
// <summary>
//   The pipeline step state.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Pipeline
{
    /// <summary>
    /// The pipeline step state.
    /// </summary>
    public enum PipelineStepState
    {
        /// <summary>
        /// The unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The starting.
        /// </summary>
        Starting = 1,

        /// <summary>
        /// The processing.
        /// </summary>
        Processing = 2,

        /// <summary>
        /// The completed.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// The processed.
        /// </summary>
        Processed = 4,

        /// <summary>
        /// The waiting.
        /// </summary>
        Waiting = 5
    }
}