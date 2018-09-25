// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentProcessingEntity.cs" company="Health Catalyst">
//   2018
// </copyright>
// <summary>
//   Defines the CurrentProcessingEntity type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchJsonWriter
{
    /// <summary>
    /// The current processing entity lock type.
    /// </summary>
    public enum CurrentProcessingEntityLockType
    {
        /// <summary>
        /// The none.
        /// </summary>
        None,

        /// <summary>
        /// The add.
        /// </summary>
        Add,

        /// <summary>
        /// The update.
        /// </summary>
        Update
    }

    /// <summary>
    /// The current processing entity.
    /// </summary>
    public class CurrentProcessingEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the processing type.
        /// </summary>
        public CurrentProcessingEntityLockType ProcessingType { get; set; }
    }
}