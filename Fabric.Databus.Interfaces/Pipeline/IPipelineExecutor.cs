// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPipelineExecutor.cs" company="">
//   
// </copyright>
// <summary>
//   The PipelineExecutor interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Pipeline
{
    using System;
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Config;

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
        /// <param name="pipelineSteps">pipelineSteps to run</param>
        /// <param name="timeoutInMilliseconds">
        /// The timeout in milliseconds.
        /// </param>
        /// <exception cref="AggregateException">exception thrown
        /// </exception>
        void RunPipelineTasks(
            IQueryConfig config,
            IList<PipelineStepInfo> pipelineSteps,
            int timeoutInMilliseconds);
    }
}