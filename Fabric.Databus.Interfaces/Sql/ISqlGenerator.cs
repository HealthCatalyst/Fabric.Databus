// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISqlGenerator.cs" company="">
//   
// </copyright>
// <summary>
//   The SqlGenerator interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Sql
{
    using System.Collections.Generic;
    using System.Text;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;

    /// <summary>
    /// The SqlGenerator interface.
    /// </summary>
    public interface ISqlGenerator
    {
        /// <summary>
        /// The add entity.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator SetEntity(string entity);

        /// <summary>
        /// The add column.
        /// </summary>
        /// <param name="entityName">
        /// The entity name.
        /// </param>
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <param name="alias">
        /// The alias.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator AddColumn(string entityName, string columnName, string alias);

        /// <summary>
        /// The add column.
        /// </summary>
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator AddColumn(string columnName);

        /// <summary>
        /// The add join.
        /// </summary>
        /// <param name="join">
        /// The join.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator AddJoin(ISqlGeneratorJoin join);

        /// <summary>
        /// The add top 1 query.
        /// </summary>
        /// <param name="maximumEntitiesToLoad">
        /// maximum entities to load</param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator AddTopFilter(int maximumEntitiesToLoad);

        /// <summary>
        /// The add cte.
        /// </summary>
        /// <param name="loadSql">
        /// The load sql.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator AddCTE(string loadSql);

        /// <summary>
        /// The add order by ascending.
        /// </summary>
        /// <param name="tableName">
        /// table name
        /// </param>
        /// <param name="columnName">
        ///     The column name.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator AddOrderByAscending(string tableName, string columnName);

        /// <summary>
        /// The add range filter.
        /// </summary>
        /// <param name="tableName">
        /// The table Name.
        /// </param>
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <param name="startVariable">
        /// The start variable.
        /// </param>
        /// <param name="endVariable">
        /// The end variable.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator AddRangeFilter(string tableName, string columnName, string startVariable, string endVariable);

        /// <summary>
        /// The add incremental column.
        /// </summary>
        /// <param name="tableOrViewName">
        /// The table Or View Name.
        /// </param>
        /// <param name="incrementalColumnName">
        /// The incremental column name.
        /// </param>
        /// <param name="incrementalColumnOperator">
        /// The incremental column operator.
        /// </param>
        /// <param name="incrementalColumnValue">
        /// The incremental column value.
        /// </param>
        /// <param name="incrementalColumnType">
        /// The incremental Column Type.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator AddIncrementalColumn(string tableOrViewName, string incrementalColumnName, string incrementalColumnOperator, string incrementalColumnValue, string incrementalColumnType);

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ToSqlString();

        /// <summary>
        /// The add incremental columns.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="incrementalColumns">
        ///     The incremental columns.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator AddIncrementalColumns(string tableName, IEnumerable<IIncrementalColumn> incrementalColumns);

        /// <summary>
        /// The create sql statement.
        /// </summary>
        /// <param name="entityName">
        /// The entity name.
        /// </param>
        /// <param name="topLevelKey">
        /// The top level key.
        /// </param>
        /// <param name="sqlRelationships">
        /// The sql relationships.
        /// </param>
        /// <param name="entityColumnMappings">
        /// The entity column mappings.
        /// </param>
        /// <param name="incrementalColumns">
        /// The incremental columns.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator CreateSqlStatement(
            string entityName,
            string topLevelKey,
            IEnumerable<ISqlRelationship> sqlRelationships,
            IEnumerable<ISqlEntityColumnMapping> entityColumnMappings,
            IEnumerable<IIncrementalColumn> incrementalColumns);
    }
}