// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlGeneratorFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlGeneratorFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.SqlGenerator
{
    using Fabric.Databus.Interfaces.Sql;

    /// <inheritdoc />
    public class SqlGeneratorFactory : ISqlGeneratorFactory
    {
        /// <inheritdoc />
        public ISqlGenerator Create()
        {
            return new SqlGenerator();
        }
    }
}
