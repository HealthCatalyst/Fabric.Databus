﻿// --------------------------------------------------------------------------------------------------------------------
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
        public static void ParseJsonIntoDataSources(IList<string> propertyNames, JToken myObject, IList<DataSource> dataSources)
        {
            JToken metadata = myObject["_metadata"];

            JsonMetadata jsonMetadata = metadata.ToObject<JsonMetadata>();

            var fullyQualifiedPropertyName = string.Join(".", propertyNames);

            var myDataSources = jsonMetadata.entities.Select(
                entity => new DataSource
                {
                    Path = propertyNames.Any() ? $"$.{fullyQualifiedPropertyName}" : "$",
                    PropertyType = "object",
                    Sql = $"SELECT * FROM {entity.databaseEntity}",
                    KeyLevels = entity.keyLevels
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
                                ParseJsonIntoDataSources(nestedPropertyNames, obj, dataSources);
                                break;
                            case JArray array:
                                ParseJsonIntoDataSources(nestedPropertyNames, array, dataSources);
                                break;
                        }
                    }
                }
            }
        }
    }
}