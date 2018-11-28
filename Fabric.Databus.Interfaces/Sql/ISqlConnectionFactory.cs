// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISqlConnectionFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ISqlConnectionFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Sql
{
    using System.Data;
    using System.Data.Common;

    /// <summary>
    /// The SqlConnectionFactory interface.
    /// </summary>
    public interface ISqlConnectionFactory
    {
        /// <summary>
        /// The get connection.
        /// </summary>
        /// <param name="connectionString">
        ///     The connection string.
        /// </param>
        /// <returns>
        /// The <see cref="IDbConnection"/>.
        /// </returns>
        DbConnection GetConnection(string connectionString);
    }
}