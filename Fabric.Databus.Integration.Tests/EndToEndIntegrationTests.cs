// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndToEndIntegrationTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the EndToEndIntegrationTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.Threading;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.PipelineRunner;
    using Fabric.Databus.Shared.Loggers;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

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

                string foo = @";WITH CTE AS ( SELECT
Text.*,Text.[TextID] AS [KeyLevel1]
FROM Text
 )  SELECT * from CTE LIMIT 1";
                command.CommandText = foo;
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

                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);

                        var fileContents = TestFileLoader.GetFileContents("Files", "SingleEntity.xml");

                        var config = new ConfigReader().ReadXmlFromText(fileContents);

                        pipelineRunner.RunRestApiPipeline(config);

                        Assert.AreEqual(1, integrationTestFileWriter.Count);

                        JObject expectedJson = new JObject(
                            new JProperty("TextID", "1"),
                            new JProperty("PatientID", 9001),
                            new JProperty("TextTXT", "This is my first note"));

                        var expectedPath = integrationTestFileWriter.CombinePath(config.Config.LocalSaveFolder, "1.json");
                        Assert.IsTrue(integrationTestFileWriter.ContainsFile(expectedPath));

                        Assert.IsTrue(
                            JToken.DeepEquals(
                                expectedJson,
                                JObject.Parse(integrationTestFileWriter.GetContents(expectedPath))));

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

                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);

                        var fileContents = TestFileLoader.GetFileContents("Files", "MultipleEntities.xml");

                        var config = new ConfigReader().ReadXmlFromText(fileContents);

                        pipelineRunner.RunRestApiPipeline(config);

                        Assert.AreEqual(2, integrationTestFileWriter.Count);
                        var expectedPath1 = integrationTestFileWriter.CombinePath(config.Config.LocalSaveFolder, "1.json");
                        Assert.IsTrue(integrationTestFileWriter.ContainsFile(expectedPath1));

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

                        var contents = integrationTestFileWriter.GetContents(expectedPath1);
                        var actualJson1 = JObject.Parse(contents);
                        Assert.IsTrue(JToken.DeepEquals(expectedJson1, actualJson1), $"Expected:<{expectedJson1}>. Actual<{actualJson1}>");

                        var expectedPath2 = integrationTestFileWriter.CombinePath(config.Config.LocalSaveFolder, "2.json");
                        Assert.IsTrue(integrationTestFileWriter.ContainsFile(expectedPath2));
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

                        var contents2 = integrationTestFileWriter.GetContents(expectedPath2);
                        var actualJson2 = JObject.Parse(contents2);
                        Assert.IsTrue(JToken.DeepEquals(expectedJson2, actualJson2), $"Expected:<{expectedJson2}>. Actual<{actualJson2}>");

                        stopwatch.Stop();
                    }

                    connection.Close();
                }
            }
        }
    }
}
