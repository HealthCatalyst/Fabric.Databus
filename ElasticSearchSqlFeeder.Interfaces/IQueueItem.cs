namespace Fabric.Databus.Interfaces
{
    public interface IQueueItem
    {
        string QueryId { get; set; }

        string PropertyName { get; set; }
    }
}
