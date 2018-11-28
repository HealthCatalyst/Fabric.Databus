// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlRelationship.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlRelationship type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using Fabric.Databus.Interfaces.Config;

    /// <inheritdoc />
    /// <summary>
    /// The sql relationship.
    /// </summary>
    public class SqlRelationship : ISqlRelationship
    {
        /// <inheritdoc />
        public string SourceEntity { get; set; }

        /// <inheritdoc />
        public string SourceEntityKey { get; set; }

        /// <inheritdoc />
        public string DestinationEntity { get; set; }

        /// <inheritdoc />
        public string DestinationEntityKey { get; set; }
    }
}