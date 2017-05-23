using System.IO;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace Fabric.Shared
{
    public static class ListHelpers
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static string ToJsonPretty(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static T FromJson<T>(this string txt)
        {
            return JsonConvert.DeserializeObject<T>(txt);
        }

        public static T FromXml<T>(this string txt) where T : class
        {
            var xmlSerializer = new XmlSerializer(typeof(T));

            var reader = new StringReader(txt);
            var jobconfig = xmlSerializer.Deserialize(reader) as T;

            return jobconfig;
        }

        public static bool DoesStringContainPeriods(this string txt)
        {
            return txt != null && txt.Contains(".");
        }
    }
}
