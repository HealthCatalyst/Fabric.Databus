// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Job.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the Job type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Runtime.Serialization;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;

    /// <inheritdoc />
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
        public IQueryConfig Config { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public IJobData Data { get; set; }
    }
}