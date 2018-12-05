// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITopLevelDataSource.cs" company="">
//   
// </copyright>
// <summary>
//   The TopLevelDataSource interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Config
{
    using System.Collections.Generic;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.ElasticSearch;

    /// <summary>
    /// The TopLevelDataSource interface.
    /// </summary>
    public interface ITopLevelDataSource
    {
        /// <summary>
        /// Gets the incremental columns.
        /// </summary>
        IEnumerable<IIncrementalColumn> IncrementalColumns { get; }

        string Name { get; set; }

        string Sql { get; set; }

        string Path { get; set; }

        string PropertyType { get; set; }

        string TableOrView { get; set; }

        List<IQueryField> Fields { get; set; }

        int SequenceNumber { get; set; }

        IList<string> KeyLevels { get; set; }

        IEnumerable<ISqlRelationship> Relationships { get; }

        IEnumerable<ISqlEntityColumnMapping> SqlEntityColumnMappings { get; }

        string Key { get; set; }
    }
}