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
			                    ""keyLevels"": [
				                    ""TextID""
			                    ],
			                    ""entities"": [
				                    {
					                    ""databaseEntity"": ""Text""
				                    },
				                    {
					                    ""databaseEntity"": ""Date""
				                    }
			                    ]
		                    },
	                        ""root"": ""Text.TextID"",
	                        ""data"": ""Text.TextTXT""
                        }";

            var dataSources = JsonToDataSourceConvertor.ParseJsonIntoDataSources(json);

            Assert.AreEqual(2, dataSources.Count);
            Assert.AreEqual("SELECT * FROM Text", dataSources[0].Sql);
            Assert.AreEqual("SELECT * FROM Date", dataSources[1].Sql);

            Assert.AreEqual("$", dataSources[0].Path);
            Assert.AreEqual("Array", dataSources[0].PropertyType);

            Assert.AreEqual("$", dataSources[1].Path);
            Assert.AreEqual("Array", dataSources[0].PropertyType);

            Assert.AreEqual(1, dataSources[0].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[0].KeyLevels[0]);

            Assert.AreEqual(1, dataSources[1].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[1].KeyLevels[0]);
        }

        /// <summary>
        /// The test parsing json mapping.
        /// </summary>
        [TestMethod]
        public void TestParsingJsonMappingOneEntitySimplerSyntax()
        {
            string json = @"
                        {
	                        ""_metadata"": {
			                    ""key"": ""TextID"",
			                    ""entity"": ""Text""
		                    },
	                        ""root"": ""Text.TextID"",
	                        ""data"": ""Text.TextTXT""
                        }";


            var dataSources = JsonToDataSourceConvertor.ParseJsonIntoDataSources(json);

            Assert.AreEqual(1, dataSources.Count);
            Assert.AreEqual("SELECT * FROM Text", dataSources[0].Sql);

            Assert.AreEqual("$", dataSources[0].Path);
            Assert.AreEqual("Array", dataSources[0].PropertyType);

            Assert.AreEqual(1, dataSources[0].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[0].KeyLevels[0]);
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
		""keyLevels"": [
			""TextID""
		],
		""entities"": [
			{
				""databaseEntity"": ""Text""
			},
			{
				""databaseEntity"": ""Date""
			}
		]
	},
	""root"": ""Text.TextID"",
	""data"": ""Text.TextTXT"",
	""patient"": {
				""_metadata"": {
				""keyLevels"": [
					""PatientID""
				],
				""entities"": [
					{
						""databaseEntity"": ""Patient""
					}
				]
			},
		""root"": ""Person.EDWPatientId"",
		""MRN"": ""Person.MRN""
	}
}
                        ";

            var dataSources = JsonToDataSourceConvertor.ParseJsonIntoDataSources(json);

            Assert.AreEqual(3, dataSources.Count);
            Assert.AreEqual("SELECT * FROM Text", dataSources[0].Sql);
            Assert.AreEqual("SELECT * FROM Date", dataSources[1].Sql);
            Assert.AreEqual("SELECT * FROM Patient", dataSources[2].Sql);

            Assert.AreEqual("$", dataSources[0].Path);
            Assert.AreEqual("$", dataSources[1].Path);
            Assert.AreEqual("$.patient", dataSources[2].Path);

            Assert.AreEqual("Object", dataSources[2].PropertyType);

            Assert.AreEqual(1, dataSources[0].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[0].KeyLevels[0]);

            Assert.AreEqual(1, dataSources[1].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[1].KeyLevels[0]);

            Assert.AreEqual(2, dataSources[2].KeyLevels.Count);
            Assert.AreEqual("TextID", dataSources[2].KeyLevels[0]);
            Assert.AreEqual("PatientID", dataSources[2].KeyLevels[1]);
        }

        /// <summary>
        /// The test json mapping full entity.
        /// </summary>
        [TestMethod]
        public void TestJsonMappingFullEntity()
        {
            string json = @"
{
	""_metadata"": {
		""keyLevels"": [
			""TextID""
		],
		""entities"": [
			{
				""databaseEntity"": ""Text""
			},
			{
				""databaseEntity"": ""Date""
			}
		]
	},
	""root"": ""Text.TextID"",
	""data"": ""Text.TextTXT"",
	""data_format"": ""Text.MimeTypeNM"",
	""extension"": ""Text.TextSourceDSC"",
	""patient"": {
		""_metadata"": {
			""keyLevels"": [
				""PatientID""
			],
			""entities"": [
				{
					""databaseEntity"": ""Patient""
				}
			]
		},
		""root"": ""Person.EDWPatientId"",
		""MRN"": ""Person.MRN""
	},
	""visit"": {
		""_metadata"": {
			""keyLevels"": [
				""EncounterID""
			],
			""entities"": [
				{
					""databaseEntity"": ""Encounter""
				}
			]
		},
		""root"": ""FacilityAccount.FacilityAccountID"",
		""facility"": {
			""_metadata"": {
				""keyLevels"": [
					""FacilityAccountID""
				],
				""entities"": [
					{
						""databaseEntity"": ""FacilityAccount""
					}
				]
			},
			""root"": ""FacilityAccount.FacilityAccountID"",
			""extension"": ""FacilityAccount.FacilityAccountID""
		},
		""people"": [
			{
				""_metadata"": {
					""keyLevels"": [
						""ProviderID""
					],
					""entities"": [
						{
							""databaseEntity"": ""Provider""
						}
					]
				},
				""root"": ""Provider.AttendingProviderID"",
				""last_name"": ""Provider.ProviderLastNM""
			}
		]
	},
	""document"": {
		""_metadata"": {
			""keyLevels"": [
				""TextID""
			],
			""entities"": [
				{
					""databaseEntity"": ""Text""
				}
			]
		},
		""root"": ""Text.TextID"",
		""source_created_at"": ""Date.DTS"",
		""people"": [
			{
				""_metadata"": {
					""keyLevels"": [
						""EDWPersonID""
					],
					""entities"": [
						{
							""databaseEntity"": ""Provider""
						}
					]
				},
				""root"": ""Provider.EDWPersonID"",
				""last_name"": ""Provider.ProviderLastNM""
			}
		]
	}
}
";

            var dataSources = JsonToDataSourceConvertor.ParseJsonIntoDataSources(json);

            Assert.AreEqual(8, dataSources.Count);
            var visitPeopleDataSource = dataSources[5];

            Assert.AreEqual("$.visit.people", visitPeopleDataSource.Path);
            Assert.AreEqual("Array", visitPeopleDataSource.PropertyType);
            Assert.AreEqual(3, visitPeopleDataSource.KeyLevels.Count);
            Assert.AreEqual("TextID", visitPeopleDataSource.KeyLevels[0]);
            Assert.AreEqual("EncounterID", visitPeopleDataSource.KeyLevels[1]);
            Assert.AreEqual("ProviderID", visitPeopleDataSource.KeyLevels[2]);
        }
    }
}
