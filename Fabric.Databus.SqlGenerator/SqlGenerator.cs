// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlGenerator.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlGeneratorColumn type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.SqlGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Fabric.Shared;

    /// <summary>
    /// The sql generator.
    /// </summary>
    public class SqlGenerator
    {
        /// <summary>
        /// The destination entity.
        /// </summary>
        private readonly string destinationEntity;

        /// <summary>
        /// Gets or sets the select columns.
        /// </summary>
        private readonly IList<SqlGeneratorColumn> selectColumns = new List<SqlGeneratorColumn>();

        /// <summary>
        /// The select joins.
        /// </summary>
        private readonly IList<SqlGeneratorJoin> selectJoins = new List<SqlGeneratorJoin>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlGenerator"/> class.
        /// </summary>
        /// <param name="destinationEntity">
        /// The destination entity.
        /// </param>
        public SqlGenerator(string destinationEntity)
        {
            this.destinationEntity = destinationEntity;
        }

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
        /// The <see cref="SqlGenerator"/>.
        /// </returns>
        public SqlGenerator AddColumn(string entityName, string columnName, string alias)
        {
            this.selectColumns.Add(new SqlGeneratorColumn
            {
                EntityName = entityName,
                ColumnName = columnName,
                Alias = alias
            });
            return this;
        }

        /// <summary>
        /// The add join.
        /// </summary>
        /// <param name="join">
        /// The join.
        /// </param>
        /// <returns>
        /// The <see cref="SqlGenerator"/>.
        /// </returns>
        public SqlGenerator AddJoin(SqlGeneratorJoin join)
        {
            this.selectJoins.Add(join);
            return this;
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT");
            var columnList = this.selectColumns.Select(
                selectColumn => string.Join(
                                    ".",
                                    new List<string>
                                        {
                                            $"{selectColumn.EntityName}",
                                            (selectColumn.ColumnName != "*"
                                                 ? $"[{selectColumn.ColumnName}]"
                                                 : selectColumn.ColumnName)
                                        }) + (selectColumn.Alias != null
                                                  ? $" AS [{selectColumn.Alias}]"
                                                  : string.Empty))
                .ToList()
                .ToCsv();

            sb.AppendLine(columnList);
            sb.AppendLine($"FROM {this.destinationEntity}");

            foreach (var selectJoin in this.selectJoins)
            {
                sb.AppendLine(
                        $"INNER JOIN {selectJoin.SourceEntity} ON {selectJoin.SourceEntity}.[{selectJoin.SourceEntityKey}] = {selectJoin.DestinationEntity}.[{selectJoin.DestinationEntityKey}]");
            }

            return sb.ToString();
        }
    }
}
