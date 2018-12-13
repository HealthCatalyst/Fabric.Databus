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
    using System.Text;

    using Fabric.Databus.SqlGenerator;

    /// <inheritdoc />
    /// <summary>
    /// The sql lite generator.
    /// </summary>
    public class SqlLiteGenerator : AbstractBaseSqlGenerator
    {
        /// <inheritdoc />
        protected override void InsertTopStatementAtBeginning(StringBuilder sb)
        {
        }

        /// <inheritdoc />
        protected override void InsertTopStatementAtEnd(StringBuilder sb)
        {
            if (this.TopFilterCount > 0)
            {
                sb.AppendLine($"LIMIT {this.TopFilterCount}");
            }
        }
    }
}