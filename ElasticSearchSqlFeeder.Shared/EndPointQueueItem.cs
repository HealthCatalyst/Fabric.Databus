using ElasticSearchSqlFeeder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchSqlFeeder.Shared
{
    public class EndPointQueueItem : IQueueItem
    {
        public string PropertyName { get; set; }

        public string QueryId { get; set; }
    }
}
