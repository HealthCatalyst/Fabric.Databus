﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateBatchItemsQueueProcessorTests.cs" company="">
//   
// </copyright>
// <summary>
//   The create batch items queue processor tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QueueProcessor.Tests
{
    using System.IO;
    using System.Linq;

    using CreateBatchItemsQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Databus.Config;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json.Linq;

    using QueueItems;

    using Serilog;

    /// <summary>
    /// The create batch items queue processor tests.
    /// </summary>
    [TestClass]
    public class CreateBatchItemsQueueProcessorTests
    {
        /// <summary>
        /// The test success.
        /// </summary>
        [TestMethod]
        public void TestSuccess()
        {
            // Arrange
            var job = new Job
                          {
                              Config = new QueryConfig
                                           {
                                               LocalSaveFolder = Path.GetTempPath(),
                                               EntitiesPerUploadFile = 1
                                           }
                          };

            var queueManager = new QueueManager();

            var queueContext = new QueueContext
                                   {
                                       Config = job.Config,
                                       QueueManager = queueManager,
                                       ProgressMonitor = new MockProgressMonitor(),
                                   };

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var createBatchItemsQueueProcessor = new CreateBatchItemsQueueProcessor(queueContext, logger);

            var stepNumber = 1;
            queueManager.CreateInputQueue<IJsonObjectQueueItem>(stepNumber);

            createBatchItemsQueueProcessor.CreateOutQueue(stepNumber);

            createBatchItemsQueueProcessor.InitializeWithStepNumber(stepNumber);

            string queryId = "1";

            var jsonObjectQueueItem1 = new JsonObjectQueueItem
                                           {
                                               Document = JObject.Parse(@"{test:'ff'}"),
                                               PropertyName = "foo",
                                               QueryId = queryId
                                           };

            createBatchItemsQueueProcessor.InternalHandle(jsonObjectQueueItem1);

            var jsonObjectQueueItem2 = new JsonObjectQueueItem
                                           {
                                               Document = JObject.Parse(@"{test2:'ff'}"),
                                               PropertyName = "foo2",
                                               QueryId = queryId
                                           };

            // Act
            createBatchItemsQueueProcessor.InternalHandle(jsonObjectQueueItem2);

            // Assert
            var queues = queueManager.Queues;

            Assert.AreEqual(2, queues.Count);

            var meteredBlockingCollection = queues.First(queue => queue.Key == "SaveBatchQueueItem2").Value;
            var outputQueue = meteredBlockingCollection as IMeteredBlockingCollection<SaveBatchQueueItem>;

            Assert.IsNotNull(outputQueue);

            Assert.AreEqual(1, outputQueue.Count);

            var saveBatchQueueItem = outputQueue.Take();

            Assert.AreEqual(0, outputQueue.Count);

            Assert.AreEqual(1, saveBatchQueueItem.ItemsToSave.Count);

            var jsonObjectQueueItem = saveBatchQueueItem.ItemsToSave.First();

            Assert.AreEqual(jsonObjectQueueItem1, jsonObjectQueueItem);

            Assert.AreEqual(jsonObjectQueueItem1.PropertyName, jsonObjectQueueItem.PropertyName);

            // Act
            // now complete the queue
            createBatchItemsQueueProcessor.TestComplete(queryId, true);

            // Assert
            Assert.AreEqual(1, outputQueue.Count);

            saveBatchQueueItem = outputQueue.Take();

            Assert.AreEqual(0, outputQueue.Count);

            Assert.AreEqual(1, saveBatchQueueItem.ItemsToSave.Count);

            jsonObjectQueueItem = saveBatchQueueItem.ItemsToSave.First();

            Assert.AreEqual(jsonObjectQueueItem2, jsonObjectQueueItem);

            Assert.AreEqual(jsonObjectQueueItem2.PropertyName, jsonObjectQueueItem.PropertyName);
        }
    }
}