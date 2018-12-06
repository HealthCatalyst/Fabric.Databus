// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReliableSqlDbConnection.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ReliableSqlDbConnection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableSql
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    /// <inheritdoc />
    public class ReliableSqlDbConnection : DbConnection
    {
        /// <summary>
        /// The underlying connection.
        /// </summary>
        private readonly SqlConnection underlyingConnection;

        /// <summary>
        /// The retry policy.
        /// </summary>
        private readonly IReliableRetryPolicy retryPolicy;

        /// <summary>
        /// The connection string.
        /// </summary>
        private string connectionString;

        /// <inheritdoc />
        public ReliableSqlDbConnection(
            string connectionString,
            IReliableRetryPolicy retryPolicy)
        {
            this.connectionString = connectionString;
            this.retryPolicy = retryPolicy;
            this.underlyingConnection = new SqlConnection(connectionString);
        }

        /// <inheritdoc />
        public ReliableSqlDbConnection(
            string connectionString)
        : this(connectionString, new DatabaseCommunicationRetryPolicy())
        {
        }

        /// <inheritdoc />
        public override string ConnectionString
        {
            get
            {
                return this.connectionString;
            }

            set
            {
                this.connectionString = value;
                this.underlyingConnection.ConnectionString = value;
            }
        }

        /// <inheritdoc />
        public override string Database => this.underlyingConnection.Database;

        /// <inheritdoc />
        public override string DataSource => this.underlyingConnection.DataSource;

        /// <inheritdoc />
        public override string ServerVersion => this.underlyingConnection.ServerVersion;

        /// <inheritdoc />
        public override ConnectionState State => this.underlyingConnection.State;

        /// <inheritdoc />
        public override void ChangeDatabase(string databaseName)
        {
            this.underlyingConnection.ChangeDatabase(databaseName);
        }

        /// <inheritdoc />
        public override void Close()
        {
            this.underlyingConnection.Close();
        }

        /// <inheritdoc />
        public override void Open()
        {
            this.retryPolicy.Execute(() =>
            {
                if (this.underlyingConnection.State != ConnectionState.Open)
                {
                    this.underlyingConnection.Open();
                }
            });
        }

        /// <inheritdoc />
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return this.underlyingConnection.BeginTransaction(isolationLevel);
        }

        /// <inheritdoc />
        protected override DbCommand CreateDbCommand()
        {
            return new ReliableSqlDbCommand(this.underlyingConnection.CreateCommand(), this.retryPolicy);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.underlyingConnection.State == ConnectionState.Open)
                {
                    this.underlyingConnection.Close();
                }

                this.underlyingConnection.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
