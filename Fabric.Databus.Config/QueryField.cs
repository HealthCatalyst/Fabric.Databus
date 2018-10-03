// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryField.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the QueryField type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;

    /// <summary>
    /// The query field.
    /// </summary>
    public class QueryField : IQueryField
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Gets or sets the destination type.
        /// </summary>
        public ElasticSearchTypes DestinationType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether skip.
        /// </summary>
        public bool Skip { get; set; }

        /// <summary>
        /// Gets or sets the transform.
        /// </summary>
        public QueryFieldTransform Transform { get; set; }

    }
}