// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlReuseConnectionFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlReuseConnectionFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System.Data.Common;
    using System.Data.SqlClient;

    using Fabric.Databus.Interfaces.Sql;

    /// <inheritdoc />
    /// <summary>
    /// The sql reuse connection factory.
    /// </summary>
    public class SqlReuseConnectionFactory : ISqlConnectionFactory
    {
        /// <summary>
        /// The connection.
        /// </summary>
        private readonly DbConnection connection;

        /// <inheritdoc />
        public SqlReuseConnectionFactory(DbConnection connection)
        {
            this.connection = connection;
        }

        /// <inheritdoc />
        public DbConnection GetConnection(string connectionString)
        {
            return this.connection;
        }
    }
}