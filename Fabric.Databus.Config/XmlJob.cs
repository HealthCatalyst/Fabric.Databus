namespace Fabric.Databus.Config
{
    using System.Runtime.Serialization;

    using ElasticSearchSqlFeeder.Interfaces;

    /// <summary>
    /// The job.
    /// </summary>
    [DataContract(Name  = "Job", Namespace = "")]
    [KnownType(typeof(QueryConfig))]
    public class XmlJob : IJob
    {
        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        public IQueryConfig Config { get; set; }

        /// <summary>
        /// Gets or sets the my config.
        /// </summary>
        [DataMember(Name = "Config")]
        public QueryConfig MyConfig
        {
            get => (QueryConfig)this.Config;
            set => this.Config = value;
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public IJobData Data { get; set; }

        /// <summary>
        /// Gets or sets the my data.
        /// </summary>
        [DataMember(Name = "Data")]
        public JobData MyData
        {
            get => (JobData)this.Data;
            set => this.Data = value;
        }
    }
}