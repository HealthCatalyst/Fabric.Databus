// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEntitySavedToJsonLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IEntitySavedToJsonLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Loggers
{
    using System.IO;

    /// <summary>
    /// The EntitySavedToJsonLogger interface.
    /// </summary>
    public interface IEntitySavedToJsonLogger
    {
        /// <summary>
        /// Gets a value indicating whether is writing enabled.
        /// </summary>
        bool IsWritingEnabled { get; }

        /// <summary>
        /// The log saved entity.
        /// </summary>
        /// <param name="workItemId">
        ///     The work item id.
        /// </param>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        void LogSavedEntity(string workItemId, Stream stream);
    }
}
