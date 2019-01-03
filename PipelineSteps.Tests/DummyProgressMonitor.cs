// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyProgressMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DummyProgressMonitor type.
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
    public class DummyProgressMonitor : IProgressMonitor
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

        /// <inheritdoc />
        public void CompleteProgressItemsWithUniqueId(int uniqueId)
        {
        }
    }
}