// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusValidationException.cs" company="">
//   
// </copyright>
// <summary>
//   The databus validation exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The databus validation exception.
    /// </summary>
    public class DatabusValidationException : Exception
    {
        /// <inheritdoc />
        public DatabusValidationException()
        {
        }

        /// <inheritdoc />
        public DatabusValidationException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public DatabusValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        protected DatabusValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
