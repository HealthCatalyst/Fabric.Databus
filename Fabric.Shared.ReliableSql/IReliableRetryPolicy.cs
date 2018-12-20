// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IReliableRetryPolicy.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IReliableRetryPolicy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableSql
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The ReliableRetryPolicy interface.
    /// </summary>
    public interface IReliableRetryPolicy
    {
        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        void Execute(Action operation);

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <typeparam name="TResult">
        /// type of result
        /// </typeparam>
        /// <returns>
        /// The <see cref="TResult"/>
        /// </returns>
        TResult Execute<TResult>(Func<TResult> operation);

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken);

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <typeparam name="TResult">
        /// type of result
        /// </typeparam>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken);
    }
}
