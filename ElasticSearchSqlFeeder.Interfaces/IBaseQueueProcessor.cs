namespace ElasticSearchSqlFeeder.Interfaces
{
    public interface IBaseQueueProcessor
    {
        void MonitorWorkQueue();
        void MarkOutputQueueAsCompleted(int stepNumber);
        void InitializeWithStepNumber(int stepNumber);
        void CreateOutQueue(int stepNumber);
    }
}