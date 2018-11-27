// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonToDataSourceConvertorTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JsonToDataSourceConvertorTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System.Collections.Generic;

    using Fabric.Databus.Config;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The parse json mapping tests.
    /// </summary>
    [TestClass]
    public class JsonToDataSourceConvertorTests
    {
        /// <summary>
        /// The test parsing json mapping.
        /// </summary>
        [TestMethod]
        public void TestParsingJsonMappingOneEntity()
        {
            string json = @"
                        {
	                        ""_metadata"": {
		                        ""entities"": [{
				                        ""databaseEntity"": ""Text"",
				                        ""keyLevels"": [
					                        ""TextID""
				                        ]
			                        }, {
				                        ""databaseEntity"": ""Date"",
				                        ""keyLevels"": [""TextID""]
			                        }
		                        ]
	                        },
	                        ""root"": ""Text.TextID"",
	                        ""data"": ""Text.TextTXT""
                        }";

            JObject myObject = JObject.Parse(json);

            var dataSources = new List<DataSource>();
            JsonToDataSourceConvertor.ParseJsonIntoDataSources(new List<string>(), myObject, dataSources);

            Assert.AreEqual(2, dataSources.Count);
            Assert.AreEqual("SELECT * FROM Text", dataSources[0].Sql);
            Assert.AreEqual("SELECT * FROM Date", dataSources[1].Sql);

            Assert.AreEqual("$", dataSources[0].Path);
            Assert.AreEqual("$", dataSources[1].Path);

            Assert.AreEqual(1, dataSources[0].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[0].KeyLevels[0]);

            Assert.AreEqual(1, dataSources[1].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[1].KeyLevels[0]);
        }

        /// <summary>
        /// The test parsing json mapping.
        /// </summary>
        [TestMethod]
        public void TestParsingJsonMappingNestedEntity()
        {
            string json = @"
{
	""_metadata"": {
		""entities"": [{
				""databaseEntity"": ""Text"",
				""keyLevels"": [
					""TextID""
				]
			}, {
				""databaseEntity"": ""Date"",
				""keyLevels"": [""TextID""]
			}
		]
	},
	""root"": ""Text.TextID"",
	""data"": ""Text.TextTXT"",
	""patient"": {
		""_metadata"": {
			""entities"": [{
					""databaseEntity"": ""Patient"",
					""keyLevels"": [
						""TextID"",
						""PatientID""
					]
				}
			]
		},
		""root"": ""Person.EDWPatientId"",
		""MRN"": ""Person.MRN""
	}
}
                        ";

            JObject myObject = JObject.Parse(json);

            var dataSources = new List<DataSource>();
            JsonToDataSourceConvertor.ParseJsonIntoDataSources(new List<string>(), myObject, dataSources);

            Assert.AreEqual(3, dataSources.Count);
            Assert.AreEqual("SELECT * FROM Text", dataSources[0].Sql);
            Assert.AreEqual("SELECT * FROM Date", dataSources[1].Sql);
            Assert.AreEqual("SELECT * FROM Patient", dataSources[2].Sql);

            Assert.AreEqual("$", dataSources[0].Path);
            Assert.AreEqual("$", dataSources[1].Path);
            Assert.AreEqual("$.patient", dataSources[2].Path);

            Assert.AreEqual(1, dataSources[0].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[0].KeyLevels[0]);

            Assert.AreEqual(1, dataSources[1].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[1].KeyLevels[0]);

            Assert.AreEqual(2, dataSources[2].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[2].KeyLevels[0]);
            Assert.AreEqual("PatientID", dataSources[2].KeyLevels[1]);
        }
    }
}
