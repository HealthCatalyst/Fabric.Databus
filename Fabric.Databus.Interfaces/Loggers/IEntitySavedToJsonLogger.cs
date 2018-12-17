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

    public interface IEntitySavedToJsonLogger
    {
        bool IsWritingEnabled { get; }

        void LogSavedEntity(string workItemId, MemoryStream stream);
    }
}
