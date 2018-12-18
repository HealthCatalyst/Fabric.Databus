// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbConnectionWrapper.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DbConnectionWrapper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Database.Testing.LocalDb
{
    using System;
    using System.Data;
    using System.Data.Common;

    /// <inheritdoc />
    /// <summary>
    /// This class wraps the connection so the connection does not get disposed when the wrapper is used in a using statement
    /// </summary>
    public class DbConnectionWrapper : DbConnection
    {
        /// <summary>
        /// The connection.
        /// </summary>
        private readonly DbConnection connection;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Database.Testing.LocalDb.DbConnectionWrapper" /> class.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        public DbConnectionWrapper(DbConnection connection)
        {
            this.connection = connection;
        }

        /// <inheritdoc />
        public override string ConnectionString { get; set; }

        /// <inheritdoc />
        public override string Database => this.connection.Database;

        /// <inheritdoc />
        public override ConnectionState State => this.connection.State;

        /// <inheritdoc />
        public override string DataSource => this.connection.DataSource;

        /// <inheritdoc />
        public override string ServerVersion => this.connection.ServerVersion;

        /// <inheritdoc />
        public override void Close()
        {
            // don't close connection since we'll do it
        }

        /// <inheritdoc />
        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Open()
        {
            // do nothing since the connection is already open
        }

        /// <inheritdoc />
        protected override DbCommand CreateDbCommand()
        {
            return this.connection.CreateCommand();
        }

        /// <inheritdoc />
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }
    }
}
