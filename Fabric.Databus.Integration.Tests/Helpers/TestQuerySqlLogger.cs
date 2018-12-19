// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestQuerySqlLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TestQuerySqlLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests.Helpers
{
    using System;
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Loggers;

    /// <inheritdoc />
    public class TestQuerySqlLogger : IQuerySqlLogger
    {
        /// <summary>
        /// Gets the query sql log events.
        /// </summary>
        public List<QuerySqlLogEvent> QuerySqlCompletedEvents { get; } = new List<QuerySqlLogEvent>();

        /// <summary>
        /// Gets the query sql started events.
        /// </summary>
        public List<QuerySqlLogEvent> QuerySqlStartedEvents { get; } = new List<QuerySqlLogEvent>();

        /// <inheritdoc />
        public void SqlQueryCompleted(QuerySqlLogEvent querySqlLogEvent)
        {
            this.QuerySqlCompletedEvents.Add(querySqlLogEvent);
        }

        /// <inheritdoc />
        public void SqlQueryStarted(QuerySqlLogEvent querySqlLogEvent)
        {
            this.QuerySqlStartedEvents.Add(querySqlLogEvent);
        }
    }
}
