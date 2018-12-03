// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateBatchesPipelineJob.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the CreateBatchesPipelineJob type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// Reads a SqlJobQueueItem and creates a set of SqlBatchQueueItems based on EntitiesPerBatch config
    /// </summary>
    public class CreateBatchesPipelineJob : BasePipelineStep<SqlJobQueueItem, SqlBatchQueueItem>
    {
        /// <summary>
        /// The databus sql reader.
        /// </summary>
        private readonly IDatabusSqlReader databusSqlReader;

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
        /// Initializes a new instance of the <see cref="T:CreateBatchesPipelineJob.CreateBatchesPipelineJob" /> class.
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
        /// <param name="detailedTemporaryFileWriter"></param>
        /// <param name="cancellationToken"></param>
        public CreateBatchesPipelineJob(
            IJobConfig jobConfig, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            IDatabusSqlReader databusSqlReader,
            IDetailedTemporaryFileWriter detailedTemporaryFileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.databusSqlReader = databusSqlReader ?? throw new ArgumentNullException(nameof(databusSqlReader));
            this.detailedTemporaryFileWriter = detailedTemporaryFileWriter;
            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.Config.LocalSaveFolder != null)
            {
                this.folder = this.detailedTemporaryFileWriter.CombinePath(this.Config.LocalSaveFolder, $"{this.UniqueId}-{this.LoggerName}");
            }
        }

        /// <inheritdoc />
        protected override sealed string LoggerName => "CreateBatches";

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

                await this.WriteDiagnosticsWithNoBatches();
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
                                PropertyTypes = workItem.Job.Data.DataSources
                                    .Where(a => a.Path != null)
                                    .GroupBy(a => a.Path)
                                    .Select(g => g.First())
                                    .ToDictionary(a => a.Path, a => a.PropertyType)
                            });

                    await this.WriteDiagnostics(currentBatchNumber, range);
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
        /// The <see cref="IEnumerable{T}"/>.
        /// </returns>
        private async Task<IEnumerable<Tuple<string, string>>> CalculateRangesAsync(IJob job)
        {
            var list = await this.databusSqlReader.GetListOfEntityKeysAsync(
                this.Config.TopLevelKeyColumn,
                this.Config.MaximumEntitiesToLoad,
                job.Data.DataSources.First(d => (d.Path == null || d.Path == "$")));

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

        /// <summary>
        /// The write diagnostics with no batches.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task WriteDiagnosticsWithNoBatches()
        {
            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.folder != null)
            {
                this.detailedTemporaryFileWriter.CreateDirectory(this.folder);

                await this.detailedTemporaryFileWriter.WriteToFileAsync(
                    this.detailedTemporaryFileWriter.CombinePath(this.folder, "1.txt"),
                    "start=null, end=null");
            }
        }

        /// <summary>
        /// The write diagnostics.
        /// </summary>
        /// <param name="currentBatchNumber">
        /// The current batch number.
        /// </param>
        /// <param name="range">
        /// The range.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task WriteDiagnostics(int currentBatchNumber, Tuple<string, string> range)
        {
            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.folder != null)
            {
                this.detailedTemporaryFileWriter.CreateDirectory(this.folder);

                await this.detailedTemporaryFileWriter.WriteToFileAsync(
                    this.detailedTemporaryFileWriter.CombinePath(this.folder, $"{currentBatchNumber}.txt"),
                    $"start={range.Item1}, end={range.Item2}");
            }
        }
    }
}
