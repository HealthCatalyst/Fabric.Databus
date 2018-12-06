// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndToEndUnitTests.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the EndToEndUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ConfigValidators;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.Http;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Pipeline;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.PipelineRunner;
    using Fabric.Databus.Shared.Loggers;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Serilog;

    using Unity;

    /// <summary>
    /// The end to end tests.
    /// </summary>
    [TestClass]
    public class EndToEndUnitTests
    {
        /// <summary>
        /// The test elastic search pipeline.
        /// </summary>
        [TestMethod]
        public void TestElasticSearchPipelineSingleThreaded()
        {
            var sql = @"fake sql";

            var job = new Job
                          {
                              Config = new QueryConfig
                                           {
                                               ConnectionString = "foo",
                                               LocalSaveFolder = Path.GetTempPath(),
                                               Url = "http://foo",
                                               UploadToUrl = false,
                                               EntitiesPerUploadFile = 1,
                                               Pipeline = PipelineNames.ElasticSearch,
                                               UseMultipleThreads = false
                                           },
                              Data = new JobData
                                         {
                                             MyTopLevelDataSource =
                                                 new TopLevelDataSource { Key = "AliasPatientID", Sql = sql }
                                         }
                          };

            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mockDatabusSqlReader = mockRepository.Create<IDatabusSqlReader>();
            mockDatabusSqlReader.Setup(
                service => service.ReadDataFromQueryAsync(
                    It.IsAny<IDataSource>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<string>())).ReturnsAsync(
                new ReadSqlDataResult
                {
                    ColumnList = new List<ColumnInfo>
                                         {
                                             new ColumnInfo { Name = "CustomerNM", ElasticSearchType = ElasticSearchTypes.keyword.ToString() },
                                             new ColumnInfo { Name = "CustomerID", ElasticSearchType = ElasticSearchTypes.integer.ToString() },
                                             new ColumnInfo { Name = "AliasPatientID", ElasticSearchType = ElasticSearchTypes.integer.ToString() },
                                             new ColumnInfo { Name = "GenderNormDSC", ElasticSearchType = ElasticSearchTypes.keyword.ToString() },
                                             new ColumnInfo { Name = "RaceNormDSC", ElasticSearchType = ElasticSearchTypes.keyword.ToString() },
                                             new ColumnInfo { Name = "MaritalStatusNormDSC", ElasticSearchType = ElasticSearchTypes.keyword.ToString() }
                                         },
                    Data = new Dictionary<string, List<object[]>>
                                   {
                                       {
                                           "ElasticsearchInputPatient",
                                           new List<object[]> { new object[] { "name", 1, 2, "M", "White", "Married" } }
                                       }
                                   }
                });

            var mockConfigValidator = mockRepository.Create<IConfigValidator>();
            mockConfigValidator.Setup(service => service.ValidateJob(It.IsAny<IJob>()));
            mockConfigValidator.Setup(service => service.ValidateDataSources(It.IsAny<IJob>()));

            var mockFileUploaderFactory = mockRepository.Create<IElasticSearchUploaderFactory>();
            var mockFileUploader = mockRepository.Create<IElasticSearchUploader>();

            mockFileUploaderFactory
                .Setup(service => service.Create(It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpMethod>()))
                .Returns(mockFileUploader.Object);

            mockFileUploader.Setup(
                service => service.SendStreamToHostsAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<Stream>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>())).ReturnsAsync(new FileUploadResult { StatusCode = HttpStatusCode.OK });

            mockFileUploader
                .Setup(service => service.StartUploadAsync())
                .Returns(Task.CompletedTask);

            mockFileUploader
                .Setup(service => service.FinishUploadAsync())
                .Returns(Task.CompletedTask);

            mockFileUploader.Setup(service => service.SendDataToHostsAsync(1, It.IsAny<Stream>(), false, false))
                .Returns(Task.CompletedTask);

            ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Debug()
                .CreateLogger();

            using (var progressMonitor = new ProgressMonitor(new TestConsoleProgressLogger()))
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    var container = new UnityContainer();
                    container.RegisterInstance<IProgressMonitor>(progressMonitor);
                    container.RegisterInstance(mockDatabusSqlReader.Object);
                    container.RegisterInstance(mockFileUploaderFactory.Object);
                    container.RegisterInstance(mockConfigValidator.Object);
                    container.RegisterInstance(logger);
                    container.RegisterType<IPipelineExecutorFactory, SingleThreadedPipelineExecutorFactory>();

                    var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);
                    try
                    {
                        pipelineRunner.RunPipeline(job);
                    }
                    catch (OperationCanceledException e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                    catch (AggregateException e)
                    {
                        Console.WriteLine(e.Flatten().ToString());
                        throw;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }        
        
        /// <summary>
        /// The test elastic search pipeline.
        /// </summary>
        [TestMethod]
        public void TestElasticSearchPipelineMultiThreaded()
        {
            var sql = @"fake sql";

            var job = new Job
                          {
                              Config = new QueryConfig
                                           {
                                               ConnectionString = "foo",
                                               LocalSaveFolder = Path.GetTempPath(),
                                               Url = "http://foo",
                                               UploadToUrl = false,
                                               EntitiesPerUploadFile = 1,
                                               Pipeline = PipelineNames.ElasticSearch,
                                               UseMultipleThreads = true
                                           },
                              Data = new JobData
                                         {
                                             MyTopLevelDataSource = new TopLevelDataSource
                                                                        {
                                                                            Key = "AliasPatientID", Sql = sql
                                                                        }
                                         }
                          };

            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mockDatabusSqlReader = mockRepository.Create<IDatabusSqlReader>();
            mockDatabusSqlReader.Setup(
                service => service.ReadDataFromQueryAsync(
                    It.IsAny<IDataSource>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<string>())).ReturnsAsync(
                new ReadSqlDataResult
                {
                    ColumnList = new List<ColumnInfo>
                                         {
                                             new ColumnInfo { Name = "CustomerNM", ElasticSearchType = ElasticSearchTypes.keyword.ToString() },
                                             new ColumnInfo { Name = "CustomerID", ElasticSearchType = ElasticSearchTypes.integer.ToString() },
                                             new ColumnInfo { Name = "AliasPatientID", ElasticSearchType = ElasticSearchTypes.integer.ToString() },
                                             new ColumnInfo { Name = "GenderNormDSC", ElasticSearchType = ElasticSearchTypes.keyword.ToString() },
                                             new ColumnInfo { Name = "RaceNormDSC", ElasticSearchType = ElasticSearchTypes.keyword.ToString() },
                                             new ColumnInfo { Name = "MaritalStatusNormDSC", ElasticSearchType = ElasticSearchTypes.keyword.ToString() }
                                         },
                    Data = new Dictionary<string, List<object[]>>
                                   {
                                       {
                                           "ElasticsearchInputPatient",
                                           new List<object[]> { new object[] { "name", 1, 2, "M", "White", "Married" } }
                                       }
                                   }
                });

            var mockConfigValidator = mockRepository.Create<IConfigValidator>();
            mockConfigValidator.Setup(service => service.ValidateJob(It.IsAny<IJob>()));
            mockConfigValidator.Setup(service => service.ValidateDataSources(It.IsAny<IJob>()));

            var mockFileUploaderFactory = mockRepository.Create<IElasticSearchUploaderFactory>();
            var mockFileUploader = mockRepository.Create<IElasticSearchUploader>();

            mockFileUploaderFactory
                .Setup(
                    service => service.Create(
                        It.IsAny<bool>(),
                        It.IsAny<List<string>>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<HttpMethod>()))
                .Returns(mockFileUploader.Object);

            mockFileUploader.Setup(
                service => service.SendStreamToHostsAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<Stream>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new FileUploadResult { StatusCode = HttpStatusCode.OK });

            mockFileUploader
                .Setup(service => service.StartUploadAsync())
                .Returns(Task.CompletedTask);

            mockFileUploader
                .Setup(service => service.FinishUploadAsync())
                .Returns(Task.CompletedTask);

            mockFileUploader.Setup(service => service.SendDataToHostsAsync(It.IsAny<int>(), It.IsAny<Stream>(), false, false))
                .Returns(Task.CompletedTask);

            ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Debug()
                .CreateLogger();

            using (ProgressMonitor progressMonitor = new ProgressMonitor(new TestConsoleProgressLogger()))
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    var container = new UnityContainer();
                    container.RegisterInstance<IProgressMonitor>(progressMonitor);
                    container.RegisterInstance(mockDatabusSqlReader.Object);
                    container.RegisterInstance(mockFileUploaderFactory.Object);
                    container.RegisterInstance(mockConfigValidator.Object);
                    container.RegisterInstance(logger);
                    container.RegisterType<IPipelineExecutorFactory, MultiThreadedPipelineExecutorFactory>();

                    var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);
                    try
                    {
                        pipelineRunner.RunPipeline(job);
                    }
                    catch (OperationCanceledException e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                    catch (AggregateException e)
                    {
                        Console.WriteLine(e.Flatten().ToString());
                        throw;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }
    }
}
