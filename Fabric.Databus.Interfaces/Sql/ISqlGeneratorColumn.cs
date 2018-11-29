// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISqlGeneratorColumn.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ISqlGeneratorColumn type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Sql
{
    /// <summary>
    /// The SqlGeneratorColumn interface.
    /// </summary>
    public interface ISqlGeneratorColumn
    {
        /// <summary>
        /// Gets or sets the entity name.
        /// </summary>
        string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        string Alias { get; set; }
    }
}