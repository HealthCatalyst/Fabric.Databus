// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlJobQueueProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlJobQueueProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SqlJobQueueProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    using QueueItems;

    using Serilog;

    /// <summary>
    /// The sql job queue processor.
    /// </summary>
    public class SqlJobQueueProcessor : BaseQueueProcessor<SqlJobQueueItem, SqlBatchQueueItem>
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:SqlJobQueueProcessor.SqlJobQueueProcessor" /> class.
        /// </summary>
        /// <param name="queueContext">
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
        /// <param name="cancellationToken"></param>
        public SqlJobQueueProcessor(
            IQueueContext queueContext, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken)
            : base(queueContext, logger, queueManager, progressMonitor, cancellationToken)
        {
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
        protected override void Handle(SqlJobQueueItem workItem)
        {
            if (this.QueueContext.Config.EntitiesPerBatch <= 0)
            {
                this.AddToOutputQueue(new SqlBatchQueueItem
                                      {
                                          BatchNumber = 1,
                                          Start = null,
                                          End = null,
                                          Loads = workItem.Job.Data.DataSources,
                                      });
            }
            else
            {
                var ranges = CalculateRanges(this.QueueContext.Config, workItem.Job);

                int currentBatchNumber = 1;

                foreach (var range in ranges)
                {
                    this.AddToOutputQueue(
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
        /// The begin.
        /// </summary>
        /// <param name="isFirstThreadForThisTask">
        /// The is first thread for this task.
        /// </param>
        /// <exception cref="NotImplementedException">exception thrown
        /// </exception>
        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        /// <summary>
        /// The complete.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="isLastThreadForThisTask">
        /// The is last thread for this task.
        /// </param>
        /// <exception cref="NotImplementedException">exception thrown
        /// </exception>
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
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
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private static IEnumerable<Tuple<string, string>> CalculateRanges(IQueryConfig config, IJob job)
        {
            var list = GetListOfEntityKeys(config, job);

            var itemsLeft = list.Count;

            var start = 1;

            var ranges = new List<Tuple<string, string>>();

            while (itemsLeft > 0)
            {
                var end = start + (itemsLeft > config.EntitiesPerBatch ? config.EntitiesPerBatch : itemsLeft) - 1;
                ranges.Add(new Tuple<string, string>(list[start - 1], list[end - 1]));
                itemsLeft = list.Count - end;
                start = end + 1;
            }

            return ranges;
        }

        /// <summary>
        /// The get list of entity keys.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private static List<string> GetListOfEntityKeys(IQueryConfig config, IJob job)
        {
            var load = job.Data.DataSources.First(c => c.Path == null);

            using (var conn = new SqlConnection(config.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (job.Config.SqlCommandTimeoutInSeconds != 0)
                {
                    cmd.CommandTimeout = job.Config.SqlCommandTimeoutInSeconds;
                }

                cmd.CommandText = config.MaximumEntitiesToLoad > 0
                                      ? $";WITH CTE AS ( {load.Sql} )  SELECT TOP {config.MaximumEntitiesToLoad} {config.TopLevelKeyColumn} from CTE ORDER BY {config.TopLevelKeyColumn} ASC;"
                                      : $";WITH CTE AS ( {load.Sql} )  SELECT {config.TopLevelKeyColumn} from CTE ORDER BY {config.TopLevelKeyColumn} ASC;";

                // Logger.Verbose($"Start: {cmd.CommandText}");
                var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

                var list = new List<string>();

                while (reader.Read())
                {
                    var obj = reader.GetValue(0);
                    list.Add(Convert.ToString(obj));
                }

                // Logger.Verbose($"Finish: {cmd.CommandText}");
                return list;
            }
        }
    }
}
