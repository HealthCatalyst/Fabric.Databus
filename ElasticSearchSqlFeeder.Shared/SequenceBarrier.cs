// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SequenceBarrier.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SequenceBarrier type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System.Collections.Concurrent;
    using System.Linq;

    public class SequenceBarrier
    {
        public readonly ConcurrentDictionary<string, string> LastProcessedEntityIdForEachQuery = new ConcurrentDictionary<string, string>();

        public readonly ConcurrentDictionary<string, string> LastCompletedEntityIdForEachQuery = new ConcurrentDictionary<string, string>();


        public string UpdateMinimumEntityIdProcessed(string queryId, string id)
        {
            if (this.LastProcessedEntityIdForEachQuery.ContainsKey(queryId) == false)
            {
                this.LastProcessedEntityIdForEachQuery.GetOrAdd(queryId, (string)null);
            }
            if (this.LastCompletedEntityIdForEachQuery.ContainsKey(queryId) == false)
            {
                this.LastCompletedEntityIdForEachQuery.GetOrAdd(queryId, (string)null);
            }

            if (id != this.LastProcessedEntityIdForEachQuery[queryId])
            {
                this.LastCompletedEntityIdForEachQuery.AddOrUpdate(queryId, this.LastProcessedEntityIdForEachQuery[queryId],
                    (a, b) => this.LastProcessedEntityIdForEachQuery[queryId]);
            }

            this.LastProcessedEntityIdForEachQuery.AddOrUpdate(queryId, id, (a, b) => id);

            var min = this.LastCompletedEntityIdForEachQuery.Min(q => q.Value);

            //Logger.Verbose($"Minimum Key in Dictionary: {min}, {JsonConvert.SerializeObject(LastCompletedEntityIdForEachQuery)}");

            return min;

        }

        public void CompleteQuery(string queryId)
        {
            if (queryId != null)
            {
                string temp;

                this.LastCompletedEntityIdForEachQuery.TryRemove(queryId, out temp);
            }
            //Logger.Verbose($"Completed {queryId}: queue: {_inQueue.Count}");
        }
    }
}
