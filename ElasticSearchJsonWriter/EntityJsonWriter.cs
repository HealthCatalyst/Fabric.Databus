namespace ElasticSearchJsonWriter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Shared;

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

        private static JToken GetJToken(object value)
        {
            return value != null ? JToken.FromObject(value) : null;
        }

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

            CopyAllProperties(originalJObject, newJObject);

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
                    MergeArrayFast(selectProperty, array, level);
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
                            MergeWithDocumentFast(originalJObject, newJObject, level);
                            break;
                        }
                    }

                    if (!found)
                    {
                        AddObjectAtEndOfArray(originalArray, newJObject);
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
                CopyAllProperties(document, newJObject);
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
