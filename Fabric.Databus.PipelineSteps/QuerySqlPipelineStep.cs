﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private readonly IDatabusSqlReader databusSqlReader;

        /// <summary>
        /// The file writer.
        /// </summary>
        private readonly IDetailedTemporaryFileWriter detailedTemporaryFileWriter;

        /// <summary>
        /// The query sql logger.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private readonly IQuerySqlLogger querySqlLogger;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:QuerySqlPipelineStep.QuerySqlPipelineStep" /> class.
        /// </summary>
        /// <param name="jobConfig">
        ///     The queue context.
        /// </param>
        /// <param name="databusSqlReader">
        ///     The databus sql reader.
        /// </param>
        /// <param name="logger">
        ///     The logger.
        /// </param>
        /// <param name="queueManager">
        ///     The queue Manager.
        /// </param>
        /// <param name="progressMonitor">
        ///     The progress monitor
        /// </param>
        /// <param name="detailedTemporaryFileWriter">
        ///     file writer
        /// </param>
        /// <param name="querySqlLogger"></param>
        /// <param name="cancellationToken">
        ///     cancellation token
        /// </param>
        /// <param name="pipelineStepState"></param>
        public QuerySqlPipelineStep(
            IJobConfig jobConfig,
            IDatabusSqlReader databusSqlReader,
            ILogger logger,
            IQueueManager queueManager,
            IProgressMonitor progressMonitor,
            IDetailedTemporaryFileWriter detailedTemporaryFileWriter,
            IQuerySqlLogger querySqlLogger,
            CancellationToken cancellationToken,
            PipelineStepState pipelineStepState)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken, pipelineStepState)
        {
            this.databusSqlReader = databusSqlReader ?? throw new ArgumentNullException(nameof(databusSqlReader));
            this.detailedTemporaryFileWriter = detailedTemporaryFileWriter ?? throw new ArgumentNullException(nameof(detailedTemporaryFileWriter));
            this.querySqlLogger = querySqlLogger ?? throw new ArgumentNullException(nameof(querySqlLogger));
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
                workItem.TopLevelDataSource,
                workItem.TotalBatches);
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
        /// <param name="topLevelDataSource">
        /// top level data source
        /// </param>
        /// <param name="totalBatches">
        /// The total Batches.
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
            ITopLevelDataSource topLevelDataSource,
            int totalBatches)
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
                    topLevelDataSource,
                    totalBatches);
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
        /// <param name="topLevelDataSource">
        /// top level data source
        /// </param>
        /// <param name="totalBatches">
        /// The total Batches.
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
            IDictionary<string, string> propertyTypes,
            ITopLevelDataSource topLevelDataSource,
            int totalBatches)
        {
            var stopwatch = new Stopwatch();

            this.querySqlLogger.SqlQueryStarted(
                new QuerySqlLogEvent
                {
                    Name = load.Name,
                    Path = load.Path,
                    BatchNumber = batchNumber,
                    SequenceNumber = load.SequenceNumber,
                    Sql = load.Sql,
                    TableOrView = load.TableOrView,
                });

            stopwatch.Start();
            var result = await this.databusSqlReader.ReadDataFromQueryAsync(
                             load,
                             start,
                             end,
                             this.MyLogger,
                             topLevelDataSource.Key,
                             topLevelDataSource.IncrementalColumns,
                             topLevelDataSource.TableOrView);

            stopwatch.Stop();

            await this.WriteDiagnosticsAsync(queryId, batchNumber, result, stopwatch.Elapsed);

            this.querySqlLogger.SqlQueryCompleted(
                new QuerySqlLogEvent
                {
                    Name = load.Name,
                    Path = load.Path,
                    BatchNumber = batchNumber,
                    SequenceNumber = load.SequenceNumber,
                    Sql = result.SqlCommandText,
                    SqlParameters = result.SqlCommandParameters?.ToList(),
                    TableOrView = load.TableOrView,
                    TimeElapsed = stopwatch.Elapsed,
                    RowCount = result.Data.Count
                });

            foreach (var frame in result.Data)
            {
                await this.AddToOutputQueueAsync(
                    new SqlDataLoadedQueueItem
                    {
                        BatchNumber = batchNumber,
                        TotalBatches = totalBatches,
                        QueryId = queryId,
                        JoinColumnValue = frame.Key,
                        Rows = frame.Value,
                        Columns = result.ColumnList,
                        PropertyName = load.Path,
                        PropertyType = load.PropertyType,
                        PropertyTypes = propertyTypes,
                        TopLevelKeyColumn = topLevelDataSource.Key
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
        /// <param name="elapsed">
        /// The elapsed.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task WriteDiagnosticsAsync(string queryId, int batchNumber, ReadSqlDataResult result, TimeSpan elapsed)
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

                    sb.AppendLine($"Time: {elapsed:c}");

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
