// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElasticSearchJsonResponse.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the ElasticSearchJsonResponse type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.ElasticSearch
{
    using System.Collections.Generic;

    /// <summary>
    /// The elastic search json response.
    /// </summary>
    public class ElasticSearchJsonResponse
    {
        /// <summary>
        /// Gets or sets the took.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once StyleCop.SA1300
        public int took { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether errors.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once StyleCop.SA1300
        public bool errors { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once StyleCop.SA1300
        public List<ElasticSearchJsonResponseItem> items { get; set; }
    }
}