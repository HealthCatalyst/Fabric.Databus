namespace Fabric.Databus.Config
{
    public class QueryField
    {
        public string Source { get; set; }
        public string Destination { get; set; }

        public ElasticSearchTypes DestinationType { get; set; }

        public bool Skip { get; set; }

        public QueryFieldTransform Transform { get; set; }

    }
}