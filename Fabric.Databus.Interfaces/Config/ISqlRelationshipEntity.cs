// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISqlRelationshipEntity.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ISqlRelationshipEntity type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Config
{
    /// <summary>
    /// The SqlRelationshipEntity interface.
    /// </summary>
    public interface ISqlRelationshipEntity
    {
        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        string Entity { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        string Key { get; set; }
    }
}