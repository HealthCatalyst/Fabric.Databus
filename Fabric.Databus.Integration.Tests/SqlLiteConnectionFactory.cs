// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlLiteConnectionFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlLiteConnectionFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System.Data.Common;

    using Fabric.Database.Testing.LocalDb;
    using Fabric.Databus.Interfaces.Sql;

    /// <inheritdoc />
    /// <summary>
    /// The sql lite connection factory.
    /// </summary>
    public class SqlLiteConnectionFactory : ISqlConnectionFactory
    {
        /// <summary>
        /// The connection.
        /// </summary>
        private readonly DbConnectionWrapper connection;

        /// <inheritdoc />
        public SqlLiteConnectionFactory(DbConnectionWrapper connection)
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