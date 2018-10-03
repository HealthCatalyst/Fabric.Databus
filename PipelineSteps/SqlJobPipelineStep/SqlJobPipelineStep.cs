// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlJobPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SqlJobPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SqlJobPipelineStep
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// Reads a SqlJobQueueItem and creates a set of SqlBatchQueueItems based on EntitiesPerBatch config
    /// </summary>
    public class SqlJobPipelineStep : BasePipelineStep<SqlJobQueueItem, SqlBatchQueueItem>
    {
        /// <summary>
        /// The databus sql reader.
        /// </summary>
        private readonly IDatabusSqlReader databusSqlReader;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:SqlJobPipelineStep.SqlJobPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager">
        /// The queue Manager.
        /// </param>
        /// <param name="progressMonitor">
        /// The progress Monitor.
        /// </param>
        /// <param name="databusSqlReader"></param>
        /// <param name="cancellationToken"></param>
        public SqlJobPipelineStep(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            IDatabusSqlReader databusSqlReader,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.databusSqlReader = databusSqlReader ?? throw new ArgumentNullException(nameof(databusSqlReader));
        }

        /// <inheritdoc />
        protected override string LoggerName => "SqlJob";

        /// <inheritdoc />
        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        /// <exception cref="T:System.NotImplementedException">exception thrown
        /// </exception>
        protected override async Task HandleAsync(SqlJobQueueItem workItem)
        {
            if (this.Config.EntitiesPerBatch <= 0)
            {
                await this.AddToOutputQueueAsync(new SqlBatchQueueItem
                                          {
                                              BatchNumber = 1,
                                              Start = null,
                                              End = null,
                                              Loads = workItem.Job.Data.DataSources,
                                          });
            }
            else
            {
                var ranges = await this.CalculateRangesAsync(workItem.Job);

                int currentBatchNumber = 1;

                foreach (var range in ranges)
                {
                    await this.AddToOutputQueueAsync(
                        new SqlBatchQueueItem
                            {
                                BatchNumber = currentBatchNumber++,
                                Start = range.Item1,
                                End = range.Item2,
                                Loads = workItem.Job.Data.DataSources,
                                PropertyTypes = workItem.Job.Data.DataSources.Where(a => a.Path != null)
                                    .ToDictionary(a => a.Path, a => a.PropertyType)
                            });
                }
            }
        }

        /// <summary>
        /// The get id.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">exception thrown
        /// </exception>
        protected override string GetId(SqlJobQueueItem workItem)
        {
            return "1";
        }

        /// <summary>
        /// The calculate ranges.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private async Task<IEnumerable<Tuple<string, string>>> CalculateRangesAsync(IJob job)
        {
            var list = await this.databusSqlReader.GetListOfEntityKeysAsync(
                this.Config.TopLevelKeyColumn,
                this.Config.MaximumEntitiesToLoad,
                job.Data.DataSources.First(d => d.Path == null));

            var itemsLeft = list.Count;

            var start = 1;

            var ranges = new List<Tuple<string, string>>();

            while (itemsLeft > 0)
            {
                var end = start + (itemsLeft > this.Config.EntitiesPerBatch ? this.Config.EntitiesPerBatch : itemsLeft) - 1;
                ranges.Add(new Tuple<string, string>(list[start - 1], list[end - 1]));
                itemsLeft = list.Count - end;
                start = end + 1;
            }

            return ranges;
        }
    }
}
