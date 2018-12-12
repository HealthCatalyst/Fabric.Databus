// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigReader.cs" company="">
//   
// </copyright>
// <summary>
//   The config reader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;

    using Fabric.Shared;

    /// <summary>
    /// The config reader.
    /// </summary>
    public class ConfigReader
    {
        /// <summary>
        /// The read xml.
        /// </summary>
        /// <param name="inputFile">
        /// The input file.
        /// </param>
        /// <returns>
        /// The <see cref="XmlJob"/>.
        /// </returns>
        [Pure]
        public XmlJob ReadXml(string inputFile)
        {
            var fileContents = File.ReadAllText(inputFile);

            return this.ReadXmlFromText(fileContents);
        }

        /// <summary>
        /// The read xml from text.
        /// </summary>
        /// <param name="fileContents">
        /// The file contents.
        /// </param>
        /// <returns>
        /// The <see cref="XmlJob"/>.
        /// </returns>
        [Pure]
        public XmlJob ReadXmlFromText(string fileContents)
        {
            var job = fileContents.FromXml<XmlJob>();

            this.MergeDataSourcesFromDataModel(job, job.Data.DataModel);

            return job;
        }

        /// <summary>
        /// The merge data sources from data model.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <param name="dataModel">
        /// The date Model.
        /// </param>
        public void MergeDataSourcesFromDataModel(XmlJob job, string dataModel)
        {
            if (!string.IsNullOrWhiteSpace(dataModel))
            {
                var jsonDataSources = JsonToDataSourceConvertor.ParseJsonIntoDataSources(dataModel);

                // find matching data sources
                foreach (var jsonDataSource in jsonDataSources)
                {
                    var matchingDataSource =
                        job.Data.DataSources.FirstOrDefault(dataSource => jsonDataSource.Name.Equals(dataSource.Name));
                    if (matchingDataSource != null)
                    {
                        // copy properties
                        matchingDataSource.Path = jsonDataSource.Path;
                        matchingDataSource.PropertyType = jsonDataSource.PropertyType;
                        matchingDataSource.KeyLevels = jsonDataSource.KeyLevels;
                    }
                    else
                    {
                        job.MyData.MyDataSources.Add(jsonDataSource);
                    }
                }
            }
        }

        /// <summary>
        /// The to xml.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string WriteXml(IJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            return (job as XmlJob).ToXml<XmlJob>();
        }
    }
}
