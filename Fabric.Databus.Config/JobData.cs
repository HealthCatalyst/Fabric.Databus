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
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using ElasticSearchSqlFeeder.Interfaces;

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
        [XmlIgnore]
        public IList<IDataSource> DataSources { get; set; }

        /// <summary>
        /// Gets or sets the my data sources.
        /// </summary>
        [DataMember(Name = "DataSources")]
        [XmlElement("DataSource")]
        public List<DataSource> MyDataSources
        {
            get
            {
                return this.DataSources?.Cast<DataSource>().ToList();
            }

            set
            {
                this.DataSources = value.Cast<IDataSource>().ToList();
            }
        }
    }
}