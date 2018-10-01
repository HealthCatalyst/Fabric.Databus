// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueProcessorInfo.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the QueueProcessorInfo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunner
{
    using System;

    /// <summary>
    /// The queue processor info.
    /// </summary>
    public class QueueProcessorInfo
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