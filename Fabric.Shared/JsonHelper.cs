using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Fabric.Shared
{
    public static class JsonHelper
    {
        private static void SetProperties(string propertyName, JObject[] newJObjects, JObject document)
        {
            foreach (var newJObject in newJObjects)
            {
                SetProperties(propertyName, newJObject, document);//TODO: optimize this
            }
        }

        public static void SetPropertiesByMerge(string propertyName, JObject[] newJObjects, JObject document)
        {
            foreach (var newJObject in newJObjects) //TODO: optimize this to avoid a loop
            {
                //MergeWithDocument(document, newJObject);
                MergeWithDocumentFast(document, newJObject, 0);
            }
        }

        private static void MergeWithDocument(JObject document, JObject newJObject)
        {
            document.Merge(newJObject, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union,
                MergeNullValueHandling = MergeNullValueHandling.Ignore
            });
        }

        private static void MergeWithDocumentFast(JObject originalJObject, JObject newJObject, int level)
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

        private static void MergeArrayFast(JArray originalArray, JArray newArray, int level)
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

        private static void AddObjectAtEndOfArray(JArray originalArray, JObject newJObject)
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

        private static void SetProperties(string propertyName, JObject newJObject, JObject document)
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

        private static void CopyAllProperties(JObject originalJObject, JObject newJObject)
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
