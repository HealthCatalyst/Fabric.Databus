// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SchemaLoader.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the SchemaLoader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Schema
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.Shared;

    /// <inheritdoc />
    public class SchemaLoader : ISchemaLoader
    {
        /// <summary>
        /// The connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// The top level key column.
        /// </summary>
        private readonly string topLevelKeyColumn;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaLoader"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <param name="topLevelKeyColumn">
        /// The top Level Key Column.
        /// </param>
        public SchemaLoader(string connectionString, string topLevelKeyColumn)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.topLevelKeyColumn = topLevelKeyColumn ?? throw new ArgumentNullException(nameof(topLevelKeyColumn));
        }

        /// <inheritdoc />
        public IList<MappingItem> GetSchemasForLoads(
            IList<IDataSource> workItemLoads)
        {
            var dictionary = new List<MappingItem>();

            foreach (var load in workItemLoads)
            {
                using (var conn = new SqlConnection(this.connectionString))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    //cmd.CommandText = "SELECT TOP 10 * FROM [CatalystDevSubset].[dbo].[Patients]";

                    cmd.CommandText = $";WITH CTE AS ( {load.Sql} )  SELECT top 0 * from CTE;";

                    try
                    {
                        var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

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
                                IsJoinColumn = columnName.Equals(this.topLevelKeyColumn, StringComparison.OrdinalIgnoreCase),
                                SqlColumnType = columnType?.FullName,
                                ElasticSearchType = SqlTypeToElasticSearchTypeConvertor.GetElasticSearchType(columnType),
                                IsCalculated = false,
                            });
                        }

                        //var joinColumnIndex = columnList.FirstOrDefault(c => c.IsJoinColumn).index;

                        // add any calculated fields
                        var calculatedFields = load.Fields.Where(f => f.Destination != null)
                            .Select(f => new ColumnInfo
                            {
                                sourceIndex =
                                    columnList.FirstOrDefault(
                                            c => c.Name.Equals(f.Source, StringComparison.OrdinalIgnoreCase))
                                        ?.index,
                                index = numberOfColumns++,
                                Name = f.Destination,
                                ElasticSearchType = f.DestinationType.ToString(),
                                IsCalculated = true,
                                Transform = f.Transform.ToString()
                            })
                            .ToList();

                        calculatedFields.ForEach(c => columnList.Add(c));


                        dictionary.Add(new MappingItem
                        {
                            SequenceNumber = load.SequenceNumber,
                            PropertyPath = load.Path,
                            PropertyType = load.PropertyType,
                            Columns = columnList,
                        });

                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException($"Error in datasource (Path={load.Path}) with Sql:{cmd.CommandText}", e);
                    }
                }
            }
            return dictionary;
        }

    }
}