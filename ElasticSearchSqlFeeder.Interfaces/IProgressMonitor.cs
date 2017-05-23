using System;
using System.Collections.Generic;
using ElasticSearchSqlFeeder.ProgressMonitor;

namespace ElasticSearchSqlFeeder.Interfaces
{
    public interface IProgressMonitor
    {
        Action JobHistoryUpdateAction { get; set; }
        void SetProgressItem(ProgressMonitorItem progressMonitorItem);
        IList<ProgressMonitorItem> GetSnapshotOfProgressItems();
    }
}