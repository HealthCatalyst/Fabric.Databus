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
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// The job data.
    /// </summary>
    [DataContract(Name = "JobData", Namespace = "")]
    public class JobData : IJobData
    {
        /// <summary>
        /// Gets or sets the data model.
        /// </summary>
        [DataMember]
        public string DataModel { get; set; }

        /// <summary>
        /// Gets or sets the data sources.
        /// </summary>
        [DataMember(Name = "DataSources")]
        [XmlElement("DataSource")]
        public List<DataSource> DataSources { get; set; }
    }
}