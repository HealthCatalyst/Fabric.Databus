// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullProgressLogger.cs" company="">
//   
// </copyright>
// <summary>
//   The null progress logger.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Loggers
{
    using System;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Loggers;

    /// <inheritdoc />
    /// <summary>
    /// The null progress logger.
    /// </summary>
    public class NullProgressLogger : IProgressLogger
    {
        /// <inheritdoc />
        public void Reset()
        {
        }

        /// <inheritdoc />
        public string GetLog()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void LogProgressMonitorItem(string key, ProgressMonitorItem progressMonitorItem)
        {
        }

        /// <inheritdoc />
        public void LogHeader()
        {
        }

        /// <inheritdoc />
        public void LogFooter()
        {
        }
    }
}