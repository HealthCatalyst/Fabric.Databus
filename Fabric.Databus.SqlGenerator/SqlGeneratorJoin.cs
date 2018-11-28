namespace Fabric.Databus.SqlGenerator
{
    public class SqlGeneratorJoin
    {
        public string SourceEntity { get; set; }
        public string SourceEntityKey { get; set; }

        public string DestinationEntity { get; set; }

        public string DestinationEntityKey { get; set; }
    }
}