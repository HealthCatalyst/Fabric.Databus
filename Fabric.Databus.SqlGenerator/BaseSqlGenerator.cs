// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseSqlGenerator.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the BaseSqlGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.SqlGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Shared;

    /// <inheritdoc />
    public abstract class BaseSqlGenerator : ISqlGenerator
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
        /// The incremental columns.
        /// </summary>
        protected readonly IList<ISqlIncrementalColumn> IncrementalColumns = new List<ISqlIncrementalColumn>();

        /// <summary>
        /// Gets the destination entity.
        /// </summary>
        protected string DestinationEntity { get; private set; }

        /// <summary>
        /// Gets the top filter count.
        /// </summary>
        protected int TopFilterCount { get; private set; } = -1;

        /// <summary>
        /// Gets the query for cte.
        /// </summary>
        protected string QueryForCTE { get; private set; }

        /// <summary>
        /// Gets the order by column ascending.
        /// </summary>
        protected Tuple<string, string> OrderByColumnAscending { get; private set; }

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
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            this.AddColumn(null, columnName, null);
            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddJoin(ISqlGeneratorJoin join)
        {
            if (join == null)
            {
                throw new ArgumentNullException(nameof(join));
            }

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
            if (string.IsNullOrWhiteSpace(loadSql))
            {
                throw new ArgumentNullException(nameof(loadSql));
            }

            this.QueryForCTE = loadSql;
            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddOrderByAscending(string tableName, string columnName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            this.OrderByColumnAscending = new Tuple<string, string>(tableName, columnName);

            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddRangeFilter(string tableName, string columnName, string startVariable, string endVariable)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            if (string.IsNullOrWhiteSpace(startVariable))
            {
                throw new ArgumentNullException(nameof(startVariable));
            }

            if (string.IsNullOrWhiteSpace(endVariable))
            {
                throw new ArgumentNullException(nameof(endVariable));
            }

            this.RangeFilter = new SqlRangeFilter
            {
                TableName = tableName,
                ColumnName = columnName,
                StartVariable = startVariable,
                EndVariable = endVariable
            };
            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddIncrementalColumn(
            string tableName,
            string incrementalColumnName,
            string incrementalColumnOperator,
            string incrementalColumnValue,
            string incrementalColumnType)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (string.IsNullOrWhiteSpace(incrementalColumnName))
            {
                throw new ArgumentNullException(nameof(incrementalColumnName));
            }

            if (string.IsNullOrWhiteSpace(incrementalColumnOperator))
            {
                throw new ArgumentNullException(nameof(incrementalColumnOperator));
            }

            this.IncrementalColumns.Add(new SqlIncrementalColumn
            {
                TableOrView = tableName,
                Name = incrementalColumnName,
                Operator = incrementalColumnOperator,
                Type = incrementalColumnType,
                Value = incrementalColumnValue
            });

            return this;
        }

        /// <inheritdoc />
        public ISqlGenerator AddIncrementalColumns(string tableName, IEnumerable<IIncrementalColumn> incrementalColumns)
        {
            foreach (var incrementalColumn in incrementalColumns)
            {
                this.AddIncrementalColumn(
                    tableName,
                    incrementalColumn.Name,
                    incrementalColumn.Operator,
                    incrementalColumn.Value,
                    incrementalColumn.Type);
            }

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

            return sb.ToString();
        }

        /// <inheritdoc />
        public ISqlGenerator CreateSqlStatement(
            string entityName,
            string topLevelKey,
            IEnumerable<ISqlRelationship> sqlRelationships,
            IEnumerable<ISqlEntityColumnMapping> entityColumnMappings,
            IEnumerable<IIncrementalColumn> incrementalColumns)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException(nameof(entityName));
            }

            if (string.IsNullOrWhiteSpace(topLevelKey))
            {
                throw new ArgumentNullException(nameof(topLevelKey));
            }

            if (sqlRelationships == null)
            {
                throw new ArgumentNullException(nameof(sqlRelationships));
            }

            if (entityColumnMappings == null)
            {
                throw new ArgumentNullException(nameof(entityColumnMappings));
            }

            if (incrementalColumns == null)
            {
                throw new ArgumentNullException(nameof(incrementalColumns));
            }

            this.SetEntity(entityName);

            var sqlRelationships1 = sqlRelationships.Reverse().ToList();

            var sqlEntityColumnMappings = entityColumnMappings.ToList();

            if (!sqlRelationships1.Any())
            {
                this.SetEntity(entityName);

                if (sqlEntityColumnMappings.Any())
                {
                    foreach (var sqlEntityColumnMapping in sqlEntityColumnMappings)
                    {
                        this.AddColumn(
                            sqlEntityColumnMapping.Entity ?? entityName,
                            sqlEntityColumnMapping.Name,
                            sqlEntityColumnMapping.Alias);
                    }
                }
                else
                {
                    this.AddColumn(entityName, "*", null);
                }

                this.AddColumn(entityName, topLevelKey, "KeyLevel1");

                this.AddIncrementalColumns(entityName, incrementalColumns);

                return this;
            }

            var sqlRelationshipsCount = sqlRelationships1.Count();

            var destinationEntity = sqlRelationships1.First().Destination.Entity;

            this.SetEntity(destinationEntity);

            if (sqlEntityColumnMappings.Any())
            {
                foreach (var sqlEntityColumnMapping in sqlEntityColumnMappings)
                {
                    this.AddColumn(
                        sqlEntityColumnMapping.Entity ?? entityName,
                        sqlEntityColumnMapping.Name,
                        sqlEntityColumnMapping.Alias);
                }
            }
            else
            {
                this.AddColumn(destinationEntity, "*", null);
            }

            int relationshipIndex = sqlRelationshipsCount + 2;
            foreach (var sqlRelationship in sqlRelationships1)
            {
                relationshipIndex--;
                this.AddColumn(
                    sqlRelationship.Destination.Entity,
                    sqlRelationship.Destination.Key,
                    $"KeyLevel{relationshipIndex}");
                this.AddJoin(
                    new SqlGeneratorJoin
                    {
                        SourceEntity = sqlRelationship.Source.Entity,
                        SourceEntityKey = sqlRelationship.Source.Key,
                        DestinationEntity = sqlRelationship.Destination.Entity,
                        DestinationEntityKey = sqlRelationship.Destination.Key
                    });
            }

            var topLevelEntity = sqlRelationships1.Last().Source.Entity;

            this.AddColumn(topLevelEntity, topLevelKey, "KeyLevel1");

            this.AddIncrementalColumns(topLevelEntity, incrementalColumns);

            return this;
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
            if (sb == null)
            {
                throw new ArgumentNullException(nameof(sb));
            }

            if (string.IsNullOrWhiteSpace(destinationEntity))
            {
                throw new ArgumentNullException(nameof(destinationEntity));
            }

            sb.AppendLine("SELECT");
            this.InsertTopStatementAtBeginning(sb);

            if (!this.SelectColumns.Any())
            {
                this.AddColumn("*");
            }

            var columnList = this.GetColumnList();

            sb.AppendLine(columnList);
            sb.AppendLine($"FROM {destinationEntity}");

            foreach (var selectJoin in this.SelectJoins)
            {
                sb.AppendLine(
                    $"INNER JOIN {selectJoin.SourceEntity} ON {selectJoin.SourceEntity}.[{selectJoin.SourceEntityKey}] = {selectJoin.DestinationEntity}.[{selectJoin.DestinationEntityKey}]");
            }

            if (this.RangeFilter != null)
            {
                sb.AppendLine(
                    $"WHERE {this.RangeFilter.TableName}.[{this.RangeFilter.ColumnName}] BETWEEN {this.RangeFilter.StartVariable} AND {this.RangeFilter.EndVariable}");
            }
            else
            {
                sb.AppendLine(
                    $"WHERE 1=1");
            }

            int i = 0;
            foreach (var incrementalColumn in this.IncrementalColumns)
            {
                sb.AppendLine(
                    $"AND {incrementalColumn.TableOrView}.[{incrementalColumn.Name}] {incrementalColumn.SqlOperator} @incrementColumnValue{++i}");
            }

            if (this.OrderByColumnAscending != null)
            {
                sb.AppendLine($"ORDER BY {this.OrderByColumnAscending.Item1}.[{this.OrderByColumnAscending.Item2}] ASC");
            }

            this.InsertTopStatementAtEnd(sb);
        }

        /// <summary>
        /// The insert top statement at beginning.
        /// </summary>
        /// <param name="sb">
        /// The sb.
        /// </param>
        protected virtual void InsertTopStatementAtBeginning(StringBuilder sb)
        {
            if (sb == null)
            {
                throw new ArgumentNullException(nameof(sb));
            }

            if (this.TopFilterCount >= 0)
            {
                sb.AppendLine($"TOP {this.TopFilterCount}");
            }
        }

        /// <summary>
        /// The insert top statement at end.
        /// </summary>
        /// <param name="sb">
        /// The sb.
        /// </param>
        protected virtual void InsertTopStatementAtEnd(StringBuilder sb)
        {
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