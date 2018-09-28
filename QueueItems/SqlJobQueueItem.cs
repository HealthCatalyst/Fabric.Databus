// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlJobQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlJobQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    /// <summary>
    /// The sql job queue item.
    /// </summary>
    public class SqlJobQueueItem : IQueueItem
    {
        /// <summary>
        /// Gets or sets the query id.
        /// </summary>
        public string QueryId { get; set; }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the job.
        /// </summary>
        public IJob Job { get; set; }
    }
}
