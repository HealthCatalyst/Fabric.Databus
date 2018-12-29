// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuerySqlPipelineStepUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the QuerySqlPipelineStepUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineStep.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

    using Serilog;

    /// <summary>
    /// The query sql unit tests.
    /// </summary>
    [TestClass]
    public class QuerySqlPipelineStepUnitTests
    {
        /// <summary>
        /// The can query one entity.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task CanQueryOneEntity()
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
                mockDatabusSqlReader
                    .Setup(
                        service => service.ReadDataFromQueryAsync(
                            It.IsAny<IDataSource>(),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<ILogger>(),
                            It.IsAny<string>(),
                            It.IsAny<IEnumerable<IIncrementalColumn>>(),
                            It.IsAny<string>())).ReturnsAsync(
                        new ReadSqlDataResult
                        {
                            ColumnList = new List<ColumnInfo>
                                                 {
                                                     new ColumnInfo
                                                         {
                                                             Name = "Id"
                                                         },
                                                     new ColumnInfo
                                                         {
                                                             Name = "Name"
                                                         }
                                                 },
                            Data = new Dictionary<string, List<object[]>>
                                           {
                                               { "2", new List<object[]> { new object[] { "2", "2000" } } },
                                               { "3", new List<object[]> { new object[] { "3", "3000" } } },
                                               { "4", new List<object[]> { new object[] { "4", "4000" } } },
                                               { "5", new List<object[]> { new object[] { "5", "5000" } } },
                                           }
                        });

                var querySqlPipelineStep = new QuerySqlPipelineStep(
                    job.Config,
                    mockDatabusSqlReader.Object,
                    logger,
                    queueManager,
                    new MockProgressMonitor(),
                    new NullFileWriter(),
                    new NullQuerySqlLogger(), 
                    cancellationTokenSource.Token,
                    new PipelineStepState());

                var sqlJobQueueItem = new SqlQueryDataSourceQueueItem
                {
                    QueryId = "$",
                    DataSource = new DataSource { Path = "$" },
                    Start = "2",
                    End = "5",
                    TopLevelDataSource = new TopLevelDataSource { Key = "Id" }
                };

                var stepNumber = 1;
                queueManager.CreateInputQueue<SqlQueryDataSourceQueueItem>(stepNumber);

                querySqlPipelineStep.CreateOutQueue(stepNumber);

                querySqlPipelineStep.InitializeWithStepNumber(stepNumber);

                await querySqlPipelineStep.InternalHandleAsync(sqlJobQueueItem);

                // Assert
                var queues = queueManager.Queues;

                Assert.AreEqual(2, queues.Count);

                Assert.IsTrue(
                    queues.Any(queue => queue.Key == nameof(SqlDataLoadedQueueItem) + "2"),
                    queues.Select(queue => queue.Key).ToCsv());
                var meteredBlockingCollection =
                    queues.First(queue => queue.Key == nameof(SqlDataLoadedQueueItem) + "2").Value;
                var outputQueue = meteredBlockingCollection as IQueue<SqlDataLoadedQueueItem>;

                Assert.IsNotNull(outputQueue);
                Assert.AreEqual(4, outputQueue.Count);

                var sqlDataLoadedQueueItem = outputQueue.TakeGeneric(CancellationToken.None) as SqlDataLoadedQueueItem;
                Assert.IsNotNull(sqlDataLoadedQueueItem);
                Assert.AreEqual("$", sqlDataLoadedQueueItem.QueryId);
                Assert.AreEqual(1, sqlDataLoadedQueueItem.Rows.Count);
                Assert.AreEqual(2, sqlDataLoadedQueueItem.Rows[0].Length);
                Assert.AreEqual("2", sqlDataLoadedQueueItem.Rows[0][0]);
                Assert.AreEqual("2000", sqlDataLoadedQueueItem.Rows[0][1]);
            }
        }

        /// <summary>
        /// The can query one entity.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task CanQueryOneEntityWithIncrementalColumns()
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
                mockDatabusSqlReader
                    .Setup(
                        service => service.ReadDataFromQueryAsync(
                            It.IsAny<IDataSource>(),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<ILogger>(),
                            It.IsAny<string>(),
                            It.IsAny<IEnumerable<IIncrementalColumn>>(),
                            It.IsAny<string>())).ReturnsAsync(
                        new ReadSqlDataResult
                        {
                            ColumnList = new List<ColumnInfo>
                                                 {
                                                     new ColumnInfo
                                                         {
                                                             Name = "Id"
                                                         },
                                                     new ColumnInfo
                                                         {
                                                             Name = "Name"
                                                         }
                                                 },
                            Data = new Dictionary<string, List<object[]>>
                                           {
                                               { "2", new List<object[]> { new object[] { "2", "2000" } } },
                                               { "3", new List<object[]> { new object[] { "3", "3000" } } },
                                               { "4", new List<object[]> { new object[] { "4", "4000" } } },
                                               { "5", new List<object[]> { new object[] { "5", "5000" } } },
                                           }
                        });

                var querySqlPipelineStep = new QuerySqlPipelineStep(
                    job.Config,
                    mockDatabusSqlReader.Object,
                    logger,
                    queueManager,
                    new MockProgressMonitor(),
                    new NullFileWriter(),
                    new NullQuerySqlLogger(), 
                    cancellationTokenSource.Token,
                    new PipelineStepState());

                var topLevelDataSource = new TopLevelDataSource
                                             {
                                                 Key = "Id",
                                                 MyIncrementalColumns =
                                                     {
                                                         new IncrementalColumn
                                                             {
                                                                 Name = "Id", Operator = "GreaterThan", Value = "3", Type = "int"
                                                             }
                                                     }
                                             };

                var sqlJobQueueItem = new SqlQueryDataSourceQueueItem
                {
                    QueryId = "$",
                    DataSource = new DataSource { Path = "$" },
                    Start = "2",
                    End = "5",
                    TopLevelDataSource = topLevelDataSource
                };

                var stepNumber = 1;
                queueManager.CreateInputQueue<SqlQueryDataSourceQueueItem>(stepNumber);

                querySqlPipelineStep.CreateOutQueue(stepNumber);

                querySqlPipelineStep.InitializeWithStepNumber(stepNumber);

                await querySqlPipelineStep.InternalHandleAsync(sqlJobQueueItem);

                // Assert
                var queues = queueManager.Queues;

                Assert.AreEqual(2, queues.Count);

                Assert.IsTrue(
                    queues.Any(queue => queue.Key == nameof(SqlDataLoadedQueueItem) + "2"),
                    queues.Select(queue => queue.Key).ToCsv());
                var meteredBlockingCollection =
                    queues.First(queue => queue.Key == nameof(SqlDataLoadedQueueItem) + "2").Value;
                var outputQueue = meteredBlockingCollection as IQueue<SqlDataLoadedQueueItem>;

                Assert.IsNotNull(outputQueue);
                Assert.AreEqual(4, outputQueue.Count);

                var sqlDataLoadedQueueItem = outputQueue.TakeGeneric(CancellationToken.None) as SqlDataLoadedQueueItem;
                Assert.IsNotNull(sqlDataLoadedQueueItem);
                Assert.AreEqual("$", sqlDataLoadedQueueItem.QueryId);
                Assert.AreEqual(1, sqlDataLoadedQueueItem.Rows.Count);
                Assert.AreEqual(2, sqlDataLoadedQueueItem.Rows[0].Length);
                Assert.AreEqual("2", sqlDataLoadedQueueItem.Rows[0][0]);
                Assert.AreEqual("2000", sqlDataLoadedQueueItem.Rows[0][1]);
            }
        }
    }
}
