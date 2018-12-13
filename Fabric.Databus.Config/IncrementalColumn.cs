// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IncrementalColumn.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IncrementalColumn type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Xml.Serialization;

    using Fabric.Databus.Interfaces.Config;

    /// <inheritdoc />
    /// <summary>
    /// The incremental column.
    /// </summary>
    public class IncrementalColumn : IIncrementalColumn
    {
        /// <inheritdoc />
        [XmlAttribute]
        public string Name { get; set; }

        /// <inheritdoc />
        [XmlAttribute]
        public string Operator { get; set; }

        /// <inheritdoc />
        [XmlAttribute]
        public string Type { get; set; }

        /// <inheritdoc />
        [XmlAttribute]
        public string Value { get; set; }
    }
}