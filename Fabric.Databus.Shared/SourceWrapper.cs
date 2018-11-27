// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceWrapper.cs" company="">
//   
// </copyright>
// <summary>
//   The my dynamic object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Fabric.Databus.Interfaces.Sql;

    using Newtonsoft.Json;

    /// <summary>
    /// The my dynamic object.
    /// </summary>
    public class SourceWrapper
    {
        /// <summary>
        /// The key level name.
        /// </summary>
        private const string KeyLevelName = "KeyLevel";

        /// <summary>
        /// The key columns.
        /// </summary>
        private readonly IList<string> keyColumns;

        /// <summary>
        /// The is array.
        /// </summary>
        private readonly bool isArray;

        /// <summary>
        /// The keep temporary lookup columns in output.
        /// </summary>
        private readonly bool keepTemporaryLookupColumnsInOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceWrapper"/> class.
        /// </summary>
        /// <param name="id">
        ///     id of source wrapper
        /// </param>
        /// <param name="columns">
        ///     The columns.
        /// </param>
        /// <param name="propertyName">
        ///     The property name.
        /// </param>
        /// <param name="rows">
        ///     The rows.
        /// </param>
        /// <param name="keyColumns">
        ///     The key Columns.
        /// </param>
        /// <param name="isArray">
        ///     The is Array.
        /// </param>
        /// <param name="keepTemporaryLookupColumnsInOutput">
        /// keep temporary lookup columns in output</param>
        public SourceWrapper(
            string id,
            List<ColumnInfo> columns,
            string propertyName,
            List<object[]> rows,
            IList<string> keyColumns,
            bool isArray,
            bool keepTemporaryLookupColumnsInOutput)
        {
            this.keyColumns = keyColumns;
            this.isArray = isArray;
            this.keepTemporaryLookupColumnsInOutput = keepTemporaryLookupColumnsInOutput;
            this.Id = id;
            this.Columns = columns;
            this.PropertyName = propertyName;
            this.PropertyNameLastPart = GetLastPart(propertyName);
            this.Rows = rows;
            this.Children = new List<SourceWrapper>();
            this.Siblings = new List<SourceWrapper>();
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        public List<ColumnInfo> Columns { get; set; }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets the property name last part.
        /// </summary>
        public string PropertyNameLastPart { get; }

        /// <summary>
        /// Gets or sets the rows.
        /// </summary>
        public List<object[]> Rows { get; set; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        private List<SourceWrapper> Children { get; }

        /// <summary>
        /// Gets the siblings.
        /// </summary>
        private List<SourceWrapper> Siblings { get; }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="childSourcesWrapper">
        ///     The patient sources wrapper.
        /// </param>
        public void AddChild(SourceWrapper childSourcesWrapper)
        {
            this.Children.Add(childSourcesWrapper);
        }

        /// <summary>
        /// The add sibling.
        /// </summary>
        /// <param name="sourceWrapper">
        /// The source wrapper.
        /// </param>
        public void AddSibling(SourceWrapper sourceWrapper)
        {
            this.Siblings.Add(sourceWrapper);
        }


        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="propertyName">
        /// property name
        /// </param>
        /// <param name="writer">
        ///     The writer.
        /// </param>
        /// <param name="keys">
        ///     The keys.
        /// </param>
        public void Write(string propertyName, JsonWriter writer, IList<KeyValuePair<string, object>> keys)
        {
            var rows = this.Rows;

            // filter the list by keys
            if (keys.Any())
            {
                foreach (var keyValuePair in keys)
                {
                    var indexOfColumn = this.GetIndexOfColumn(keyValuePair.Key);

                    rows = rows.Where(row => keyValuePair.Value.Equals(row[indexOfColumn])).ToList();
                }
            }

            if (!rows.Any())
            {
                return;
            }

            if (!string.IsNullOrEmpty(propertyName))
            {
                writer.WritePropertyName(propertyName);
            }

            if (this.isArray)
            {
                writer.WriteStartArray();
            }
            else
            {
                // writer.WriteStartObject();
            }

            if (!this.isArray)
            {
                if (rows.Count > 1)
                {
                    rows = rows.Take(1).ToList();
                }
            }

            foreach (var row in rows)
            {
                writer.WriteStartObject();
                foreach (var column in this.Columns)
                {
                    if (!this.keepTemporaryLookupColumnsInOutput)
                    {
                        if (column.Name.StartsWith(KeyLevelName))
                        {
                            continue;
                        }
                    }

                    writer.WritePropertyName(column.Name);
                    writer.WriteValue(row[column.index]);
                }

                // write siblings
                foreach (var sibling in this.Siblings)
                {
                    // find sibling rows matched by key
                    var keyValuePairs = this.keyColumns.Select(
                        keyColumnName => new KeyValuePair<string, object>(
                            keyColumnName,
                            row[this.GetIndexOfColumn(keyColumnName)])).ToList();

                    sibling.WriteProperties(writer, keyValuePairs);
                }

                foreach (var child in this.Children)
                {
                    var keyValuePairs = this.keyColumns.Select(
                        keyColumnName => new KeyValuePair<string, object>(
                            keyColumnName,
                            row[this.GetIndexOfColumn(keyColumnName)])).ToList();

                    child.Write(child.PropertyNameLastPart, writer, keyValuePairs);
                }

                writer.WriteEndObject();
            }

            if (this.isArray)
            {
                writer.WriteEndArray();
            }
        }

        /// <summary>
        /// The write properties.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        public void WriteProperties(JsonWriter writer, IList<KeyValuePair<string, object>> keys)
        {
            var rows = this.Rows;

            // filter the list by keys
            if (keys.Any())
            {
                foreach (var keyValuePair in keys)
                {
                    var indexOfColumn = this.GetIndexOfColumn(keyValuePair.Key);

                    rows = rows.Where(row => keyValuePair.Value.Equals(row[indexOfColumn])).ToList();
                }
            }

            if (!rows.Any())
            {
                return;
            }

            if (rows.Count > 1)
            {
                rows = rows.Take(1).ToList();
            }

            foreach (var row in rows)
            {
                foreach (var column in this.Columns)
                {
                    if (!this.keepTemporaryLookupColumnsInOutput)
                    {
                        if (column.Name.StartsWith(KeyLevelName))
                        {
                            continue;
                        }
                    }

                    writer.WritePropertyName(column.Name);
                    writer.WriteValue(row[column.index]);
                }

                foreach (var child in this.Children)
                {
                    var keyValuePairs = this.keyColumns.Select(
                        keyColumnName => new KeyValuePair<string, object>(
                            keyColumnName,
                            row[this.GetIndexOfColumn(keyColumnName)])).ToList();

                    child.Write(child.PropertyNameLastPart, writer, keyValuePairs);
                }
            }
        }

        /// <summary>
        /// The get last part.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetLastPart(string propertyName)
        {
            return string.IsNullOrWhiteSpace(propertyName) ? propertyName : propertyName.Split('.').Last();
        }

        /// <summary>
        /// The get index of column.
        /// </summary>
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int GetIndexOfColumn(string columnName)
        {
            return this.Columns
                .Where(column => column.Name.Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                .Select(column => column.index).First();
        }
    }
}