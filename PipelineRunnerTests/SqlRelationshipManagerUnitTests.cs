// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlRelationshipManagerUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlRelationshipManagerUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System.Collections.Generic;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.SqlGenerator;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The sql relationship manager unit tests.
    /// </summary>
    [TestClass]
    public class SqlRelationshipManagerUnitTests
    {
        /// <summary>
        /// The test no relationships.
        /// </summary>
        [TestMethod]
        public void TestNoRelationships()
        {
            var sqlStatement = SqlSelectStatementGenerator.GetSqlStatement("Text.Text", "TextID", new List<ISqlRelationship>(), new List<ISqlEntityColumnMapping>());

            string expected = @"SELECT
Text.Text.*,Text.Text.[TextID] AS [KeyLevel1]
FROM Text.Text
";
            Assert.AreEqual(expected, sqlStatement);
        }

        /// <summary>
        /// The test no relationships.
        /// </summary>
        [TestMethod]
        public void TestNoRelationshipsWithSpecificColumnsWithoutEntity()
        {
            var sqlStatement = SqlSelectStatementGenerator.GetSqlStatement(
                "Text.Text",
                "TextID",
                new List<ISqlRelationship>(),
                new List<ISqlEntityColumnMapping>
                    {
                        new SqlEntityColumnMapping
                            {
                                Name = "TextSourceDSC"
                            }
                    });


            string expected = @"SELECT
Text.Text.[TextSourceDSC],Text.Text.[TextID] AS [KeyLevel1]
FROM Text.Text
";
            Assert.AreEqual(expected, sqlStatement);
        }

        /// <summary>
        /// The test no relationships.
        /// </summary>
        [TestMethod]
        public void TestNoRelationshipsWithSpecificColumns()
        {
            var sqlStatement = SqlSelectStatementGenerator.GetSqlStatement(
                "Text.Text",
                "TextID",
                new List<ISqlRelationship>(),
                new List<ISqlEntityColumnMapping>
                    {
                        new SqlEntityColumnMapping
                            {
                                Entity = "Text.Text",
                                Name = "TextSourceDSC"
                            }
                    });


            string expected = @"SELECT
Text.Text.[TextSourceDSC],Text.Text.[TextID] AS [KeyLevel1]
FROM Text.Text
";
            Assert.AreEqual(expected, sqlStatement);
        }

        /// <summary>
        /// The test no relationships.
        /// </summary>
        [TestMethod]
        public void TestNoRelationshipsWithSpecificColumnsUsingAlias()
        {
            var sqlStatement = SqlSelectStatementGenerator.GetSqlStatement(
                "Text.Text",
                "TextID",
                new List<ISqlRelationship>(),
                new List<ISqlEntityColumnMapping>
                    {
                        new SqlEntityColumnMapping
                            {
                                Name = "TextSourceDSC",
                                Alias = "extension"
                            }
                    });


            string expected = @"SELECT
Text.Text.[TextSourceDSC] AS [extension],Text.Text.[TextID] AS [KeyLevel1]
FROM Text.Text
";
            Assert.AreEqual(expected, sqlStatement);
        }

        /// <summary>
        /// The test simple relationship.
        /// </summary>
        [TestMethod]
        public void TestSingleLevelRelationship()
        {
            var sqlRelationship = new SqlRelationship
                                      {
                                          SourceEntity = "Text.Text",
                                          SourceEntityKey = "EdwPatientID",
                                          DestinationEntity = "Person.Patient",
                                          DestinationEntityKey = "EdwPatientID"
                                      };

            var sqlStatement = SqlSelectStatementGenerator.GetSqlStatement(
                "Person.Patient",
                "TextID",
                new List<ISqlRelationship> { sqlRelationship },
                new List<ISqlEntityColumnMapping>());

            string expected = @"SELECT
Person.Patient.*,Person.Patient.[EdwPatientID] AS [KeyLevel2],Text.Text.[TextID] AS [KeyLevel1]
FROM Person.Patient
INNER JOIN Text.Text ON Text.Text.[EdwPatientID] = Person.Patient.[EdwPatientID]
";

            Assert.AreEqual(expected, sqlStatement);
        }

        /// <summary>
        /// The test two level relationship.
        /// </summary>
        [TestMethod]
        public void TestTwoLevelRelationship()
        {
            var sqlRelationship1 = new SqlRelationship
                                      {
                                          SourceEntity = "Text.Text",
                                          SourceEntityKey = "EncounterID",
                                          DestinationEntity = "Clinical.Encounter",
                                          DestinationEntityKey = "EncounterID"
            };

            var sqlRelationship2 = new SqlRelationship
                                      {
                                          SourceEntity = "Clinical.Encounter",
                                          SourceEntityKey = "FacilityAccountID",
                                          DestinationEntity = "Clinical.FacilityAccount",
                                          DestinationEntityKey = "FacilityAccountID"
            };

            var sqlStatement = SqlSelectStatementGenerator.GetSqlStatement(
                "Clinical.FacilityAccount",
                "TextID",
                new List<ISqlRelationship> { sqlRelationship1, sqlRelationship2 },
                new List<ISqlEntityColumnMapping>());

            string expected = @"SELECT
Clinical.FacilityAccount.*,Clinical.FacilityAccount.[FacilityAccountID] AS [KeyLevel3],Clinical.Encounter.[EncounterID] AS [KeyLevel2],Text.Text.[TextID] AS [KeyLevel1]
FROM Clinical.FacilityAccount
INNER JOIN Clinical.Encounter ON Clinical.Encounter.[FacilityAccountID] = Clinical.FacilityAccount.[FacilityAccountID]
INNER JOIN Text.Text ON Text.Text.[EncounterID] = Clinical.Encounter.[EncounterID]
";

            Assert.AreEqual(expected, sqlStatement);
        }

        /// <summary>
        /// The test three level relationship.
        /// </summary>
        [TestMethod]
        public void TestThreeLevelRelationship()
        {
            var sqlRelationship1 = new SqlRelationship
                                       {
                                           SourceEntity = "Text.Text",
                                           SourceEntityKey = "EncounterID",
                                           DestinationEntity = "Clinical.Encounter",
                                           DestinationEntityKey = "EncounterID"
                                       };

            var sqlRelationship2 = new SqlRelationship
                                       {
                                           SourceEntity = "Clinical.Encounter",
                                           SourceEntityKey = "FacilityAccountID",
                                           DestinationEntity = "Clinical.FacilityAccount",
                                           DestinationEntityKey = "FacilityAccountID"
                                       };

            var sqlRelationship3 = new SqlRelationship
                                       {
                                           SourceEntity = "Clinical.FacilityAccount",
                                           SourceEntityKey = "EDWAttendingProviderID",
                                           DestinationEntity = "Person.Provider",
                                           DestinationEntityKey = "EDWProviderID"
                                       };

            var sqlStatement = SqlSelectStatementGenerator.GetSqlStatement(
                "Person.Provider",
                "TextID",
                new List<ISqlRelationship> { sqlRelationship1, sqlRelationship2, sqlRelationship3 },
                new List<ISqlEntityColumnMapping>());

            string expected = @"SELECT
Person.Provider.*,Person.Provider.[EDWProviderID] AS [KeyLevel4],Clinical.FacilityAccount.[FacilityAccountID] AS [KeyLevel3],Clinical.Encounter.[EncounterID] AS [KeyLevel2],Text.Text.[TextID] AS [KeyLevel1]
FROM Person.Provider
INNER JOIN Clinical.FacilityAccount ON Clinical.FacilityAccount.[EDWAttendingProviderID] = Person.Provider.[EDWProviderID]
INNER JOIN Clinical.Encounter ON Clinical.Encounter.[FacilityAccountID] = Clinical.FacilityAccount.[FacilityAccountID]
INNER JOIN Text.Text ON Text.Text.[EncounterID] = Clinical.Encounter.[EncounterID]
";

            Assert.AreEqual(expected, sqlStatement);
        }
    }
}
