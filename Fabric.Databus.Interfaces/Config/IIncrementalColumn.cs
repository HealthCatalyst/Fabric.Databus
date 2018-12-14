// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIncrementalColumn.cs" company="">
//   
// </copyright>
// <summary>
//   The IncrementalColumn interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Config
{
    /// <summary>
    /// The IncrementalColumn interface.
    /// </summary>
    public interface IIncrementalColumn
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
        /// Gets or sets the value.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        string Type { get; set; }
    }
}