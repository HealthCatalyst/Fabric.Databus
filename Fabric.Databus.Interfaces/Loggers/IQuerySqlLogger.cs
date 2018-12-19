// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQuerySqlLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IQuerySqlLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Loggers
{
    /// <summary>
    /// The QuerySqlLogger interface.
    /// </summary>
    public interface IQuerySqlLogger
    {
        /// <summary>
        /// The log sql query.
        /// </summary>
        /// <param name="querySqlLogEvent">
        /// The query sql log event.
        /// </param>
        void SqlQueryCompleted(QuerySqlLogEvent querySqlLogEvent);

        /// <summary>
        /// The sql query started.
        /// </summary>
        /// <param name="querySqlLogEvent">
        /// The query sql log event.
        /// </param>
        void SqlQueryStarted(QuerySqlLogEvent querySqlLogEvent);
    }
}
