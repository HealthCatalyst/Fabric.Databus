// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlRangeFilter.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlRangeFilter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.SqlGenerator
{
    /// <summary>
    /// The sql range filter.
    /// </summary>
    public class SqlRangeFilter
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the start variable.
        /// </summary>
        public string StartVariable { get; set; }

        /// <summary>
        /// Gets or sets the end variable.
        /// </summary>
        public string EndVariable { get; set; }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string TableName { get; set; }
    }
}