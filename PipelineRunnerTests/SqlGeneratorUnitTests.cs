// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlGeneratorUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlGeneratorUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using Fabric.Databus.SqlGenerator;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The sql generator unit tests.
    /// </summary>
    [TestClass]
    public class SqlGeneratorUnitTests
    {
        /// <summary>
        /// The generates sql for top 1.
        /// </summary>
        [TestMethod]
        public void GeneratesSqlForCTE()
        {
            string expected = @";WITH CTE AS ( SELECT * FROM Clinical.Orders )
SELECT
*
FROM CTE
;";

            var actual = new SqlGenerator().AddCTE("SELECT * FROM Clinical.Orders").ToSqlString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// The generates sql for top 1.
        /// </summary>
        [TestMethod]
        public void GeneratesSqlForCTETop1()
        {
            string expected = @";WITH CTE AS ( SELECT * FROM Clinical.Orders )
SELECT
TOP 1
*
FROM CTE
;";

            var actual = new SqlGenerator().AddCTE("SELECT * FROM Clinical.Orders").AddTopFilter(1).ToSqlString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// The generates sql for top 1.
        /// </summary>
        [TestMethod]
        public void GeneratesSqlForCTETopWithOrdering()
        {
            string expected = @";WITH CTE AS ( SELECT * FROM Clinical.Orders )
SELECT
TOP 5
*
FROM CTE
ORDER BY [OrderID] ASC
;";

            var actual = new SqlGenerator().AddCTE("SELECT * FROM Clinical.Orders")
                .AddTopFilter(5)
                .AddOrderByAscending("OrderID")
                .ToSqlString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// The generates sql for top 1.
        /// </summary>
        [TestMethod]
        public void GeneratesSqlForCTEOrderBy()
        {
            string expected = @";WITH CTE AS ( SELECT * FROM Clinical.Orders )
SELECT
*
FROM CTE
ORDER BY [BindingID] ASC
;";

            var actual = new SqlGenerator().AddCTE("SELECT * FROM Clinical.Orders").AddOrderByAscending("BindingID").ToSqlString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// The generates sql for top 1.
        /// </summary>
        [TestMethod]
        public void GeneratesSqlForCTEOrderBySelectOneColumn()
        {
            string expected = @";WITH CTE AS ( SELECT * FROM Clinical.Orders )
SELECT
[OrderID]
FROM CTE
ORDER BY [BindingID] ASC
;";

            var actual = new SqlGenerator().AddCTE("SELECT * FROM Clinical.Orders").AddOrderByAscending("BindingID")
                .AddColumn("OrderID")
                .ToSqlString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// The generates sql for top 1.
        /// </summary>
        [TestMethod]
        public void GeneratesSqlForCTEOrderByWithRange()
        {
            string expected = @";WITH CTE AS ( SELECT * FROM Clinical.Orders )
SELECT
*
FROM CTE
WHERE [BindingID] BETWEEN @start AND @end
ORDER BY [BindingID] ASC
;";

            var actual = new SqlGenerator().AddCTE("SELECT * FROM Clinical.Orders")
                .AddRangeFilter("BindingID", "@start", "@end")
                .AddOrderByAscending("BindingID")
                .ToSqlString();

            Assert.AreEqual(expected, actual);
        }
    }
}
