// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJob.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IJob type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Xml.Serialization;

    using Fabric.Databus.Interfaces.Config;

    /// <summary>
    /// The Job interface.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        [XmlIgnore]
        IQueryConfig Config { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [XmlIgnore]
        IJobData Data { get; set; }
    }
}