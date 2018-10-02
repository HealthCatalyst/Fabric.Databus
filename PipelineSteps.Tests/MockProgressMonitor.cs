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

    using Fabric.Databus.Interfaces;

    /// <summary>
    /// The mock progress monitor.
    /// </summary>
    public class MockProgressMonitor : IProgressMonitor
    {
        /// <summary>
        /// Gets or sets the job history update action.
        /// </summary>
        public Action JobHistoryUpdateAction { get; set; }

        /// <summary>
        /// The set progress item.
        /// </summary>
        /// <param name="progressMonitorItem">
        /// The progress monitor item.
        /// </param>
        public void SetProgressItem(ProgressMonitorItem progressMonitorItem)
        {
        }

        /// <summary>
        /// The get snapshot of progress items.
        /// </summary>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">exception thrown
        /// </exception>
        public IList<ProgressMonitorItem> GetSnapshotOfProgressItems()
        {
            throw new NotImplementedException();
        }
    }
}