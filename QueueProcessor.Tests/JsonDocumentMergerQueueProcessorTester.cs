// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonDocumentMergerQueueProcessorTester.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   Defines the JsonDocumentMergerQueueProcessorTester type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueProcessor.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using ElasticSearchJsonWriter;
    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.ProgressMonitor;
    using ElasticSearchSqlFeeder.Shared;
    using Fabric.Databus.Config;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                    LocalSaveFolder = Path.GetTempPath()
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
