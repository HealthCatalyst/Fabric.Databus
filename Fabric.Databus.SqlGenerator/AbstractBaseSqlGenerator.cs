// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractBaseSqlGenerator.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the AbstractBaseSqlGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.SqlGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Shared;

    /// <inheritdoc />
    public abstract class AbstractBaseSqlGenerator : ISqlGenerator
    {
        /// <summary>
        /// Gets or sets the select columns.
        /// </summary>
        protected readonly IList<ISqlGeneratorColumn> SelectColumns = new List<ISqlGeneratorColumn>();

        /// <summary>
        /// The select joins.
        /// </summary>
        protected readonly IList<ISqlGeneratorJoin> SelectJoins = new List<ISqlGeneratorJoin>();

        /// <summary>
        /// Gets the destination entity.
        /// </summary>
        protected string DestinationEntity { get; private set; }

        /// <summary>
        /// Gets the top filter count.
        /// </summary>
        protected int TopFilterCount { get; private set; }

        /// <summary>
        /// Gets the query for cte.
        /// </summary>
        protected string QueryForCTE { get; private set; }

        /// <summary>
        /// Gets the order by column ascending.
        /// </summary>
        protected string OrderByColumnAscending { get; private set; }

        /// <summary>
        /// Gets the range filter.
        /// </summary>
        protected SqlRangeFilter RangeFilter { get; private set; }

        /// <inheritdoc />
        public ISqlGenerator SetEntity(string entity)
        {
            this.DestinationEntity = entity;
            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddColumn(string entityName, string columnName, string alias)
        {
            this.SelectColumns.Add(new SqlGeneratorColumn
                                       {
                                           EntityName = entityName,
                                           ColumnName = columnName,
                                           Alias = alias
                                       });
            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddColumn(string columnName)
        {
            this.AddColumn(null, columnName, null);
            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddJoin(ISqlGeneratorJoin join)
        {
            this.SelectJoins.Add(@join);
            return this;
        }

        /// <param name="maximumEntitiesToLoad"></param>
        /// <inheritdoc />
        public ISqlGenerator AddTopFilter(int maximumEntitiesToLoad)
        {
            this.TopFilterCount = maximumEntitiesToLoad;
            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddCTE(string loadSql)
        {
            this.QueryForCTE = loadSql;
            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddOrderByAscending(string columnName)
        {
            this.OrderByColumnAscending = columnName;
            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddRangeFilter(string columnName, string startVariable, string endVariable)
        {
            this.RangeFilter = new SqlRangeFilter
                                   {
                                       ColumnName = columnName,
                                       StartVariable = startVariable,
                                       EndVariable = endVariable
                                   };
            return this;
        }

        /// <inheritdoc />
        public virtual string ToSqlString()
        {
            if (this.QueryForCTE != null)
            {
                return this.GenerateSqlStringForCTE();
            }

            var sb = new StringBuilder();
            this.AppendSelectStatement(sb, this.DestinationEntity);

            foreach (var selectJoin in this.SelectJoins)
            {
                sb.AppendLine(
                    $"INNER JOIN {selectJoin.SourceEntity} ON {selectJoin.SourceEntity}.[{selectJoin.SourceEntityKey}] = {selectJoin.DestinationEntity}.[{selectJoin.DestinationEntityKey}]");
            }

            return sb.ToString();
        }

        /// <summary>
        /// The append select statement.
        /// </summary>
        /// <param name="sb">
        ///     The sb.
        /// </param>
        /// <param name="destinationEntity">
        /// destination entity</param>
        protected virtual void AppendSelectStatement(StringBuilder sb, string destinationEntity)
        {
            sb.AppendLine("SELECT");
            if (this.TopFilterCount > 0)
            {
                sb.AppendLine($"TOP {this.TopFilterCount}");
            }

            if (!this.SelectColumns.Any())
            {
                this.AddColumn("*");
            }

            var columnList = this.GetColumnList();

            sb.AppendLine(columnList);
            sb.AppendLine($"FROM {destinationEntity}");

            if (this.RangeFilter != null)
            {
                sb.AppendLine(
                    $"WHERE [{this.RangeFilter.ColumnName}] BETWEEN {this.RangeFilter.StartVariable} AND {this.RangeFilter.EndVariable}");
            }

            if (this.OrderByColumnAscending != null)
            {
                sb.AppendLine($"ORDER BY [{this.OrderByColumnAscending}] ASC");
            }
        }

        /// <summary>
        /// The generate sql string for cte.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected virtual string GenerateSqlStringForCTE()
        {
            var sb = new StringBuilder();

            sb.AppendLine($";WITH CTE AS ( {this.QueryForCTE} )");

            this.AppendSelectStatement(sb, "CTE");

            sb.Append(";");
            return sb.ToString();
        }

        /// <summary>
        /// The get column list.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected virtual string GetColumnList()
        {
            var list = new List<string>();
            foreach (var selectColumn in this.SelectColumns)
            {
                string fullColumnName = string.Empty;
                if (selectColumn.EntityName != null)
                {
                    fullColumnName = $"{selectColumn.EntityName}.";
                }

                if (selectColumn.ColumnName == "*")
                {
                    fullColumnName += "*";
                }
                else
                {
                    fullColumnName += $"[{selectColumn.ColumnName}]";
                }

                if (selectColumn.Alias != null)
                {
                    fullColumnName += $" AS [{selectColumn.Alias}]";
                }

                list.Add(fullColumnName);
            }

            return list.ToCsv();
        }
    }
}