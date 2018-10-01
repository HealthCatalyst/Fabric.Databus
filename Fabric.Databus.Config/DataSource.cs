// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSource.cs" company="">
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

    using ElasticSearchSqlFeeder.Interfaces;

    /// <summary>
    /// The data source.
    /// </summary>
    [DataContract(Name = "DataSource", Namespace = "")]
    [XmlType("DataSource")]
    public class DataSource : IDataSource
    {
        /// <summary>
        /// Gets or sets the sql.
        /// </summary>
        [DataMember]
        public string Sql
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        [XmlAttribute("Path")]
        [DataMember]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the property type.
        /// </summary>
        [XmlAttribute("PropertyType")]
        [DataMember]
        public string PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        [XmlIgnore]
        public List<IQueryField> Fields { get; set; } = new List<IQueryField>();

        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public int SequenceNumber { get; set; }
    }
}