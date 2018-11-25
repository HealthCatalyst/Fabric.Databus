// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergeUsingObjectArrayTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MergeUsingObjectArrayTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System.Collections.Generic;
    using System.IO;

    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.Shared;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The merge using object array tests.
    /// </summary>
    [TestClass]
    public class MergeUsingObjectArrayTests
    {
        /// <summary>
        /// The test method 1.
        /// </summary>
        [TestMethod]
        public void TestMergeUsingObjectArray()
        {
            var sourceWrapperCollection = new SourceWrapperCollection();

            string propertyName = "$";
            var textSourceWrapper = new SourceWrapper(
                new List<ColumnInfo>
                    {
                        new ColumnInfo { index = 0, Name = "TextID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 1, Name = "EDWPatientId", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 2, Name = "TextTXT", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 3, Name = "EncounterID", SqlColumnType = "varchar(256)" }
                    },
                propertyName,
                new List<object[]>
                    {
                        new object[] { "1", "100", "This is my first test", "301" },
                        new object[] { "2", "100", "This is my second test", "302" },
                        new object[] { "3", "101", "This is my third test", "303" },
                    },
                new List<string> { "TextID" },
                true);

            sourceWrapperCollection.Add(textSourceWrapper);

            propertyName = "$.Patient";
            var patientSourcesWrapper = new SourceWrapper(
                new List<ColumnInfo>
                    {
                        new ColumnInfo { index = 0, Name = "TextID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 1, Name = "EDWPatientId", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 2, Name = "MRN", SqlColumnType = "varchar(256)" }
                    },
                propertyName,
                new List<object[]>
                    {
                        new object[] { "1", "100", "Mrn100" },
                        new object[] { "2", "100", "Mrn100" },
                        new object[] { "3", "101", "Mrn101" },
                    },
                new List<string> { "TextID", "EDWPatientId" },
                false);

            sourceWrapperCollection.Add(patientSourcesWrapper);

            propertyName = "$.Visit";
            var encounterSourceWrapper = new SourceWrapper(
                new List<ColumnInfo>
                    {
                        new ColumnInfo { index = 0, Name = "TextID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 1, Name = "EncounterID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 2, Name = "FacilityAccountID", SqlColumnType = "varchar(256)" }
                    },
                propertyName,
                new List<object[]>
                    {
                        new object[] { "1", "301", "401" },
                        new object[] { "2", "302", "402" },
                        new object[] { "3", "303", "403" },
                    },
                new List<string> { "TextID", "EncounterID" },
                false);

            sourceWrapperCollection.Add(encounterSourceWrapper);

            propertyName = "$.Visit.Facility";
            var facilitySourceWrapper = new SourceWrapper(
                new List<ColumnInfo>
                    {
                        new ColumnInfo { index = 0, Name = "TextID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 1, Name = "EncounterID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 2, Name = "FacilityAccountID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 3, Name = "EDWAttendingProviderID", SqlColumnType = "varchar(256)" }
                    },
                propertyName,
                new List<object[]>
                    {
                        new object[] { "1", "301", "401", "501" },
                        new object[] { "2", "302", "402", "502" },
                        new object[] { "3", "303", "403", "503" },
                    },
                new List<string> { "TextID", "EncounterID", "FacilityAccountID" },
                false);

            sourceWrapperCollection.Add(facilitySourceWrapper);

            propertyName = "$.Visit.Facility.People";
            var peopleSourceWrapper = new SourceWrapper(
                new List<ColumnInfo>
                    {
                        new ColumnInfo { index = 0, Name = "TextID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 1, Name = "EncounterID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 2, Name = "FacilityAccountID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 3, Name = "EDWProviderID", SqlColumnType = "varchar(256)" },
                        new ColumnInfo { index = 4, Name = "last_name", SqlColumnType = "varchar(256)" }
                    },
                propertyName,
                new List<object[]>
                    {
                        new object[] { "1", "301", "401", "501", "Jones" },
                        new object[] { "2", "302", "402", "502", "Smith" },
                        new object[] { "3", "303", "403", "503", "Smith" },
                        new object[] { "3", "303", "403", "504", "Bradford" },
                    },
                new List<string> { "TextID", "EncounterID", "FacilityAccountID", "EDWProviderID" },
                true);

            sourceWrapperCollection.Add(peopleSourceWrapper);

            string expectedJsonText = @"
[{
		""TextID"": ""1"",
		""EDWPatientId"": ""100"",
		""TextTXT"": ""This is my first test"",
		""EncounterID"": ""301"",
		""Patient"": {
			""TextID"": ""1"",
			""EDWPatientId"": ""100"",
			""MRN"": ""Mrn100""
		},
		""Visit"": {
			""TextID"": ""1"",
			""EncounterID"": ""301"",
			""FacilityAccountID"": ""401"",
			""Facility"": {
				""TextID"": ""1"",
				""EncounterID"": ""301"",
				""FacilityAccountID"": ""401"",
				""EDWAttendingProviderID"": ""501"",
				""People"": [{
						""TextID"": ""1"",
						""EncounterID"": ""301"",
						""FacilityAccountID"": ""401"",
						""EDWProviderID"": ""501"",
						""last_name"": ""Jones""
					}
				]
			}
		}
	}, {
		""TextID"": ""2"",
		""EDWPatientId"": ""100"",
		""TextTXT"": ""This is my second test"",
		""EncounterID"": ""302"",
		""Patient"": {
			""TextID"": ""2"",
			""EDWPatientId"": ""100"",
			""MRN"": ""Mrn100""
		},
		""Visit"": {
			""TextID"": ""2"",
			""EncounterID"": ""302"",
			""FacilityAccountID"": ""402"",
			""Facility"": {
				""TextID"": ""2"",
				""EncounterID"": ""302"",
				""FacilityAccountID"": ""402"",
				""EDWAttendingProviderID"": ""502"",
				""People"": [{
						""TextID"": ""2"",
						""EncounterID"": ""302"",
						""FacilityAccountID"": ""402"",
						""EDWProviderID"": ""502"",
						""last_name"": ""Smith""
					}
				]
			}
		}
	}, {
		""TextID"": ""3"",
		""EDWPatientId"": ""101"",
		""TextTXT"": ""This is my third test"",
		""EncounterID"": ""303"",
		""Patient"": {
			""TextID"": ""3"",
			""EDWPatientId"": ""101"",
			""MRN"": ""Mrn101""
		},
		""Visit"": {
			""TextID"": ""3"",
			""EncounterID"": ""303"",
			""FacilityAccountID"": ""403"",
			""Facility"": {
				""TextID"": ""3"",
				""EncounterID"": ""303"",
				""FacilityAccountID"": ""403"",
				""EDWAttendingProviderID"": ""503"",
				""People"": [{
						""TextID"": ""3"",
						""EncounterID"": ""303"",
						""FacilityAccountID"": ""403"",
						""EDWProviderID"": ""503"",
						""last_name"": ""Smith""
					}, {
						""TextID"": ""3"",
						""EncounterID"": ""303"",
						""FacilityAccountID"": ""403"",
						""EDWProviderID"": ""504"",
						""last_name"": ""Bradford""
					}
				]
			}
		}
	}
]
";
            var expectedJson = JArray.Parse(expectedJsonText);

            using (var textWriter = new StringWriter())
            {
                using (var writer = new JsonTextWriter(textWriter))
                {
                    sourceWrapperCollection.WriteToJson(writer);
                }

                var result = textWriter.ToString();
                var actualJson = JArray.Parse(result);
                Assert.IsTrue(JToken.DeepEquals(expectedJson, actualJson));
            }
        }

        /// <summary>
        /// The test parsing json.
        /// </summary>
        [TestMethod]
        public void TestParsingJson()
        {
            string json = @"
{
	""root"": ""Text.TextID"",
	""data"": ""Text.TextTXT"",
	""data_format"": ""Text.MimeTypeNM"",
	""extension"": ""Text.TextSourceDSC"",
	""Patient"": {
		""root"": ""Person.EDWPatientId"",
		""MRN"": ""Person.MRN""
	},
	""Visit"": {
		""root"": ""FacilityAccount.FacilityAccountID"",
		""Facility"": {
			""root"": ""FacilityAccount.FacilityAccountID"",
			""extension"": ""FacilityAccount.FacilityAccountID""
		},
		""People"": [
			{
				""root"": ""FacilityAccount.AttendingProviderID"",
				""last_name"": ""FacilityAccount.ProviderLastNM""
			}
		]
	},
	""Document"": {
		""root"": ""Text.TextID"",
		""source_created_at"": ""Date.DTS"",
		""People"": [
			{
				""root"": ""Provider.EDWPersonID"",
				""last_name"": ""Provider.ProviderLastNM""
			}
		]
	}
}";

            JObject o = JObject.Parse(json);
        }
    }
}
