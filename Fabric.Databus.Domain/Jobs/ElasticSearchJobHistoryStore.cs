using System;
using System.Collections.Generic;
using System.Linq;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Domain.Configuration;
using Nest;
using Serilog;

namespace Fabric.Databus.Domain.Jobs
{
    public class ElasticSearchJobHistoryStore : IJobHistoryStore
    {
        private readonly ElasticClient _client;
        public ElasticSearchJobHistoryStore(ILogger logger, ElasticSearchSettings settings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            _client = MakeElasticSearchClient(settings);
        }
        public IList<JobHistoryItem> GetJobHistory(string jobName)
        {
            var searchResults = _client.Search<JobHistoryItem>(s => s
                .Query(q => q.Term(item => item.Name, jobName))
                    .Sort(ss => ss
                        .Descending(item => item.StartDateTimeUtc))
            );
            return searchResults.Documents.ToList();
        }

        public IList<JobHistoryItem> GetMostRecentJobs(int numberOfJobs)
        {
            var searchResults = _client.Search<JobHistoryItem>(s => s
                .From(0)
                .Size(numberOfJobs)
                .Sort(ss => ss
                    .Descending(item => item.StartDateTimeUtc))
            );
            return searchResults.Documents.ToList();
        }

        public JobHistoryItem GetLatestJobHistoryItem(string jobName)
        {
            var searchResults = _client.Search<JobHistoryItem>(s => s
                .From(0)
                .Size(1)
                .Query(q => q.Term(item => item.Name, jobName))
                .Sort(ss => ss 
                    .Descending(item => item.StartDateTimeUtc))
            );
            return searchResults.Documents.FirstOrDefault();
        }

        public JobHistoryItem GetJobHistoryItem(Guid executionId)
        {
            var searchResults = _client.Search<JobHistoryItem>(s => s
                .From(0)
                .Size(1)
                .Query(q => q.Ids(c => c.Values(executionId)))
                .Sort(ss => ss
                    .Descending(item => item.StartDateTimeUtc))
            );
            return searchResults.Documents.FirstOrDefault();
        }

        public void AddJobHistoryItem(JobHistoryItem item)
        {
            _client.Index(item);
        }

        public void UpdateJobHistoryItem(JobHistoryItem item)
        {
            AddJobHistoryItem(item);
        }

        private ElasticClient MakeElasticSearchClient(ElasticSearchSettings settings)
        {
            var node = settings.GetElasticSearchUri();
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("fabric-databus");
            return new ElasticClient(connSettings);
        }

        
    }
}
