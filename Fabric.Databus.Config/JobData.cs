// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobData.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the JobData type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Fabric.Databus.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The job data.
    /// </summary>
    [DataContract(Name = "JobData", Namespace = "")]
    public class JobData : IJobData
    {
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the data model.
        /// </summary>
        [DataMember]
        public string DataModel { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the data sources.
        /// </summary>
        [XmlIgnore]
        public IList<IDataSource> DataSources
        {
            get
            {
                return this.MyDataSources.Cast<IDataSource>().ToList();
            }
        }

        /// <summary>
        /// Gets or sets the data sources.
        /// </summary>
        [DataMember(Name = "DataSources")]
        [XmlElement("DataSource")]
        public List<DataSource> MyDataSources { get; set; }
    }
}