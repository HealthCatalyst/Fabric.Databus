// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateBatchesForEachDataSourcePipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   The sql batch queue processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// Reads a SqlBatchQueueItem with a number of queries and splits it into one SqlQueryDataSourceQueueItem for each query
    /// </summary>
    public class CreateBatchesForEachDataSourcePipelineStep : BasePipelineStep<SqlBatchQueueItem, SqlQueryDataSourceQueueItem>
    {
        /// <summary>
        /// The file writer.
        /// </summary>
        private readonly IDetailedTemporaryFileWriter detailedTemporaryFileWriter;

        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateBatchesForEachDataSourcePipelineStep.CreateBatchesForEachDataSourcePipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The queue context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager"></param>
        /// <param name="progressMonitor"></param>
        /// <param name="detailedTemporaryFileWriter"></param>
        /// <param name="cancellationToken"></param>
        public CreateBatchesForEachDataSourcePipelineStep(
            IJobConfig jobConfig,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            IDetailedTemporaryFileWriter detailedTemporaryFileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.detailedTemporaryFileWriter = detailedTemporaryFileWriter ?? throw new ArgumentNullException(nameof(detailedTemporaryFileWriter));

            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.Config.LocalSaveFolder != null)
            {
                this.folder = this.detailedTemporaryFileWriter.CombinePath(this.Config.LocalSaveFolder, $"{this.UniqueId}-{this.LoggerName}");
            }
        }

        /// <inheritdoc />
        protected override sealed string LoggerName => "CreateDataSourceBatches";

        /// <inheritdoc />
        protected override async Task HandleAsync(SqlBatchQueueItem workItem)
        {
            int seed = 0;

            int i = 0;
            foreach (var dataSource in workItem.Loads)
            {
                var queryName = dataSource.Path ?? "Main";
                i++;
                var queryId = $"{Convert.ToString(i)}-{queryName}";

                await this.AddToOutputQueueAsync(
                    new SqlQueryDataSourceQueueItem
                    {
                        BatchNumber = workItem.BatchNumber,
                        QueryId = queryId,
                        PropertyName = dataSource.Path,
                        Seed = seed,
                        DataSource = dataSource,
                        Start = workItem.Start,
                        End = workItem.End,
                        PropertyTypes = workItem.PropertyTypes,
                        TopLevelDataSource = workItem.TopLevelDataSource
                    });
            }

            await this.WriteDiagnostics(workItem);

            await this.WaitTillOutputQueueIsEmptyAsync(workItem.BatchNumber);
        }

        /// <inheritdoc />
        protected override string GetId(SqlBatchQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The write diagnostics.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task WriteDiagnostics(SqlBatchQueueItem workItem)
        {
            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.folder != null)
            {
                foreach (var workItemLoad in workItem.Loads)
                {
                    var queryName = workItemLoad.Path ?? "Main";
                    var queryId = queryName;

                    var path = this.detailedTemporaryFileWriter.CombinePath(this.folder, queryId);

                    this.detailedTemporaryFileWriter.CreateDirectory(path);

                    var filepath = this.detailedTemporaryFileWriter.CombinePath(
                        path,
                        Convert.ToString(workItem.BatchNumber) + ".xml");

                    await this.detailedTemporaryFileWriter.WriteToFileAsync(
                        filepath,
                        $@"<?xml version=""1.0""?><batch><start>{workItem.Start}</start><end>{workItem.End}</end><sql>{workItemLoad.Sql}</sql></batch>");
                }
            }
        }
    }
}
