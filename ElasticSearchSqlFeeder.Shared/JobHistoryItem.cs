using System;
using System.Collections.Generic;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.ProgressMonitor;
using Fabric.Databus.Config;
using Newtonsoft.Json;

namespace ElasticSearchSqlFeeder.Shared
{
    public class JobHistoryItem
    {
        public const string CompletedStatus = "Completed";
        public const string ExecutingStatus = "Executing";
        public const string ErrorStatus = "Error";
        public const string ScheduledStatus = "Scheduled";

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public bool IsComplete => Status == CompletedStatus;

        public Job ExecutedQuery { get; set; }

        public DateTime StartDateTimeUtc { get; set; }

        public DateTime EndDateTimeUtc { get; set; }

        public double ExecutionTime { get; set; }

        public Guid CorrelationId { get; set; }

        [JsonIgnore]
        public IProgressMonitor ProgressMonitor { get; set; }

        public IList<ProgressMonitorItem> ProgressLogItems{ get; set; }
        public string ErrorText { get; set; }
    }
}
