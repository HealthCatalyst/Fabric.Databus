// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullEntitySavedToJsonLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NullEntitySavedToJsonLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Loggers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Loggers;

    /// <summary>
    /// The null entity saved to json logger.
    /// </summary>
    public class NullEntitySavedToJsonLogger : IEntitySavedToJsonLogger
    {
        /// <inheritdoc />
        public bool IsWritingEnabled => false;

        /// <inheritdoc />
        public Task LogSavedEntityAsync(string workItemId, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
