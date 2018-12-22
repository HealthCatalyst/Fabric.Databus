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
    using System.Threading.Tasks;

    /// <summary>
    /// The EntitySavedToJSONLogger interface.
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
        /// The work item id.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task LogSavedEntityAsync(string workItemId, Stream stream);
    }
}
