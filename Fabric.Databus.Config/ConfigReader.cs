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

        public Job ReadXml(string inputFile)
        {
            var fileContents = File.ReadAllText(inputFile);

            return ReadXmlFromText(fileContents);
        }

        public Job ReadXmlFromText(string fileContents)
        {
            return fileContents.FromXml<Job>();
        }
    }

}
