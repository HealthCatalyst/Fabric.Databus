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

    using BasePipelineStep;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Shared;

    using QueueItems;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The sql import queue processor.
    /// </summary>
    public class SqlImportPipelineStep : BasePipelineStep<SqlImportQueueItem, ConvertDatabaseToJsonQueueItem>
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
        private readonly IFileWriter fileWriter;

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
            IFileWriter fileWriter,
            CancellationToken cancellationToken)
            : base(jobConfig, logger, queueManager, progressMonitor, cancellationToken)
        {
            this.folder = Path.Combine(this.Config.LocalSaveFolder, $"{this.UniqueId}-SqlImport");

            this.databusSqlReader = databusSqlReader ?? throw new ArgumentNullException(nameof(databusSqlReader));
            this.fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        }

        /// <inheritdoc />
        protected override string LoggerName => "SqlImport";

        /// <inheritdoc />
        protected override void Handle(SqlImportQueueItem workItem)
        {
            this.ReadOneQueryFromDatabase(workItem.QueryId, workItem.DataSource, workItem.Seed, workItem.Start, workItem.End, workItem.BatchNumber, workItem.PropertyTypes);
        }

        /// <inheritdoc />
        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        /// <inheritdoc />
        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            //MarkOutputQueueAsCompleted();
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
        private void ReadOneQueryFromDatabase(
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
                this.InternalReadOneQueryFromDatabase(queryId, load, start, end, workItemBatchNumber, propertyTypes);
            }
            catch (Exception e)
            {
                if (this.Config.WriteDetailedTemporaryFilesToDisk)
                {
                    var path = Path.Combine(this.folder, queryId);
                    this.fileWriter.CreateDirectory(path);

                    var filepath = Path.Combine(path, Convert.ToString(workItemBatchNumber) + "-exceptions.txt");

                    this.fileWriter.WriteToFile(filepath, e.ToString());
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
        private void InternalReadOneQueryFromDatabase(
            string queryId,
            IDataSource load,
            string start,
            string end,
            int batchNumber,
            IDictionary<string, string> propertyTypes)
        {
            var sqlJsonValueWriter = new SqlJsonValueWriter();

            var result = this.databusSqlReader.ReadDataFromQuery(load, start, end, this.MyLogger, this.Config.TopLevelKeyColumn);

            if (this.Config.WriteDetailedTemporaryFilesToDisk)
            {
                var path = Path.Combine(Path.Combine(this.folder, queryId), Convert.ToString(batchNumber));

                Directory.CreateDirectory(path);

                foreach (var frame in result.Data)
                {
                    var key = frame.Key;

                    var filepath = Path.Combine(path, Convert.ToString(key) + ".csv");

                    using (var stream = this.fileWriter.OpenStreamForWriting(filepath))
                    {
                        using (var streamWriter = new StreamWriter(stream))
                        {
                            var columns = result.ColumnList.Select(c => c.Name).ToList();
                            var text = $@"""Key""," + string.Join(",", columns.Select(c => $@"""{c}"""));

                            streamWriter.WriteLine(text);

                            var list = frame.Value.Select(c => string.Join(",", c.Select(c1 => $@"""{c1}"""))).ToList();
                            foreach (var item in list)
                            {
                                streamWriter.WriteLine($@"""{key}""," + item);
                            }
                        }
                    }
                }
            }

            foreach (var frame in result.Data)
            {
                this.AddToOutputQueue(new ConvertDatabaseToJsonQueueItem
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
            var untransformedFields = load.Fields.Where(f => f.Transform == QueryFieldTransform.None)
                .ToList();

            untransformedFields.ForEach(f => { });

            //EsJsonWriter.WriteRawDataToJson(data, columnList, seed, load.PropertyPath, 
            //    new SqlJsonValueWriter(), load.Index, load.EntityType);

            //esJsonWriter.WriteRawObjectsToJson(data, columnList, seed, load.PropertyPath, 
            //    new SqlJsonValueWriter(), load.Index, load.EntityType);

            this.MyLogger.Verbose($"Finished reading rows for {queryId}");
        }
    }
}
