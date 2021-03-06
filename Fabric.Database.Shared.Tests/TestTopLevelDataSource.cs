﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestTopLevelDataSource.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TestTopLevelDataSource type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Database.Shared.Tests
{
    using System.Collections.Generic;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;

    /// <inheritdoc />
    /// <summary>
    /// The test top level data source.
    /// </summary>
    public class TestTopLevelDataSource : ITopLevelDataSource
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Sql { get; set; }

        /// <inheritdoc />
        public string Path { get; set; }

        /// <inheritdoc />
        public string PropertyType { get; set; }

        /// <inheritdoc />
        public List<IQueryField> Fields { get; set; } = new List<IQueryField>();

        /// <inheritdoc />
        public int SequenceNumber { get; set; }

        /// <inheritdoc />
        public IList<string> KeyLevels { get; set; } = new List<string>();

        /// <inheritdoc />
        public string TableOrView { get; set; }

        /// <inheritdoc />
        public IEnumerable<ISqlRelationship> Relationships { get; set; } = new List<ISqlRelationship>();

        /// <inheritdoc />
        public IEnumerable<ISqlEntityColumnMapping> SqlEntityColumnMappings => this.MySqlEntityColumnMappings;

        /// <summary>
        /// Gets or sets the data sources.
        /// </summary>
        public List<SqlEntityColumnMapping> MySqlEntityColumnMappings { get; set; } = new List<SqlEntityColumnMapping>();

        /// <inheritdoc />
        public IEnumerable<IIncrementalColumn> IncrementalColumns => this.MyIncrementalColumns;

        /// <summary>
        /// Gets or sets the data sources.
        /// </summary>
        public List<IncrementalColumn> MyIncrementalColumns { get; set; } = new List<IncrementalColumn>();

        /// <inheritdoc />
        public string Key { get; set; }
    }
}
