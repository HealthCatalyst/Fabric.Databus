﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndToEndIntegrationTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the EndToEndIntegrationTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System;
    using System.Data.Common;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Http;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.PipelineRunner;
    using Fabric.Databus.Shared.Loggers;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;
    using Moq.Protected;

    using Newtonsoft.Json.Linq;

    using Unity;

    /// <summary>
    /// The end to end integration tests.
    /// </summary>
    [TestClass]
    public class EndToEndIntegrationTests
    {
        /// <summary>
        /// The test creating database.
        /// </summary>
        [TestMethod]
        public void TestCreatingDatabase()
        {
            string connectionString = "Data Source=:memory:";

            var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string sql = "create table highscores (name varchar(20), score int)";

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();

            sql = "insert into highscores (name, score) values ('Me', 9001)";

            command.CommandText = sql;
            command.ExecuteNonQuery();

            sql = "select * from highscores";

            command.CommandText = sql;
            var reader = command.ExecuteReader();

            var values = new object[2];

            while (reader.Read())
            {
                var read = reader.GetValues(values);
            }

            connection.Close();

            Assert.AreEqual("Me", values[0]);
            Assert.AreEqual(9001, values[1]);
        }

        /// <summary>
        /// The can connect to sql lite via sql connection.
        /// </summary>
        [TestMethod]
        public void CanConnectToSqlLiteViaSqlConnection()
        {
            string connectionString = "Data Source=:memory:";

            using (DbConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Close();
            }
        }

        /// <summary>
        /// The can run successfully end to end.
        /// </summary>
        [TestMethod]
        public void CanRunSingleEntityEndToEnd()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "SingleEntity.xml");
            var config = new ConfigReader().ReadXmlFromText(fileContents);

            string connectionString = "Data Source=:memory:";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // setup the database
                string sql = "CREATE TABLE Text (TextID varchar(64), PatientID int, TextTXT varchar(255))";

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();

                sql = "INSERT INTO Text (TextID, PatientID, TextTXT) values ('1', 9001, 'This is my first note')";

                command.CommandText = sql;
                command.ExecuteNonQuery();

                sql = @";WITH CTE AS ( SELECT
Text.*,Text.[TextID] AS [KeyLevel1]
FROM Text
 )  SELECT * from CTE LIMIT 1";
                command.CommandText = sql;
                command.ExecuteNonQuery();

                using (var progressMonitor = new ProgressMonitor(new TestConsoleProgressLogger()))
                {
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var container = new UnityContainer();
                        container.RegisterInstance<IProgressMonitor>(progressMonitor);
                        container.RegisterInstance<ISqlConnectionFactory>(
                            new SqlLiteConnectionFactory(new SqlLiteConnectionWrapper(connection)));

                        var integrationTestFileWriter = new IntegrationTestFileWriter { IsWritingEnabled = true };
                        container.RegisterInstance<IFileWriter>(integrationTestFileWriter);
                        container.RegisterInstance<ITemporaryFileWriter>(integrationTestFileWriter);

                        container.RegisterType<ISqlGeneratorFactory, SqlLiteGeneratorFactory>();

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

                                    var expectedByteArray = Encoding.ASCII.GetBytes($"{config.Config.ElasticSearchUserName}:{config.Config.ElasticSearchPassword}");
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

                        // Act
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);

                        pipelineRunner.RunPipeline(config);

                        // Assert
                        Assert.AreEqual(1, integrationTestFileWriter.Count);

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

            var config = new ConfigReader().ReadXmlFromText(fileContents);

            string connectionString = "Data Source=:memory:";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // setup the database
                string sql = "CREATE TABLE Text (TextID varchar(64), PatientID int, TextTXT varchar(255))";

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();

                sql = "CREATE TABLE Patients (TextID varchar(64), PatientID int, PatientLastNM varchar(255))";

                command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();

                sql = "INSERT INTO Text (TextID, PatientID, TextTXT) values ('1', 9001, 'This is my first note')";
                command.CommandText = sql;
                command.ExecuteNonQuery();

                sql = "INSERT INTO Text (TextID, PatientID, TextTXT) values ('2', 9002, 'This is my second note')";
                command.CommandText = sql;
                command.ExecuteNonQuery();

                sql = "INSERT INTO Patients (TextID, PatientID, PatientLastNM) values ('1', 9001, 'Jones')";
                command.CommandText = sql;
                command.ExecuteNonQuery();

                sql = "INSERT INTO Patients (TextID, PatientID, PatientLastNM) values ('2', 9002, 'Smith')";
                command.CommandText = sql;
                command.ExecuteNonQuery();

                using (var progressMonitor = new ProgressMonitor(new TestConsoleProgressLogger()))
                {
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var container = new UnityContainer();
                        container.RegisterInstance<IProgressMonitor>(progressMonitor);
                        container.RegisterInstance<ISqlConnectionFactory>(
                            new SqlLiteConnectionFactory(new SqlLiteConnectionWrapper(connection)));

                        var integrationTestFileWriter = new IntegrationTestFileWriter { IsWritingEnabled = true };
                        container.RegisterInstance<IFileWriter>(integrationTestFileWriter);
                        container.RegisterInstance<ITemporaryFileWriter>(integrationTestFileWriter);

                        container.RegisterType<ISqlGeneratorFactory, SqlLiteGeneratorFactory>();

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

                                    var expectedByteArray = Encoding.ASCII.GetBytes($"{config.Config.ElasticSearchUserName}:{config.Config.ElasticSearchPassword}");
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

                        Assert.AreEqual(2, integrationTestFileWriter.Count);
                        var expectedPath1 = integrationTestFileWriter.CombinePath(config.Config.LocalSaveFolder, "1.json");
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
