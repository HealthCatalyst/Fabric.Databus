// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlImportPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SqlImportPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SqlImportPipelineStep
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.Shared;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// Reads a SqlImportQueueItem and calls SqlServer to load the data for that query and put it in a SqlDataLoadedQueueItem
    /// </summary>
    public class SqlImportPipelineStep : BasePipelineStep<SqlImportQueueItem, SqlDataLoadedQueueItem>
    {
        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <summary>
        /// The databus sql reader.
        /// </summary>
        private readonly IDatabusSqlReader databusSqlReader;

        /// <summary>
        /// The file writer.
        /// </summary>
        private readonly IDetailedTemporaryFileWriter fileWriter;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:SqlImportPipelineStep.SqlImportPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        /// The queue context.
        /// </param>
        /// <param name="databusSqlReader">
        /// The databus sql reader.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="queueManager">
        /// The queue Manager.
        /// </param>
        /// <param name="progressMonitor">
        /// The progress monitor
        /// </param>
        /// <param name="fileWriter">
        /// file writer
        /// </param>
        /// <param name="cancellationToken">
        /// cancellation token
        /// </param>
        public SqlImportPipelineStep(
            IJobConfig jobConfig, 
            IDatabusSqlReader databusSqlReader, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            IDetailedTemporaryFileWriter fileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            if (this.fileWriter?.IsWritingEnabled == true && this.Config.LocalSaveFolder != null)
            {
                this.folder = this.fileWriter.CombinePath(this.Config.LocalSaveFolder, $"{this.UniqueId}-SqlImport");
            }

            this.databusSqlReader = databusSqlReader ?? throw new ArgumentNullException(nameof(databusSqlReader));
            this.fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        }

        /// <inheritdoc />
        protected override string LoggerName => "SqlImport";

        /// <inheritdoc />
        protected override async Task HandleAsync(SqlImportQueueItem workItem)
        {
            // throw new ArgumentNullException(nameof(workItem));
            await this.ReadOneQueryFromDatabaseAsync(
                workItem.QueryId,
                workItem.DataSource,
                workItem.Seed,
                workItem.Start,
                workItem.End,
                workItem.BatchNumber,
                workItem.PropertyTypes);
        }

        /// <inheritdoc />
        protected override string GetId(SqlImportQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The read one query from database.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="load">
        /// The load.
        /// </param>
        /// <param name="seed">
        /// The seed.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="workItemBatchNumber">
        /// The workItem batch number.
        /// </param>
        /// <param name="propertyTypes">
        /// The property Types.
        /// </param>
        /// <exception cref="Exception">
        /// exception thrown
        /// </exception>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ReadOneQueryFromDatabaseAsync(
            string queryId,
            IDataSource load,
            int seed,
            string start,
            string end,
            int workItemBatchNumber,
            IDictionary<string, string> propertyTypes)
        {
            try
            {
                await this.InternalReadOneQueryFromDatabase(queryId, load, start, end, workItemBatchNumber, propertyTypes);
            }
            catch (Exception e)
            {
                if (this.folder != null)
                {
                    var path = this.fileWriter.CombinePath(this.folder, queryId);
                    this.fileWriter.CreateDirectory(path);

                    var filepath = this.fileWriter.CombinePath(path, Convert.ToString(workItemBatchNumber) + "-exceptions.txt");

                    await this.fileWriter.WriteToFileAsync(filepath, e.ToString());
                }

                throw;
            }
        }

        /// <summary>
        /// The internal read one query from database.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="load">
        /// The load.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="batchNumber">
        /// The batch number.
        /// </param>
        /// <param name="propertyTypes">
        /// The property Types.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task InternalReadOneQueryFromDatabase(
            string queryId,
            IDataSource load,
            string start,
            string end,
            int batchNumber,
            IDictionary<string, string> propertyTypes)
        {
            var sqlJsonValueWriter = new SqlJsonValueWriter();

            var result = await this.databusSqlReader.ReadDataFromQueryAsync(
                load,
                start,
                end,
                this.MyLogger,
                this.Config.TopLevelKeyColumn);

            if (this.fileWriter?.IsWritingEnabled == true && this.folder != null)
            {
                var path = this.fileWriter.CombinePath(this.fileWriter.CombinePath(this.folder, queryId), Convert.ToString(batchNumber));

                this.fileWriter.CreateDirectory(path);

                foreach (var frame in result.Data)
                {
                    var key = frame.Key;

                    var filepath = this.fileWriter.CombinePath(path, this.GetSafeFilename(Convert.ToString(key)) + ".csv");

                    using (var stream = this.fileWriter.OpenStreamForWriting(filepath))
                    {
                        using (var streamWriter = new StreamWriter(stream))
                        {
                            var columns = result.ColumnList.Select(c => c.Name).ToList();
                            var text = $@"""Key""," + string.Join(",", columns.Select(c => $@"""{c}"""));

                            await streamWriter.WriteLineAsync(text);

                            var list = frame.Value.Select(c => string.Join(",", c.Select(c1 => $@"""{c1}"""))).ToList();
                            foreach (var item in list)
                            {
                                await streamWriter.WriteLineAsync($@"""{key}""," + item);
                            }
                        }
                    }
                }
            }

            foreach (var frame in result.Data)
            {
                await this.AddToOutputQueueAsync(
                    new SqlDataLoadedQueueItem
                        {
                            BatchNumber = batchNumber,
                            QueryId = queryId,
                            JoinColumnValue = frame.Key,
                            Rows = frame.Value,
                            Columns = result.ColumnList,
                            PropertyName = load.Path,
                            PropertyType = load.PropertyType,
                            JsonValueWriter = sqlJsonValueWriter,
                            PropertyTypes = propertyTypes
                        });
            }

            // now all the source data has been loaded

            // handle fields without any transform
            var untransformedFields = load.Fields.Where(f => f.Transform == QueryFieldTransform.None).ToList();

            untransformedFields.ForEach(f => { });

            this.MyLogger.Verbose($"Finished reading rows for {queryId}");
        }

        /// <summary>
        /// The get safe filename.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
