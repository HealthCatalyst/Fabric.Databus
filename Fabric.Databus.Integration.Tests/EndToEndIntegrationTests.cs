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
        public void CanRunSuccessfullyEndToEnd()
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

                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);

                        var config = new Job
                                         {
                                             Config = new QueryConfig
                                                          {
                                                              ConnectionString = connectionString,
                                                              TopLevelKeyColumn = "TextID",
                                                              UseMultipleThreads = false,
                                                              LocalSaveFolder = "foo"
                                                          },
                                             Data = new JobData
                                                        {
                                                            MyDataSources = new List<DataSource>
                                                                                {
                                                                                    new DataSource
                                                                                        {
                                                                                            Name = "First",
                                                                                            Path = "$",
                                                                                            TableOrView = "Text",
                                                                                            MyRelationships = new List<SqlRelationship>(),
                                                                                            MySqlEntityColumnMappings = new List<SqlEntityColumnMapping>()
                                                                                        }
                                                                                }
                                                        }
                                         };

                        pipelineRunner.RunRestApiPipeline(config);

                        Assert.AreEqual(1, integrationTestFileWriter.Count);
                        string expected =
                            @"{""TextID"":""1"",""PatientID"":9001,""TextTXT"":""This is my first note""}";

                        var expectedPath = integrationTestFileWriter.CombinePath(config.Config.LocalSaveFolder, "1.json");
                        Assert.IsTrue(integrationTestFileWriter.ContainsFile(expectedPath));
                        Assert.AreEqual(expected, integrationTestFileWriter.GetContents(expectedPath));

                        stopwatch.Stop();
                    }

                    connection.Close();
                }
            }
        }
    }
}
