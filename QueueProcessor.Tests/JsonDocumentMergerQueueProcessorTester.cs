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
    using System.Collections.Generic;
    using System.IO;

    using ElasticSearchJsonWriter;

    using ElasticSearchSqlFeeder.Shared;
    using Fabric.Databus.Config;

    using JsonDocumentMergerQueueProcessor;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using QueueItems;

    /// <summary>
    /// The json document merger queue processor tester.
    /// </summary>
    [TestClass]
    public class JsonDocumentMergerQueueProcessorTester
    {
        /// <summary>
        /// The test simple.
        /// </summary>
        [TestMethod]
        public void TestSimple()
        {
            var maximumDocumentsInQueue = 1;
            var documentDictionary =
                new MeteredConcurrentDictionary<string, IJsonObjectQueueItem>(maximumDocumentsInQueue);

            var job = new Job
            {
                Config = new QueryConfig
                {
                    LocalSaveFolder = Path.GetTempPath()
                },
                Data = new JobData
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
                DocumentDictionary = documentDictionary
            };

            // DatabusSqlReader.ReadDataFromQuery(job.Config, )
            var jsonDocumentMergerQueueProcessor = new JsonDocumentMergerQueueProcessor(queueContext);

            var jsonDocumentMergerQueueItem = new JsonDocumentMergerQueueItem();

            jsonDocumentMergerQueueProcessor.InternalHandle(jsonDocumentMergerQueueItem);
        }
    }
}
