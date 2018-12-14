// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuerySqlPipelineStep.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the QuerySqlPipelineStep type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Shared;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// Reads a SqlQueryDataSourceQueueItem and calls SqlServer to load the data for that query and put it in a SqlDataLoadedQueueItem
    /// </summary>
    public class QuerySqlPipelineStep : BasePipelineStep<SqlQueryDataSourceQueueItem, SqlDataLoadedQueueItem>
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
        private readonly IDetailedTemporaryFileWriter detailedTemporaryFileWriter;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:QuerySqlPipelineStep.QuerySqlPipelineStep" /> class.
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
        /// <param name="detailedTemporaryFileWriter">
        /// file writer
        /// </param>
        /// <param name="cancellationToken">
        /// cancellation token
        /// </param>
        public QuerySqlPipelineStep(
            IJobConfig jobConfig, 
            IDatabusSqlReader databusSqlReader, 
            ILogger logger, 
            IQueueManager queueManager, 
            IProgressMonitor progressMonitor,
            IDetailedTemporaryFileWriter detailedTemporaryFileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.databusSqlReader = databusSqlReader ?? throw new ArgumentNullException(nameof(databusSqlReader));
            this.detailedTemporaryFileWriter = detailedTemporaryFileWriter ?? throw new ArgumentNullException(nameof(detailedTemporaryFileWriter));
            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.Config.LocalSaveFolder != null)
            {
                this.folder = this.detailedTemporaryFileWriter.CombinePath(this.Config.LocalSaveFolder, $"{this.UniqueId}-{this.LoggerName}");
            }
        }

        /// <inheritdoc />
        protected override sealed string LoggerName => "QuerySql";

        /// <inheritdoc />
        protected override async Task HandleAsync(SqlQueryDataSourceQueueItem workItem)
        {
            // throw new ArgumentNullException(nameof(workItem));
            await this.ReadOneQueryFromDatabaseAsync(
                workItem.QueryId,
                workItem.DataSource,
                workItem.Seed,
                workItem.Start,
                workItem.End,
                workItem.BatchNumber,
                workItem.PropertyTypes,
                workItem.TopLevelDataSource);
        }

        /// <inheritdoc />
        protected override string GetId(SqlQueryDataSourceQueueItem workItem)
        {
            return workItem.QueryId;
        }

        /// <summary>
        /// The read one query from database.
        /// </summary>
        /// <param name="queryId">
        ///     The query id.
        /// </param>
        /// <param name="load">
        ///     The load.
        /// </param>
        /// <param name="seed">
        ///     The seed.
        /// </param>
        /// <param name="start">
        ///     The start.
        /// </param>
        /// <param name="end">
        ///     The end.
        /// </param>
        /// <param name="workItemBatchNumber">
        ///     The workItem batch number.
        /// </param>
        /// <param name="propertyTypes">
        ///     The property Types.
        /// </param>
        /// <param name="topLevelDataSource">
        /// top level data source
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
            IDictionary<string, string> propertyTypes,
            ITopLevelDataSource topLevelDataSource)
        {
            try
            {
                await this.InternalReadOneQueryFromDatabase(
                    queryId,
                    load,
                    start,
                    end,
                    workItemBatchNumber,
                    propertyTypes,
                    topLevelDataSource);
            }
            catch (Exception e)
            {
                if (this.folder != null)
                {
                    var path = this.detailedTemporaryFileWriter.CombinePath(this.folder, queryId);
                    this.detailedTemporaryFileWriter.CreateDirectory(path);

                    var filepath = this.detailedTemporaryFileWriter.CombinePath(path, Convert.ToString(workItemBatchNumber) + "-exceptions.txt");

                    await this.detailedTemporaryFileWriter.WriteToFileAsync(filepath, e.ToString());
                }

                throw;
            }
        }

        /// <summary>
        /// The internal read one query from database.
        /// </summary>
        /// <param name="queryId">
        ///     The query id.
        /// </param>
        /// <param name="load">
        ///     The load.
        /// </param>
        /// <param name="start">
        ///     The start.
        /// </param>
        /// <param name="end">
        ///     The end.
        /// </param>
        /// <param name="batchNumber">
        ///     The batch number.
        /// </param>
        /// <param name="propertyTypes">
        ///     The property Types.
        /// </param>
        /// <param name="topLevelDataSource">
        /// top level data source</param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task InternalReadOneQueryFromDatabase(
            string queryId,
            IDataSource load,
            string start,
            string end,
            int batchNumber,
            IDictionary<string, string> propertyTypes,
            ITopLevelDataSource topLevelDataSource)
        {
            var result = await this.databusSqlReader.ReadDataFromQueryAsync(
                             load,
                             start,
                             end,
                             this.MyLogger,
                             topLevelDataSource.Key,
                             topLevelDataSource.IncrementalColumns,
                             topLevelDataSource.TableOrView);

            await this.WriteDiagnosticsAsync(queryId, batchNumber, result);

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
                            PropertyTypes = propertyTypes
                        });
            }

            // now all the source data has been loaded

            // handle fields without any transform
            var untransformedFields = load.Fields.Where(f => f.Transform == QueryFieldTransform.None).ToList();

            untransformedFields.ForEach(f => { });
        }

        /// <summary>
        /// The write diagnostics.
        /// </summary>
        /// <param name="queryId">
        /// The query id.
        /// </param>
        /// <param name="batchNumber">
        /// The batch number.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task WriteDiagnosticsAsync(string queryId, int batchNumber, ReadSqlDataResult result)
        {
            if (this.detailedTemporaryFileWriter?.IsWritingEnabled == true && this.folder != null)
            {
                var path = this.detailedTemporaryFileWriter.CombinePath(
                    this.detailedTemporaryFileWriter.CombinePath(this.folder, queryId),
                    Convert.ToString(batchNumber));

                this.detailedTemporaryFileWriter.CreateDirectory(path);

                if (result.SqlCommandText != null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(result.SqlCommandText);
                    sb.AppendLine();

                    if (result.SqlCommandParameters != null)
                    {
                        foreach (var parameter in result.SqlCommandParameters)
                        {
                            sb.AppendLine($"{parameter.Key} = {parameter.Value}");
                        }
                    }

                    await this.detailedTemporaryFileWriter.WriteToFileAsync(
                        this.detailedTemporaryFileWriter.CombinePath(path, "query.sql"),
                        sb.ToString());
                }

                foreach (var frame in result.Data)
                {
                    var key = frame.Key;

                    var filepath = this.detailedTemporaryFileWriter.CombinePath(
                        path,
                        PathHelpers.GetSafeFilename(Convert.ToString(key)) + ".csv");

                    using (var stream = this.detailedTemporaryFileWriter.OpenStreamForWriting(filepath))
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
        }
    }
}
