using ElasticSearchSqlFeeder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchSqlFeeder.Shared
{
    public class EndPointCollection : IMeteredBlockingCollection<EndPointQueueItem>
    {
        public int Count => 0;

        public bool IsCompleted => true;


        public void Add(EndPointQueueItem item)
        {
        }

        public bool Any()
        {
            return false;
        }

        public void CompleteAdding()
        {

        }

        public EndPointQueueItem Take()
        {
            throw new NotImplementedException();
        }

        public bool TryTake(out EndPointQueueItem cacheItem)
        {
            throw new NotImplementedException();
        }

        public string Name => "EndPoint";
    }

}
