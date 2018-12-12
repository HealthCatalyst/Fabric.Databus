// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReliableHttpException.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ReliableHttpException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <inheritdoc />
    /// <summary>
    /// The reliable http exception.
    /// </summary>
    public class ReliableHttpException : Exception
    {
        /// <inheritdoc />
        public ReliableHttpException()
        {
        }

        /// <inheritdoc />
        public ReliableHttpException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public ReliableHttpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        public ReliableHttpException(Uri fullUri, string method, Exception e)
            : base($"Error for url: {method} {fullUri}", e)
        {
        }

        /// <inheritdoc />
        protected ReliableHttpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
