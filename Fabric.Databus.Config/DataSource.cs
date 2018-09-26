using System.Collections.Generic;
using System.Xml.Serialization;

namespace Fabric.Databus.Config
{
    public class DataSource : IDataSource
    {
        public string Sql { get; set; }

        [XmlAttribute("Path")]
        public string Path { get; set; }

        [XmlAttribute("PropertyType")]
        public string PropertyType { get; set; }

        public List<QueryField> Fields { get; set; } = new List<QueryField>();
        public int SequenceNumber { get; set; }
    }
}