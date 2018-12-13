// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDatabusSqlReader.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IDatabusSqlReader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;

    using Serilog;

    /// <summary>
    /// The DatabusSqlReader interface.
    /// </summary>
    public interface IDatabusSqlReader
    {
        /// <summary>
        /// The read data from query.
        /// </summary>
        /// <param name="load">
        ///     The load.
        /// </param>
        /// <param name="start">
        ///     The start.
        /// </param>
        /// <param name="end">
        ///     The end.
        /// </param>
        /// <param name="logger">
        ///     The logger.
        /// </param>
        /// <param name="topLevelKeyColumn">
        ///     The top Level Key Column.
        /// </param>
        /// <param name="incrementalColumns">
        /// incremental columns
        /// </param>
        /// <returns>
        /// The <see cref="ReadSqlDataResult"/>ReadSqlDataResult
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// exception thrown
        /// </exception>
        Task<ReadSqlDataResult> ReadDataFromQueryAsync(
            IDataSource load,
            string start,
            string end,
            ILogger logger,
            string topLevelKeyColumn,
            IEnumerable<IIncrementalColumn> incrementalColumns);

        /// <summary>
        /// The get list of entity keys.
        /// </summary>
        /// <param name="topLevelKeyColumn">
        ///     The top level key column.
        /// </param>
        /// <param name="maximumEntitiesToLoad">
        ///     The maximum entities to load.
        /// </param>
        /// <param name="dataSource">
        ///     The data source.
        /// </param>
        /// <returns>
        /// The <see cref="IList{T}"/>.
        /// </returns>
        Task<IList<string>> GetListOfEntityKeysAsync(
            string topLevelKeyColumn,
            int maximumEntitiesToLoad,
            ITopLevelDataSource dataSource);

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
        Task<List<object[]>> CalculateFields(IDataSource load, List<ColumnInfo> columnList, List<object[]> rows);

        /// <summary>
        /// The create sql command.
        /// </summary>
        /// <param name="load">
        /// The load.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="topLevelKeyColumn">
        /// The top level key column.
        /// </param>
        /// <param name="incrementalColumns">
        /// The incremental columns.
        /// </param>
        /// <param name="conn">
        /// The conn.
        /// </param>
        /// <returns>
        /// The <see cref="DbCommand"/>.
        /// </returns>
        DbCommand CreateSqlCommand(
            IDataSource load,
            string start,
            string end,
            string topLevelKeyColumn,
            IEnumerable<IIncrementalColumn> incrementalColumns,
            IDbConnection conn);
    }
}