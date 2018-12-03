// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Class1.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlCalculateFieldsPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The sql calculate fields pipeline step.
    /// </summary>
    public class SqlCalculateFieldsPipelineStep : BasePipelineStep<SqlDataLoadedQueueItem, SqlDataLoadedQueueItem>
    {
        /// <summary>
        /// The databus sql reader.
        /// </summary>
        private readonly IDatabusSqlReader databusSqlReader;

        /// <inheritdoc />
        public SqlCalculateFieldsPipelineStep(
            IJobConfig jobConfig,
            IDatabusSqlReader databusSqlReader,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.databusSqlReader = databusSqlReader;
        }

        /// <inheritdoc />
        protected override string LoggerName => "SqlCalculateFields";

        /// <inheritdoc />
        protected override async Task HandleAsync(SqlDataLoadedQueueItem workItem)
        {
            var rows = await this.databusSqlReader.CalculateFields(null, workItem.Columns, workItem.Rows);

            workItem.Rows = rows;

            await this.AddToOutputQueueAsync(workItem);
        }

        /// <inheritdoc />
        protected override string GetId(SqlDataLoadedQueueItem workItem)
        {
            return workItem.QueryId;
        }
    }
}
