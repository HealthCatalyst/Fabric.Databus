// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPipelineExecutor.cs" company="">
//   
// </copyright>
// <summary>
//   The PipelineExecutor interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunner
{
    using System;
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces;

    /// <summary>
    /// The PipelineExecutor interface.
    /// </summary>
    public interface IPipelineExecutor
    {
        /// <summary>
        /// The run pipeline tasks.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="processors">processors to run</param>
        /// <param name="timeoutInMilliseconds">
        /// The timeout in milliseconds.
        /// </param>
        /// <exception cref="AggregateException">exception thrown
        /// </exception>
        void RunPipelineTasks(
            IQueryConfig config,
            IList<PipelineStepInfo> processors,
            int timeoutInMilliseconds);
    }
}