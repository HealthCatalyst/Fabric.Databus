// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlJob.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   The job.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;

    /// <inheritdoc />
    /// <summary>
    /// The job.
    /// </summary>
    [DataContract(Name  = "Job", Namespace = "")]
    [KnownType(typeof(QueryConfig))]
    [XmlType("Job")]
    public class XmlJob : IJob
    {
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        [XmlIgnore]
        public IQueryConfig Config { get; set; }

        /// <summary>
        /// Gets or sets the my config.
        /// </summary>
        [DataMember(Name = "Config")]
        [XmlElement("Config")]
        public QueryConfig MyConfig
        {
            get => (QueryConfig)this.Config;
            set => this.Config = value;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [XmlIgnore]
        public IJobData Data { get; set; }

        /// <summary>
        /// Gets or sets the my data.
        /// </summary>
        [DataMember(Name = "Data")]
        [XmlElement("Data")]
        public JobData MyData
        {
            get => (JobData)this.Data;
            set => this.Data = value;
        }
    }
}