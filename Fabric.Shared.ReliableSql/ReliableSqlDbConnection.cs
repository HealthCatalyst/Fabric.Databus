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

        /// <inheritdoc />
        public ReliableSqlDbConnection(
            SqlConnection connection,
            IReliableRetryPolicy retryPolicy)
        {
            this.retryPolicy = retryPolicy;
            this.underlyingConnection = connection;
        }

        /// <inheritdoc />
        public ReliableSqlDbConnection(
            string connectionString,
            IReliableRetryPolicy retryPolicy)
        : this(new SqlConnection(connectionString), retryPolicy)
        {
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
            get => this.underlyingConnection.ConnectionString;

            set => this.underlyingConnection.ConnectionString = value;
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
        }
    }
}
