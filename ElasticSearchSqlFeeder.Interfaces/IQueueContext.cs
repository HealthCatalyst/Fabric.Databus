// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueueContext.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IQueueContext type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System.Threading;

    /// <summary>
    /// The QueueContext interface.
    /// </summary>
    public interface IQueueContext
    {
        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        IQueryConfig Config { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        CancellationToken CancellationToken { get; set; }
    }
}
