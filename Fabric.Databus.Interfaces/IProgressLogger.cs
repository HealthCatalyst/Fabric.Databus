namespace Fabric.Databus.Interfaces
{
    public interface IProgressLogger
    {
        void Reset();
        string GetLog();
        void LogProgressMonitorItem(int key, ProgressMonitorItem progressMonitorItem);
    }
}