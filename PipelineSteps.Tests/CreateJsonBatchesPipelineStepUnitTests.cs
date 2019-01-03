// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateJsonBatchesPipelineStepUnitTests.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   The create batch items queue processor tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineStep.Tests
{
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.PipelineSteps;
    using Fabric.Databus.QueueItems;
    using Fabric.Databus.Shared.Queues;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json.Linq;

    using Serilog;

    /// <summary>
    /// The create batch items queue processor tests.
    /// </summary>
    [TestClass]
    public class CreateJsonBatchesPipelineStepUnitTests
    {
        /// <summary>
        /// The test success.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task TestSuccess()
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

            var queueManager = new QueueManager(new InMemoryQueueFactory());

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var createBatchItemsQueueProcessor = new CreateJsonBatchesPipelineStep(
                    job.Config,
                    logger,
                    queueManager,
                    new DummyProgressMonitor(),
                    cancellationTokenSource.Token,
                    new PipelineStepState());

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

                await createBatchItemsQueueProcessor.InternalHandleAsync(jsonObjectQueueItem1);

                var jsonObjectQueueItem2 = new JsonObjectQueueItem
                {
                    Document = JObject.Parse(@"{test2:'ff'}"),
                    PropertyName = "foo2",
                    QueryId = queryId
                };

                // Act
                await createBatchItemsQueueProcessor.InternalHandleAsync(jsonObjectQueueItem2);

                // Assert
                var queues = queueManager.Queues;

                Assert.AreEqual(2, queues.Count);

                var meteredBlockingCollection = queues.First(queue => queue.Key == nameof(SaveBatchQueueItem) + "2").Value;
                var outputQueue = meteredBlockingCollection as IQueue<SaveBatchQueueItem>;

                Assert.IsNotNull(outputQueue);

                Assert.AreEqual(1, outputQueue.Count);

                var saveBatchQueueItem = outputQueue.TakeGeneric(cancellationTokenSource.Token) as SaveBatchQueueItem;

                Assert.AreEqual(0, outputQueue.Count);

                Assert.IsNotNull(saveBatchQueueItem);

                Assert.AreEqual(1, saveBatchQueueItem.ItemsToSave.Count);

                var jsonObjectQueueItem = saveBatchQueueItem.ItemsToSave.First();

                Assert.AreEqual(jsonObjectQueueItem1, jsonObjectQueueItem);

                Assert.AreEqual(jsonObjectQueueItem1.PropertyName, jsonObjectQueueItem.PropertyName);

                // Act
                // now complete the queue
                await createBatchItemsQueueProcessor.CompleteBatchForTestingAsync(queryId, true, 1, new BatchCompletedQueueItem());

                // Assert
                Assert.AreEqual(1, outputQueue.Count);

                saveBatchQueueItem = outputQueue.TakeGeneric(cancellationTokenSource.Token) as SaveBatchQueueItem;

                Assert.IsNotNull(saveBatchQueueItem);

                Assert.AreEqual(0, outputQueue.Count);

                Assert.AreEqual(1, saveBatchQueueItem.ItemsToSave.Count);

                jsonObjectQueueItem = saveBatchQueueItem.ItemsToSave.First();

                Assert.AreEqual(jsonObjectQueueItem2, jsonObjectQueueItem);

                Assert.AreEqual(jsonObjectQueueItem2.PropertyName, jsonObjectQueueItem.PropertyName);
            }
        }
    }
}
