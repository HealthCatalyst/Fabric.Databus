// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceWrapperCollection.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SourceWrapperCollection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// The source wrapper collection.
    /// </summary>
    public class SourceWrapperCollection
    {
        /// <summary>
        /// The wrappers.
        /// </summary>
        private readonly IDictionary<string, SourceWrapper> wrappers = new Dictionary<string, SourceWrapper>();

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="sourceWrapper">
        /// The text source wrapper.
        /// </param>
        public void Add(SourceWrapper sourceWrapper)
        {
            if (string.IsNullOrWhiteSpace(sourceWrapper.PropertyName))
            {
                throw new ArgumentNullException(nameof(sourceWrapper.PropertyName));
            }

            if (this.wrappers.ContainsKey(sourceWrapper.PropertyName))
            {
                var previousWrapper = this.wrappers[sourceWrapper.PropertyName];
                previousWrapper.Rows.AddRange(sourceWrapper.Rows);
            }
            else
            {
                this.wrappers.Add(new KeyValuePair<string, SourceWrapper>(sourceWrapper.PropertyName, sourceWrapper));

                if (sourceWrapper.PropertyName != "$")
                {
                    var lastIndexOf = sourceWrapper.PropertyName.LastIndexOf('.');
                    var parentPropertyName = sourceWrapper.PropertyName.Substring(0, lastIndexOf);

                    if (!this.wrappers.ContainsKey(parentPropertyName))
                    {
                        throw new Exception($"Parent entity of {sourceWrapper.PropertyName} was not found");
                    }

                    this.wrappers[parentPropertyName].Merge(sourceWrapper.PropertyName, sourceWrapper);
                }
            }
        }

        /// <summary>
        /// The add or merge.
        /// </summary>
        /// <param name="sourceWrapper">
        /// The source wrapper.
        /// </param>
        public void AddOrMerge(SourceWrapper sourceWrapper)
        {

        }

        /// <summary>
        /// The write to json.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        public void WriteToJson(JsonTextWriter writer)
        {
            if (!this.wrappers.ContainsKey("$"))
            {
                throw new Exception($"No entity with property name $ was found");
            }

            var sourceWrapper = this.wrappers["$"];
            sourceWrapper.Write(null, writer, new List<KeyValuePair<string, object>>());
        }
    }
}
