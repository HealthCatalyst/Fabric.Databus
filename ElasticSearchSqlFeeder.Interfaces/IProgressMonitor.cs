namespace Fabric.Databus.Interfaces
{
    using System;
    using System.Collections.Generic;

    public interface IProgressMonitor
    {
        Action JobHistoryUpdateAction { get; set; }
        void SetProgressItem(ProgressMonitorItem progressMonitorItem);
        IList<ProgressMonitorItem> GetSnapshotOfProgressItems();
    }
}