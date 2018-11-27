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
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
#pragma warning disable IDE1006 // Naming Styles
    // ReSharper disable InconsistentNaming
    public class JsonMetadata
    {

        /// <summary>
        /// Gets or sets the key levels.
        /// </summary>
        public IList<string> keyLevels { get; set; } = new List<string>();

        /// <summary>
        /// Sets the key.
        /// </summary>
        public string key
        {
            set => this.keyLevels = new List<string> { value };
        }

        /// <summary>
        /// Gets or sets the entities.
        /// </summary>
        public IList<JsonMetadataEntity> entities { get; set; } = new List<JsonMetadataEntity>();

        /// <summary>
        /// Sets the entity.
        /// </summary>
        public string entity
        {
            set => this.entities = new List<JsonMetadataEntity> { new JsonMetadataEntity { databaseEntity = value } };
        }
    }
    // ReSharper restore InconsistentNaming
#pragma warning restore IDE1006 // Naming Styles
}