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
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    using Fabric.Databus.Interfaces.Sql;

    /// <inheritdoc />
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        /// <inheritdoc />
        public DbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
