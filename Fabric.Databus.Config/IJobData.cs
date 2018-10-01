// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJobData.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IJobData type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;

    using ElasticSearchSqlFeeder.Interfaces;

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
        /// Gets or sets the data sources.
        /// </summary>
        IList<IDataSource> DataSources { get; set; }
    }
}