// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullQuerySqlLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NullQuerySqlLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Loggers
{
    using Fabric.Databus.Interfaces.Loggers;

    /// <inheritdoc />
    /// <summary>
    /// The null query sql logger.
    /// </summary>
    public class NullQuerySqlLogger : IQuerySqlLogger
    {
        /// <inheritdoc />
        public void SqlQueryCompleted(QuerySqlLogEvent querySqlLogEvent)
        {
        }

        /// <inheritdoc />
        public void SqlQueryStarted(QuerySqlLogEvent querySqlLogEvent)
        {
        }
    }
}
