namespace QueueProcessor.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CreateBatchItemsQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Databus.Config;

    using JsonDocumentMergerQueueProcessor;

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

            var jsonObjectQueueItem1 = new JsonObjectQueueItem
                                           {
                                               Document = JObject.Parse(@"{test:'ff'}"),
                                               PropertyName = "foo"
                                           };

            createBatchItemsQueueProcessor.InternalHandle(jsonObjectQueueItem1);

            var jsonObjectQueueItem2 = new JsonObjectQueueItem();

            createBatchItemsQueueProcessor.InternalHandle(jsonObjectQueueItem2);

            var queues = queueManager.Queues;

            Assert.AreEqual(2, queues.Count);

            var meteredBlockingCollection = queues.First(queue => queue.Key == "SaveBatchQueueItem2").Value;
            var outputQueue = meteredBlockingCollection as IMeteredBlockingCollection<SaveBatchQueueItem>;

            Assert.IsNotNull(outputQueue);

            Assert.AreEqual(1, outputQueue.Count);

            var saveBatchQueueItem = outputQueue.Take();

            Assert.AreEqual(1, saveBatchQueueItem.ItemsToSave.Count);

            var jsonObjectQueueItem = saveBatchQueueItem.ItemsToSave.First();

            Assert.AreEqual(jsonObjectQueueItem1, jsonObjectQueueItem);

            Assert.AreEqual(jsonObjectQueueItem1.PropertyName, jsonObjectQueueItem.PropertyName);
        }
    }
}
