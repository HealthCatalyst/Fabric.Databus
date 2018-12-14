// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the CollectionExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;
    using System.Linq;

    using Fabric.Databus.Interfaces.Config;

    /// <summary>
    /// The collection extensions.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <param name="incrementalColumn">
        /// The incremental column.
        /// </param>
        public static void Add(this IEnumerable<IIncrementalColumn> list, IncrementalColumn incrementalColumn)
        {
            list.Append(incrementalColumn);
        }
    }
}
