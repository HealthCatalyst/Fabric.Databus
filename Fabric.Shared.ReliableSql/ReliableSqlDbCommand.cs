// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReliableSqlDbCommand.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ReliableSqlDbCommand type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableSql
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <summary>
    /// The reliable sql db command.
    /// </summary>
    public class ReliableSqlDbCommand : DbCommand
    {
        /// <summary>
        /// The underlying sql command.
        /// </summary>
        private readonly SqlCommand underlyingSqlCommand;

        /// <summary>
        /// The retry policy.
        /// </summary>
        private readonly IReliableRetryPolicy retryPolicy;

        /// <inheritdoc />
        public ReliableSqlDbCommand(SqlCommand command, IReliableRetryPolicy retryPolicy)
        {
            this.underlyingSqlCommand = command;
            this.retryPolicy = retryPolicy;
        }

        /// <inheritdoc />
        public override string CommandText
        {
            get => this.underlyingSqlCommand.CommandText;
            set => this.underlyingSqlCommand.CommandText = value;
        }

        /// <inheritdoc />
        public override int CommandTimeout
        {
            get => this.underlyingSqlCommand.CommandTimeout;
            set => this.underlyingSqlCommand.CommandTimeout = value;
        }

        /// <inheritdoc />
        public override CommandType CommandType
        {
            get => this.underlyingSqlCommand.CommandType;
            set => this.underlyingSqlCommand.CommandType = value;
        }

        /// <inheritdoc />
        public override bool DesignTimeVisible
        {
            get => this.underlyingSqlCommand.DesignTimeVisible;
            set => this.underlyingSqlCommand.DesignTimeVisible = value;
        }

        /// <inheritdoc />
        public override UpdateRowSource UpdatedRowSource
        {
            get => this.underlyingSqlCommand.UpdatedRowSource;
            set => this.underlyingSqlCommand.UpdatedRowSource = value;
        }

        /// <inheritdoc />
        protected override DbConnection DbConnection
        {
            get => this.underlyingSqlCommand.Connection;
            set => this.underlyingSqlCommand.Connection = (SqlConnection)value;
        }

        /// <inheritdoc />
        protected override DbParameterCollection DbParameterCollection => this.underlyingSqlCommand.Parameters;

        /// <inheritdoc />
        protected override DbTransaction DbTransaction
        {
            get => this.underlyingSqlCommand.Transaction;
            set => this.underlyingSqlCommand.Transaction = (SqlTransaction)value;
        }

        /// <inheritdoc />
        public override void Cancel()
        {
            this.underlyingSqlCommand.Cancel();
        }

        /// <inheritdoc />
        public override int ExecuteNonQuery()
        {
            return this.retryPolicy.Execute(() => this.underlyingSqlCommand.ExecuteNonQuery());
        }

        /// <inheritdoc />
        public override object ExecuteScalar()
        {
            return this.retryPolicy.Execute(() => this.underlyingSqlCommand.ExecuteScalar());
        }

        /// <inheritdoc />
        public override void Prepare()
        {
            this.retryPolicy.Execute(() => this.underlyingSqlCommand.Prepare());
        }

        /// <inheritdoc />
        protected override DbParameter CreateDbParameter()
        {
            return this.underlyingSqlCommand.CreateParameter();
        }

        /// <inheritdoc />
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return this.retryPolicy.Execute(() => this.underlyingSqlCommand.ExecuteReader(behavior));
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            return await this.retryPolicy.ExecuteAsync(async () =>
            {
                if (this.DbConnection.State != ConnectionState.Open)
                {
                    await this.DbConnection.OpenAsync(cancellationToken);
                }
                return await this.underlyingSqlCommand.ExecuteReaderAsync(behavior, cancellationToken);
            }, cancellationToken);
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            return this.retryPolicy.ExecuteAsync(() => this.underlyingSqlCommand.ExecuteNonQueryAsync(cancellationToken), cancellationToken);
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            return this.retryPolicy.ExecuteAsync(() => this.underlyingSqlCommand.ExecuteScalarAsync(cancellationToken), cancellationToken);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.underlyingSqlCommand.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
