using Fabric.Databus.Domain.Configuration;

namespace Fabric.Databus.API.Configuration
{
    public interface IAppConfiguration
    {
        ElasticSearchSettings ElasticSearchSettings { get; }
    }
}
