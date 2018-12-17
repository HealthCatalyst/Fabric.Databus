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
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.PipelineRunner;
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
        [TestMethod]
        public void CanRunSingleEntityEndToEnd()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "SingleEntity.xml");

            var sqlLines = TestFileLoader.GetFileContentsAsList("Files", "SingleEntity.sql");
            Assert.AreEqual(2, sqlLines.Count);

            var config = new ConfigReader().ReadXmlFromText(fileContents);

            config.Config.UseMultipleThreads = true;

            using (var db = new TempLocalDb("Test"))
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

                        container.RegisterType<IQueueFactory, MultiThreadedQueueFactory>();

                        // set up a mock web service
                        var mockRepository = new MockRepository(MockBehavior.Strict);
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
                            service => service.InterceptRequest(HttpMethod.Put, It.IsAny<HttpRequestMessage>()))
                            .Callback<HttpMethod, HttpRequestMessage>(
                                (method, message) =>
                                    {
                                        basicAuthorizationRequestInterceptor.InterceptRequest(method, message);
                                    })
                            .Verifiable();

                        container.RegisterInstance(mockHttpRequestInterceptor.Object);

                        var mockHttpResponseInterceptor = mockRepository.Create<IHttpResponseInterceptor>();
                        mockHttpResponseInterceptor.Setup(
                            service => service.InterceptResponse(
                                HttpMethod.Put,
                                expectedUri,
                                It.IsAny<Stream>(),
                                HttpStatusCode.OK,
                                It.IsAny<HttpContent>(),
                                It.IsAny<long>()))
                            .Verifiable();

                        container.RegisterInstance(mockHttpResponseInterceptor.Object);

                        // Act
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);

                        try
                        {
                            pipelineRunner.RunPipeline(config);
                        }
                        catch (AggregateException e)
                        {
                            foreach (var exception in e.InnerExceptions)
                            {
                                Console.WriteLine(exception);
                            }

                            throw e.Flatten();
                        }

                        // Assert
                        Assert.AreEqual(1 + 1, integrationTestFileWriter.Count); // first file is job.json

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
                            interceptor => interceptor.InterceptRequest(HttpMethod.Put, It.IsAny<HttpRequestMessage>()),
                            Times.Once);

                        mockHttpResponseInterceptor.Verify(
                            service => service.InterceptResponse(
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
        public void CanRunSingleWithIncrementalEndToEnd()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "SingleEntityIncremental.xml");
            var sqlLines = TestFileLoader.GetFileContentsAsList("Files", "SingleEntityIncremental.sql");
            Assert.AreEqual(3, sqlLines.Count);

            var config = new ConfigReader().ReadXmlFromText(fileContents);

            config.Config.UseMultipleThreads = true;

            using (var db = new TempLocalDb("Test"))
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

                        container.RegisterType<IQueueFactory, MultiThreadedQueueFactory>();

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
                            service => service.InterceptRequest(HttpMethod.Put, It.IsAny<HttpRequestMessage>()))
                            .Callback<HttpMethod, HttpRequestMessage>(
                                (method, message) =>
                                    {
                                        basicAuthorizationRequestInterceptor.InterceptRequest(method, message);
                                    })
                            .Verifiable();

                        container.RegisterInstance(mockHttpRequestInterceptor.Object);

                        var mockHttpResponseInterceptor = mockRepository.Create<IHttpResponseInterceptor>();
                        mockHttpResponseInterceptor.Setup(
                            service => service.InterceptResponse(
                                HttpMethod.Put,
                                expectedUri,
                                It.IsAny<Stream>(),
                                HttpStatusCode.OK,
                                It.IsAny<HttpContent>(),
                                It.IsAny<long>()))
                            .Verifiable();

                        container.RegisterInstance(mockHttpResponseInterceptor.Object);

                        // Act
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);

                        try
                        {
                            pipelineRunner.RunPipeline(config);
                        }
                        catch (AggregateException e)
                        {
                            throw e.Flatten();
                        }

                        // Assert
                        Assert.AreEqual(1 + 1, integrationTestFileWriter.Count, integrationTestFileWriter.GetAllFileNames().ToCsv()); // first file is job.json

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
                            interceptor => interceptor.InterceptRequest(HttpMethod.Put, It.IsAny<HttpRequestMessage>()),
                            Times.Once);

                        mockHttpResponseInterceptor.Verify(
                            service => service.InterceptResponse(
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
        public void CanRunMultipleEntitiesEndToEnd()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "MultipleEntities.xml");
            var sqlLines = TestFileLoader.GetFileContentsAsList("Files", "MultipleEntities.sql");
            Assert.AreEqual(6, sqlLines.Count);

            var config = new ConfigReader().ReadXmlFromText(fileContents);

            using (var db = new TempLocalDb("Test"))
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

                        var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);

                        pipelineRunner.RunPipeline(config);

                        Assert.AreEqual(2 + 1, integrationTestFileWriter.Count); // first file is job.json
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
    }
}
