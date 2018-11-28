// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlRelationshipEntity.cs" company="">
//   
// </copyright>
// <summary>
//   The sql relationship entity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Xml.Serialization;

    using Fabric.Databus.Interfaces.Config;

    /// <inheritdoc />
    /// <summary>
    /// The sql relationship entity.
    /// </summary>
    public class SqlRelationshipEntity : ISqlRelationshipEntity
    {
        /// <inheritdoc />
        [XmlAttribute("Entity")]
        public string Entity { get; set; }

        /// <inheritdoc />
        [XmlAttribute("Key")]
        public string Key { get; set; }
    }
}