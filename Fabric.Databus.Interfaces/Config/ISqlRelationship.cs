// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISqlRelationship.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ISqlRelationship type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Config
{
    /// <summary>
    /// The SqlRelationship interface.
    /// </summary>
    public interface ISqlRelationship
    {
        /// <summary>
        /// Gets or sets the source entity.
        /// </summary>
        string SourceEntity { get; set; }

        /// <summary>
        /// Gets or sets the source entity key.
        /// </summary>
        string SourceEntityKey { get; set; }

        /// <summary>
        /// Gets or sets the destination entity.
        /// </summary>
        string DestinationEntity { get; set; }

        /// <summary>
        /// Gets or sets the destination entity key.
        /// </summary>
        string DestinationEntityKey { get; set; }
    }
}