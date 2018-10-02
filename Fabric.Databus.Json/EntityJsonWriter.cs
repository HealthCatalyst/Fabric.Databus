namespace Fabric.Databus.Json
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Shared;
    using Fabric.Shared;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <inheritdoc />
    public class EntityJsonWriter : IEntityJsonWriter
    {
        /// <summary>
        /// The get json for row for merge.
        /// </summary>
        /// <param name="columns">
        ///     The columns.
        /// </param>
        /// <param name="rows">
        ///     The rows.
        /// </param>
        /// <param name="propertyName">
        ///     The property name.
        /// </param>
        /// <param name="propertyTypes">
        /// The property types
        /// </param>
        /// <returns>
        /// The <see cref="JObject"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">exception thrown
        /// </exception>
        public JObject[] GetJsonForRowForMerge(
            List<ColumnInfo> columns,
            List<object[]> rows,
            string propertyName,
            IDictionary<string, string> propertyTypes)
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            // ReSharper disable once StyleCop.SA1305
            var jObjects = new List<JObject>();

            foreach (var row in rows)
            {
                // ReSharper disable once StyleCop.SA1305
                var jObjectOuter = new JObject();

                // ReSharper disable once StyleCop.SA1305
                var jObject = jObjectOuter;

                if (!string.IsNullOrEmpty(propertyName))
                {
                    var properties = propertyName.Split('.');

                    // ReSharper disable once StyleCop.SA1305
                    var jObject1 = jObjectOuter;
                    int level = 1;

                    string currentFullPropertyName = null;

                    foreach (var property in properties)
                    {
                        currentFullPropertyName = currentFullPropertyName != null
                            ? currentFullPropertyName + '.' + property
                            : property;

                        jObject = new JObject();

                        var propertyType = propertyTypes[currentFullPropertyName];

                        if (propertyType != null && propertyType.Equals("object", StringComparison.OrdinalIgnoreCase))
                        {
                            jObject1[property] = jObject;
                            jObject1 = jObject;
                        }
                        else
                        {
                            // it is a nested list

                            // add a key column so we know what key this object corresponds to
                            var key = "KeyLevel" + level++;
                            var keyValue =
                                row[columns.FirstOrDefault(c => c.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                                    .Verify($"{key} column not found")
                                    .index];

                            jObject.Add(key, GetJToken(keyValue));
                            // ReSharper disable once StyleCop.SA1305
                            var jArray = new JArray { jObject };
                            jObject1[property] = jArray;
                            jObject1 = jObject;
                        }
                    }
                }

                foreach (var col in columns)
                {
                    var value = row[col.index];

                    var shouldWriteColumn = value != null && value != DBNull.Value;

                    // only write if it is not the default
                    if (
                        col.ElasticSearchType.Equals("keyword", StringComparison.OrdinalIgnoreCase)
                        || col.ElasticSearchType.Equals("text", StringComparison.OrdinalIgnoreCase))
                    {
                        if (value == null
                            || value == DBNull.Value
                            || string.IsNullOrEmpty((string)value)
                            || ((string)value).Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        {
                            shouldWriteColumn = false;
                        }
                    }

                    if (shouldWriteColumn)
                    {
                        JToken tempToken;
                        if (!jObject.TryGetValue(col.Name, out tempToken))
                        {
                            jObject.Add(col.Name, GetJToken(value));
                        }

                        //jsonValueWriter.WriteValue(writer, col.ElasticSearchType, value);
                    }
                }
                jObjects.Add(jObjectOuter);
            }

            return jObjects.ToArray();
        }

        /// <summary>
        /// The set properties by merge.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="newJObjects">
        /// The new j objects.
        /// </param>
        /// <param name="document">
        /// The document.
        /// </param>
        public void SetPropertiesByMerge(string propertyName, JObject[] newJObjects, JObject document)
        {
            // TODO: optimize this to avoid a loop
            foreach (var newJObject in newJObjects)
            {
                this.MergeWithDocumentFast(document, newJObject, 0);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// The remove temporary columns.
        /// </summary>
        /// <param name="node">
        ///     The node.
        /// </param>
        /// <param name="topLevelKeyColumn">
        /// The top level key column
        /// </param>
        public void RemoveTemporaryColumns(JObject node, string topLevelKeyColumn)
        {
            this.WalkNode(
                node,
                n =>
                    {
                        var properties = n.Properties().Select(p => p.Name).ToList();
                        var propertiesToRemove = properties
                            .Where(p => p.StartsWith("KeyLevel", StringComparison.OrdinalIgnoreCase)).ToList();

                        foreach (var property in propertiesToRemove)
                        {
                            n.Remove(property);
                        }

                        if (n.Parent != null)
                        {
                            propertiesToRemove = properties.Where(
                                p => p.StartsWith(topLevelKeyColumn, StringComparison.OrdinalIgnoreCase)).ToList();

                            foreach (var property in propertiesToRemove)
                            {
                                n.Remove(property);
                            }
                        }
                    });
        }

        /// <inheritdoc />
        public void WriteMappingToStream(List<ColumnInfo> columnList, string propertyPath, StreamWriter textWriter, string propertyType, string entity)
        {
            using (var writer = new JsonTextWriter(textWriter))
            {
                writer.WriteStartObject(); // begin

                using (new JsonPropertyWrapper(writer, "mappings", propertyPath != null))
                {
                    using (new JsonPropertyWrapper(writer, entity, propertyPath != null))
                    {
                        using (new JsonPropertyWrapper(writer, "properties"))
                        {
                            // see if propertyPath was specified
                            if (!string.IsNullOrEmpty(propertyPath))
                            {
                                var queue = new Queue<string>();
                                foreach (var property in propertyPath.Split('.'))
                                {
                                    queue.Enqueue(property);
                                }

                                this.WriteNestedProperty(columnList, queue, writer, propertyType);
                            }
                            else
                            {
                                this.InternalWriteColumnsToJson(columnList, writer);
                            }
                        }
                    }
                }

                writer.WriteEndObject(); // end
            }
        }

        /// <summary>
        /// The write nested property.
        /// </summary>
        /// <param name="columnList">
        /// The column list.
        /// </param>
        /// <param name="properties">
        /// The properties.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        private void WriteNestedProperty(List<ColumnInfo> columnList, Queue<string> properties, JsonTextWriter writer, string propertyType)
        {
            if (properties.Any())
            {
                var propertyName = properties.Dequeue();

                using (new JsonPropertyWrapper(writer, propertyName))
                {
                    writer.WritePropertyName("type");
                    var type = propertyType != null && propertyType.Equals("object", StringComparison.OrdinalIgnoreCase)
                        ? "object"
                        : "nested";

                    writer.WriteValue(type);
                    using (new JsonPropertyWrapper(writer, "properties"))
                    {
                        this.WriteNestedProperty(columnList, properties, writer, propertyType);
                    }
                }
            }
            else
            {
                this.InternalWriteColumnsToJson(columnList, writer);
            }
        }

        /// <summary>
        /// The internal write columns to json.
        /// </summary>
        /// <param name="columnList">
        /// The column list.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        private void InternalWriteColumnsToJson(List<ColumnInfo> columnList, JsonTextWriter writer)
        {
            foreach (var column in columnList)
            {
                using (new JsonPropertyWrapper(writer, column.Name))
                {
                    writer.WritePropertyName("type");
                    writer.WriteValue(column.ElasticSearchType);
                }
            }
        }

        /// <summary>
        /// The walk node.
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        private void WalkNode(JToken node, Action<JObject> action)
        {
            if (node.Type == JTokenType.Object)
            {
                action((JObject)node);

                foreach (JProperty child in node.Children<JProperty>())
                {
                    this.WalkNode(child.Value, action);
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                foreach (JToken child in node.Children())
                {
                    this.WalkNode(child, action);
                }
            }
        }

        /// <summary>
        /// The get j token.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="JToken"/>.
        /// </returns>
        private static JToken GetJToken(object value)
        {
            return value != null ? JToken.FromObject(value) : null;
        }

        /// <summary>
        /// The get json for rows.
        /// </summary>
        /// <param name="columns">
        /// The columns.
        /// </param>
        /// <param name="jsonValueWriter">
        /// The json value writer.
        /// </param>
        /// <param name="rows">
        /// The rows.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <returns>
        /// The <see cref="JObject[]"/>.
        /// </returns>
        private JObject[] GetJsonForRows(List<ColumnInfo> columns, IJsonValueWriter jsonValueWriter, List<object[]> rows, string propertyName)
        {
            // ReSharper disable once StyleCop.SA1305
            var jObjects = new List<JObject>();

            foreach (var row in rows)
            {
                // ReSharper disable once StyleCop.SA1305
                var jObject = new JObject();
                foreach (var col in columns)
                {
                    var value = row[col.index];

                    var shouldWriteColumn = value != null && value != DBNull.Value;

                    // only write if it is not the default
                    if (col.ElasticSearchType.Equals("keyword", StringComparison.OrdinalIgnoreCase)
                        || col.ElasticSearchType.Equals("text", StringComparison.OrdinalIgnoreCase))
                    {
                        if (value == null
                            || value == DBNull.Value
                            || string.IsNullOrEmpty((string)value)
                            || ((string)value).Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        {
                            shouldWriteColumn = false;
                        }
                    }

                    if (shouldWriteColumn)
                    {
                        jObject.Add(col.Name, value != null ? JToken.FromObject(value) : null);

                        //jsonValueWriter.WriteValue(writer, col.ElasticSearchType, value);
                    }
                }

                jObjects.Add(jObject);
            }

            return jObjects.ToArray();
        }

        /// <summary>
        /// The merge with document fast.
        /// </summary>
        /// <param name="originalJObject">
        /// The original j object.
        /// </param>
        /// <param name="newJObject">
        /// The new j object.
        /// </param>
        /// <param name="level">
        /// The level.
        /// </param>
        private void MergeWithDocumentFast(JObject originalJObject, JObject newJObject, int level)
        {
            level++;

            //check if the keys match then this is the same object so merge else add to list

            // iterate through all the arrays in newJObject
            //var jTokenType = newJObject.Type;
            //var jEnumerable = newJObject.Children();
            //var count = jEnumerable.Count();
            //var firstOrDefault = jEnumerable.FirstOrDefault();

            this.CopyAllProperties(originalJObject, newJObject);

            foreach (KeyValuePair<string, JToken> property in newJObject)
            {
                if (property.Value.Type == JTokenType.Array)
                {
                    var array = property.Value as JArray;
                    var selectProperty = originalJObject.SelectToken(property.Key) as JArray;
                    if (selectProperty == null)
                    {
                        // create array if it does not exist
                        // else create a new array
                        selectProperty = new JArray { };
                        originalJObject.Add(property.Key, selectProperty);
                    }
                    this.MergeArrayFast(selectProperty, array, level);
                }

            }
        }

        private void MergeArrayFast(JArray originalArray, JArray newArray, int level)
        {
            var jEnumerable = newArray.Children();
            foreach (var child in jEnumerable)
            {
                if (child.Type == JTokenType.Object)
                {
                    var key = "KeyLevel" + level;

                    // try to match on key to see if originalArray also has this item
                    var newJObject = (child as JObject);
                    var newKeyValue = newJObject.Property(key).Value;

                    bool found = false;
                    foreach (var originalChild in originalArray.Children())
                    {
                        var originalJObject = (originalChild as JObject);
                        var originalKeyValue = originalJObject.Property(key).Value;
                        if (originalKeyValue.Value<string>() == newKeyValue.Value<string>())
                        {
                            found = true;
                            // found a match on key so try to merge
                            this.MergeWithDocumentFast(originalJObject, newJObject, level);
                            break;
                        }
                    }

                    if (!found)
                    {
                        this.AddObjectAtEndOfArray(originalArray, newJObject);
                    }
                }
            }
        }

        private void AddObjectAtEndOfArray(JArray originalArray, JObject newJObject)
        {
            originalArray.Add(newJObject);
            //            // not found so add at the end
            //            if (originalArray.Last == null)
            //            {
            //                if (originalArray.First != null)
            //                {
            //// if property exists then add to the current array
            //                    originalArray.First.AddAfterSelf(newJObject);
            //                }
            //                else
            //                {
            //                    originalArray.Children().
            //                }
            //            }
            //            else
            //            {
            //                // if property exists then add to the current array
            //                originalArray.Last.AddAfterSelf(newJObject);
            //            }
        }

        private void SetProperties(string propertyName, JObject newJObject, JObject document)
        {
            // if propertyName is not set then copy all the properties into this new object
            if (propertyName == null)
            {
                this.CopyAllProperties(document, newJObject);
            }
            else
            {
                // else see if the property already exists
                var selectProperty = document.SelectToken(propertyName);
                if (selectProperty != null)
                {
                    if (selectProperty.Last == null)
                    {
                        // if property exists then add to the current array
                        selectProperty.First.AddAfterSelf(newJObject);
                    }
                    else
                    {
                        // if property exists then add to the current array
                        selectProperty.Last.AddAfterSelf(newJObject);
                    }

                }
                else
                {
                    // else create a new array
                    var jArray = new JArray { newJObject };
                    document[propertyName] = jArray;
                }
            }
        }

        private void CopyAllProperties(JObject originalJObject, JObject newJObject)
        {
            foreach (var property in newJObject.Properties())
            {
                if (property.Value.Type != JTokenType.Array
                    //&& property.Value.Type != JTokenType.Object
                    && !property.Name.StartsWith("KeyLevel", StringComparison.OrdinalIgnoreCase))
                {
                    originalJObject[property.Name] = property.Value;
                }
            }
        }
    }
}
