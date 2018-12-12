// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusPipelineStepWorkItemException.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DatabusPipelineStepWorkItemException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    /// <summary>
    /// The databus pipeline step work item exception.
    /// </summary>
    public class DatabusPipelineStepWorkItemException : Exception
    {
        /// <inheritdoc />
        public DatabusPipelineStepWorkItemException()
        {
        }

        /// <inheritdoc />
        public DatabusPipelineStepWorkItemException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public DatabusPipelineStepWorkItemException(string message, Exception innerException) : base(message, innerException)
        {
        }        
        
        /// <inheritdoc />
        public DatabusPipelineStepWorkItemException(IQueueItem workItem, Exception innerException)
            : base("Error in work item " + workItem.QueryId, innerException)
        {
        }

        /// <inheritdoc />
        protected DatabusPipelineStepWorkItemException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
