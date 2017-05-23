using System.Collections.Generic;
using System.Xml.Serialization;

namespace Fabric.Databus.Config
{
    public class Job
    {
        public QueryConfig Config { get; set; }

        public Data Data { get; set; }
    }

    public class Data
    {
        public string DataModel { get; set; }

        [XmlElement("DataSource")]
        public List<DataSource> DataSources { get; set; }
    }
}