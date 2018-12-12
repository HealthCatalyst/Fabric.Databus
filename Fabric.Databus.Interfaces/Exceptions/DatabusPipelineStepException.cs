// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusPipelineStepException.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DatabusPipelineStepException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <inheritdoc />
    /// <summary>
    /// The pipeline step exception.
    /// </summary>
    public class DatabusPipelineStepException : Exception
    {
        /// <inheritdoc />
        public DatabusPipelineStepException()
        {
        }

        /// <inheritdoc />
        public DatabusPipelineStepException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public DatabusPipelineStepException(string loggerName, Exception innerException) : base(loggerName, innerException)
        {
        }

        /// <inheritdoc />
        protected DatabusPipelineStepException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
