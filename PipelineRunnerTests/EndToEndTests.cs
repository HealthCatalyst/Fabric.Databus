// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndToEndTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the EndToEndTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using ElasticSearchApiCaller;
    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using PipelineRunner;

    using Serilog;

    using Unity;

    /// <summary>
    /// The end to end tests.
    /// </summary>
    [TestClass]
    public class EndToEndTests
    {
        /// <summary>
        /// The test elastic search pipeline.
        /// </summary>
        [TestMethod]
        public void TestElasticSearchPipelineSingleThreaded()
        {
            var sql = @"SELECT
                      CustomerNM
                      ,CustomerID
                      ,	AliasPatientID
                      ,	GenderNormDSC
                      ,RaceNormDSC
                      ,MaritalStatusNormDSC  
                      FROM CAFEEDW.SharedClinicalUnion.ElasticsearchInputPatient where CustomerID = 4";

            var job = new Job
            {
                Config = new QueryConfig
                {
                    ConnectionString = "foo",
                    LocalSaveFolder = Path.GetTempPath(),
                    TopLevelKeyColumn = "AliasPatientID",
                    Url = "http://foo",
                    UploadToElasticSearch = false,
                    EntitiesPerUploadFile = 1
                },
                Data = new JobData
                {
                    DataSources = new List<DataSource>
                                    {
                                       new DataSource
                                           {
                                               Sql = sql
                                           }
                                    }
                }
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mockDatabusSqlReader = mockRepository.Create<IDatabusSqlReader>();
            mockDatabusSqlReader.Setup(
                service => service.ReadDataFromQuery(
                    It.IsAny<IQueryConfig>(),
                    It.IsAny<IDataSource>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ILogger>())).Returns(
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

            var mockFileUploaderFactory = mockRepository.Create<IFileUploaderFactory>();
            var mockFileUploader = mockRepository.Create<IFileUploader>();

            mockFileUploaderFactory
                .Setup(service => service.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(mockFileUploader.Object);

            mockFileUploader.Setup(
                service => service.SendStreamToHosts(
                    It.IsAny<List<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<Stream>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>())).Returns(Task.CompletedTask);

            mockFileUploader
                .Setup(service => service.StartUpload(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()))
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
                    container.RegisterInstance(mockDatabusSqlReader.Object);
                    container.RegisterInstance(mockFileUploaderFactory.Object);
                    container.RegisterInstance(logger);
                    container.RegisterType<IPipelineExecutorFactory, SingleThreadedPipelineExecutorFactory>();

                    var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);
                    try
                    {
                        pipelineRunner.RunPipeline(job, progressMonitor);
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
            var sql = @"SELECT
                      CustomerNM
                      ,CustomerID
                      ,	AliasPatientID
                      ,	GenderNormDSC
                      ,RaceNormDSC
                      ,MaritalStatusNormDSC  
                      FROM CAFEEDW.SharedClinicalUnion.ElasticsearchInputPatient where CustomerID = 4";

            var job = new Job
            {
                Config = new QueryConfig
                {
                    ConnectionString = "foo",
                    LocalSaveFolder = Path.GetTempPath(),
                    TopLevelKeyColumn = "AliasPatientID",
                    Url = "http://foo",
                    UploadToElasticSearch = false,
                    EntitiesPerUploadFile = 1
                },
                Data = new JobData
                {
                    DataSources = new List<DataSource>
                                    {
                                       new DataSource
                                           {
                                               Sql = sql
                                           }
                                    }
                }
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mockDatabusSqlReader = mockRepository.Create<IDatabusSqlReader>();
            mockDatabusSqlReader.Setup(
                service => service.ReadDataFromQuery(
                    It.IsAny<IQueryConfig>(),
                    It.IsAny<IDataSource>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ILogger>())).Returns(
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

            var mockFileUploaderFactory = mockRepository.Create<IFileUploaderFactory>();
            var mockFileUploader = mockRepository.Create<IFileUploader>();

            mockFileUploaderFactory
                .Setup(service => service.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(mockFileUploader.Object);

            mockFileUploader.Setup(
                service => service.SendStreamToHosts(
                    It.IsAny<List<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<Stream>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>())).Returns(Task.CompletedTask);

            mockFileUploader
                .Setup(service => service.StartUpload(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()))
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
                    container.RegisterInstance(mockDatabusSqlReader.Object);
                    container.RegisterInstance(mockFileUploaderFactory.Object);
                    container.RegisterInstance(logger);
                    container.RegisterType<IPipelineExecutorFactory, MultiThreadedPipelineExecutorFactory>();

                    var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);
                    try
                    {
                        pipelineRunner.RunPipeline(job, progressMonitor);
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
