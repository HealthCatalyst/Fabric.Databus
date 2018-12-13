// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusSqlException.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DatabusSqlException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <inheritdoc />
    /// <summary>
    /// The databus sql exception.
    /// </summary>
    public class DatabusSqlException : Exception
    {
        /// <inheritdoc />
        public DatabusSqlException()
        {
        }

        /// <inheritdoc />
        public DatabusSqlException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public DatabusSqlException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        protected DatabusSqlException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
