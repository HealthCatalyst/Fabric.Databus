// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueryField.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IQueryField type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.ElasticSearch
{
    /// <summary>
    /// The QueryField interface.
    /// </summary>
    public interface IQueryField
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        string Source { get; set; }

        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        string Destination { get; set; }

        /// <summary>
        /// Gets or sets the destination type.
        /// </summary>
        ElasticSearchTypes DestinationType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether skip.
        /// </summary>
        bool Skip { get; set; }

        /// <summary>
        /// Gets or sets the transform.
        /// </summary>
        QueryFieldTransform Transform { get; set; }
    }
}