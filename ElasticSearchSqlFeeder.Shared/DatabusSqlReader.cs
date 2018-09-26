// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusSqlReader.cs" company="">
//   
// </copyright>
// <summary>
//   The databus sql reader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    using NLog;

    using ZipCodeToGeoCodeConverter;

    /// <summary>
    /// The databus sql reader.
    /// </summary>
    public class DatabusSqlReader : IDatabusSqlReader
    {
        /// <summary>
        /// The read data from query.
        /// </summary>
        /// <param name="config">
        /// The config.
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
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <returns>
        /// The <see cref="ReadSqlDataResult"/>ReadSqlDataResult
        /// </returns>
        /// <exception cref="ArgumentNullException">exception thrown
        /// </exception>
        public ReadSqlDataResult ReadDataFromQuery(IQueryConfig config, IDataSource load, string start, string end, ILogger logger)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.ConnectionString == null)
            {
                throw new ArgumentNullException(nameof(config.ConnectionString));
            }

            using (var conn = new SqlConnection(config.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (config.SqlCommandTimeoutInSeconds != 0)
                    cmd.CommandTimeout = config.SqlCommandTimeoutInSeconds;

                if (start == null)
                {
                    cmd.CommandText =
                        $";WITH CTE AS ( {load.Sql} )  SELECT * from CTE ORDER BY {config.TopLevelKeyColumn} ASC;";
                }
                else
                {
                    cmd.CommandText =
                        $";WITH CTE AS ( {load.Sql} )  SELECT * from CTE WHERE {config.TopLevelKeyColumn} BETWEEN '{start}' AND '{end}' ORDER BY {config.TopLevelKeyColumn} ASC;";
                }

                logger.Trace($"Start: {cmd.CommandText}");
                var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

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
                        IsJoinColumn = columnName.Equals(config.TopLevelKeyColumn, StringComparison.OrdinalIgnoreCase),
                        ElasticSearchType = SqlTypeToElasticSearchTypeConvertor.GetElasticSearchType(columnType),
                        IsCalculated = false,
                    });
                }

                var joinColumnIndex = columnList.FirstOrDefault(c => c.IsJoinColumn).index;

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
                                        zipToGeocodeConverter.Convert3DigitZipcodeToGeocode(sourceValueText);
                                }

                                if (calculatedField.Transform == QueryFieldTransform.Zip5ToGeocode.ToString())
                                {
                                    var sourceValueText = sourceValue.ToString();
                                    var convertZipcodeToGeocode =
                                        zipToGeocodeConverter.ConvertZipcodeToGeocode(sourceValueText);
                                    values[calculatedField.index] = convertZipcodeToGeocode;
                                }
                            }
                        }
                    }

                    data[key].Add(values);
                }

                logger.Trace($"Finish: {cmd.CommandText} rows={rows}");

                return new ReadSqlDataResult { Data = data, ColumnList = columnList };
            }
        }
    }
}
