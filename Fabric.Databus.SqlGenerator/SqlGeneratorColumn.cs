// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlGeneratorColumn.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlGeneratorColumn type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.SqlGenerator
{
    using Fabric.Databus.Interfaces.Sql;

    /// <inheritdoc />
    public class SqlGeneratorColumn : ISqlGeneratorColumn
    {
        /// <inheritdoc />
        public string EntityName { get; set; }

        /// <inheritdoc />
        public string ColumnName { get; set; }

        /// <inheritdoc />
        public string Alias { get; set; }
    }
}