// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlLiteConnectionWrapper.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlLiteConnectionWrapper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System;
    using System.Data;
    using System.Data.Common;

    /// <inheritdoc />
    /// <summary>
    /// The sql lite connection wrapper.
    /// </summary>
    public class SqlLiteConnectionWrapper : DbConnection
    {
        /// <summary>
        /// The connection.
        /// </summary>
        private readonly DbConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlLiteConnectionWrapper"/> class.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        public SqlLiteConnectionWrapper(DbConnection connection)
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