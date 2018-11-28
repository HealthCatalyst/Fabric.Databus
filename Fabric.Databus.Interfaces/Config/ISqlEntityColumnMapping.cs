namespace Fabric.Databus.Config
{
    public interface ISqlEntityColumnMapping
    {
        string Entity { get; set; }

        string Name { get; set; }

        string Alias { get; set; }
    }
}