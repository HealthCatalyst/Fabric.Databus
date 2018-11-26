﻿// --------------------------------------------------------------------------------------------------------------------
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
        /// The key columns.
        /// </summary>
        private readonly IList<string> keyColumns;

        /// <summary>
        /// The is array.
        /// </summary>
        private readonly bool isArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceWrapper"/> class.
        /// </summary>
        /// <param name="columns">
        /// The columns.
        /// </param>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="rows">
        /// The rows.
        /// </param>
        /// <param name="keyColumns">
        /// The key Columns.
        /// </param>
        /// <param name="isArray">
        /// The is Array.
        /// </param>
        public SourceWrapper(List<ColumnInfo> columns, string propertyName, List<object[]> rows, IList<string> keyColumns, bool isArray)
        {
            this.keyColumns = keyColumns;
            this.isArray = isArray;
            this.Columns = columns;
            this.PropertyName = propertyName;
            this.PropertyNameLastPart = GetLastPart(propertyName);
            this.Rows = rows;
            this.Children = new List<SourceWrapper>();
        }

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
        /// The merge.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="childSourcesWrapper">
        /// The patient sources wrapper.
        /// </param>
        public void Merge(string propertyName, SourceWrapper childSourcesWrapper)
        {
            this.Children.Add(childSourcesWrapper);
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

                writer.WriteEndObject();
            }

            if (this.isArray)
            {
                writer.WriteEndArray();
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