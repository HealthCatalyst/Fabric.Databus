namespace PipelineRunner
{
    using System;
    using System.Collections.Generic;

    using ElasticSearchSqlFeeder.Interfaces;

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
            IList<QueueProcessorInfo> processors,
            int timeoutInMilliseconds);
    }
}