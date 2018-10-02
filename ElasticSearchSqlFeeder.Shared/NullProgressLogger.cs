// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullProgressLogger.cs" company="">
//   
// </copyright>
// <summary>
//   The null progress logger.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System;

    using Fabric.Databus.Interfaces;

    /// <summary>
    /// The null progress logger.
    /// </summary>
    public class NullProgressLogger : IProgressLogger
    {

        public void Reset()
        {
        }

        public string GetLog()
        {
            throw new NotImplementedException();
        }

        public void LogProgressMonitorItem(int key, ProgressMonitorItem progressMonitorItem)
        {
        }
    }
}