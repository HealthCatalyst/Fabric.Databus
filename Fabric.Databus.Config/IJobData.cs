// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJobData.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IJobData type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;

    /// <summary>
    /// The JobData interface.
    /// </summary>
    public interface IJobData
    {
        /// <summary>
        /// Gets or sets the data model.
        /// </summary>
        string DataModel { get; set; }

        /// <summary>
        /// Gets the data sources.
        /// </summary>
        IList<IDataSource> DataSources { get; }
    }
}