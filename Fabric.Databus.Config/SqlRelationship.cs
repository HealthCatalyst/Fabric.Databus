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
    using System.Xml;
    using System.Xml.Serialization;

    using Fabric.Databus.Interfaces.Config;

    /// <inheritdoc />
    /// <summary>
    /// The sql relationship.
    /// </summary>
    public class SqlRelationship : ISqlRelationship
    {
        /// <inheritdoc />
        public ISqlRelationshipEntity Source => this.MySource;

        /// <summary>
        /// Gets or sets the my source.
        /// </summary>
        [XmlElement("Source")]
        public SqlRelationshipEntity MySource { get; set; }

        /// <inheritdoc />
        public ISqlRelationshipEntity Destination => this.MyDestination;

        /// <summary>
        /// Gets or sets the my destination.
        /// </summary>
        [XmlElement("Destination")]
        public SqlRelationshipEntity MyDestination { get; set; }
    }
}