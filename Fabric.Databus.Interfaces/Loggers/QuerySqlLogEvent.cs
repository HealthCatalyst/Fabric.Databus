// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuerySqlLogEvent.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the QuerySqlLogEvent type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Loggers
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// The query sql log event.
    /// </summary>
    public class QuerySqlLogEvent
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the sql.
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// Gets or sets the sql parameters.
        /// </summary>
        public List<KeyValuePair<string, object>> SqlParameters { get; set; }

        /// <summary>
        /// Gets or sets the time elapsed.
        /// </summary>
        public TimeSpan TimeElapsed { get; set; }

        /// <summary>
        /// Gets or sets the table or view.
        /// </summary>
        public string TableOrView { get; set; }

        /// <summary>
        /// Gets or sets the batch number.
        /// </summary>
        public int BatchNumber { get; set; }

        /// <summary>
        /// Gets or sets the row count.
        /// </summary>
        public int RowCount { get; set; }
    }
}
