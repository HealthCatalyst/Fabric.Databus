// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlGeneratorJoin.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlGeneratorJoin type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.SqlGenerator
{
    using Fabric.Databus.Interfaces.Sql;

    /// <inheritdoc />
    public class SqlGeneratorJoin : ISqlGeneratorJoin
    {
        /// <inheritdoc />
        public string SourceEntity { get; set; }

        /// <inheritdoc />
        public string SourceEntityKey { get; set; }

        /// <inheritdoc />
        public string DestinationEntity { get; set; }

        /// <inheritdoc />
        public string DestinationEntityKey { get; set; }
    }
}