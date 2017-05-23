using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchSqlFeeder.Interfaces
{
    public interface IQueueItem
    {
        string QueryId { get; set; }

        string PropertyName { get; set; }
    }
}
