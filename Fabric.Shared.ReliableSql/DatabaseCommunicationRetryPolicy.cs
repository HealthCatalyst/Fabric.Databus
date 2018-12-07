// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabaseCommunicationRetryPolicy.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DatabaseCommunicationRetryPolicy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



namespace Fabric.Shared.ReliableSql
{
    using System;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Polly;

    /// <inheritdoc />
    /// <summary>
    /// The database communication retry policy.
    /// </summary>
    public class DatabaseCommunicationRetryPolicy : IReliableRetryPolicy
    {
        /// <summary>
        /// The retry count.
        /// </summary>
        private const int RetryCount = 3;

        /// <summary>
        /// The wait between retries in milliseconds.
        /// </summary>
        private const int WaitBetweenRetriesInMilliseconds = 1000;

        /// <summary>
        /// The _sql exceptions.
        /// </summary>
        private readonly int[] sqlExceptions =
            {
                -2, /* timeout */ 53, // connection failure
                701, // Out of Memory
                1204, // Lock Issue
                1205, // Deadlock Victim
                1222, // Lock request time out period exceeded.
                7214, // Remote procedure time out of %d seconds exceeded. Remote procedure '%.*ls' is canceled.
                7604, // Full-text operation failed due to a time out.
                7618, // %d is not a valid value for a full-text connection time out.
                8628, // A time out occurred while waiting to optimize the query. Rerun the query.
                8645, // A time out occurred while waiting for memory resources to execute the query. Rerun the query.
                8651 // Low memory condition
            };

        /// <summary>
        /// The _retry policy async.
        /// </summary>
        private readonly Policy retryPolicyAsync;

        /// <summary>
        /// The retry policy.
        /// </summary>
        private readonly Policy retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCommunicationRetryPolicy"/> class.
        /// </summary>
        public DatabaseCommunicationRetryPolicy()
        {
            this.retryPolicyAsync = Policy
                .Handle<SqlException>(exception => this.sqlExceptions.Contains(exception.Number))
                .WaitAndRetryAsync(
                    RetryCount,
                    attempt => TimeSpan.FromMilliseconds(WaitBetweenRetriesInMilliseconds));

            this.retryPolicy = Policy
                .Handle<SqlException>(exception => this.sqlExceptions.Contains(exception.Number))
                .WaitAndRetry(
                    RetryCount,
                    attempt => TimeSpan.FromMilliseconds(WaitBetweenRetriesInMilliseconds));
        }

        /// <inheritdoc />
        public void Execute(Action operation)
        {
            this.retryPolicy.Execute(operation.Invoke);
        }

        /// <inheritdoc />
        public TResult Execute<TResult>(Func<TResult> operation)
        {
            return this.retryPolicy.Execute(operation.Invoke);
        }

        /// <inheritdoc />
        public async Task Execute(Func<Task> operation, CancellationToken cancellationToken)
        {
            await this.retryPolicyAsync.ExecuteAsync(operation.Invoke);
        }

        /// <inheritdoc />
        public async Task<TResult> Execute<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken)
        {
            return await this.retryPolicyAsync.ExecuteAsync(operation.Invoke);
        }
    }
}
