// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBatchCompletedQueueItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IBatchCompletedQueueItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Queues
{
    /// <inheritdoc />
    /// <summary>
    /// The BatchCompletedQueueItem interface.
    /// </summary>
    public interface IBatchCompletedQueueItem : IQueueItem
    {
        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        string Start { get; set; }

        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        string End { get; set; }

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