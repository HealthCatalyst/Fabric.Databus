namespace Fabric.Databus.Config
{
    public interface IQueryField
    {
        string Source { get; set; }

        string Destination { get; set; }

        ElasticSearchTypes DestinationType { get; set; }

        bool Skip { get; set; }

        QueryFieldTransform Transform { get; set; }
    }
}