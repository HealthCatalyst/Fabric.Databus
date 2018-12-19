// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJobCompletedQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IJobCompletedQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Queues
{
    /// <inheritdoc />
    /// <summary>
    /// The JobCompletedQueueItem interface.
    /// </summary>
    public interface IJobCompletedQueueItem : IQueueItem
    {
        /// <summary>
        /// Gets or sets the number of entities.
        /// </summary>
        int NumberOfEntities { get; set; }

        /// <summary>
        /// Gets or sets the number of entities uploaded.
        /// </summary>
        int NumberOfEntitiesUploaded { get; set; }
    }
}