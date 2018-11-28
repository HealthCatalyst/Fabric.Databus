// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigReaderTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ConfigReaderTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System.Collections.Generic;
    using System.Linq;

    using Fabric.Databus.Config;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The config reader tests.
    /// </summary>
    [TestClass]
    public class ConfigReaderTests
    {
        /// <summary>
        /// The test reading data model.
        /// </summary>
        [TestMethod]
        public void TestReadingSimpleConfigWithNewDataModel()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "SimpleConfigWithNewModel.xml");

            Assert.IsNotNull(fileContents);
            Assert.AreNotEqual(0, fileContents.Length);

            var job = new ConfigReader().ReadXmlFromText(fileContents);

            Assert.AreEqual(4, job.Data.DataSources.Count);
        }

        /// <summary>
        /// The test reading data model.
        /// </summary>
        [TestMethod]
        public void TestReadingSimpleConfigWithUpdatingModel()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "SimpleConfigWithUpdatingModel.xml");

            Assert.IsNotNull(fileContents);
            Assert.AreNotEqual(0, fileContents.Length);

            var job = new ConfigReader().ReadXmlFromText(fileContents);

            Assert.AreEqual(2, job.Data.DataSources.Count);
            Assert.AreEqual("$", job.Data.DataSources[0].Path);
        }

        /// <summary>
        /// The test reading data model.
        /// </summary>
        [TestMethod]
        public void TestReadingSimpleConfigWithNoDataSources()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "SimpleConfigWithNoDataSources.xml");

            Assert.IsNotNull(fileContents);
            Assert.AreNotEqual(0, fileContents.Length);

            var job = new ConfigReader().ReadXmlFromText(fileContents);

            Assert.AreEqual(2, job.Data.DataSources.Count);
            Assert.AreEqual("$", job.Data.DataSources[0].Path);
        }

        /// <summary>
        /// The read simple data model.
        /// </summary>
        [TestMethod]
        public void ReadSimpleDataModel()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "SimpleDataModel.json");

            Assert.IsNotNull(fileContents);
            Assert.AreNotEqual(0, fileContents.Length);

            var job = new XmlJob { MyData = new JobData { MyDataSources = new List<DataSource>() } };

            new ConfigReader().MergeDataSourcesFromDataModel(job, fileContents);

            Assert.AreEqual(2, job.Data.DataSources.Count);
            Assert.AreEqual("$", job.Data.DataSources[0].Path);
            Assert.AreEqual("SELECT * FROM Text", job.Data.DataSources[0].Sql);
            Assert.AreEqual("SELECT * FROM TextDate", job.Data.DataSources[1].Sql);
        }

        /// <summary>
        /// The read config with tables.
        /// </summary>
        [TestMethod]
        public void ReadConfigWithTables()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "ConfigWithTables.xml");

            Assert.IsNotNull(fileContents);
            Assert.AreNotEqual(0, fileContents.Length, "Could not read file from assembly.  Did you mark it as Embedded Resource?");

            var job = new ConfigReader().ReadXmlFromText(fileContents);

            Assert.AreEqual(2, job.Data.DataSources.Count);
            Assert.AreEqual("$", job.Data.DataSources[0].Path);
            Assert.AreEqual("Text.Text", job.Data.DataSources[0].TableOrView);
            Assert.AreEqual(0, job.Data.DataSources[0].Relationships.Count());
            Assert.AreEqual(2, job.Data.DataSources[0].SqlEntityColumnMappings.Count());
            Assert.AreEqual("TextID", job.Data.DataSources[0].SqlEntityColumnMappings.First().Name);

            Assert.AreEqual("$.patient", job.Data.DataSources[1].Path);
            Assert.AreEqual("Person.Patient", job.Data.DataSources[1].TableOrView);
            Assert.AreEqual(1, job.Data.DataSources[1].Relationships.Count());
            Assert.AreEqual("Text.Text", job.Data.DataSources[1].Relationships.First().SourceEntity);
        }
    }
}
