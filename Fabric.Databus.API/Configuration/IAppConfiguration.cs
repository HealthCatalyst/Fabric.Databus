using Fabric.Platform.Shared.Configuration;

namespace Fabric.Databus.API.Configuration
{
		public interface IAppConfiguration
		{
				Domain.Configuration.ElasticSearchSettings ElasticSearchSettings { get; }
				IdentityServerConfidentialClientSettings IdentityServerConfidentialClientSettings { get; }
		}
}
