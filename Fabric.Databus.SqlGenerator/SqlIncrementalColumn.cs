// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlIncrementalColumn.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlIncrementalColumn type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.SqlGenerator
{
    using System;
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Sql;

    /// <inheritdoc />
    public class SqlIncrementalColumn : ISqlIncrementalColumn
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Operator { get; set; }

        /// <inheritdoc />
        public string Type { get; set; }

        /// <inheritdoc />
        public string TableOrView { get; set; }

        /// <inheritdoc />
        public string SqlOperator => this.ConvertOperatorToSqlOperator();

        /// <inheritdoc />
        public string Value { get; set; }

        /// <summary>
        /// The convert operator to sql operator.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// operator not found
        /// </exception>
        private string ConvertOperatorToSqlOperator()
        {
            switch (this.Operator)
            {
                case "GreaterThan":
                    return ">";

                case "GreaterThanOrEqualTo":
                    return ">=";

                case "LessThan":
                    return "<";

                case "LessThanOrEqualTo":
                    return "<=";

                case "EqualTo":
                    return "=";

                case "NotEqualTo":
                    return "<>";

                default:
                    throw new NotImplementedException($"No sql operator defined for {this.Operator}");
            }
        }

    }
}
