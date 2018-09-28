using System.IO;
using System.Xml.Serialization;
using Fabric.Shared;
using Newtonsoft.Json;

namespace Fabric.Databus.Config
{
    public class ConfigReader
    {
        //public QueryConfig Read(string inputFile)
        //{
        //    var fileContents = File.ReadAllText(inputFile);
        //    return ReadFromText(fileContents);
        //}

        //public QueryConfig ReadFromText(string fileContents)
        //{
        //    var configs = JsonConvert.DeserializeObject<QueryConfig>(fileContents);
        //    return configs;
        //}

        public XmlJob ReadXml(string inputFile)
        {
            var fileContents = File.ReadAllText(inputFile);

            return this.ReadXmlFromText(fileContents);
        }

        public XmlJob ReadXmlFromText(string fileContents)
        {
            return fileContents.FromXml<XmlJob>();
        }
    }

}
