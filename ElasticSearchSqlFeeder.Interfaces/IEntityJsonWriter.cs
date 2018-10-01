// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEntityJsonWriter.cs" company="">
//   
// </copyright>
// <summary>
//   The EntityJsonWriter interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The EntityJsonWriter interface.
    /// </summary>
    public interface IEntityJsonWriter
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
        JObject[] GetJsonForRowForMerge(
            List<ColumnInfo> columns,
            List<object[]> rows,
            string propertyName,
            IDictionary<string, string> propertyTypes);

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
        void SetPropertiesByMerge(string propertyName, JObject[] newJObjects, JObject document);

        /// <summary>
        /// The remove temporary columns.
        /// </summary>
        /// <param name="node">
        ///     The node.
        /// </param>
        /// <param name="topLevelKeyColumn">
        /// The top level key column
        /// </param>
        void RemoveTemporaryColumns(JObject node, string topLevelKeyColumn);

        /// <summary>
        /// The write mapping to stream.
        /// </summary>
        /// <param name="columnList">
        /// The column list.
        /// </param>
        /// <param name="propertyPath">
        /// The property path.
        /// </param>
        /// <param name="textWriter">
        /// The text writer.
        /// </param>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <param name="entity">
        /// The entity.
        /// </param>
        void WriteMappingToStream(List<ColumnInfo> columnList, string propertyPath, StreamWriter textWriter, string propertyType, string entity);
    }
}