// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockDatabusSqlReader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MockDatabusSqlReader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineStep.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Sql;

    using Serilog;

    /// <summary>
    /// The mock databus sql reader.
    /// </summary>
    public class MockDatabusSqlReader : IDatabusSqlReader
    {
        /// <inheritdoc />
        public Task<ReadSqlDataResult> ReadDataFromQueryAsync(IDataSource load, string start, string end, ILogger logger, string topLevelKeyColumn)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IList<string>> GetListOfEntityKeysAsync(string topLevelKeyColumn, int maximumEntitiesToLoad, IDataSource dataSource)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<List<object[]>> CalculateFields(IDataSource load, List<ColumnInfo> columnList, List<object[]> rows)
        {
            throw new NotImplementedException();
        }
    }
}