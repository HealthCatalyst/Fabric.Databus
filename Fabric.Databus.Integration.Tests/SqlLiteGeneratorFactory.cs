// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlLiteGeneratorFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlLiteGeneratorFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using Fabric.Databus.Interfaces.Sql;

    /// <inheritdoc />
    public class SqlLiteGeneratorFactory : ISqlGeneratorFactory
    {
        /// <inheritdoc />
        public ISqlGenerator Create()
        {
            return new SqlLiteGenerator();
        }
    }
}