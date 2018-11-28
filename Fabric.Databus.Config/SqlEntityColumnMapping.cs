// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlEntityColumnMapping.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlEntityColumnMapping type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// The sql entity column mapping.
    /// </summary>
    public class SqlEntityColumnMapping : ISqlEntityColumnMapping
    {
        /// <inheritdoc />
        [XmlAttribute("Entity")]
        [DataMember]
        public string Entity { get; set; }

        /// <inheritdoc />
        [XmlAttribute("Name")]
        [DataMember]
        public string Name { get; set; }

        /// <inheritdoc />
        [XmlAttribute("Alias")]
        [DataMember]
        public string Alias { get; set; }
    }
}