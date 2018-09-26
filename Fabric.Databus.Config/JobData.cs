// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobData.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JobData type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// The job data.
    /// </summary>
    public class JobData : IJobData
    {
        /// <summary>
        /// Gets or sets the data model.
        /// </summary>
        public string DataModel { get; set; }

        /// <summary>
        /// Gets or sets the data sources.
        /// </summary>
        [XmlElement("DataSource")]
        public List<DataSource> DataSources { get; set; }
    }
}