using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchSqlFeeder.Interfaces
{
    public interface IQueueManager
    {
        //void CompleteAdding<T>();
        //void WaitTillAllQueuesAreCompleted<T>();
        int GetUniqueId();
        IMeteredBlockingCollection<T> GetInputQueue<T>(int stepNumber);
        IMeteredBlockingCollection<T> GetOutputQueue<T>(int stepNumber);
        IMeteredBlockingCollection<T> CreateOutputQueue<T>(int stepNumber);
        IMeteredBlockingCollection<T> CreateInputQueue<T>(int stepNumber);
    }
}
