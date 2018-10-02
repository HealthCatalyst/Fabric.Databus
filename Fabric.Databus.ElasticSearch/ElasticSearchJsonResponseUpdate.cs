// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElasticSearchJsonResponseUpdate.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the ElasticSearchJsonResponseUpdate type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.ElasticSearch
{
    /// <summary>
    /// The elastic search json response update.
    /// </summary>
    public class ElasticSearchJsonResponseUpdate
    {
        /// <summary>
        /// Gets or sets the _index.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once InconsistentNaming
        public string _index { get; set; }

        /// <summary>
        /// Gets or sets the _type.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Global
        public string _type { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        // ReSharper disable once StyleCop.SA1300
        // ReSharper disable once InconsistentNaming
        public int status { get; set; }
    }
}