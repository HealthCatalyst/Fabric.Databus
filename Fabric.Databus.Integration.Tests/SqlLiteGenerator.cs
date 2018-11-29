// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlLiteGenerator.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlLiteGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System.Linq;
    using System.Text;

    using Fabric.Databus.SqlGenerator;

    /// <inheritdoc />
    /// <summary>
    /// The sql lite generator.
    /// </summary>
    public class SqlLiteGenerator : AbstractBaseSqlGenerator
    {
        /// <inheritdoc />
        protected override void AppendSelectStatement(StringBuilder sb, string destinationEntity)
        {
            sb.AppendLine("SELECT");
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
            if (this.TopFilterCount > 0)
            {
                sb.AppendLine($"LIMIT {this.TopFilterCount}");
            }
        }

    }
}