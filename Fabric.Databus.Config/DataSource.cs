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
    using System.Xml.Serialization;

    using ElasticSearchSqlFeeder.Interfaces;

    /// <summary>
    /// The data source.
    /// </summary>
    public class DataSource : IDataSource
    {
        /// <summary>
        /// Gets or sets the sql.
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        [XmlAttribute("Path")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the property type.
        /// </summary>
        [XmlAttribute("PropertyType")]
        public string PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        public List<IQueryField> Fields { get; set; } = new List<IQueryField>();

        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public int SequenceNumber { get; set; }
    }
}