// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusSqlReaderUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DatabusSqlReaderUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Database.Shared.Tests
{
    using System.Collections.Generic;
    using System.Data.SqlClient;

    using Fabric.Database.Shared.Tests.Helpers;
    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Shared;
    using Fabric.Databus.SqlGenerator;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The databus sql reader unit tests.
    /// </summary>
    [TestClass]
    public class DatabusSqlReaderUnitTests
    {
        /// <summary>
        /// The create sql command succeeds.
        /// </summary>
        [TestMethod]
        public void CreateSqlCommandWithSimpleEntitySucceeds()
        {
            var connectionString = string.Empty;
            var sqlCommandTimeoutInSeconds = 1;

            var databusSqlReader = new DatabusSqlReader(
                connectionString,
                sqlCommandTimeoutInSeconds,
                new SqlConnectionFactory(),
                new SqlGeneratorFactory());

            var topLevelKeyColumn = "TextID";
            string start = null;
            string end = null;

            var sqlCommand = databusSqlReader.CreateSqlCommand(
                new TestDataSource { TableOrView = "Text" },
                // ReSharper disable once ExpressionIsAlwaysNull
                start,
                // ReSharper disable once ExpressionIsAlwaysNull
                end,
                topLevelKeyColumn,
                new List<IIncrementalColumn>(),
                new SqlConnection(),
                "Text");

            string expected = @"SELECT
Text.*,Text.[TextID] AS [KeyLevel1]
FROM Text
WHERE 1=1
ORDER BY Text.[TextID] ASC
";

            Assert.AreEqual(expected, sqlCommand.CommandText);
        }

        /// <summary>
        /// The create sql command succeeds.
        /// </summary>
        [TestMethod]
        public void CreateSqlCommandWithSimpleEntityAndRangeSucceeds()
        {
            var connectionString = string.Empty;
            var sqlCommandTimeoutInSeconds = 1;

            var databusSqlReader = new DatabusSqlReader(
                connectionString,
                sqlCommandTimeoutInSeconds,
                new SqlConnectionFactory(),
                new SqlGeneratorFactory());

            var topLevelKeyColumn = "TextID";
            string start = "5";
            string end = "10";

            var sqlCommand = databusSqlReader.CreateSqlCommand(
                new TestDataSource { TableOrView = "Text" },
                // ReSharper disable once ExpressionIsAlwaysNull
                start,
                // ReSharper disable once ExpressionIsAlwaysNull
                end,
                topLevelKeyColumn,
                new List<IIncrementalColumn>(),
                new SqlConnection(),
                "Text");

            string expected = @"SELECT
Text.*,Text.[TextID] AS [KeyLevel1]
FROM Text
WHERE Text.[TextID] BETWEEN @start AND @end
ORDER BY Text.[TextID] ASC
";

            Assert.AreEqual(expected, sqlCommand.CommandText);
        }

        /// <summary>
        /// The create sql command succeeds.
        /// </summary>
        [TestMethod]
        public void CreateSqlCommandWithSimpleEntityAndRangeAndIncrementalColumnsSucceeds()
        {
            var connectionString = string.Empty;
            var sqlCommandTimeoutInSeconds = 1;

            var databusSqlReader = new DatabusSqlReader(
                connectionString,
                sqlCommandTimeoutInSeconds,
                new SqlConnectionFactory(),
                new SqlGeneratorFactory());

            var topLevelKeyColumn = "TextID";
            string start = "5";
            string end = "10";

            var sqlCommand = databusSqlReader.CreateSqlCommand(
                new TestDataSource { TableOrView = "Text" },
                // ReSharper disable once ExpressionIsAlwaysNull
                start,
                // ReSharper disable once ExpressionIsAlwaysNull
                end,
                topLevelKeyColumn,
                new List<IIncrementalColumn>
                    {
                        new IncrementalColumn { Name = "TextID", Operator = "GreaterThan", Value = "6" }
                    },
                new SqlConnection(),
                "Text");

            string expected = @"SELECT
Text.*,Text.[TextID] AS [KeyLevel1]
FROM Text
WHERE Text.[TextID] BETWEEN @start AND @end
AND Text.[TextID] > @incrementColumnValue1
ORDER BY Text.[TextID] ASC
";

            Assert.AreEqual(expected, sqlCommand.CommandText);
        }

        /// <summary>
        /// The create sql command succeeds.
        /// </summary>
        [TestMethod]
        public void CreateSqlCommandWithJoinedEntityAndRangeAndIncrementalColumnsSucceeds()
        {
            var connectionString = string.Empty;
            var sqlCommandTimeoutInSeconds = 1;

            var databusSqlReader = new DatabusSqlReader(
                connectionString,
                sqlCommandTimeoutInSeconds,
                new SqlConnectionFactory(),
                new SqlGeneratorFactory());

            var topLevelKeyColumn = "TextID";
            string start = "5";
            string end = "10";

            var testDataSource = new TestDataSource
            {
                TableOrView = "Text",
                Relationships = new List<ISqlRelationship>
                                                             {
                                                                 new SqlRelationship
                                                                     {
                                                                         MyDestination =
                                                                             new SqlRelationshipEntity
                                                                                 {
                                                                                     Entity = "Text", Key = "TextKEY"
                                                                                 },
                                                                         MySource = new SqlRelationshipEntity
                                                                                        {
                                                                                            Entity = "Patient",
                                                                                            Key = "TextKEY"
                                                                                        }
                                                                     },
                                                             }
            };

            var sqlCommand = databusSqlReader.CreateSqlCommand(
                testDataSource,
                // ReSharper disable once ExpressionIsAlwaysNull
                start,
                // ReSharper disable once ExpressionIsAlwaysNull
                end,
                topLevelKeyColumn,
                new List<IIncrementalColumn>
                    {
                        new IncrementalColumn { Name = "TextID", Operator = "GreaterThan", Value = "6" }
                    },
                new SqlConnection(),
                "Text");

            string expected = @"SELECT
Text.*,Text.[TextKEY] AS [KeyLevel2],Patient.[TextID] AS [KeyLevel1]
FROM Text
INNER JOIN Patient ON Patient.[TextKEY] = Text.[TextKEY]
WHERE Text.[TextID] BETWEEN @start AND @end
AND Patient.[TextID] > @incrementColumnValue1
ORDER BY Text.[TextID] ASC
";

            Assert.AreEqual(expected, sqlCommand.CommandText);

            Assert.AreEqual(3, sqlCommand.Parameters.Count);

            Assert.AreEqual("@start", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("5", sqlCommand.Parameters[0].Value);

            Assert.AreEqual("@end", sqlCommand.Parameters[1].ParameterName);
            Assert.AreEqual("10", sqlCommand.Parameters[1].Value);

            Assert.AreEqual("@incrementColumnValue1", sqlCommand.Parameters[2].ParameterName);
            Assert.AreEqual("6", sqlCommand.Parameters[2].Value);
        }

        /// <summary>
        /// The create sql command succeeds.
        /// </summary>
        [TestMethod]
        public void CreateSqlCommandWithSimpleEntityWithIncrementalSucceeds()
        {
            var connectionString = string.Empty;
            var sqlCommandTimeoutInSeconds = 1;

            var databusSqlReader = new DatabusSqlReader(
                connectionString,
                sqlCommandTimeoutInSeconds,
                new SqlConnectionFactory(),
                new SqlGeneratorFactory());

            var topLevelKeyColumn = "TextID";
            string start = null;
            string end = null;

            var sqlCommand = databusSqlReader.CreateSqlCommand(
                new TestDataSource { TableOrView = "Text" },
                // ReSharper disable once ExpressionIsAlwaysNull
                start,
                // ReSharper disable once ExpressionIsAlwaysNull
                end,
                topLevelKeyColumn,
                new List<IIncrementalColumn>
                    {
                        new IncrementalColumn
                            {
                                Name = "TextID",
                                Operator = "GreaterThan",
                                Value = "6"
                            }
                    },
                new SqlConnection(),
                "Text");

            string expected = @"SELECT
Text.*,Text.[TextID] AS [KeyLevel1]
FROM Text
WHERE 1=1
AND Text.[TextID] > @incrementColumnValue1
ORDER BY Text.[TextID] ASC
";

            Assert.AreEqual(expected, sqlCommand.CommandText);

            Assert.AreEqual(1, sqlCommand.Parameters.Count);
            Assert.AreEqual("@incrementColumnValue1", sqlCommand.Parameters[0].ParameterName);
            Assert.AreEqual("6", sqlCommand.Parameters[0].Value);
        }

        /// <summary>
        /// The get query for entity keys unit tests.
        /// </summary>
        [TestClass]
        public class GetQueryForEntityKeysUnitTests
        {
            /// <summary>
            /// The create sql command succeeds.
            /// </summary>
            [TestMethod]
            public void GetQueryForEntityKeysWithSingleEntitySucceeds()
            {
                var connectionString = string.Empty;
                var sqlCommandTimeoutInSeconds = 1;

                var databusSqlReader = new DatabusSqlReader(
                    connectionString,
                    sqlCommandTimeoutInSeconds,
                    new SqlConnectionFactory(),
                    new SqlGeneratorFactory());

                var topLevelKeyColumn = "id0";

                var testDataSource = new TestTopLevelDataSource
                                         {
                                             TableOrView = "[SAM].[HSAM].[Level0Entity]",
                                             Name = null,
                                             Sql = null,
                                             Path = "$",
                                             PropertyType = null,
                                             MySqlEntityColumnMappings = new List<SqlEntityColumnMapping>
                                                                             {
                                                                                 new SqlEntityColumnMapping
                                                                                     {
                                                                                         Name = "col02", Alias = "col02"
                                                                                     },
                                                                                 new SqlEntityColumnMapping
                                                                                     {
                                                                                         Name = "col01", Alias = "col01"
                                                                                     },
                                                                                 new SqlEntityColumnMapping
                                                                                     {
                                                                                         Name = "id0", Alias = "id0"
                                                                                     }
                                                                             }
                                         };

                var actual = databusSqlReader.GetQueryForEntityKeys(topLevelKeyColumn, 0, testDataSource);

                string expected = @"SELECT
[SAM].[HSAM].[Level0Entity].[id0]
FROM [SAM].[HSAM].[Level0Entity]
WHERE 1=1
ORDER BY [SAM].[HSAM].[Level0Entity].[id0] ASC
";

                Assert.AreEqual(expected, actual);
            }
            
            /// <summary>
            /// The create sql command succeeds.
            /// </summary>
            [TestMethod]
            public void GetQueryForEntityKeysWithSingleEntityAndMaximumEntitiesSucceeds()
            {
                var connectionString = string.Empty;
                var sqlCommandTimeoutInSeconds = 1;

                var databusSqlReader = new DatabusSqlReader(
                    connectionString,
                    sqlCommandTimeoutInSeconds,
                    new SqlConnectionFactory(),
                    new SqlGeneratorFactory());

                var topLevelKeyColumn = "id0";

                var testDataSource = new TestTopLevelDataSource
                                         {
                                             TableOrView = "[SAM].[HSAM].[Level0Entity]",
                                             Name = null,
                                             Sql = null,
                                             Path = "$",
                                             PropertyType = null,
                                             MySqlEntityColumnMappings = new List<SqlEntityColumnMapping>
                                                                             {
                                                                                 new SqlEntityColumnMapping
                                                                                     {
                                                                                         Name = "col02", Alias = "col02"
                                                                                     },
                                                                                 new SqlEntityColumnMapping
                                                                                     {
                                                                                         Name = "col01", Alias = "col01"
                                                                                     },
                                                                                 new SqlEntityColumnMapping
                                                                                     {
                                                                                         Name = "id0", Alias = "id0"
                                                                                     }
                                                                             }
                                         };

                var actual = databusSqlReader.GetQueryForEntityKeys(topLevelKeyColumn, 15, testDataSource);

                string expected = @"SELECT
TOP 15
[SAM].[HSAM].[Level0Entity].[id0]
FROM [SAM].[HSAM].[Level0Entity]
WHERE 1=1
ORDER BY [SAM].[HSAM].[Level0Entity].[id0] ASC
";

                Assert.AreEqual(expected, actual);
            }

            /// <summary>
            /// The create sql command succeeds.
            /// </summary>
            [TestMethod]
            public void GetQueryForEntityKeysWithJoinedEntityAndRangeAndRelationshipsSucceeds()
            {
                var connectionString = string.Empty;
                var sqlCommandTimeoutInSeconds = 1;

                var databusSqlReader = new DatabusSqlReader(
                    connectionString,
                    sqlCommandTimeoutInSeconds,
                    new SqlConnectionFactory(),
                    new SqlGeneratorFactory());

                var topLevelKeyColumn = "TextID";

                var testDataSource = new TestTopLevelDataSource
                {
                    TableOrView = "Text",
                    Relationships = new List<ISqlRelationship>
                                                             {
                                                                 new SqlRelationship
                                                                     {
                                                                         MyDestination =
                                                                             new SqlRelationshipEntity
                                                                                 {
                                                                                     Entity = "Text", Key = "TextKEY"
                                                                                 },
                                                                         MySource = new SqlRelationshipEntity
                                                                                        {
                                                                                            Entity = "Patient",
                                                                                            Key = "TextKEY"
                                                                                        }
                                                                     },
                                                             }
                };

                var actual = databusSqlReader.GetQueryForEntityKeys(topLevelKeyColumn, 10, testDataSource);

                string expected = @"SELECT
TOP 10
Text.[TextID]
FROM Text
WHERE 1=1
ORDER BY Text.[TextID] ASC
";

                Assert.AreEqual(expected, actual);
            }
        }
    }
}
