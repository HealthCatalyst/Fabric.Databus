// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSource.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the DataSource type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;

    /// <inheritdoc />
    [DataContract(Name = "DataSource", Namespace = "")]
    [XmlType("DataSource")]
    public class DataSource : IDataSource
    {
        /// <inheritdoc />
        [XmlAttribute("Name")]
        [DataMember]
        public string Name { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string Sql
        {
            get;
            set;
        }

        /// <inheritdoc />
        [XmlAttribute("Path")]
        [DataMember]
        public string Path { get; set; }

        /// <inheritdoc />
        [XmlAttribute("PropertyType")]
        [DataMember]
        public string PropertyType { get; set; }

        /// <inheritdoc />
        [XmlIgnore]
        public List<IQueryField> Fields { get; set; } = new List<IQueryField>();

        /// <inheritdoc />
        public int SequenceNumber { get; set; }

        /// <inheritdoc />
        [XmlIgnore]
        public IList<string> KeyLevels { get; set; }
    }
}