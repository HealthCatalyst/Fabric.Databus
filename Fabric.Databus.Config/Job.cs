// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Job.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the Job type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using ElasticSearchSqlFeeder.Interfaces;

    /// <summary>
    /// The job.
    /// </summary>
    [DataContract(Name  = "Job", Namespace = "")]
    [KnownType(typeof(QueryConfig))]
    public class Job : IJob
    {
        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        [DataMember(Name = "Config")]
        public IQueryConfig Config { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [DataMember]
        public IJobData Data { get; set; }
    }
}