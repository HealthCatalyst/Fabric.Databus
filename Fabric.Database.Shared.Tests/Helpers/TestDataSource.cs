// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDataSource.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TestDataSource type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;

namespace Fabric.Database.Shared.Tests.Helpers
{
    using System.Collections.Generic;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;

    /// <inheritdoc />
    public class TestDataSource : IDataSource
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
        public IEnumerable<ISqlEntityColumnMapping> SqlEntityColumnMappings { get; } = new List<ISqlEntityColumnMapping>();

        /// <inheritdoc />
        public int NestingLevel => this.Path.GetNestedLevel();

        public IDataSource PrependRelationships(IEnumerable<ISqlRelationship> relationships)
        {
            this.Relationships = relationships.Concat(this.Relationships).ToList();
            return this;
        }
    }
}