namespace Fabric.Databus.Integration.Tests
{
    using System;
    using System.Data.SQLite;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EndToEndIntegrationTests
    {
        [TestMethod]
        public void TestCreatingDatabase()
        {
            string connectionString = "Data Source=:memory:";

            var connection = new SQLiteConnection(connectionString);
            connection.Open();

            string sql = "create table highscores (name varchar(20), score int)";

            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = "insert into highscores (name, score) values ('Me', 9001)";

            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = "select * from highscores";

            command = new SQLiteCommand(sql, connection);
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
    }
}
