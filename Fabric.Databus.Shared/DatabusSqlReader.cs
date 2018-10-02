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
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces;
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
        /// Initializes a new instance of the <see cref="DatabusSqlReader"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <param name="sqlCommandTimeoutInSeconds">
        /// The sql Command Timeout In Seconds.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// exception thrown
        /// </exception>
        public DatabusSqlReader(string connectionString, int sqlCommandTimeoutInSeconds)
        {
            if (sqlCommandTimeoutInSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sqlCommandTimeoutInSeconds));
            }

            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.sqlCommandTimeoutInSeconds = sqlCommandTimeoutInSeconds;
        }

        /// <inheritdoc />
        public async Task<ReadSqlDataResult> ReadDataFromQuery(IDataSource load, string start, string end, ILogger logger, string topLevelKeyColumn)
        {
            using (var conn = new SqlConnection(this.connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (this.sqlCommandTimeoutInSeconds != 0)
                {
                    cmd.CommandTimeout = this.sqlCommandTimeoutInSeconds;
                }

                if (start == null)
                {
                    cmd.CommandText =
                        $";WITH CTE AS ( {load.Sql} )  SELECT * from CTE ORDER BY {topLevelKeyColumn} ASC;";
                }
                else
                {
                    cmd.CommandText =
                        $";WITH CTE AS ( {load.Sql} )  SELECT * from CTE WHERE {topLevelKeyColumn} BETWEEN '{start}' AND '{end}' ORDER BY {topLevelKeyColumn} ASC;";
                }

                logger.Verbose($"Start: {cmd.CommandText}");
                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

                //var schema = reader.GetSchemaTable();

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

                var joinColumn = columnList.FirstOrDefault(c => c.IsJoinColumn);
                Debug.Assert(joinColumn != null, nameof(joinColumn) + " != null");

                var joinColumnIndex = joinColumn.index;

                // add any calculated fields
                var calculatedFields = load.Fields.Where(f => f.Destination != null)
                    .Select(f => new ColumnInfo
                    {
                        sourceIndex =
                            columnList.FirstOrDefault(c => c.Name.Equals(f.Source, StringComparison.OrdinalIgnoreCase))?.index,
                        index = numberOfColumns++,
                        Name = f.Destination,
                        ElasticSearchType = f.DestinationType.ToString(),
                        IsCalculated = true,
                        Transform = f.Transform.ToString()
                    })
                    .ToList();

                calculatedFields.ForEach(c => columnList.Add(c));

                //EsJsonWriter.WriteMappingToJson(columnList, load.PropertyPath);

                // now write the data
                var data = new Dictionary<string, List<object[]>>();

                var zipToGeocodeConverter = new ZipToGeocodeConverter();

                int rows = 0;

                while (reader.Read())
                {
                    rows++;
                    var values = new object[numberOfColumns];

                    reader.GetValues(values);

                    var key = Convert.ToString(values[joinColumnIndex]);
                    if (!data.ContainsKey(key))
                    {
                        data.Add(key, new List<object[]>());
                    }


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

                    data[key].Add(values);
                }

                logger.Verbose($"Finish: {cmd.CommandText} rows={rows}");

                return new ReadSqlDataResult { Data = data, ColumnList = columnList };
            }
        }

        /// <inheritdoc />
        public async Task<IList<string>> GetListOfEntityKeysAsync(string topLevelKeyColumn, int maximumEntitiesToLoad, IDataSource dataSource)
        {
            var load = dataSource;

            using (var conn = new SqlConnection(this.connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (this.sqlCommandTimeoutInSeconds != 0)
                {
                    cmd.CommandTimeout = this.sqlCommandTimeoutInSeconds;
                }

                cmd.CommandText = maximumEntitiesToLoad > 0
                                      ? $";WITH CTE AS ( {load.Sql} )  SELECT TOP {maximumEntitiesToLoad} {topLevelKeyColumn} from CTE ORDER BY {topLevelKeyColumn} ASC;"
                                      : $";WITH CTE AS ( {load.Sql} )  SELECT {topLevelKeyColumn} from CTE ORDER BY {topLevelKeyColumn} ASC;";

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
