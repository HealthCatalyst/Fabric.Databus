namespace QueueProcessor.Tests
{
    using System;
    using System.Collections.Generic;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.ProgressMonitor;

    public class MockProgressMonitor : IProgressMonitor
    {
        public Action JobHistoryUpdateAction { get; set; }
        public void SetProgressItem(ProgressMonitorItem progressMonitorItem)
        {
        }

        public IList<ProgressMonitorItem> GetSnapshotOfProgressItems()
        {
            throw new NotImplementedException();
        }
    }
}