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
    using Fabric.Databus.Interfaces.Config;

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
        public IEnumerable<IDataSource> DataSources => this.MyDataSources;

        /// <summary>
        /// Gets or sets the data sources.
        /// </summary>
        [DataMember(Name = "DataSources")]
        [XmlElement("DataSource")]
        public List<DataSource> MyDataSources { get; set; }

        /// <inheritdoc />
        [XmlIgnore]
        public ITopLevelDataSource TopLevelDataSource { get; set; }

        /// <summary>
        /// Gets or sets the top level data source.
        /// </summary>
        [XmlElement(nameof(TopLevelDataSource))]
        public TopLevelDataSource MyTopLevelDataSource
        {
            get => (TopLevelDataSource)this.TopLevelDataSource;
            set
            {
                this.TopLevelDataSource = value;
                this.MyDataSources.Add(value);
            }
        }
    }
}