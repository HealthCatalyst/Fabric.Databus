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

    using Fabric.Databus.Interfaces.Loggers;

    /// <summary>
    /// The null entity saved to json logger.
    /// </summary>
    public class NullEntitySavedToJsonLogger : IEntitySavedToJsonLogger
    {
        /// <inheritdoc />
        public bool IsWritingEnabled => false;

        /// <inheritdoc />
        public void LogSavedEntity(string workItemId, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
