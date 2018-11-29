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
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator AddOrderByAscending(string columnName);

        /// <summary>
        /// The add range filter.
        /// </summary>
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
        ISqlGenerator AddRangeFilter(string columnName, string startVariable, string endVariable);

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ToSqlString();
    }
}