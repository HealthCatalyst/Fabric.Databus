// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleThreadedEndToEndIntegrationTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SingleThreadedEndToEndIntegrationTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Database.Testing.FileLoader;
    using Fabric.Database.Testing.LocalDb;
    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.Integration.Tests.Helpers;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.Shared.Loggers;
    using Fabric.Databus.Shared.Queues;
    using Fabric.Databus.SqlGenerator;
    using Fabric.Shared;
    using Fabric.Shared.ReliableHttp.Interceptors;
    using Fabric.Shared.ReliableHttp.Interfaces;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;
    using Moq.Protected;

    using Newtonsoft.Json.Linq;

    using Unity;

    /// <summary>
    /// The end to end integration tests.
    /// </summary>
    [TestClass]
    public class MultiThreadedEndToEndIntegrationTests
    {
        /// <summary>
        /// The can run successfully end to end.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task CanRunSingleEntityEndToEnd()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "SingleEntity.xml");

            var sqlLines = TestFileLoader.GetFileContentsAsList("Files", "SingleEntity.sql");
            Assert.AreEqual(2, sqlLines.Count);

            var config = new ConfigReader().ReadXmlFromText(fileContents);

            config.Config.UseMultipleThreads = true;

            using (var db = new TempLocalDb("Test1"))
            using (var connection = db.CreateConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();

                // setup the database
                foreach (var sql in sqlLines)
                {
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }

                command.CommandText = @";WITH CTE AS ( SELECT
Text.*,Text.[TextID] AS [KeyLevel1]
FROM Text
 )  SELECT * from CTE";
                command.ExecuteNonQuery();

                using (var progressMonitor = new ProgressMonitor(new TestConsoleProgressLogger()))
                {
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var container = new UnityContainer();
                        container.RegisterInstance<IProgressMonitor>(progressMonitor);
                        container.RegisterInstance<ISqlConnectionFactory>(
                            new SqlReuseConnectionFactory(new DbConnectionWrapper(connection)));

                        var integrationTestFileWriter = new IntegrationTestFileWriter { IsWritingEnabled = true };
                        container.RegisterInstance<IFileWriter>(integrationTestFileWriter);
                        container.RegisterInstance<ITemporaryFileWriter>(integrationTestFileWriter);

                        container.RegisterType<ISqlGeneratorFactory, SqlGeneratorFactory>();

                        container.RegisterType<IQueueFactory, InMemoryQueueFactory>();

                        // set up a mock web service
                        var mockRepository = new MockRepository(MockBehavior.Strict);

                        var actualJsonObjects = new Dictionary<string, JObject>();

                        var mockEntitySavedToJsonLogger = mockRepository.Create<IEntitySavedToJsonLogger>();
                        mockEntitySavedToJsonLogger.Setup(service => service.IsWritingEnabled).Returns(true);
                        mockEntitySavedToJsonLogger
                            .Setup(service => service.LogSavedEntityAsync(It.IsAny<string>(), It.IsAny<Stream>()))
                            .Callback<string, Stream>(
                                (workItemId, stream) =>
                                    {
                                        stream.Seek(0, SeekOrigin.Begin);

                                        // verify that Byte Order Mark (BOM) is not included
                                        using (var reader = new BinaryReader(
                                            stream,
                                            Encoding.UTF8,
                                            true))
                                        {
                                            var buffer = reader.ReadBytes(3);

                                            Assert.AreNotEqual(239, buffer[0], "BOM was found in string");
                                            Assert.AreEqual((int)'{', buffer[0]);
                                        }

                                        stream.Seek(0, SeekOrigin.Begin);

                                        using (var streamReader = new StreamReader(
                                            stream,
                                            Encoding.UTF8,
                                            true,
                                            1024,
                                            true))
                                        {
                                            actualJsonObjects.Add(workItemId, JObject.Parse(streamReader.ReadToEnd()));
                                        }
                                    }).Returns(Task.CompletedTask);

                        container.RegisterInstance<IEntitySavedToJsonLogger>(mockEntitySavedToJsonLogger.Object);

                        var mockHttpRequestLogger = mockRepository.Create<IHttpRequestLogger>();
                        mockHttpRequestLogger.Setup(
                                service => service.LogRequestAsync(It.IsAny<string>(), HttpMethod.Put, It.IsAny<HttpRequestMessage>()))
                            .Callback<string, HttpMethod, HttpRequestMessage>(
                                async (requestId, method, request) =>
                                    {
                                        var buffer = await request.Content.ReadAsByteArrayAsync();

                                        // verify that Byte Order Mark (BOM) is not included
                                        Assert.AreNotEqual(239, buffer[0], "BOM was found in string");
                                        Assert.AreEqual((int)'{', buffer[0]);
                                    })
                            .Returns(Task.CompletedTask);

                        container.RegisterInstance<IHttpRequestLogger>(mockHttpRequestLogger.Object);

                        var testBatchEventsLogger = new TestBatchEventsLogger();
                        container.RegisterInstance<IBatchEventsLogger>(testBatchEventsLogger);

                        var testJobEventsLogger = new TestJobEventsLogger();
                        container.RegisterInstance<IJobEventsLogger>(testJobEventsLogger);

                        var testQuerySqlLogger = new TestQuerySqlLogger();
                        container.RegisterInstance<IQuerySqlLogger>(testQuerySqlLogger);

                        JObject expectedJson = new JObject(
                            new JProperty("TextID", "1"),
                            new JProperty("PatientID", 9001),
                            new JProperty("TextTXT", "This is my first note"));

                        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                        handlerMock
                            .Protected()
                            .Setup<Task<HttpResponseMessage>>(
                                "SendAsync",
                                ItExpr.IsAny<HttpRequestMessage>(),
                                ItExpr.IsAny<CancellationToken>())
                            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
                                {
                                    var content = request.Content.ReadAsStringAsync().Result;
                                    Assert.IsTrue(JToken.DeepEquals(expectedJson, JObject.Parse(content)), content);

                                    Assert.AreEqual("Basic", request.Headers.Authorization.Scheme);
                                    var actualParameter = request.Headers.Authorization.Parameter;

                                    var expectedByteArray = Encoding.ASCII.GetBytes($"{config.Config.UrlUserName}:{config.Config.UrlPassword}");
                                    var expectedParameter = Convert.ToBase64String(expectedByteArray);

                                    Assert.AreEqual(expectedParameter, actualParameter);
                                })
                            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(string.Empty)
                            })
                            .Verifiable();

                        var expectedUri = new Uri("http://foo");

                        var mockHttpClientFactory = mockRepository.Create<IHttpClientFactory>();
                        mockHttpClientFactory.Setup(service => service.Create())
                            .Returns(new HttpClient(handlerMock.Object));

                        container.RegisterInstance(mockHttpClientFactory.Object);

                        var basicAuthorizationRequestInterceptor = new BasicAuthorizationRequestInterceptor(
                            config.Config.UrlUserName,
                            config.Config.UrlPassword);

                        var mockHttpRequestInterceptor = mockRepository.Create<IHttpRequestInterceptor>();
                        mockHttpRequestInterceptor.Setup(
                            service => service.InterceptRequestAsync(It.IsAny<string>(), HttpMethod.Put, It.IsAny<HttpRequestMessage>()))
                            .Callback<string, HttpMethod, HttpRequestMessage>(
                                async (requestId, method, message) =>
                                    {
                                        await basicAuthorizationRequestInterceptor.InterceptRequestAsync(requestId, method, message);
                                    })
                            .Returns(Task.CompletedTask)
                            .Verifiable();

                        container.RegisterInstance(mockHttpRequestInterceptor.Object);

                        var mockHttpResponseInterceptor = mockRepository.Create<IHttpResponseInterceptor>();
                        mockHttpResponseInterceptor.Setup(
                            service => service.InterceptResponseAsync(
                                It.IsAny<string>(),
                                HttpMethod.Put,
                                expectedUri,
                                It.IsAny<Stream>(),
                                HttpStatusCode.OK,
                                It.IsAny<HttpContent>(),
                                It.IsAny<long>()))
                            .Returns(Task.CompletedTask)
                            .Verifiable();

                        container.RegisterInstance(mockHttpResponseInterceptor.Object);

                        // Act
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner.PipelineRunner(container, cancellationTokenSource.Token);

                        try
                        {
                            await pipelineRunner.RunPipelineAsync(config);
                        }
                        catch (AggregateException e)
                        {
                            foreach (var inner in e.InnerExceptions)
                            {
                                Console.WriteLine(inner);
                            }

                            throw e.Flatten();
                        }

                        // Assert
                        Assert.AreEqual(1, testBatchEventsLogger.BatchStartedQueueItems.Count);
                        var batchStartedQueueItem = testBatchEventsLogger.BatchStartedQueueItems.First();
                        Assert.AreEqual(1, batchStartedQueueItem.BatchNumber);
                        Assert.AreEqual(1, batchStartedQueueItem.NumberOfEntities);

                        Assert.AreEqual(1, testBatchEventsLogger.BatchCompletedQueueItems.Count);
                        var batchCompletedQueueItem = testBatchEventsLogger.BatchCompletedQueueItems.First();
                        Assert.AreEqual(1, batchCompletedQueueItem.BatchNumber);
                        Assert.AreEqual(1, batchCompletedQueueItem.NumberOfEntities);

                        Assert.AreEqual(1, testJobEventsLogger.JobCompletedQueueItems.Count);
                        Assert.AreEqual(1, testJobEventsLogger.JobCompletedQueueItems.First().NumberOfEntities);
                        Assert.AreEqual(1, testJobEventsLogger.JobCompletedQueueItems.First().NumberOfEntitiesUploaded);

                        Assert.AreEqual(1, testQuerySqlLogger.QuerySqlStartedEvents.Count);
                        Assert.AreEqual(1, testQuerySqlLogger.QuerySqlCompletedEvents.Count);

                        Assert.AreEqual(1, actualJsonObjects.Count);
                        mockEntitySavedToJsonLogger.Verify(
                            service => service.LogSavedEntityAsync(It.IsAny<string>(), It.IsAny<Stream>()),
                            Times.Once);

                        var expectedPath = integrationTestFileWriter.CombinePath(config.Config.LocalSaveFolder, "1.json");
                        Assert.IsTrue(integrationTestFileWriter.ContainsFile(expectedPath));

                        Assert.IsTrue(
                            JToken.DeepEquals(
                                expectedJson,
                                JObject.Parse(integrationTestFileWriter.GetContents(expectedPath))));

                        handlerMock.Protected()
                            .Verify(
                                "SendAsync",
                                Times.Exactly(1),
                                ItExpr.Is<HttpRequestMessage>(
                                    req => req.Method == HttpMethod.Put
                                           && req.RequestUri == expectedUri),
                                ItExpr.IsAny<CancellationToken>());

                        mockHttpRequestInterceptor.Verify(
                            interceptor => interceptor.InterceptRequestAsync(It.IsAny<string>(), HttpMethod.Put, It.IsAny<HttpRequestMessage>()),
                            Times.Once);

                        mockHttpResponseInterceptor.Verify(
                            service => service.InterceptResponseAsync(
                                It.IsAny<string>(),
                                HttpMethod.Put,
                                expectedUri,
                                It.IsAny<Stream>(),
                                HttpStatusCode.OK,
                                It.IsAny<HttpContent>(),
                                It.IsAny<long>()),
                            Times.Once);
                        stopwatch.Stop();
                    }

                    connection.Close();
                }
            }
        }

        /// <summary>
        /// The can run successfully end to end.
        /// </summary>
        [TestMethod]
        public async Task CanRunSingleWithIncrementalEndToEnd()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "SingleEntityIncremental.xml");
            var sqlLines = TestFileLoader.GetFileContentsAsList("Files", "SingleEntityIncremental.sql");
            Assert.AreEqual(3, sqlLines.Count);

            var config = new ConfigReader().ReadXmlFromText(fileContents);

            config.Config.UseMultipleThreads = true;

            using (var db = new TempLocalDb("Test2"))
            using (var connection = db.CreateConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();

                // setup the database
                foreach (var sql in sqlLines)
                {
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }

                using (var progressMonitor = new ProgressMonitor(new TestConsoleProgressLogger()))
                {
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var container = new UnityContainer();
                        container.RegisterInstance<IProgressMonitor>(progressMonitor);
                        container.RegisterInstance<ISqlConnectionFactory>(
                            new SqlReuseConnectionFactory(new DbConnectionWrapper(connection)));

                        var integrationTestFileWriter = new IntegrationTestFileWriter { IsWritingEnabled = true };
                        container.RegisterInstance<IFileWriter>(integrationTestFileWriter);
                        container.RegisterInstance<ITemporaryFileWriter>(integrationTestFileWriter);

                        container.RegisterType<ISqlGeneratorFactory, SqlGeneratorFactory>();

                        container.RegisterType<IQueueFactory, InMemoryQueueFactory>();

                        // set up a mock web service
                        var mockRepository = new MockRepository(MockBehavior.Strict);
                        JObject expectedJson = new JObject(
                            new JProperty("TextID", "2"),
                            new JProperty("PatientID", 9002),
                            new JProperty("TextTXT", "This is my second note"));

                        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                        handlerMock
                            .Protected()
                            .Setup<Task<HttpResponseMessage>>(
                                "SendAsync",
                                ItExpr.IsAny<HttpRequestMessage>(),
                                ItExpr.IsAny<CancellationToken>())
                            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
                                {
                                    var content = request.Content.ReadAsStringAsync().Result;
                                    Assert.IsTrue(JToken.DeepEquals(expectedJson, JObject.Parse(content)), content);

                                    Assert.AreEqual("Basic", request.Headers.Authorization.Scheme);
                                    var actualParameter = request.Headers.Authorization.Parameter;

                                    var expectedByteArray = Encoding.ASCII.GetBytes($"{config.Config.UrlUserName}:{config.Config.UrlPassword}");
                                    var expectedParameter = Convert.ToBase64String(expectedByteArray);

                                    Assert.AreEqual(expectedParameter, actualParameter);
                                })
                            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(string.Empty)
                            })
                            .Verifiable();

                        var expectedUri = new Uri("http://foo");

                        var mockHttpClientFactory = mockRepository.Create<IHttpClientFactory>();
                        mockHttpClientFactory.Setup(service => service.Create())
                            .Returns(new HttpClient(handlerMock.Object));

                        container.RegisterInstance(mockHttpClientFactory.Object);

                        var basicAuthorizationRequestInterceptor = new BasicAuthorizationRequestInterceptor(
                            config.Config.UrlUserName,
                            config.Config.UrlPassword);

                        var mockHttpRequestInterceptor = mockRepository.Create<IHttpRequestInterceptor>();
                        mockHttpRequestInterceptor.Setup(
                            service => service.InterceptRequestAsync(It.IsAny<string>(), HttpMethod.Put, It.IsAny<HttpRequestMessage>()))
                            .Callback<string, HttpMethod, HttpRequestMessage>(
                                async (requestId, method, message) =>
                                    {
                                        await basicAuthorizationRequestInterceptor.InterceptRequestAsync(requestId, method, message);
                                    })
                            .Returns(Task.CompletedTask)
                            .Verifiable();

                        container.RegisterInstance(mockHttpRequestInterceptor.Object);

                        var mockHttpResponseInterceptor = mockRepository.Create<IHttpResponseInterceptor>();
                        mockHttpResponseInterceptor.Setup(
                            service => service.InterceptResponseAsync(
                                It.IsAny<string>(),
                                HttpMethod.Put,
                                expectedUri,
                                It.IsAny<Stream>(),
                                HttpStatusCode.OK,
                                It.IsAny<HttpContent>(),
                                It.IsAny<long>()))
                            .Returns(Task.CompletedTask)
                            .Verifiable();

                        container.RegisterInstance(mockHttpResponseInterceptor.Object);

                        // Act
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner.PipelineRunner(container, cancellationTokenSource.Token);

                        try
                        {
                            await pipelineRunner.RunPipelineAsync(config);
                        }
                        catch (AggregateException e)
                        {
                            foreach (var inner in e.InnerExceptions)
                            {
                                Console.WriteLine(inner);
                            }

                            throw e.Flatten();
                        }

                        // Assert
                        var expectedPath = integrationTestFileWriter.CombinePath(
                            config.Config.LocalSaveFolder,
                            "2.json");
                        Assert.IsTrue(
                            integrationTestFileWriter.ContainsFile(expectedPath),
                            $"{expectedPath} not found in {integrationTestFileWriter.GetAllFileNames().ToCsv()}");

                        Assert.IsTrue(
                            JToken.DeepEquals(
                                expectedJson,
                                JObject.Parse(integrationTestFileWriter.GetContents(expectedPath))));

                        handlerMock.Protected()
                            .Verify(
                                "SendAsync",
                                Times.Exactly(1),
                                ItExpr.Is<HttpRequestMessage>(
                                    req => req.Method == HttpMethod.Put
                                           && req.RequestUri == expectedUri),
                                ItExpr.IsAny<CancellationToken>());

                        mockHttpRequestInterceptor.Verify(
                            interceptor => interceptor.InterceptRequestAsync(It.IsAny<string>(), HttpMethod.Put, It.IsAny<HttpRequestMessage>()),
                            Times.Once);

                        mockHttpResponseInterceptor.Verify(
                            service => service.InterceptResponseAsync(It.IsAny<string>(), HttpMethod.Put, expectedUri, It.IsAny<Stream>(), HttpStatusCode.OK, It.IsAny<HttpContent>(), It.IsAny<long>()),
                            Times.Once);
                        stopwatch.Stop();
                    }

                    connection.Close();
                }
            }
        }

        /// <summary>
        /// The can run successfully end to end.
        /// </summary>
        [TestMethod]
        public async Task CanRunMultipleEntitiesEndToEnd()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "MultipleEntities.xml");
            var sqlLines = TestFileLoader.GetFileContentsAsList("Files", "MultipleEntities.sql");
            Assert.AreEqual(6, sqlLines.Count);

            var config = new ConfigReader().ReadXmlFromText(fileContents);

            using (var db = new TempLocalDb("Test3"))
            using (var connection = db.CreateConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();

                // setup the database
                foreach (var sql in sqlLines)
                {
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }

                using (var progressMonitor = new ProgressMonitor(new TestConsoleProgressLogger()))
                {
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var container = new UnityContainer();
                        container.RegisterInstance<IProgressMonitor>(progressMonitor);
                        container.RegisterInstance<ISqlConnectionFactory>(
                            new SqlReuseConnectionFactory(new DbConnectionWrapper(connection)));

                        var integrationTestFileWriter = new IntegrationTestFileWriter { IsWritingEnabled = true };
                        container.RegisterInstance<IFileWriter>(integrationTestFileWriter);
                        container.RegisterInstance<ITemporaryFileWriter>(integrationTestFileWriter);

                        container.RegisterType<ISqlGeneratorFactory, SqlGeneratorFactory>();

                        JObject expectedJson1 = new JObject(
                            new JProperty("TextID", "1"),
                            new JProperty("PatientID", 9001),
                            new JProperty("TextTXT", "This is my first note"),
                            new JProperty(
                                "patients",
                                new JObject(
                                    new JProperty("TextID", "1"),
                                    new JProperty("PatientID", 9001),
                                    new JProperty("PatientLastNM", "Jones"))));

                        JObject expectedJson2 = new JObject(
                            new JProperty("TextID", "2"),
                            new JProperty("PatientID", 9002),
                            new JProperty("TextTXT", "This is my second note"),
                            new JProperty(
                                "patients",
                                new JObject(
                                    new JProperty("TextID", "2"),
                                    new JProperty("PatientID", 9002),
                                    new JProperty("PatientLastNM", "Smith"))));


                        // set up a mock web service
                        var mockRepository = new MockRepository(MockBehavior.Strict);

                        var actualJsonObjects = new Dictionary<string, JObject>();

                        var mockEntitySavedToJsonLogger = mockRepository.Create<IEntitySavedToJsonLogger>();
                        mockEntitySavedToJsonLogger.Setup(service => service.IsWritingEnabled).Returns(true);
                        mockEntitySavedToJsonLogger
                            .Setup(service => service.LogSavedEntityAsync(It.IsAny<string>(), It.IsAny<Stream>()))
                            .Callback<string, Stream>(
                                (workItemId, stream) =>
                                    {
                                        using (var streamReader = new StreamReader(
                                            stream,
                                            Encoding.UTF8,
                                            true,
                                            1024,
                                            true))
                                        {
                                            actualJsonObjects.Add(workItemId, JObject.Parse(streamReader.ReadToEnd()));
                                        }
                                    })
                            .Returns(Task.CompletedTask);
                        container.RegisterInstance(mockEntitySavedToJsonLogger.Object);

                        int numHttpCall = 0;
                        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                        httpMessageHandlerMock
                            .Protected()
                            .Setup<Task<HttpResponseMessage>>(
                                "SendAsync",
                                ItExpr.IsAny<HttpRequestMessage>(),
                                ItExpr.IsAny<CancellationToken>())
                            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
                                {
                                    numHttpCall++;
                                    var content = request.Content.ReadAsStringAsync().Result;
                                    var expectedJson = numHttpCall == 1 ? expectedJson1 : expectedJson2;
                                    Assert.IsTrue(JToken.DeepEquals(expectedJson, JObject.Parse(content)), content);

                                    Assert.AreEqual("Basic", request.Headers.Authorization.Scheme);
                                    var actualParameter = request.Headers.Authorization.Parameter;

                                    var expectedByteArray = Encoding.ASCII.GetBytes($"{config.Config.UrlUserName}:{config.Config.UrlPassword}");
                                    var expectedParameter = Convert.ToBase64String(expectedByteArray);

                                    Assert.AreEqual(expectedParameter, actualParameter);
                                })
                            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(string.Empty)
                            })
                            .Verifiable();

                        var expectedUri = new Uri("http://foo");

                        var mockHttpClientFactory = mockRepository.Create<IHttpClientFactory>();
                        mockHttpClientFactory.Setup(service => service.Create())
                            .Returns(new HttpClient(httpMessageHandlerMock.Object));

                        container.RegisterInstance(mockHttpClientFactory.Object);

                        // Act
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner.PipelineRunner(container, cancellationTokenSource.Token);

                        try
                        {
                            await pipelineRunner.RunPipelineAsync(config);
                        }
                        catch (AggregateException e)
                        {
                            foreach (var inner in e.InnerExceptions)
                            {
                                Console.WriteLine(inner);
                            }

                            throw e.Flatten();
                        }

                        // assert
                        Assert.AreEqual(2, actualJsonObjects.Count);

                        var expectedPath1 = integrationTestFileWriter.CombinePath(
                            config.Config.LocalSaveFolder,
                            "1.json");
                        Assert.IsTrue(integrationTestFileWriter.ContainsFile(expectedPath1));

                        var contents = integrationTestFileWriter.GetContents(expectedPath1);
                        var actualJson1 = JObject.Parse(contents);
                        Assert.IsTrue(JToken.DeepEquals(expectedJson1, actualJson1), $"Expected:<{expectedJson1}>. Actual<{actualJson1}>");

                        var expectedPath2 = integrationTestFileWriter.CombinePath(config.Config.LocalSaveFolder, "2.json");
                        Assert.IsTrue(integrationTestFileWriter.ContainsFile(expectedPath2));
                        var contents2 = integrationTestFileWriter.GetContents(expectedPath2);
                        var actualJson2 = JObject.Parse(contents2);
                        Assert.IsTrue(JToken.DeepEquals(expectedJson2, actualJson2), $"Expected:<{expectedJson2}>. Actual<{actualJson2}>");

                        httpMessageHandlerMock.Protected()
                            .Verify(
                                "SendAsync",
                                Times.Exactly(2),
                                ItExpr.Is<HttpRequestMessage>(
                                    req => req.Method == HttpMethod.Put
                                           && req.RequestUri == expectedUri),
                                ItExpr.IsAny<CancellationToken>());

                        stopwatch.Stop();
                    }

                    connection.Close();
                }
            }
        }

        /// <summary>
        /// The can run successfully end to end.
        /// </summary>
        [TestMethod]
        public async Task CanRunMultipleEntitiesEndToEndWithBatching()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "MultipleEntitiesWithBatching.xml");
            var sqlLines = TestFileLoader.GetFileContentsAsList("Files", "MultipleEntitiesWithBatching.sql");
            Assert.AreEqual(11, sqlLines.Count);

            var config = new ConfigReader().ReadXmlFromText(fileContents);

            using (var db = new TempLocalDb("Test3"))
            using (var connection = db.CreateConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();

                // setup the database
                foreach (var sql in sqlLines)
                {
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }

                using (var progressMonitor = new ProgressMonitor(new TestConsoleProgressLogger()))
                {
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var container = new UnityContainer();
                        container.RegisterInstance<IProgressMonitor>(progressMonitor);
                        container.RegisterInstance<ISqlConnectionFactory>(
                            new SqlReuseConnectionFactory(new DbConnectionWrapper(connection)));

                        var integrationTestFileWriter = new IntegrationTestFileWriter { IsWritingEnabled = true };
                        container.RegisterInstance<IFileWriter>(integrationTestFileWriter);
                        container.RegisterInstance<ITemporaryFileWriter>(integrationTestFileWriter);

                        container.RegisterType<ISqlGeneratorFactory, SqlGeneratorFactory>();

                        JObject expectedJson1 = new JObject(
                            new JProperty("TextID", "1"),
                            new JProperty("PatientID", 9001),
                            new JProperty("TextTXT", "This is my first note"),
                            new JProperty(
                                "patients",
                                new JObject(
                                    new JProperty("TextID", "1"),
                                    new JProperty("PatientID", 9001),
                                    new JProperty("PatientLastNM", "Jones"))));

                        JObject expectedJson2 = new JObject(
                            new JProperty("TextID", "2"),
                            new JProperty("PatientID", 9002),
                            new JProperty("TextTXT", "This is my second note"),
                            new JProperty(
                                "patients",
                                new JObject(
                                    new JProperty("TextID", "2"),
                                    new JProperty("PatientID", 9002),
                                    new JProperty("PatientLastNM", "Smith"))));

                        // set up a mock web service
                        var mockRepository = new MockRepository(MockBehavior.Strict);

                        var actualJsonObjects = new Dictionary<string, JObject>();

                        var mockEntitySavedToJsonLogger = mockRepository.Create<IEntitySavedToJsonLogger>();
                        mockEntitySavedToJsonLogger.Setup(service => service.IsWritingEnabled).Returns(true);
                        mockEntitySavedToJsonLogger
                            .Setup(service => service.LogSavedEntityAsync(It.IsAny<string>(), It.IsAny<Stream>()))
                            .Callback<string, Stream>(
                                (workItemId, stream) =>
                                    {
                                        using (var streamReader = new StreamReader(
                                            stream,
                                            Encoding.UTF8,
                                            true,
                                            1024,
                                            true))
                                        {
                                            actualJsonObjects.Add(workItemId, JObject.Parse(streamReader.ReadToEnd()));
                                        }
                                    }).Returns(Task.CompletedTask);
                        container.RegisterInstance(mockEntitySavedToJsonLogger.Object);

                        var testBatchEventsLogger = new TestBatchEventsLogger();
                        container.RegisterInstance<IBatchEventsLogger>(testBatchEventsLogger);

                        var testJobEventsLogger = new TestJobEventsLogger();
                        container.RegisterInstance<IJobEventsLogger>(testJobEventsLogger);

                        var testQuerySqlLogger = new TestQuerySqlLogger();
                        container.RegisterInstance<IQuerySqlLogger>(testQuerySqlLogger);

                        int numHttpCall = 0;
                        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                        httpMessageHandlerMock
                            .Protected()
                            .Setup<Task<HttpResponseMessage>>(
                                "SendAsync",
                                ItExpr.IsAny<HttpRequestMessage>(),
                                ItExpr.IsAny<CancellationToken>())
                            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
                                {
                                    numHttpCall++;
                                    if (numHttpCall < 3)
                                    {
                                        var content = request.Content.ReadAsStringAsync().Result;
                                        var expectedJson = numHttpCall == 1 ? expectedJson1 : expectedJson2;
                                        Assert.IsTrue(JToken.DeepEquals(expectedJson, JObject.Parse(content)), content);

                                        Assert.AreEqual("Basic", request.Headers.Authorization.Scheme);
                                        var actualParameter = request.Headers.Authorization.Parameter;

                                        var expectedByteArray = Encoding.ASCII.GetBytes($"{config.Config.UrlUserName}:{config.Config.UrlPassword}");
                                        var expectedParameter = Convert.ToBase64String(expectedByteArray);

                                        Assert.AreEqual(expectedParameter, actualParameter);
                                    }
                                })
                            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(string.Empty)
                            })
                            .Verifiable();

                        var expectedUri = new Uri("http://foo");

                        var mockHttpClientFactory = mockRepository.Create<IHttpClientFactory>();
                        mockHttpClientFactory.Setup(service => service.Create())
                            .Returns(new HttpClient(httpMessageHandlerMock.Object));

                        container.RegisterInstance(mockHttpClientFactory.Object);

                        // Act
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner.PipelineRunner(container, cancellationTokenSource.Token);

                        try
                        {
                            await pipelineRunner.RunPipelineAsync(config);
                        }
                        catch (AggregateException e)
                        {
                            foreach (var inner in e.InnerExceptions)
                            {
                                Console.WriteLine(inner);
                            }

                            throw e.Flatten();
                        }

                        // assert
                        Assert.AreEqual(2, testBatchEventsLogger.BatchStartedQueueItems.Count);
                        var batchStartedQueueItem = testBatchEventsLogger.BatchStartedQueueItems.First();
                        Assert.AreEqual(1, batchStartedQueueItem.BatchNumber);
                        Assert.AreEqual(3, batchStartedQueueItem.NumberOfEntities);

                        Assert.AreEqual(2, testBatchEventsLogger.BatchCompletedQueueItems.Count);
                        var batchCompletedQueueItem = testBatchEventsLogger.BatchCompletedQueueItems.First();
                        Assert.AreEqual(1, batchCompletedQueueItem.BatchNumber);
                        Assert.AreEqual(3, batchCompletedQueueItem.NumberOfEntities);
                        Assert.AreEqual(3, batchCompletedQueueItem.NumberOfEntitiesUploaded);

                        batchCompletedQueueItem = testBatchEventsLogger.BatchCompletedQueueItems.Skip(1).First();
                        Assert.AreEqual(2, batchCompletedQueueItem.BatchNumber);
                        Assert.AreEqual(2, batchCompletedQueueItem.NumberOfEntities);
                        Assert.AreEqual(2, batchCompletedQueueItem.NumberOfEntitiesUploaded);

                        const int NumberOfEntities = 5;

                        Assert.AreEqual(2 + 2, testQuerySqlLogger.QuerySqlCompletedEvents.Count); // one per entity per batch
                        Assert.AreEqual(1, testQuerySqlLogger.QuerySqlCompletedEvents.First().BatchNumber);
                        Assert.AreEqual(3, testQuerySqlLogger.QuerySqlCompletedEvents.First().RowCount);
                        Assert.AreEqual(1, testQuerySqlLogger.QuerySqlCompletedEvents.Skip(1).First().BatchNumber);
                        Assert.AreEqual(3, testQuerySqlLogger.QuerySqlCompletedEvents.Skip(1).First().RowCount);
                        Assert.AreEqual(2, testQuerySqlLogger.QuerySqlCompletedEvents.Skip(2).First().BatchNumber);
                        Assert.AreEqual(2, testQuerySqlLogger.QuerySqlCompletedEvents.Skip(2).First().RowCount);
                        Assert.AreEqual(2, testQuerySqlLogger.QuerySqlCompletedEvents.Skip(3).First().BatchNumber);
                        Assert.AreEqual(1, testQuerySqlLogger.QuerySqlCompletedEvents.Skip(3).First().RowCount);

                        Assert.AreEqual(1, testJobEventsLogger.JobCompletedQueueItems.Count);
                        Assert.AreEqual(5, testJobEventsLogger.JobCompletedQueueItems.First().NumberOfEntities);
                        Assert.AreEqual(5, testJobEventsLogger.JobCompletedQueueItems.First().NumberOfEntitiesUploaded);

                        Assert.AreEqual(NumberOfEntities, actualJsonObjects.Count);

                        var expectedPath1 = integrationTestFileWriter.CombinePath(
                            config.Config.LocalSaveFolder,
                            "1.json");
                        Assert.IsTrue(integrationTestFileWriter.ContainsFile(expectedPath1));

                        var contents = integrationTestFileWriter.GetContents(expectedPath1);
                        var actualJson1 = JObject.Parse(contents);
                        Assert.IsTrue(JToken.DeepEquals(expectedJson1, actualJson1), $"Expected:<{expectedJson1}>. Actual<{actualJson1}>");

                        var expectedPath2 = integrationTestFileWriter.CombinePath(config.Config.LocalSaveFolder, "2.json");
                        Assert.IsTrue(integrationTestFileWriter.ContainsFile(expectedPath2));
                        var contents2 = integrationTestFileWriter.GetContents(expectedPath2);
                        var actualJson2 = JObject.Parse(contents2);
                        Assert.IsTrue(JToken.DeepEquals(expectedJson2, actualJson2), $"Expected:<{expectedJson2}>. Actual<{actualJson2}>");

                        httpMessageHandlerMock.Protected()
                            .Verify(
                                "SendAsync",
                                Times.Exactly(NumberOfEntities),
                                ItExpr.Is<HttpRequestMessage>(
                                    req => req.Method == HttpMethod.Put
                                           && req.RequestUri == expectedUri),
                                ItExpr.IsAny<CancellationToken>());

                        stopwatch.Stop();
                    }

                    connection.Close();
                }
            }
        }

        /// <summary>
        /// The can run successfully end to end.
        /// </summary>
        [TestMethod]
        public async Task CanRunNestedEntitiesEndToEndWithBatching()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "NestedEntitiesWithBatching.xml");
            var sqlLines = TestFileLoader.GetFileContentsAsList("Files", "NestedEntitiesWithBatching.sql");
            Assert.AreEqual(22, sqlLines.Count);

            var config = new ConfigReader().ReadXmlFromText(fileContents);

            using (var db = new TempLocalDb("NestedEntitiesWithBatching"))
            using (var connection = db.CreateConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();

                // setup the database
                foreach (var sql in sqlLines)
                {
                    if (!string.IsNullOrWhiteSpace(sql))
                    {
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }

                var files = new[]
                {
                    "HCOSText.DataBASE.Table.sql",
                    "HCOSText.PatientBASE.Table.sql",
                    "HCOSText.VisitBASE.Table.sql",
                    "HCOSText.VisitFacilityBASE.Table.sql",
                    "HCOSText.VisitPeopleBASE.Table.sql",
                    "HCOSText.DocumentBASE.Table.sql",
                    "HCOSText.DocumentPeopleBASE.Table.sql"
                };

                foreach (var file in files)
                {
                    var contents = TestFileLoader.GetFileContents("Files\\NestedEntities", file);
                    if (!string.IsNullOrWhiteSpace(contents))
                    {
                        command.CommandText = contents;
                        command.ExecuteNonQuery();
                    }
                }

                using (var progressMonitor = new ProgressMonitor(new TestConsoleProgressLogger()))
                {
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var container = new UnityContainer();
                        container.RegisterInstance<IProgressMonitor>(progressMonitor);
                        container.RegisterInstance<ISqlConnectionFactory>(
                            new SqlReuseConnectionFactory(new DbConnectionWrapper(connection)));

                        var integrationTestFileWriter = new IntegrationTestFileWriter { IsWritingEnabled = true };
                        container.RegisterInstance<IFileWriter>(integrationTestFileWriter);
                        container.RegisterInstance<ITemporaryFileWriter>(integrationTestFileWriter);

                        container.RegisterType<ISqlGeneratorFactory, SqlGeneratorFactory>();

                        // set up a mock web service
                        var mockRepository = new MockRepository(MockBehavior.Strict);

                        var actualJsonObjects = new Dictionary<string, JObject>();

                        var mockEntitySavedToJsonLogger = mockRepository.Create<IEntitySavedToJsonLogger>();
                        mockEntitySavedToJsonLogger.Setup(service => service.IsWritingEnabled).Returns(true);
                        mockEntitySavedToJsonLogger
                            .Setup(service => service.LogSavedEntityAsync(It.IsAny<string>(), It.IsAny<Stream>()))
                            .Callback<string, Stream>(
                                (workItemId, stream) =>
                                    {
                                        using (var streamReader = new StreamReader(
                                            stream,
                                            Encoding.UTF8,
                                            true,
                                            1024,
                                            true))
                                        {
                                            actualJsonObjects.Add(workItemId, JObject.Parse(streamReader.ReadToEnd()));
                                        }
                                    }).Returns(Task.CompletedTask);
                        container.RegisterInstance(mockEntitySavedToJsonLogger.Object);

                        var testBatchEventsLogger = new TestBatchEventsLogger();
                        container.RegisterInstance<IBatchEventsLogger>(testBatchEventsLogger);

                        var testJobEventsLogger = new TestJobEventsLogger();
                        container.RegisterInstance<IJobEventsLogger>(testJobEventsLogger);

                        var testQuerySqlLogger = new TestQuerySqlLogger();
                        container.RegisterInstance<IQuerySqlLogger>(testQuerySqlLogger);

                        var expectedJsonObjects = new Dictionary<string, JObject>();

                        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                        httpMessageHandlerMock
                            .Protected()
                            .Setup<Task<HttpResponseMessage>>(
                                "SendAsync",
                                ItExpr.IsAny<HttpRequestMessage>(),
                                ItExpr.IsAny<CancellationToken>())
                            .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
                                {
                                    {
                                        var content = request.Content.ReadAsStringAsync().Result;
                                        var jObject = JObject.Parse(content);
                                        var selectToken = jObject.SelectToken("TextKEY");
                                        expectedJsonObjects.Add(selectToken.ToString(), jObject);
                                    }
                                })
                            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(string.Empty)
                            })
                            .Verifiable();

                        var expectedUri = new Uri("http://foo");

                        var mockHttpClientFactory = mockRepository.Create<IHttpClientFactory>();
                        mockHttpClientFactory.Setup(service => service.Create())
                            .Returns(new HttpClient(httpMessageHandlerMock.Object));

                        container.RegisterInstance(mockHttpClientFactory.Object);

                        // Act
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner.PipelineRunner(container, cancellationTokenSource.Token);

                        try
                        {
                            await pipelineRunner.RunPipelineAsync(config);
                        }
                        catch (AggregateException e)
                        {
                            foreach (var inner in e.InnerExceptions)
                            {
                                Console.WriteLine(inner);
                            }

                            throw e.Flatten();
                        }

                        // assert
                        Assert.AreEqual(4, testBatchEventsLogger.BatchStartedQueueItems.Count);
                        var batchStartedQueueItem = testBatchEventsLogger.BatchStartedQueueItems.First();
                        Assert.AreEqual(1, batchStartedQueueItem.BatchNumber);
                        Assert.AreEqual(3, batchStartedQueueItem.NumberOfEntities);

                        Assert.AreEqual(4, testBatchEventsLogger.BatchCompletedQueueItems.Count);
                        var batchCompletedQueueItem = testBatchEventsLogger.BatchCompletedQueueItems.First();
                        Assert.AreEqual(1, batchCompletedQueueItem.BatchNumber);
                        Assert.AreEqual(3, batchCompletedQueueItem.NumberOfEntities);
                        Assert.AreEqual(3, batchCompletedQueueItem.NumberOfEntitiesUploaded);

                        batchCompletedQueueItem = testBatchEventsLogger.BatchCompletedQueueItems.Skip(1).First();
                        Assert.AreEqual(2, batchCompletedQueueItem.BatchNumber);
                        Assert.AreEqual(3, batchCompletedQueueItem.NumberOfEntities);
                        Assert.AreEqual(3, batchCompletedQueueItem.NumberOfEntitiesUploaded);

                        const int NumberOfEntities = 10;

                        Assert.AreEqual(26 + 2, testQuerySqlLogger.QuerySqlCompletedEvents.Count); // one per entity per batch
                        Assert.AreEqual(1, testQuerySqlLogger.QuerySqlCompletedEvents.First().BatchNumber);
                        Assert.AreEqual(3, testQuerySqlLogger.QuerySqlCompletedEvents.First().RowCount);
                        Assert.AreEqual(1, testQuerySqlLogger.QuerySqlCompletedEvents.Skip(1).First().BatchNumber);

                        Assert.AreEqual(1, testJobEventsLogger.JobCompletedQueueItems.Count);
                        Assert.AreEqual(NumberOfEntities, testJobEventsLogger.JobCompletedQueueItems.First().NumberOfEntities);
                        Assert.AreEqual(NumberOfEntities, testJobEventsLogger.JobCompletedQueueItems.First().NumberOfEntitiesUploaded);

                        Assert.AreEqual(NumberOfEntities, actualJsonObjects.Count);

                        var expectedJsonFiles = new[]
                        {
                            "58010A71478E5C521A4157B2FB8E1904ACAD37C324ECFFA359F14F02B4D7F4AF.json"
                        };

                        foreach (var expectedJsonFile in expectedJsonFiles)
                        {
                            var contents = TestFileLoader.GetFileContents("Files\\NestedEntities", expectedJsonFile);
                            var entityId = expectedJsonFile.Replace(".json", string.Empty);
                            var expectedJson = expectedJsonObjects[entityId];
                            var document = JObject.Parse(contents);
                            Assert.IsTrue(JToken.DeepEquals(expectedJson, document), expectedJsonFile);
                        }

                        httpMessageHandlerMock.Protected()
                            .Verify(
                                "SendAsync",
                                Times.Exactly(NumberOfEntities),
                                ItExpr.Is<HttpRequestMessage>(
                                    req => req.Method == HttpMethod.Put
                                           && req.RequestUri == expectedUri),
                                ItExpr.IsAny<CancellationToken>());

                        stopwatch.Stop();
                    }

                    connection.Close();
                }
            }
        }
    }
}
