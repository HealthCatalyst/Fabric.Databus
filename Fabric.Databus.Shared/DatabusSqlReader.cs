// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusSqlReader.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   The databus sql reader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Exceptions;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.Shared.Sql;
    using Fabric.Databus.ZipCodeToGeoCode;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The databus sql reader.
    /// </summary>
    public class DatabusSqlReader : IDatabusSqlReader
    {
        /// <summary>
        /// The connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// The sql command timeout in seconds.
        /// </summary>
        private readonly int sqlCommandTimeoutInSeconds;

        /// <summary>
        /// The sql connection factory.
        /// </summary>
        private readonly ISqlConnectionFactory sqlConnectionFactory;

        /// <summary>
        /// The sql generator factory.
        /// </summary>
        private readonly ISqlGeneratorFactory sqlGeneratorFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabusSqlReader"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <param name="sqlCommandTimeoutInSeconds">
        /// The sql Command Timeout In Seconds.
        /// </param>
        /// <param name="sqlConnectionFactory">
        /// The sql Connection Factory.
        /// </param>
        /// <param name="sqlGeneratorFactory">
        /// The sql Generator Factory.
        /// </param>
        public DatabusSqlReader(string connectionString, int sqlCommandTimeoutInSeconds, ISqlConnectionFactory sqlConnectionFactory, ISqlGeneratorFactory sqlGeneratorFactory)
        {
            if (sqlCommandTimeoutInSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sqlCommandTimeoutInSeconds));
            }

            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.sqlCommandTimeoutInSeconds = sqlCommandTimeoutInSeconds;
            this.sqlConnectionFactory = sqlConnectionFactory;
            this.sqlGeneratorFactory = sqlGeneratorFactory;
        }

        /// <inheritdoc />
        public async Task<ReadSqlDataResult> ReadDataFromQueryAsync(
            IDataSource load,
            string start,
            string end,
            ILogger logger,
            string topLevelKeyColumn,
            IEnumerable<IIncrementalColumn> incrementalColumns,
            string topLevelTableName)
        {
            using (var conn = this.sqlConnectionFactory.GetConnection(this.connectionString))
            {
                conn.Open();

                using (var cmd = this.CreateSqlCommand(
                    load,
                    start,
                    end,
                    topLevelKeyColumn,
                    incrementalColumns,
                    conn,
                    topLevelTableName))
                {
                    try
                    {
                        logger.Verbose("Sql Begin [{Path}]: {@load} {@cmd}", load.Path, load, cmd);
                        using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                        {
                            /* var schema = reader.GetSchemaTable(); */

                            var numberOfColumns = reader.FieldCount;

                            var columnList = new List<ColumnInfo>(numberOfColumns);

                            for (int columnNumber = 0; columnNumber < numberOfColumns; columnNumber++)
                            {
                                var columnName = reader.GetName(columnNumber);

                                var columnType = reader.GetFieldType(columnNumber);
                                columnList.Add(
                                    new ColumnInfo
                                    {
                                        index = columnNumber,
                                        Name = columnName,
                                        IsJoinColumn =
                                                columnName.Equals(
                                                    topLevelKeyColumn,
                                                    StringComparison.OrdinalIgnoreCase),
                                        ElasticSearchType =
                                                SqlTypeToElasticSearchTypeConvertor.GetElasticSearchType(columnType),
                                        IsCalculated = false,
                                    });
                            }

                            var joinColumnIndex = 0;

                            // add any calculated fields
                            var calculatedFields = load.Fields.Where(f => f.Destination != null).Select(
                                f => new ColumnInfo
                                {
                                    sourceIndex =
                                                 columnList.FirstOrDefault(
                                                         c => c.Name.Equals(
                                                             f.Source,
                                                             StringComparison.OrdinalIgnoreCase))
                                                     ?.index,
                                    index = numberOfColumns++,
                                    Name = f.Destination,
                                    ElasticSearchType = f.DestinationType.ToString(),
                                    IsCalculated = true,
                                    Transform = f.Transform.ToString()
                                }).ToList();

                            calculatedFields.ForEach(c => columnList.Add(c));

                            // now write the data
                            var data = new Dictionary<string, List<object[]>>();

                            int rows = 0;

                            while (reader.Read())
                            {
                                rows++;
                                var values = new object[numberOfColumns];

                                reader.GetValues(values);

                                var key = Convert.ToString(values[joinColumnIndex]);
                                if (!data.ContainsKey(key))
                                {
                                    data.Add(key, new List<object[]> { values });
                                }
                                else
                                {
                                    data[key].Add(values);
                                }
                            }

                            logger.Verbose("Sql Finish [{Path}] ({rows}): {@load} {@cmd}", load.Path, rows, load, cmd);

                            var parameters = new Dictionary<string, object>();
                            foreach (DbParameter parameter in cmd.Parameters)
                            {
                                parameters.Add(parameter.ParameterName, parameter.Value);
                            }

                            return new ReadSqlDataResult
                            {
                                Data = data,
                                ColumnList = columnList,
                                SqlCommandText = cmd.CommandText,
                                SqlCommandParameters = parameters
                            };
                        }
                    }
                    catch (Exception e)
                    {
                        throw new DatabusSqlException(cmd.CommandText, e);
                    }
                }
            }
        }

        /// <inheritdoc />
        public DbCommand CreateSqlCommand(
            IDataSource load,
            string start,
            string end,
            string topLevelKeyColumn,
            IEnumerable<IIncrementalColumn> incrementalColumns,
            IDbConnection conn,
            string topLevelTableName)
        {
            var cmd = conn.CreateCommand();

            if (this.sqlCommandTimeoutInSeconds != 0)
            {
                cmd.CommandTimeout = this.sqlCommandTimeoutInSeconds;
            }

            var sqlGenerator = this.sqlGeneratorFactory.Create().AddOrderByAscending(topLevelTableName, topLevelKeyColumn);

            if (!string.IsNullOrWhiteSpace(load.Sql))
            {
                sqlGenerator.AddCTE(load.Sql)
                    // ReSharper disable once PossibleMultipleEnumeration
                    .AddIncrementalColumns(topLevelTableName, incrementalColumns);
            }
            else
            {
                sqlGenerator.SetEntity(load.TableOrView).CreateSqlStatement(
                    load.TableOrView,
                    topLevelKeyColumn,
                    load.Relationships,
                    load.SqlEntityColumnMappings,
                    // ReSharper disable once PossibleMultipleEnumeration
                    incrementalColumns);
            }

            if (start != null)
            {
                sqlGenerator.AddRangeFilter(topLevelTableName, topLevelKeyColumn, "@start", "@end");

                cmd.AddParameterWithValue("@start", start);
                cmd.AddParameterWithValue("@end", end);
            }

            int i = 0;
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var incrementalColumn in incrementalColumns)
            {
                cmd.AddParameterWithValueAndType($"@incrementColumnValue{++i}", DbType.String, incrementalColumn.Value);
            }

            cmd.CommandText = sqlGenerator.ToSqlString();
            return cmd as DbCommand;
        }

        /// <inheritdoc />
        public async Task<List<object[]>> CalculateFields(
            IDataSource load,
            List<ColumnInfo> columnList,
            List<object[]> rows)
        {
            var zipToGeocodeConverter = new ZipToGeocodeConverter();

            var calculatedFields = columnList.Where(c => c.IsCalculated).ToList();

            foreach (var row in rows)
            {
                var values = row;
                foreach (var calculatedField in calculatedFields)
                {
                    if (calculatedField.Transform != null && calculatedField.sourceIndex != null)
                    {
                        var sourceValue = values[calculatedField.sourceIndex.Value];
                        if (sourceValue != null)
                        {
                            if (calculatedField.Transform == QueryFieldTransform.Zip3ToGeocode.ToString())
                            {
                                var sourceValueText = sourceValue.ToString();
                                values[calculatedField.index] =
                                    await zipToGeocodeConverter.Convert3DigitZipcodeToGeocodeAsync(sourceValueText);
                            }

                            if (calculatedField.Transform == QueryFieldTransform.Zip5ToGeocode.ToString())
                            {
                                var sourceValueText = sourceValue.ToString();
                                var convertZipcodeToGeocode =
                                    await zipToGeocodeConverter.ConvertZipcodeToGeocodeAsync(sourceValueText);
                                values[calculatedField.index] = convertZipcodeToGeocode;
                            }
                        }
                    }
                }
            }

            return rows;
        }

        /// <inheritdoc />
        public async Task<IList<string>> GetListOfEntityKeysAsync(
            string topLevelKeyColumn,
            int maximumEntitiesToLoad,
            ITopLevelDataSource dataSource)
        {
            var load = dataSource;

            using (var conn = this.sqlConnectionFactory.GetConnection(this.connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (this.sqlCommandTimeoutInSeconds != 0)
                {
                    cmd.CommandTimeout = this.sqlCommandTimeoutInSeconds;
                }

                cmd.CommandText = this.GetQueryForEntityKeys(topLevelKeyColumn, maximumEntitiesToLoad, load);

                int i = 0;
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var incrementalColumn in dataSource.IncrementalColumns)
                {
                    cmd.AddParameterWithValue($"@incrementColumnValue{++i}", incrementalColumn.Value);
                }

                // Logger.Verbose($"Start: {cmd.CommandText}");
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                {
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

        /// <summary>
        /// The get query for entity keys.
        /// </summary>
        /// <param name="topLevelKeyColumn">
        /// The top level key column.
        /// </param>
        /// <param name="maximumEntitiesToLoad">
        /// The maximum entities to load.
        /// </param>
        /// <param name="load">
        /// The load.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetQueryForEntityKeys(
            string topLevelKeyColumn,
            int maximumEntitiesToLoad,
            ITopLevelDataSource load)
        {
            var sqlGenerator = this.sqlGeneratorFactory.Create();

            if (!string.IsNullOrWhiteSpace(load.Sql))
            {
                sqlGenerator.AddCTE(load.Sql)
                    .AddColumn(null, topLevelKeyColumn, null);
            }
            else
            {
                sqlGenerator.CreateSqlStatement(
                    load.TableOrView,
                    topLevelKeyColumn,
                    load.Relationships,
                    load.SqlEntityColumnMappings,
                    load.IncrementalColumns);
            }

            sqlGenerator.AddOrderByAscending(load.TableOrView, topLevelKeyColumn);

            if (maximumEntitiesToLoad > 0)
            {
                sqlGenerator.AddTopFilter(maximumEntitiesToLoad);
            }

            return sqlGenerator.ToSqlString();
        }
    }
}
