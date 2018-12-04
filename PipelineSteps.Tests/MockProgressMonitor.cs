// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockProgressMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MockProgressMonitor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineStep.Tests
{
    using System;
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Loggers;

    /// <inheritdoc />
    /// <summary>
    /// The mock progress monitor.
    /// </summary>
    public class MockProgressMonitor : IProgressMonitor
    {
        /// <inheritdoc />
        public Action JobHistoryUpdateAction { get; set; }

        /// <inheritdoc />
        public void SetProgressItem(ProgressMonitorItem progressMonitorItem)
        {
        }

        /// <inheritdoc />
        public IList<ProgressMonitorItem> GetSnapshotOfProgressItems()
        {
            throw new NotImplementedException();
        }
    }
}