// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateBatchesPipelineStepUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the CreateBatchesPipelineStepUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineStep.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.PipelineSteps;
    using Fabric.Databus.QueueItems;
    using Fabric.Databus.Shared.FileWriters;
    using Fabric.Databus.Shared.Loggers;
    using Fabric.Databus.Shared.Queues;
    using Fabric.Shared;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;
    using Moq.Protected;

    using Serilog;

    /// <summary>
    /// The create batches unit tests.
    /// </summary>
    [TestClass]
    public class CreateBatchesPipelineStepUnitTests
    {
        /// <summary>
        /// The can create batches.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task CanCreateBatchesWhenBatchingIsOff()
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
                var mockRepository = new MockRepository(MockBehavior.Strict);
                var mockDatabusSqlReader = mockRepository.Create<IDatabusSqlReader>();

                var createBatchesPipelineStep = new CreateBatchesPipelineStep(
                    job.Config,
                    logger,
                    queueManager,
                    new MockProgressMonitor(),
                    mockDatabusSqlReader.Object,
                    new NullFileWriter(),
                    new NullBatchEventsLogger(),
                    cancellationTokenSource.Token,
                    new PipelineStepState());

                var sqlJobQueueItem = new SqlJobQueueItem
                {
                    Job = new Job
                    {
                        Data = new JobData
                        {
                            MyDataSources = new List<DataSource>()
                        }
                    }
                };

                var stepNumber = 1;
                queueManager.CreateInputQueue<SqlJobQueueItem>(stepNumber);

                createBatchesPipelineStep.CreateOutQueue(stepNumber);

                createBatchesPipelineStep.InitializeWithStepNumber(stepNumber);

                await createBatchesPipelineStep.InternalHandleAsync(sqlJobQueueItem);

                // Assert
                var queues = queueManager.Queues;

                Assert.AreEqual(2, queues.Count);

                Assert.IsTrue(queues.Any(queue => queue.Key == nameof(SqlBatchQueueItem) + "2"), queues.Select(queue => queue.Key).ToCsv());
                var meteredBlockingCollection = queues.First(queue => queue.Key == nameof(SqlBatchQueueItem) + "2").Value;
                var outputQueue = meteredBlockingCollection as IQueue<SqlBatchQueueItem>;

                Assert.IsNotNull(outputQueue);
                Assert.AreEqual(1, outputQueue.Count);

                var sqlBatchQueueItem = outputQueue.TakeGeneric(CancellationToken.None) as SqlBatchQueueItem;

                Assert.IsNotNull(sqlBatchQueueItem);
                Assert.AreEqual(1, sqlBatchQueueItem.BatchNumber);
                Assert.AreEqual(null, sqlBatchQueueItem.Start);
                Assert.AreEqual(null, sqlBatchQueueItem.End);
            }
        }

        /// <summary>
        /// The can create batches.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task CanCreateBatchesWhenBatchingIsOn()
        {
            // Arrange
            var job = new Job
            {
                Config = new QueryConfig
                {
                    LocalSaveFolder = Path.GetTempPath(),
                    EntitiesPerUploadFile = 1,
                    EntitiesPerBatch = 2,
                    MaximumEntitiesToLoad = 4
                },
                Data = new JobData { MyTopLevelDataSource = new TopLevelDataSource { Key = "TextID" } }
            };

            var queueManager = new QueueManager(new InMemoryQueueFactory());

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {

                var mockRepository = new MockRepository(MockBehavior.Strict);
                var mockDatabusSqlReader = mockRepository.Create<IDatabusSqlReader>();
                mockDatabusSqlReader
                    .Setup(service => service.GetListOfEntityKeysAsync("TextID", 4, It.IsAny<ITopLevelDataSource>()))
                    .ReturnsAsync(new List<string>());

                var createBatchesPipelineStep = new CreateBatchesPipelineStep(
                    job.Config,
                    logger,
                    queueManager,
                    new MockProgressMonitor(),
                    mockDatabusSqlReader.Object,
                    new NullFileWriter(),
                    new NullBatchEventsLogger(), 
                    cancellationTokenSource.Token,
                    new PipelineStepState());

                var sqlJobQueueItem = new SqlJobQueueItem
                {
                    Job = new Job
                    {
                        Data = new JobData
                        {
                            MyTopLevelDataSource = new TopLevelDataSource
                            {
                                Key =
                                                                                                              "TextID",
                                Path = "$"
                            }
                        }
                    }
                };

                var stepNumber = 1;
                queueManager.CreateInputQueue<SqlJobQueueItem>(stepNumber);

                createBatchesPipelineStep.CreateOutQueue(stepNumber);

                createBatchesPipelineStep.InitializeWithStepNumber(stepNumber);

                await createBatchesPipelineStep.InternalHandleAsync(sqlJobQueueItem);

                // Assert
                var queues = queueManager.Queues;

                Assert.AreEqual(2, queues.Count);

                Assert.IsTrue(queues.Any(queue => queue.Key == nameof(SqlBatchQueueItem) + "2"), queues.Select(queue => queue.Key).ToCsv());
                var meteredBlockingCollection = queues.First(queue => queue.Key == nameof(SqlBatchQueueItem) + "2").Value;
                var outputQueue = meteredBlockingCollection as IQueue<SqlBatchQueueItem>;

                Assert.IsNotNull(outputQueue);
                Assert.AreEqual(1, outputQueue.Count);

                var sqlBatchQueueItem = outputQueue.TakeGeneric(CancellationToken.None) as SqlBatchQueueItem;

                Assert.IsNotNull(sqlBatchQueueItem);

                Assert.AreEqual(1, sqlBatchQueueItem.BatchNumber);
                Assert.AreEqual(null, sqlBatchQueueItem.Start);
                Assert.AreEqual(null, sqlBatchQueueItem.End);
            }
        }

        /// <summary>
        /// The can create batches.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task CanCreateBatchesWhenBatchingIsOnWithMultipleBatches()
        {
            // Arrange
            var job = new Job
            {
                Config = new QueryConfig
                {
                    LocalSaveFolder = Path.GetTempPath(),
                    EntitiesPerUploadFile = 1,
                    EntitiesPerBatch = 2,
                    MaximumEntitiesToLoad = 4
                },
                Data = new JobData { MyTopLevelDataSource = new TopLevelDataSource { Key = "TextID" } }
            };

            var queueManager = new QueueManager(new InMemoryQueueWithoutBlockingFactory());

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {

                var mockRepository = new MockRepository(MockBehavior.Strict);
                var mockDatabusSqlReader = mockRepository.Create<IDatabusSqlReader>();
                mockDatabusSqlReader
                    .Setup(service => service.GetListOfEntityKeysAsync("TextID", 4, It.IsAny<ITopLevelDataSource>()))
                    .ReturnsAsync(new List<string> { "1", "3", "5", "6" });

                var createBatchesPipelineStep = new CreateBatchesPipelineStep(
                    job.Config,
                    logger,
                    queueManager,
                    new MockProgressMonitor(),
                    mockDatabusSqlReader.Object,
                    new NullFileWriter(),
                    new NullBatchEventsLogger(), 
                    cancellationTokenSource.Token,
                    new PipelineStepState());

                var sqlJobQueueItem = new SqlJobQueueItem
                                          {
                                              Job = new Job
                                                        {
                                                            Data = new JobData
                                                                       {
                                                                           MyDataSources =
                                                                               new List<DataSource>
                                                                                   {
                                                                                       new DataSource { Path = "$" }
                                                                                   },
                                                                           MyTopLevelDataSource = new TopLevelDataSource
                                                                                                      {
                                                                                                          Key =
                                                                                                              "TextID",
                                                                                                          Path = "$"
                                                                                                      }
                                                                       }
                                                        }
                                          };

                var stepNumber = 1;
                queueManager.CreateInputQueue<SqlJobQueueItem>(stepNumber);

                createBatchesPipelineStep.CreateOutQueue(stepNumber);

                createBatchesPipelineStep.InitializeWithStepNumber(stepNumber);

                await createBatchesPipelineStep.InternalHandleAsync(sqlJobQueueItem);

                // Assert
                var queues = queueManager.Queues;

                Assert.AreEqual(2, queues.Count);

                Assert.IsTrue(queues.Any(queue => queue.Key == nameof(SqlBatchQueueItem) + "2"), queues.Select(queue => queue.Key).ToCsv());
                var meteredBlockingCollection = queues.First(queue => queue.Key == nameof(SqlBatchQueueItem) + "2").Value;
                var outputQueue = meteredBlockingCollection as IQueue<SqlBatchQueueItem>;

                Assert.IsNotNull(outputQueue);
                Assert.AreEqual(2, outputQueue.Count);

                var queueItem = outputQueue.TakeGeneric(CancellationToken.None);
                Assert.IsTrue(queueItem is SqlBatchQueueItem);
                var sqlBatchQueueItem = queueItem as SqlBatchQueueItem;
                Assert.AreEqual(1, sqlBatchQueueItem.BatchNumber);
                Assert.AreEqual("1", sqlBatchQueueItem.Start);
                Assert.AreEqual("3", sqlBatchQueueItem.End);

                Assert.AreEqual(typeof(BatchCompletedQueueItem), outputQueue.TakeGeneric(CancellationToken.None).GetType());

                queueItem = outputQueue.TakeGeneric(CancellationToken.None);
                Assert.IsTrue(queueItem is SqlBatchQueueItem);
                sqlBatchQueueItem = queueItem as SqlBatchQueueItem;
                Assert.AreEqual(2, sqlBatchQueueItem.BatchNumber);
                Assert.AreEqual("5", sqlBatchQueueItem.Start);
                Assert.AreEqual("6", sqlBatchQueueItem.End);

                Assert.AreEqual(typeof(BatchCompletedQueueItem), outputQueue.TakeGeneric(CancellationToken.None).GetType());

                Assert.AreEqual(typeof(JobCompletedQueueItem), outputQueue.TakeGeneric(CancellationToken.None).GetType());
            }
        }
    }
}
