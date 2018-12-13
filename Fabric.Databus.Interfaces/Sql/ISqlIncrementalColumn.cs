// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISqlIncrementalColumn.cs" company="">
//   
// </copyright>
// <summary>
//   The SqlIncrementalColumn interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Sql
{
    /// <summary>
    /// The SqlIncrementalColumn interface.
    /// </summary>
    public interface ISqlIncrementalColumn
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        string Operator { get; set; }

        /// <summary>
        /// Gets the sql operator.
        /// </summary>
        string SqlOperator { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Gets or sets the sql type.
        /// </summary>
        string Type { get; set; }
    }
}