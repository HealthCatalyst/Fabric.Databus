// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopLevelDataSource.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TopLevelDataSource type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Fabric.Databus.Interfaces.Config;

    /// <summary>
    /// The top level data source.
    /// </summary>
    // ReSharper disable once CommentTypo
    // ReSharper disable once InheritdocConsiderUsage
    public class TopLevelDataSource : DataSource, ITopLevelDataSource
    {
        /// <inheritdoc />
        [XmlIgnore]
        public IEnumerable<IIncrementalColumn> IncrementalColumns => this.MyIncrementalColumns;

        /// <summary>
        /// Gets or sets the data sources.
        /// </summary>
        [DataMember(Name = "IncrementalColumns")]
        [XmlElement("IncrementalColumn")]
        public List<IncrementalColumn> MyIncrementalColumns { get; set; } = new List<IncrementalColumn>();

        /// <inheritdoc />
        [XmlAttribute]
        public string Key { get; set; }
    }
}