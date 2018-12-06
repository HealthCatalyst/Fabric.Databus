// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlConnectionFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlConnectionFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System.Data.Common;

    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Shared.ReliableSql;

    /// <inheritdoc />
    public class ReliableSqlConnectionFactory : ISqlConnectionFactory
    {
        /// <inheritdoc />
        public DbConnection GetConnection(string connectionString)
        {
            return new ReliableSqlDbConnection(connectionString);
        }
    }
}
