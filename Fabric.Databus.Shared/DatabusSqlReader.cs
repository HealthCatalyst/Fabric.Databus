﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Linq;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Sql;
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
        public async Task<ReadSqlDataResult> ReadDataFromQueryAsync(IDataSource load, string start, string end, ILogger logger, string topLevelKeyColumn)
        {
            if (string.IsNullOrWhiteSpace(load.Sql))
            {
                throw new ArgumentNullException($"Sql property is null for load with path: {load.Path}");
            }

            using (var conn = this.sqlConnectionFactory.GetConnection(this.connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (this.sqlCommandTimeoutInSeconds != 0)
                {
                    cmd.CommandTimeout = this.sqlCommandTimeoutInSeconds;
                }

                if (start == null)
                {
                    cmd.CommandText = this.sqlGeneratorFactory.Create()
                        .AddCTE(load.Sql)
                        .AddOrderByAscending("KeyLevel1")
                        .ToSqlString();
                }
                else
                {
                    cmd.CommandText = this.sqlGeneratorFactory.Create()
                        .AddCTE(load.Sql)
                        .AddOrderByAscending("KeyLevel1")
                        .AddRangeFilter("KeyLevel1", "@start", "@end")
                        .ToSqlString();

                    cmd.AddParameterWithValue("@start", start);
                    cmd.AddParameterWithValue("@end", end);
                }

                logger.Verbose("Sql Begin [{Path}]: {@load} {@cmd}", load.Path, load, cmd);
                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

                /* var schema = reader.GetSchemaTable(); */

                var numberOfColumns = reader.FieldCount;

                var columnList = new List<ColumnInfo>(numberOfColumns);

                for (int columnNumber = 0; columnNumber < numberOfColumns; columnNumber++)
                {
                    var columnName = reader.GetName(columnNumber);

                    var columnType = reader.GetFieldType(columnNumber);
                    columnList.Add(new ColumnInfo
                    {
                        index = columnNumber,
                        Name = columnName,
                        IsJoinColumn = columnName.Equals(topLevelKeyColumn, StringComparison.OrdinalIgnoreCase),
                        ElasticSearchType = SqlTypeToElasticSearchTypeConvertor.GetElasticSearchType(columnType),
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
                                         c => c.Name.Equals(f.Source, StringComparison.OrdinalIgnoreCase))?.index,
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

                return new ReadSqlDataResult { Data = data, ColumnList = columnList };
            }
        }

        /// <summary>
        /// The calculate fields.
        /// </summary>
        /// <param name="load">
        ///     The load.
        /// </param>
        /// <param name="columnList">
        ///     The column list.
        /// </param>
        /// <param name="rows">
        ///     The rows.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
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
        public async Task<IList<string>> GetListOfEntityKeysAsync(string topLevelKeyColumn, int maximumEntitiesToLoad, IDataSource dataSource)
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

                var sqlGenerator = this.sqlGeneratorFactory.Create().AddCTE(load.Sql).AddColumn(null, topLevelKeyColumn, null)
                    .AddOrderByAscending(topLevelKeyColumn);

                if (maximumEntitiesToLoad > 0)
                {
                    sqlGenerator.AddTopFilter(maximumEntitiesToLoad);
                }

                cmd.CommandText = sqlGenerator.ToSqlString();

                // Logger.Verbose($"Start: {cmd.CommandText}");
                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

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
