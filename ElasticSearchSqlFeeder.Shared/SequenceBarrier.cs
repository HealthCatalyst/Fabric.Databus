using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchSqlFeeder.Shared
{

    public class SequenceBarrier
    {
        public readonly ConcurrentDictionary<string, string> LastProcessedEntityIdForEachQuery = new ConcurrentDictionary<string, string>();

        public readonly ConcurrentDictionary<string, string> LastCompletedEntityIdForEachQuery = new ConcurrentDictionary<string, string>();


        public string UpdateMinimumEntityIdProcessed(string queryId, string id)
        {
            if (LastProcessedEntityIdForEachQuery.ContainsKey(queryId) == false)
            {
                LastProcessedEntityIdForEachQuery.GetOrAdd(queryId, (string)null);
            }
            if (LastCompletedEntityIdForEachQuery.ContainsKey(queryId) == false)
            {
                LastCompletedEntityIdForEachQuery.GetOrAdd(queryId, (string)null);
            }

            if (id != LastProcessedEntityIdForEachQuery[queryId])
            {
                LastCompletedEntityIdForEachQuery.AddOrUpdate(queryId, LastProcessedEntityIdForEachQuery[queryId],
                    (a, b) => LastProcessedEntityIdForEachQuery[queryId]);
            }

            LastProcessedEntityIdForEachQuery.AddOrUpdate(queryId, id, (a, b) => id);

            var min = LastCompletedEntityIdForEachQuery.Min(q => q.Value);

            //Logger.Trace($"Minimum Key in Dictionary: {min}, {JsonConvert.SerializeObject(LastCompletedEntityIdForEachQuery)}");

            return min;

        }

        public void CompleteQuery(string queryId)
        {
            if (queryId != null)
            {
                string temp;

                LastCompletedEntityIdForEachQuery.TryRemove(queryId, out temp);
            }
            //Logger.Trace($"Completed {queryId}: queue: {_inQueue.Count}");
        }
    }
}
