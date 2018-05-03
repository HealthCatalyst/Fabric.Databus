using System;
using System.Collections.Generic;
using ElasticSearchJsonWriter;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.ProgressMonitor;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueueProcessor.Tests
{
    [TestClass]
    public class JsonDocumentMergerQueueProcessorTester
    {
        [TestMethod]
        public void TestSimple()
        {
            var maximumDocumentsInQueue = 1;
            var documentDictionary =
                new MeteredConcurrentDictionary<string, JsonObjectQueueItem>(maximumDocumentsInQueue);

            var job = new Job
            {
                Config = new QueryConfig
                {

                },
                Data = new Data
                {
                    DataSources = new List<DataSource>
                    {
                        new DataSource
                        {
                            Sql = @"SELECT
  	                              CustomerNM
	                              ,CustomerID
                                  ,	AliasPatientID
                                  ,	GenderNormDSC
                                  ,RaceNormDSC
                                  ,MaritalStatusNormDSC  
                                  FROM CAFEEDW.SharedClinicalUnion.ElasticsearchInputPatient where CustomerID = 4"
                        }
                    }
                }
            };

            var queueContext = new QueueContext
            {
                Config = job.Config,
                QueueManager = new QueueManager(),
                ProgressMonitor = new MockProgressMonitor(),
            };

            // DatabusSqlReader.ReadDataFromQuery(job.Config, )

            var jsonDocumentMergerQueueProcessor = new JsonDocumentMergerQueueProcessor(documentDictionary, queueContext);

            var jsonDocumentMergerQueueItem = new JsonDocumentMergerQueueItem();
            // jsonDocumentMergerQueueProcessor.Handle(jsonDocumentMergerQueueItem);
        }

    }

    public class MockProgressMonitor : IProgressMonitor
    {
        public Action JobHistoryUpdateAction { get; set; }
        public void SetProgressItem(ProgressMonitorItem progressMonitorItem)
        {
        }

        public IList<ProgressMonitorItem> GetSnapshotOfProgressItems()
        {
            throw new NotImplementedException();
        }
    }
}
