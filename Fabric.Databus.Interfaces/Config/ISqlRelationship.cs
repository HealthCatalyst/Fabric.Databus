﻿// --------------------------------------------------------------------------------------------------------------------
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
        /// Gets the source.
        /// </summary>
        ISqlRelationshipEntity Source { get; }

        /// <summary>
        /// Gets the destination.
        /// </summary>
        ISqlRelationshipEntity Destination { get; }
    }
}