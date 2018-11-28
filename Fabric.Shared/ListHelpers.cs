// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListHelpers.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ListHelpers type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared
{
    using System.IO;
    using System.Xml.Serialization;

    using Newtonsoft.Json;

    using Formatting = Newtonsoft.Json.Formatting;

    /// <summary>
    /// The list helpers.
    /// </summary>
    public static class ListHelpers
    {
        /// <summary>
        /// The to json.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// The to json pretty.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToJsonPretty(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        /// <summary>
        /// The from json.
        /// </summary>
        /// <param name="txt">
        /// The txt.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T FromJson<T>(this string txt)
        {
            return JsonConvert.DeserializeObject<T>(txt);
        }

        /// <summary>
        /// The from xml.
        /// </summary>
        /// <param name="txt">
        /// The txt.
        /// </param>
        /// <typeparam name="T">
        /// type param
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>
        /// </returns>
        public static T FromXml<T>(this string txt) where T : class
        {
            var xmlSerializer = new XmlSerializer(typeof(T));

            var reader = new StringReader(txt);
            var jobConfig = xmlSerializer.Deserialize(reader) as T;

            return jobConfig;
        }

        /// <summary>
        /// The does string contain periods.
        /// </summary>
        /// <param name="txt">
        /// The txt.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool DoesStringContainPeriods(this string txt)
        {
            return txt != null && txt.Contains(".");
        }
    }
}
