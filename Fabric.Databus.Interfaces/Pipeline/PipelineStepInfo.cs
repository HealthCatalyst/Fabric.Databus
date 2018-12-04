// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineStepInfo.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the PipelineStepInfo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Pipeline
{
    using System;

    /// <summary>
    /// The queue processor info.
    /// </summary>
    public class PipelineStepInfo
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        public int Count { get; set; } = 1;
    }
}