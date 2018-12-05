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
    using System.Linq;

    using Newtonsoft.Json;

    /// <summary>
    /// The source wrapper collection.
    /// </summary>
    public class SourceWrapperCollection
    {
        /// <summary>
        /// The wrappers.
        /// </summary>
        private readonly IList<SourceWrapper> wrappers = new List<SourceWrapper>();

        /// <summary>
        /// The wrappers.
        /// </summary>
        private readonly IList<SourceWrapper> sortedWrappers = new List<SourceWrapper>();

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

            if (string.IsNullOrWhiteSpace(sourceWrapper.Id))
            {
                throw new ArgumentNullException(nameof(sourceWrapper.Id));
            }

            if (this.wrappers.Any(wrapper => wrapper.Id.Equals(sourceWrapper.Id)))
            {
                var previousWrapper = this.wrappers.First(wrapper => wrapper.Id.Equals(sourceWrapper.Id));
                previousWrapper.Rows.AddRange(sourceWrapper.Rows);
            }
            else
            {
                this.wrappers.Add(sourceWrapper);
            }
        }

        /// <summary>
        /// The sort all.
        /// </summary>
        public void SortAll()
        {
            var sourceWrappers = this.wrappers.OrderBy(wrapper => wrapper.Depth).ToList();
            foreach (var sourceWrapper in sourceWrappers)
            {
                this.Sort(sourceWrapper);
            }
        }

        /// <summary>
        /// The write to json.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        public void WriteToJson(JsonTextWriter writer)
        {
            if (!this.wrappers.Any())
            {
                throw new Exception("No source wrappers found");
            }

            if (this.wrappers.All(wrapper => wrapper.PropertyName != "$"))
            {
                throw new Exception($"No entity with property name $ was found");
            }

            var sourceWrapper = this.sortedWrappers.First(wrapper => wrapper.Parent == null);
            sourceWrapper.Write(null, writer, new List<KeyValuePair<string, object>>());
        }

        /// <summary>
        /// The any.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Any()
        {
            return this.wrappers.Any();
        }

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="sourceWrapper">
        /// The text source wrapper.
        /// </param>
        private void Sort(SourceWrapper sourceWrapper)
        {
            if (string.IsNullOrWhiteSpace(sourceWrapper.PropertyName))
            {
                throw new ArgumentNullException(nameof(sourceWrapper.PropertyName));
            }

            if (string.IsNullOrWhiteSpace(sourceWrapper.Id))
            {
                throw new ArgumentNullException(nameof(sourceWrapper.Id));
            }

            // if we already have a match by Id then use that
            if (this.sortedWrappers.Any(wrapper => wrapper.Id.Equals(sourceWrapper.Id)))
            {
                var previousWrapper = this.sortedWrappers.First(wrapper => wrapper.Id.Equals(sourceWrapper.Id));
                previousWrapper.Rows.AddRange(sourceWrapper.Rows);
            }
            else if (this.sortedWrappers.Any(wrapper => wrapper.PropertyName.Equals(sourceWrapper.PropertyName)))
            {
                this.wrappers.Add(sourceWrapper);
                var previousWrapper = this.sortedWrappers.First(wrapper => wrapper.PropertyName.Equals(sourceWrapper.PropertyName));
                previousWrapper.AddSibling(sourceWrapper);
            }
            else
            {
                this.sortedWrappers.Add(sourceWrapper);

                if (sourceWrapper.PropertyName != "$")
                {
                    var lastIndexOf = sourceWrapper.PropertyName.LastIndexOf('.');
                    var parentPropertyName = sourceWrapper.PropertyName.Substring(0, lastIndexOf);

                    if (!this.sortedWrappers.Any(wrapper => wrapper.PropertyName.Equals(parentPropertyName)))
                    {
                        throw new Exception($"Parent entity of {sourceWrapper.PropertyName} was not found");
                    }

                    var parentWrapper = this.sortedWrappers.First(wrapper => wrapper.PropertyName.Equals(parentPropertyName));
                    parentWrapper.AddChild(sourceWrapper);
                }
            }
        }
    }
}
