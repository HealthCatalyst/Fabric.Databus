// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndPointQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the EndPointQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueItems
{
    using ElasticSearchSqlFeeder.Interfaces;

    public class EndPointQueueItem : IQueueItem
    {
        public string PropertyName { get; set; }

        public string QueryId { get; set; }
    }
}
