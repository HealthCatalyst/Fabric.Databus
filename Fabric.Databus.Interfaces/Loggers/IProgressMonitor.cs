// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProgressMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IProgressMonitor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Loggers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The ProgressMonitor interface.
    /// </summary>
    public interface IProgressMonitor
    {
        /// <summary>
        /// Gets or sets the job history update action.
        /// </summary>
        Action JobHistoryUpdateAction { get; set; }

        /// <summary>
        /// The set progress item.
        /// </summary>
        /// <param name="progressMonitorItem">
        /// The progress monitor item.
        /// </param>
        void SetProgressItem(ProgressMonitorItem progressMonitorItem);

        /// <summary>
        /// The get snapshot of progress items.
        /// </summary>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        IList<ProgressMonitorItem> GetSnapshotOfProgressItems();

        /// <summary>
        /// The complete progress items with unique id.
        /// </summary>
        /// <param name="uniqueId">
        /// The unique id.
        /// </param>
        void CompleteProgressItemsWithUniqueId(int uniqueId);
    }
}