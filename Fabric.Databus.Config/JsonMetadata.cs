// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonMetadata.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JsonMetadata type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The json metadata.
    /// </summary>
    public class JsonMetadata
    {
        /// <summary>
        /// Gets or sets the entities.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles
        public IList<JsonMetadataEntity> entities { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}