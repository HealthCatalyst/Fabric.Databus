using Fabric.Databus.Domain.Configuration;

namespace Fabric.Databus.API.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        public ElasticSearchSettings ElasticSearchSettings { get; set; }
    }
}
