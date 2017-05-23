using ElasticSearchSqlFeeder.ProgressMonitor;

namespace ElasticSearchSqlFeeder.Interfaces
{
    public interface IProgressLogger
    {
        void Reset();
        string GetLog();
        void LogProgressMonitorItem(int key, ProgressMonitorItem progressMonitorItem);
    }
}