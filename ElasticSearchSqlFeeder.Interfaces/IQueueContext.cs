// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueueContext.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IQueueContext type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces
{
    /// <summary>
    /// The QueueContext interface.
    /// </summary>
    public interface IQueueContext
    {
        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        IQueryConfig Config { get; set; }
    }
}
