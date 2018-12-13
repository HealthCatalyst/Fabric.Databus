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
AND [TextID] > @incrementColumnValue1
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
AND [TextID] > @incrementColumnValue1
ORDER BY Text.[TextID] ASC
";

            Assert.AreEqual(expected, sqlCommand.CommandText);
        }

        /// <summary>
        /// The create sql command succeeds.
        /// </summary>
        [TestMethod]
        public void GetQueryForEntityKeysWithJoinedEntityAndRangeAndIncrementalColumnsSucceeds()
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
Text.*,Text.[TextKEY] AS [KeyLevel2],Patient.[TextID] AS [KeyLevel1]
FROM Text
INNER JOIN Patient ON Patient.[TextKEY] = Text.[TextKEY]
WHERE 1=1
ORDER BY Text.[TextID] ASC
";

            Assert.AreEqual(expected, actual);
        }
    }
}
