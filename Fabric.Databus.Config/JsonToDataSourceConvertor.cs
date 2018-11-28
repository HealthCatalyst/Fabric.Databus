// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonToDataSourceConvertor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JsonToDataSourceConvertor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The json to data source convertor.
    /// </summary>
    public class JsonToDataSourceConvertor
    {
        /// <summary>
        /// The parse json into data sources.
        /// </summary>
        /// <param name="propertyNames">
        /// The property Names.
        /// </param>
        /// <param name="myObject">
        /// The my object.
        /// </param>
        /// <param name="dataSources">
        /// The data Sources.
        /// </param>
        /// <param name="keyLevels">
        /// The key Levels.
        /// </param>
        /// <param name="objectType">
        /// The j Object Type.
        /// </param>
        private static void InternalParseJsonIntoDataSources(IList<string> propertyNames, JToken myObject, IList<DataSource> dataSources, IList<string> keyLevels, JsonMetadataEntityType objectType)
        {
            JToken metadata = myObject is JObject ? myObject["_metadata"] : myObject.First["_metadata"];

            JsonMetadata jsonMetadata = metadata.ToObject<JsonMetadata>();

            var fullyQualifiedPropertyName = string.Join(".", propertyNames);

            var newKeyLevels = new List<string>(keyLevels)
                .Concat(jsonMetadata.keyLevels)
                .ToList();

            // get column names
            foreach (JToken property in myObject)
            {
                // ReSharper disable once InvertIf
                if (property is JProperty jProperty)
                {

                }
            }

            var myDataSources = jsonMetadata.entities.Select(
                entity => new DataSource
                              {
                                  Name = entity.databaseEntity,
                                  Path = propertyNames.Any() ? $"$.{fullyQualifiedPropertyName}" : "$",
                                  PropertyType = objectType.ToString(),
                                  Sql = $"SELECT * FROM {entity.databaseEntity}",
                                  KeyLevels = newKeyLevels
                              })
                .ToList();

            myDataSources.ForEach(dataSources.Add);

            foreach (JToken property in myObject)
            {
                // ReSharper disable once InvertIf
                if (property is JProperty jProperty)
                {
                    // ReSharper disable once InvertIf
                    if (jProperty.Name != "_metadata")
                    {
                        var nestedPropertyNames = new List<string>();
                        nestedPropertyNames.AddRange(propertyNames);
                        nestedPropertyNames.Add(jProperty.Name);
                        switch (jProperty.Value)
                        {
                            case JObject obj:
                                InternalParseJsonIntoDataSources(nestedPropertyNames, obj, dataSources, newKeyLevels, JsonMetadataEntityType.Object);
                                break;
                            case JArray array:
                                InternalParseJsonIntoDataSources(nestedPropertyNames, array, dataSources, newKeyLevels, JsonMetadataEntityType.Array);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The parse json into data sources.
        /// </summary>
        /// <param name="json">
        /// The json.
        /// </param>
        /// <returns>
        /// The <see cref="List{T}"/>
        /// data sources
        /// </returns>
        [Pure]
        public static List<DataSource> ParseJsonIntoDataSources(string json)
        {
            JObject myObject = JObject.Parse(json);
            var dataSources = new List<DataSource>();

            InternalParseJsonIntoDataSources(new List<string>(), myObject, dataSources, new List<string>(), JsonMetadataEntityType.Array);

            return dataSources;
        }
    }
}
