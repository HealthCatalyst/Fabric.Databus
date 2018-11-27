// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonMetadataEntity.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JsonMetadataEntity type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The json metadata entity.
    /// </summary>
    public class JsonMetadataEntity
    {
        /// <summary>
        /// Gets or sets the database entity.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
#pragma warning disable IDE1006 // Naming Styles
        // ReSharper disable once InconsistentNaming
        public string databaseEntity { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets the key levels.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
#pragma warning disable IDE1006 // Naming Styles
        // ReSharper disable once InconsistentNaming
        public IList<string> keyLevels { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}