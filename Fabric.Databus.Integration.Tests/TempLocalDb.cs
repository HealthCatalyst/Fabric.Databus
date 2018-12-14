// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TempLocalDb.cs" company="">
//   
// </copyright>
// <summary>
//   Represents a local database using SQL LocalDB that is automatically (re)created each time an instance of this class is created and is deleted when the instance is disposed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System;

    /// <summary>
    /// Represents a local database using SQL LocalDB that is automatically (re)created each time an instance of this class is created and is deleted when the instance is disposed.
    /// </summary>
    // ReSharper disable once CommentTypo
    // ReSharper disable once InheritdocConsiderUsage
    public sealed class TempLocalDb : LocalDb, IDisposable
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.Integration.Tests.TempLocalDb" /> class.
        /// </summary>
        /// <param name="databaseName">The name of the SQL LocalDB database.</param>
        /// <param name="dataSource">The SQL Server instance to connect to, by default v11.0</param>
        public TempLocalDb(string databaseName, string dataSource = @"(localdb)\MSSQLLocalDB")
            : base(databaseName, dataSource)
        {
            this.DeleteDatabase();
            this.CreateDatabase();
        }

        /// <inheritdoc />
        /// <summary>
        /// Deletes the database.
        /// </summary>
        public void Dispose()
        {
            this.DeleteDatabase();
        }
    }
}
