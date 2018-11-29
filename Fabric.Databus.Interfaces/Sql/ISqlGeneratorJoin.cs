// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISqlGeneratorJoin.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ISqlGeneratorJoin type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Sql
{
    /// <summary>
    /// The SqlGeneratorJoin interface.
    /// </summary>
    public interface ISqlGeneratorJoin
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