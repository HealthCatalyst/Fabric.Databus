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
    /// <summary>
    /// The QueueContext interface.
    /// </summary>
    public interface IQueueContext
    {
        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        IQueryConfig Config { get; set; }
    }
}
