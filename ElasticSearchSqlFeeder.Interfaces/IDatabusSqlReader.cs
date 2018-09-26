// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDatabusSqlReader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IDatabusSqlReader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System;

    using NLog;

    /// <summary>
    /// The DatabusSqlReader interface.
    /// </summary>
    public interface IDatabusSqlReader
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
        ReadSqlDataResult ReadDataFromQuery(IQueryConfig config, IDataSource load, string start, string end, ILogger logger);
    }
}