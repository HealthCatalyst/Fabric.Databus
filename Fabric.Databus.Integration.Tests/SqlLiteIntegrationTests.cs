// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlLiteIntegrationTests.cs" company="">
//   
// </copyright>
// <summary>
//   The sql lite integration tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System;
    using System.Data.Common;
    using System.Data.SQLite;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The sql lite integration tests.
    /// </summary>
    [TestClass]
    public class SqlLiteIntegrationTests
    {
        /// <summary>
        /// The test creating database.
        /// </summary>
        [TestMethod]
        public void TestCreatingDatabase()
        {
            string connectionString = "Data Source=:memory:";

            var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string sql = "create table highscores (name varchar(20), score int)";

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();

            sql = "insert into highscores (name, score) values ('Me', 9001)";

            command.CommandText = sql;
            command.ExecuteNonQuery();

            sql = "select * from highscores";

            command.CommandText = sql;
            var reader = command.ExecuteReader();

            var values = new object[2];

            while (reader.Read())
            {
                var read = reader.GetValues(values);
            }

            connection.Close();

            Assert.AreEqual("Me", values[0]);
            Assert.AreEqual(9001, values[1]);
        }

        /// <summary>
        /// The can connect to sql lite via sql connection.
        /// </summary>
        [TestMethod]
        public void CanConnectToSqlLiteViaSqlConnection()
        {
            string connectionString = "Data Source=:memory:";

            using (DbConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Close();
            }
        }
    }
}
