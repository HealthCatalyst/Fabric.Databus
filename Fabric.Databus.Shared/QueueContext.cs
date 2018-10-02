// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueContext.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the QueueContext type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using Fabric.Databus.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The queue context.
    /// </summary>
    public class QueueContext : IQueueContext
    {
        /// <inheritdoc />
        public IQueryConfig Config { get; set; }
    }
}
